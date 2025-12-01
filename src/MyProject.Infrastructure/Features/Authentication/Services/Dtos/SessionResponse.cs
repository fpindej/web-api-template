namespace MyProject.Infrastructure.Features.Authentication.Services.Dtos;

public record SessionResponse
{
    public required string UserId { get; set; }

    public required string Username { get; set; }

    public required string Email { get; set; }

    public required List<string> Roles { get; set; }
}
