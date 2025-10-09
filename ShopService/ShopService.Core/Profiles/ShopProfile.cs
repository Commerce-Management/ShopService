using AutoMapper;
using ShopService.Core.Entities;
using ShopService.Shared.Dtos;

namespace ShopService.Core.Profiles;

public class ShopProfile : Profile
{
    public ShopProfile()
    {
        CreateMap<CreateShopDto, Shop>();
        CreateMap<UpdateShopDto, Shop>();
        CreateMap<Shop, GetShopDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.OwnerUserId, opt => opt.MapFrom(src => src.OwnerUserId.ToString()));
    }
}