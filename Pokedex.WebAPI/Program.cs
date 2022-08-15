using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Pokedex.WebAPI.Logic;
using Pokedex.WebAPI.Middlewares;
using Sharplus.System;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
Environment.CurrentDirectory = AppContext.BaseDirectory;
EnvironmentUtils.LoadVariables("Environment.env");

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<Classifier>();
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("Basic", null);
/*builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthenticationHandler>("BasicAuthentication", null);*/
builder.Services.AddAuthorization();
builder.Services.AddControllers();

WebApplication app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Pokedex Web API :3");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    Console.ReadLine();
}
