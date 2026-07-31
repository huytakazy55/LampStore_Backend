using AutoMapper;
using LampStoreProjects.Data;
using LampStoreProjects.Models;
using LampStoreProjects.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LampStoreProjects.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDiscountCodeRepository _discountCodeRepository;

        // Tolerance (in VND) allowed between the client-displayed total and the
        // server-computed total before we reject the order as "prices changed".
        private const decimal PriceMismatchTolerance = 1000m;

        public OrderRepository(ApplicationDbContext context, IMapper mapper, IDiscountCodeRepository discountCodeRepository)
        {
            _context = context;
            _mapper = mapper;
            _discountCodeRepository = discountCodeRepository;
        }

        public async Task<IEnumerable<OrderModel>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100;

            var orders = await _context.Orders!
                .AsNoTracking()
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Orders!.CountAsync();
        }

        public async Task<OrderStatsModel> GetStatsAsync()
        {
            var orders = _context.Orders!.AsNoTracking();
            return new OrderStatsModel
            {
                Total = await orders.CountAsync(),
                Pending = await orders.CountAsync(o => o.Status == "Pending"),
                Unpaid = await orders.CountAsync(o => o.PaymentStatus == "Unpaid"),
                Shipping = await orders.CountAsync(o => o.Status == "Shipping"),
                Completed = await orders.CountAsync(o => o.Status == "Completed"),
                FailedDelivery = await orders.CountAsync(o => o.Status == "FailedDelivery"),
                ReturnRequested = await orders.CountAsync(o => o.Status == "ReturnRequested"),
                Revenue = await orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m
            };
        }

        public async Task<IEnumerable<OrderModel>> GetByUserIdAsync(string userId)
        {
            var orders = await _context.Orders!
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<OrderModel?> GetByIdAsync(Guid id)
        {
            var order = await _context.Orders!
                .AsNoTracking()
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;
            return MapOrderToModel(order);
        }

        /// <summary>
        /// Creates an order with fully server-computed pricing and atomic stock
        /// decrement. Never trusts client-supplied prices/totals:
        ///  - Unit price is re-fetched from ProductVariant (or the active FlashSaleItem)
        ///  - Discount amount is recomputed from the DiscountCode record's own rules
        ///  - Stock (ProductVariant and, if applicable, FlashSaleItem) is decremented
        ///    atomically via ExecuteUpdateAsync inside a DB transaction, so concurrent
        ///    orders can never oversell.
        ///  - If the client-shown total disagrees with the server-computed total by
        ///    more than a small rounding tolerance, the order is rejected instead of
        ///    silently overridden (prices likely changed between add-to-cart and checkout).
        /// </summary>
        public async Task<OrderCreationResult> CreateOrderAsync(OrderModel orderModel, string? idempotencyKey = null)
        {
            if (orderModel.OrderItems == null || orderModel.OrderItems.Count == 0)
            {
                return OrderCreationResult.Fail(ErrorCodes.ORDER_EMPTY);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var now = DateTimeHelper.VietnamNow;

                // Idempotency guard: reserve the client-supplied key before doing any
                // work. A retried/duplicate request (double-submit, network retry,
                // duplicate tab) collides on the unique key and is routed back to the
                // original order instead of creating — and paying for — a second one.
                IdempotencyKey? idempotencyRow = null;
                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    idempotencyRow = new IdempotencyKey { RequestKey = idempotencyKey, CreatedAt = now };
                    _context.IdempotencyKeys!.Add(idempotencyRow);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                    {
                        await transaction.RollbackAsync();

                        var existing = await _context.IdempotencyKeys!
                            .AsNoTracking()
                            .FirstOrDefaultAsync(k => k.RequestKey == idempotencyKey);

                        if (existing?.OrderId != null)
                        {
                            var existingOrder = await GetByIdAsync(existing.OrderId.Value);
                            if (existingOrder != null)
                            {
                                return OrderCreationResult.Replay(existingOrder);
                            }
                        }

                        // Key exists but no OrderId yet — the original request that owns
                        // this key is still mid-flight (very rare: near-simultaneous
                        // double-submit). Ask the client to retry shortly rather than
                        // racing it.
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_DUPLICATE_REQUEST_IN_PROGRESS);
                    }
                }

                var serverItems = new List<OrderItem>();
                decimal subtotal = 0m;

                foreach (var oi in orderModel.OrderItems)
                {
                    if (oi.ProductId == null || oi.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_ITEM_INVALID);
                    }

                    var productId = oi.ProductId.Value;
                    var product = await _context.Products!
                        .Include(p => p.ProductVariant)
                        .FirstOrDefaultAsync(p => p.Id == productId);

                    if (product == null || product.ProductVariant == null)
                    {
                        await transaction.RollbackAsync();
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_PRODUCT_UNAVAILABLE, $"Sản phẩm '{oi.ProductName}' không còn tồn tại.");
                    }

                    var variant = product.ProductVariant;

                    // Is there an active flash sale for this product right now?
                    var flashItem = await _context.FlashSaleItems!
                        .Include(f => f.FlashSale)
                        .FirstOrDefaultAsync(f => f.ProductId == productId
                            && f.FlashSale != null
                            && f.FlashSale.IsActive
                            && f.FlashSale.StartTime <= now
                            && f.FlashSale.EndTime >= now);

                    decimal unitPrice;
                    if (flashItem != null)
                    {
                        unitPrice = flashItem.FlashSalePrice;

                        // Atomic conditional decrement of flash sale allocation
                        var flashRows = await _context.FlashSaleItems!
                            .Where(f => f.Id == flashItem.Id && f.Stock >= oi.Quantity)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(f => f.Stock, f => f.Stock - oi.Quantity)
                                .SetProperty(f => f.SoldCount, f => f.SoldCount + oi.Quantity));

                        if (flashRows == 0)
                        {
                            await transaction.RollbackAsync();
                            return OrderCreationResult.Fail(ErrorCodes.ORDER_OUT_OF_STOCK, $"Sản phẩm '{product.Name}' trong chương trình Flash Sale đã hết hàng.");
                        }
                    }
                    else
                    {
                        unitPrice = variant.DiscountPrice > 0 ? variant.DiscountPrice : variant.Price;
                    }

                    // Atomic conditional decrement of real inventory (applies whether or not
                    // this was a flash-sale purchase — flash sale units still come from stock).
                    var stockRows = await _context.ProductVariants!
                        .Where(v => v.Id == variant.Id && v.Stock >= oi.Quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock - oi.Quantity));

                    if (stockRows == 0)
                    {
                        await transaction.RollbackAsync();
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_OUT_OF_STOCK, $"Sản phẩm '{product.Name}' đã hết hàng hoặc không đủ số lượng.");
                    }

                    subtotal += unitPrice * oi.Quantity;

                    serverItems.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ProductName = string.IsNullOrWhiteSpace(oi.ProductName) ? product.Name : oi.ProductName,
                        ProductImage = oi.ProductImage,
                        Quantity = oi.Quantity,
                        Price = unitPrice,
                        SelectedOptions = oi.SelectedOptions
                    });
                }

                // Recompute discount from the DiscountCode record's own rules against the
                // server-computed subtotal — never trust a client-sent DiscountAmount.
                decimal discountAmount = 0m;
                string? appliedDiscountCode = null;

                if (!string.IsNullOrWhiteSpace(orderModel.DiscountCode))
                {
                    var validateUserId = orderModel.UserId ?? string.Empty;
                    var discount = await _discountCodeRepository.ValidateDiscountCodeAsync(orderModel.DiscountCode, validateUserId, subtotal);
                    if (discount == null)
                    {
                        await transaction.RollbackAsync();
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_DISCOUNT_INVALID);
                    }

                    if (discount.DiscountType?.Trim().Equals("Percentage", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        discountAmount = subtotal * discount.DiscountPercentage / 100m;
                        if (discount.MaxDiscountAmount > 0 && discountAmount > discount.MaxDiscountAmount)
                        {
                            discountAmount = discount.MaxDiscountAmount;
                        }
                    }
                    else
                    {
                        discountAmount = discount.DiscountAmount;
                        if (discountAmount > subtotal) discountAmount = subtotal;
                    }

                    // Atomic conditional decrement — protects against concurrent over-redemption.
                    var marked = await _discountCodeRepository.MarkDiscountCodeAsUsedAsync(orderModel.DiscountCode);
                    if (!marked)
                    {
                        await transaction.RollbackAsync();
                        return OrderCreationResult.Fail(ErrorCodes.ORDER_DISCOUNT_EXHAUSTED);
                    }

                    appliedDiscountCode = orderModel.DiscountCode;
                }

                // NOTE: there is currently no server-side delivery-method/fee rate table in
                // this codebase (Delivery/DeliveryModel only tracks DeliveryDate/DeliveryStatus,
                // not a fee). Until one exists we can only sanity-check the client-supplied
                // shipping fee (reject negative values) rather than fully recompute it.
                var shippingFee = orderModel.ShippingFee < 0 ? 0 : orderModel.ShippingFee;

                var serverTotal = subtotal + shippingFee - discountAmount;
                if (serverTotal < 0) serverTotal = 0;

                // If the client-displayed total disagrees with the server-computed total by
                // more than a small rounding tolerance, reject rather than silently override —
                // this happens when a product's price changed between add-to-cart and checkout.
                if (Math.Abs(orderModel.TotalAmount - serverTotal) > PriceMismatchTolerance)
                {
                    await transaction.RollbackAsync();
                    return OrderCreationResult.Fail(ErrorCodes.ORDER_PRICE_MISMATCH);
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    OrderCode = GenerateOrderCode(now),
                    UserId = orderModel.UserId,
                    GuestToken = orderModel.GuestToken,
                    OrderDate = now,
                    Status = "Pending",
                    PaymentStatus = orderModel.PaymentMethod == "cod" ? "COD" : "Unpaid",
                    FullName = orderModel.FullName,
                    Phone = orderModel.Phone,
                    Email = orderModel.Email,
                    Address = orderModel.Address,
                    City = orderModel.City,
                    District = orderModel.District,
                    Ward = orderModel.Ward,
                    Note = orderModel.Note,
                    PaymentMethod = orderModel.PaymentMethod,
                    TotalAmount = serverTotal,
                    ShippingFee = shippingFee,
                    DiscountCode = appliedDiscountCode,
                    DiscountAmount = discountAmount,
                    OrderItems = serverItems
                };

                if (idempotencyRow != null)
                {
                    idempotencyRow.OrderId = order.Id;
                }

                _context.Orders!.Add(order);

                // OrderCode is unique in the DB; on the (very unlikely) chance two orders
                // generate the same code in the same second, regenerate and retry the
                // insert instead of failing the whole checkout.
                const int maxOrderCodeAttempts = 5;
                for (var attempt = 1; ; attempt++)
                {
                    try
                    {
                        await _context.SaveChangesAsync();
                        break;
                    }
                    catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex) && attempt < maxOrderCodeAttempts)
                    {
                        order.OrderCode = GenerateOrderCode(DateTimeHelper.VietnamNow);
                    }
                }

                await transaction.CommitAsync();

                orderModel.Id = order.Id;
                orderModel.OrderCode = order.OrderCode;
                orderModel.OrderDate = order.OrderDate;
                orderModel.Status = order.Status;
                orderModel.PaymentStatus = order.PaymentStatus;
                orderModel.TotalAmount = order.TotalAmount;
                orderModel.ShippingFee = order.ShippingFee;
                orderModel.DiscountCode = order.DiscountCode;
                orderModel.DiscountAmount = order.DiscountAmount;
                orderModel.OrderItems = serverItems.Select(oi => new OrderItemModel
                {
                    Id = oi.Id,
                    OrderId = order.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedOptions = oi.SelectedOptions
                }).ToList();

                return OrderCreationResult.Ok(orderModel);
            });
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTimeHelper.VietnamNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePaymentStatusAsync(Guid id, string paymentStatus)
        {
            var order = await _context.Orders!.FindAsync(id);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTimeHelper.VietnamNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetCheckoutUrlAsync(Guid id, string checkoutUrl)
        {
            await _context.Orders!
                .Where(o => o.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.CheckoutUrl, checkoutUrl));
        }

        private static long GenerateOrderCode(DateTime now) =>
            long.Parse(now.ToString("yyMMddHHmmss") + Random.Shared.Next(10, 99).ToString());

        private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);

        public async Task DeleteAsync(Guid id)
        {
            var order = await _context.Orders!
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order != null)
            {
                if (order.OrderItems != null)
                    _context.OrderItems!.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        private OrderModel MapOrderToModel(Order order)
        {
            return new OrderModel
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                UserId = order.UserId,
                GuestToken = order.GuestToken,
                OrderDate = order.OrderDate,
                Status = order.Status,
                CheckoutUrl = order.CheckoutUrl,
                FullName = order.FullName,
                Phone = order.Phone,
                Email = order.Email,
                Address = order.Address,
                City = order.City,
                District = order.District,
                Ward = order.Ward,
                Note = order.Note,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                TotalAmount = order.TotalAmount,
                ShippingFee = order.ShippingFee,
                DiscountCode = order.DiscountCode,
                DiscountAmount = order.DiscountAmount,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemModel
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName ?? oi.Product?.Name ?? "",
                    ProductImage = oi.ProductImage ?? oi.Product?.Images?.FirstOrDefault()?.ImagePath,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    SelectedOptions = oi.SelectedOptions
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderModel>> GetByGuestTokenAsync(string guestToken)
        {
            var orders = await _context.Orders!
                .AsNoTracking()
                .Where(o => o.GuestToken == guestToken)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(o => MapOrderToModel(o)).ToList();
        }

        public async Task<int> ClaimGuestOrdersAsync(string guestToken, string userId)
        {
            var guestOrders = await _context.Orders!
                .Where(o => o.GuestToken == guestToken && o.UserId == null)
                .ToListAsync();

            foreach (var order in guestOrders)
            {
                order.UserId = userId;
                order.GuestToken = null;
                order.UpdatedAt = DateTimeHelper.VietnamNow;
            }

            await _context.SaveChangesAsync();
            return guestOrders.Count;
        }
    }
}
