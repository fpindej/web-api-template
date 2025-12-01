namespace MyProject.Infrastructure.Features.Authentication.Services.Dtos;

public record RegisterRequest
{
    public required string Username { get; set; }

    public required string Email { get; set; }

    public required string Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
