using Minimal_Api;
using Minimal_Api.Models;
using Minimal_Api.Repositories;
using Minimal_Api.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJwtConfiguration();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("manager"));
    options.AddPolicy("Employee", policy => policy.RequireRole("employee"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (User model) =>
{
    var user = UserRepository.Get(model.Username, model.Password);

    if (user == null)
    {
        return Results.NotFound(new { message = "Invalid Username or password" });
    }

    var token = TokenService.GenerateToken(user);

    user.Password = "";

    return Results.Ok(new
    {
        user,
        token
    });
});

app.MapGet("/anonymous", () =>
{
    Results.Ok(new {message = "anonymous route" });
});

app.MapGet("/authenticated", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as {user.Identity.Name}" });
}).RequireAuthorization();

app.MapGet("/employee", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as {user.Identity.Name}" });
}).RequireAuthorization("Employee");

app.MapGet("/manager", (ClaimsPrincipal user) =>
{
    Results.Ok(new { message = $"Authenticated as {user.Identity.Name}" });
}).RequireAuthorization("Admin");

app.Run();
