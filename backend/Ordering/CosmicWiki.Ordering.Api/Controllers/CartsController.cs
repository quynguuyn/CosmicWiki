using CosmicWiki.SharedKernel.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CosmicWiki.Ordering.Application.Abstractions;

namespace CosmicWiki.Ordering.Api.Controllers;

[ApiController]
[Route("api/carts")]
[Authorize(Policy = Permissions.OrderingWrite)]
public sealed class CartsController(ICartRepository cartRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(subject))
            return Forbid();

        var cartJson = await cartRepository.GetCartAsync(subject, cancellationToken);
        if (string.IsNullOrEmpty(cartJson))
            return Ok(new object[] { });

        return Content(cartJson, "application/json");
    }

    [HttpPost]
    public async Task<IActionResult> SaveCart([FromBody] JsonElement cart, CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(subject))
            return Forbid();

        await cartRepository.SaveCartAsync(subject, cart.GetRawText(), cancellationToken);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(subject))
            return Forbid();

        await cartRepository.DeleteCartAsync(subject, cancellationToken);
        return NoContent();
    }
}
