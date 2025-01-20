using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;

public class ApplicationMapper : Profile
{
    public ApplicationMapper()
    {
        CreateMap<ProductModel, Product>().ReverseMap();
        CreateMap<ProductImageModel, ProductImage>().ReverseMap();
        CreateMap<ProductVariantModel, ProductVariant>().ReverseMap();
        CreateMap<VariantTypeModel, VariantType>().ReverseMap();
        CreateMap<VariantValueModel, VariantValue>().ReverseMap();
        CreateMap<CategoryModel, Category>().ReverseMap();
        CreateMap<CartModel, Cart>().ReverseMap();
        CreateMap<CartItemModel, CartItem>().ReverseMap();
        CreateMap<CheckInModel, CheckIn>().ReverseMap();
        CreateMap<DeliveryModel, Delivery>().ReverseMap();
        CreateMap<OrderModel, Order>().ReverseMap();
        CreateMap<OrderItemModel, OrderItem>().ReverseMap();
        CreateMap<UserProfileModel, UserProfile>().ReverseMap();
    }
}