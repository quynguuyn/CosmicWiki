using CosmicWiki.Newsletter.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CosmicWiki.Newsletter.Api.Controllers;

[ApiController]
[Route("api/newsletter")]
public sealed class NewsletterController(ISender sender) : ControllerBase
{
    [HttpPost("subscriptions")]
    [EnableRateLimiting("newsletter-subscribe")]
    public async Task<IActionResult> Subscribe(
        SubscribeNewsletterCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        // Do not reveal whether an address was already subscribed.
        return Accepted(new
        {
            message = "If the address can receive the newsletter, a confirmation email will be sent."
        });
    }

    [HttpGet("confirm/{token}")]
    public async Task<IActionResult> Confirm(
        string token,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ConfirmNewsletterSubscriptionCommand(token),
            cancellationToken);

        if (!result.Found)
        {
            return NotFound();
        }

        if (result.Expired)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Confirmation token expired.",
                Detail = "Request a new newsletter subscription confirmation."
            });
        }

        return Ok(new { message = "Newsletter subscription confirmed." });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe(
        UnsubscribeNewsletterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.Found ? NoContent() : NotFound();
    }
}
