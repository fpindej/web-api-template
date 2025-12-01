namespace MyProject.Infrastructure.Features.Authentication.Services.Dtos;

public record RefreshResponse
{
    public required string AccessToken { get; set; }

    public required string RefreshToken { get; set; }
}
