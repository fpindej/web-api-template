using Microsoft.Extensions.Options;
using MyProject.WebApi.Features.Cors.Options;

namespace MyProject.WebApi.Features.Cors.Extensions;

internal static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCors(this IApplicationBuilder app)
    {
        var corsOptions = app.ApplicationServices.GetRequiredService<IOptions<CorsOptions>>().Value;

        app.UseCors(corsOptions.PolicyName);

        return app;
    }
}
