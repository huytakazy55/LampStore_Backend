using AutoMapper;
using LampStoreProjects.Models;
using LampStoreProjects.Data;
using LampStoreProjects.DTOs;

public class ApplicationMapper : Profile
{
    public ApplicationMapper()
    {
        CreateMap<ProductModel, Product>().ReverseMap();
        CreateMap<Product, ProductCreateDto>()
            .ForMember(dest => dest.ProductVariants, opt => opt.MapFrom(src => src.ProductVariants))
            .ForMember(dest => dest.VariantTypes, opt => opt.MapFrom(src => src.VariantTypes))
            .ReverseMap();

        CreateMap<ProductVariantDto, ProductVariant>().ReverseMap();
        CreateMap<VariantTypeDto, VariantType>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.Select(v => new VariantValue { Value = v })))
            .ReverseMap();
        
        CreateMap<Product, ProductUpdateDto>().ReverseMap();
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
        CreateMap<TagModel, Tag>().ReverseMap();
        CreateMap<ProductTagModel, ProductTag>().ReverseMap();
    }
}