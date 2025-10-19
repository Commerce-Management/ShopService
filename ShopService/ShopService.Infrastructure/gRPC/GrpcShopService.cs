using ShopService.Core.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ShopService.Shared.Protos;

namespace ShopService.Infrastructure.gRPC;

public class GrpcShopService : ShopService.Shared.Protos.ShopService.ShopServiceBase
{
    private readonly IShopService _shopService;
    private readonly ILogger<GrpcShopService> _logger;

    public GrpcShopService(IShopService shopService, ILogger<GrpcShopService> logger)
    {
        _shopService = shopService;
        _logger = logger;
    }

    public override async Task<GetShopByOwnerResponse> GetShopByOwner(GetShopByOwnerRequest request, ServerCallContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.OwnerUserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ownerUserId is required"));

        if (!Guid.TryParse(request.OwnerUserId, out var ownerId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ownerUserId invalid"));

        try
        {
            var shopDto = await _shopService.GetShopByOwnerIdAsync(ownerId);
            if (shopDto == null)
            {
                // Return not found via gRPC status OR empty response. We'll use NOT_FOUND.
                throw new RpcException(new Status(StatusCode.NotFound, "Shop not found for owner"));
            }

            return new GetShopByOwnerResponse
            {
                ShopId = shopDto.Id.ToString(),
                IsActive = shopDto.IsActive,
                Message = "OK"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetShopByOwner failed for owner {Owner}", request.OwnerUserId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}