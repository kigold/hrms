using OpenIddict.Validation.AspNetCore;
using Shared.Auth;

namespace Auth.API.Apis
{
    public static class WeatherApi
    {
        private static string[] summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private const string PATH_BASE = "weatherforecast";
        private const string TAGS = "weatherforecast api";

        public static WebApplication MapWeatherRequestEndpoints(this WebApplication app)
        {
            var weatherMapGroup = app.MapGroup(PATH_BASE).WithTags(TAGS);
            weatherMapGroup.MapGet("/", GetWeather).RequireAuthorization(new AuthorizationOptions { AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme });

            return app;
        }

        //GET /weather
        public static async Task<WeatherForecast[]> GetWeather()
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
            return forecast;
        }


    }
    public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}