namespace MyProject.Infrastructure.Features.Authentication.Services.Dtos;

public record LoginResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}
