using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using ShopService.Core.Interfaces;
using ShopService.Shared.Dtos;
using Serilog;
using Asp.Versioning;

namespace ShopService.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ShopsController(IShopService shopService) : ControllerBase
{
    [HttpPost]
    [Authorize("ShopOwner")]
    public async Task<ActionResult<GetShopDto>> CreateShop([FromBody] CreateShopDto createShopDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value.Errors.Any())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            Log.Warning("ShopsController.CreateShop: Validation failed: {@Errors}", errors);
            return BadRequest(new
            {
                ErrorCode = "ValidationError",
                Message = "Некорректные входные данные.",
                Details = errors
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerUserId))
        {
            Log.Error("ShopsController.CreateShop: Invalid user claim. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));
            return Unauthorized(new
            {
                ErrorCode = "UserIdentificationFailed",
                Message = "Не удалось определить пользователя из токена."
            });
        }

        try
        {
            Log.Information("ShopsController.CreateShop: User {OwnerId} creating shop {@Dto}", ownerUserId,
                createShopDto);
            var shop = await shopService.CreateShopAsync(createShopDto, ownerUserId);
            return CreatedAtAction(nameof(GetShopById), new { id = shop.Id }, shop);
        }
        catch (InvalidOperationException ex)
        {
            // Здесь уже точно catch попадёт, если SaveChangesAsync вернул false или другая бизнес-ошибка.
            Log.Warning(ex, "ShopsController.CreateShop: Business rule violation for user {OwnerId}. DTO: {@Dto}",
                ownerUserId, createShopDto);

            // Возвращаем более подробный ответ, чтобы видеть причину.
            return BadRequest(new
            {
                ErrorCode = "BusinessRuleViolation",
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.CreateShop: Unexpected error for user {OwnerId}. DTO: {@Dto}", ownerUserId,
                createShopDto);

            return StatusCode(500, new
            {
                ErrorCode = "InternalServerError",
                Message = "Неожиданная ошибка на сервере при создании магазина.",
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace
            });
        }
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetShopDto>> GetShopById(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { Error = "Cannot determine user." });

        try
        {
            var shop = await shopService.GetShopByIdAsync(id);
            if (shop == null)
                return NotFound(new { Error = $"Shop with ID {id} not found." });

            if (User.IsInRole("ShopOwner") && shop.OwnerUserId != currentUserId.ToString())
                return Forbid();

            return Ok(shop);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.GetShopById: Error fetching shop {ShopId}", id);
            return StatusCode(500, new { Error = "Server error." });
        }
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateShop(Guid id, [FromForm] UpdateShopDto updateShopDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Invalid data." });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { Error = "Cannot determine user." });

        try
        {
            var existing = await shopService.GetShopByIdAsync(id);
            if (existing == null)
                return NotFound(new { Error = $"Shop with ID {id} not found." });

            if (User.IsInRole("ShopOwner") && existing.OwnerUserId != currentUserId.ToString())
                return Forbid();

            var success = await shopService.UpdateShopAsync(id, updateShopDto);
            if (!success)
                return NotFound(new { Error = $"Shop with ID {id} not found." });

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.UpdateShop: Error updating shop {ShopId}", id);

            return StatusCode(500, new
            {
                Error = "Server error.",
                ExceptionMessage = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException?.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteShop(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { Error = "Cannot determine user." });

        try
        {
            var existing = await shopService.GetShopByIdAsync(id);
            if (existing == null)
                return NotFound(new { Error = $"Shop with ID {id} not found." });

            if (User.IsInRole("ShopOwner") && existing.OwnerUserId != currentUserId.ToString())
                return Forbid();

            var success = await shopService.DeleteShopAsync(id);
            if (!success)
                return NotFound(new { Error = $"Shop with ID {id} not found." });

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.DeleteShop: Error deleting shop {ShopId}", id);
            return StatusCode(500, new { Error = "Server error." });
        }
    }

    [HttpGet("subdomain/{subdomain}")]
    public async Task<ActionResult<GetShopDto>> GetShopBySubdomain(string subdomain)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { Error = "Cannot determine user." });

        try
        {
            var shop = await shopService.GetShopBySubdomainAsync(subdomain);
            if (shop == null)
                return NotFound(new { Error = $"Shop with subdomain {subdomain} not found." });

            if (User.IsInRole("ShopOwner") && shop.OwnerUserId != currentUserId.ToString())
                return Forbid();

            return Ok(shop);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.GetShopBySubdomain: Error fetching shop {Subdomain}", subdomain);
            return StatusCode(500, new { Error = "Server error." });
        }
    }

    [HttpGet("owner/{ownerId:guid}")]
    public async Task<ActionResult<GetShopDto>> GetShopByOwnerId(Guid ownerId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            return Unauthorized(new { Error = "Cannot determine user." });

        try
        {
            var shop = await shopService.GetShopByOwnerIdAsync(ownerId);
            if (shop == null)
                return NotFound(new { Error = $"Shop for owner {ownerId} not found." });

            if (User.IsInRole("ShopOwner") && shop.OwnerUserId != currentUserId.ToString())
                return Forbid();

            return Ok(shop);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.GetShopByOwnerId: Error fetching shop for owner {OwnerId}", ownerId);
            return StatusCode(500, new { Error = "Server error." });
        }
    }

    [HttpGet]
    [Authorize("CustomerAndOwner")]
    public async Task<ActionResult<IEnumerable<GetShopDto>>> GetPaginatedShops(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(new { Error = "PageNumber and PageSize must be greater than 0." });

        try
        {
            var (shops, totalCount) = await shopService.GetPaginatedShopsAsync(pageNumber, pageSize);

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page-Number"] = pageNumber.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            if (!shops.Any())
                return NotFound(new { Error = "No shops found." });

            return Ok(shops);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "ShopsController.GetPaginatedShops: Error fetching shops");
            return StatusCode(500, new { Error = "Server error." });
        }
    }
}