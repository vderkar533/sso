# ASP.NET Web Client (Example)

This example shows a minimal MVC-style client that redirects to a login page and then exchanges credentials for tokens.
For internal applications, this sample uses a direct login call to the SSO server.

```csharp
// Program.cs (minimal sample)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSession();

var app = builder.Build();
app.UseSession();

app.MapGet("/login", () =>
{
    // Display your login form here.
    return Results.Text("Render login page.");
});

app.MapPost("/login", async (HttpContext http, IHttpClientFactory factory) =>
{
    var form = await http.Request.ReadFormAsync();
    var payload = new
    {
        email = form["email"].ToString(),
        password = form["password"].ToString(),
        clientId = "internal-portal"
    };

    var client = factory.CreateClient();
    var response = await client.PostAsJsonAsync("https://sso.yourcompany.com/api/auth/login", payload);
    response.EnsureSuccessStatusCode();

    var token = await response.Content.ReadFromJsonAsync<AuthResponse>();
    http.Session.SetString("access_token", token!.AccessToken);
    http.Session.SetString("refresh_token", token.RefreshToken);

    return Results.Redirect("/profile");
});

app.MapGet("/profile", async (HttpContext http, IHttpClientFactory factory) =>
{
    var accessToken = http.Session.GetString("access_token");
    var client = factory.CreateClient();
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

    var response = await client.GetAsync("https://sso.yourcompany.com/api/user/profile");
    response.EnsureSuccessStatusCode();

    var profileJson = await response.Content.ReadAsStringAsync();
    return Results.Text(profileJson, "application/json");
});

app.Run();

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
```
