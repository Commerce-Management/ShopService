using Grpc.Core;
using Microsoft.Extensions.Logging;
using ShopService.Infrastructure.Interfaces.Entities;
using ShopService.Shared.Protos.GrpcShopService;

namespace ShopService.Infrastructure.gRPC;

public class GrpcShopService : ShopService.Shared.Protos.GrpcShopService.ShopService.ShopServiceBase
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
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ShopId invalid"));

        try
        {
            var shop = await _shopRepository.GetShopByIdAsync(shopId);
            if (shop == null)
            {
                _logger.LogWarning("Shop {ShopId} not found", shopId);
                throw new RpcException(new Status(StatusCode.NotFound, "Shop not found"));
            }

            if (!shop.IsActive)
            {
                _logger.LogWarning("Shop {ShopId} is not active", shopId);
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Shop is not active"));
            }
            
            return new GetShopByIdResponse
            {
                Id = shop.Id.ToString(),
                OwnerId = shop.OwnerUserId.ToString(),
                Name = shop.Name,
                IsActive = shop.IsActive,
            };
        }
        catch (RpcException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shop {ShopId}", shopId);
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while processing your request"));
        }
    }
    public override async Task<ValidateShopOwnershipResponse> ValidateShopOwnership(
        ValidateShopOwnershipRequest request,
        ServerCallContext context)
    {
        
        if (request == null || string.IsNullOrWhiteSpace(request.ShopId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ShopId is required"));

        if (!Guid.TryParse(request.ShopId, out var shopId) || 
            !Guid.TryParse(request.UserId, out var userId))
        {
            return new ValidateShopOwnershipResponse
            {
                IsOwner = false,
                ShopExists = false,
                ShopIsActive = false
            };
        }

        try
        {
            var shop = await _shopRepository.GetShopByIdAsync(shopId);

            return new ValidateShopOwnershipResponse
            {
                ShopExists = shop != null,
                IsOwner = shop?.OwnerUserId == userId,
                ShopIsActive = shop?.IsActive ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating shop ownership");
            throw new RpcException(new Status(StatusCode.Internal, "Validation error"));
        }
    }

}