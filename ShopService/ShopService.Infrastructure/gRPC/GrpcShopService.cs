using ShopService.Core.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ShopService.Infrastructure.Interfaces.Entities;
using ShopService.Shared.Protos;

namespace ShopService.Infrastructure.gRPC;

public class GrpcShopService : ShopService.Shared.Protos.ShopService.ShopServiceBase
{
    private readonly IShopRepository _shopRepository;
    
    private readonly ILogger<GrpcShopService> _logger;

    public GrpcShopService(IShopRepository shopRepository,ILogger<GrpcShopService> logger)
    {
        _shopRepository = shopRepository;
        _logger = logger;
    }

    public override async Task<GetShopByIdResponse> GetShopById(GetShopByIdRequest request, ServerCallContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ShopId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ShopId is required"));

        if (!Guid.TryParse(request.ShopId, out var shopId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ownerUserId invalid"));


        try
        {
            var shop = await _shopRepository.GetShopByIdAsync(shopId); 
            if (shop == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Shop not found for owner"));
            }

            return new GetShopByIdResponse()
            {
                Id = shop.Id.ToString()
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetShopById failed for id {Id}", request.ShopId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}