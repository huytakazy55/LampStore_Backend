using LampStoreProjects.Data;
using LampStoreProjects.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

public class Orderquery : IOrderQuery
{
    private readonly ApplicationDbContext _context;

    public Orderquery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
    {
        // Call stored procedure using FromSqlRaw
        var orders = await _context.Orders!
            .FromSqlRaw("EXEC GetOrdersByCustomerId @CustomerId", new SqlParameter("@CustomerId", customerId))
            .ToListAsync();

        return orders;
    }
}