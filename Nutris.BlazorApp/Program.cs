using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Nutris.BlazorApp;
using Nutris.BlazorApp.Components.Layout;
using Nutris.BlazorApp.Features.Customize;
using NutrisBlazor;
using NutrisBlazor.Services;
using System.Net.Http;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient para cargar appsettings.json
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Cargar configuración desde appsettings.json
using var response = await httpClient.GetAsync("appsettings.json");
if (response.IsSuccessStatusCode)
{
    using var stream = await response.Content.ReadAsStreamAsync();
    builder.Configuration.AddJsonStream(stream);
}

// Obtener la configuración de la API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ??
    "http://nutris.bca-365.com:7048/BC/api/beta/companies(26671de8-ca21-4927-a4e7-4bd8b962a35d)/";

// Registrar HttpClient para la API con nombre
builder.Services.AddHttpClient("NutrisAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

    // Agregar headers básicos
    var username = builder.Configuration["ApiSettings:Username"] ?? "API";
    var password = builder.Configuration["ApiSettings:Password"] ?? "Sb3cBC2n8r7F+Puk6aokQ8m5vJ0OUIgRO6QzXLkkGXs=";
    var authString = $"{username}:{password}";
    var authBytes = System.Text.Encoding.UTF8.GetBytes(authString);
    var authHeader = Convert.ToBase64String(authBytes);

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);
});

// Registrar HttpClient por defecto para otros usos (incluyendo localización)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// IMPORTANTE: Registrar LocalStorageService primero
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Registrar LocalizationService con las dependencias correctas
builder.Services.AddScoped<ILocalizationService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new LocalizationService(httpClient, localStorage);
});
builder.Services.AddScoped<ICustomizeApi, CustomizeApi>();
// CORREGIDO: Registrar SimpleApiService con las dependencias correctas
// Registrar HttpClient para la API con nombre
builder.Services.AddHttpClient("NutrisAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);

    var username = builder.Configuration["ApiSettings:Username"] ?? "API";
    var password = builder.Configuration["ApiSettings:Password"] ?? "Sb3cBC2n8r7F+Puk6aokQ8m5vJ0OUIgRO6QzXLkkGXs=";
    var authString = $"{username}:{password}";
    var authBytes = System.Text.Encoding.UTF8.GetBytes(authString);
    var authHeader = Convert.ToBase64String(authBytes);

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

    // Recomendado para BC
    client.DefaultRequestHeaders.Add("Isolation", "snapshot");
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});
// SimpleApiService CONCRETO (lo que necesita CustomizeApi)
builder.Services.AddScoped<SimpleApiService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var apiClient = factory.CreateClient("NutrisAPI");
    var config = sp.GetRequiredService<IConfiguration>();
    return new SimpleApiService(apiClient, config);
});

// (Opcional) Si en alguna clase pides IApiService, mapea a la misma instancia
builder.Services.AddScoped<IApiService>(sp =>
    sp.GetRequiredService<SimpleApiService>());

builder.Services.AddScoped<ICustomizeApi, CustomizeApi>();

// Registrar AuthService con factory para usar el HttpClient correcto
builder.Services.AddScoped<IAuthService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var apiClient = httpClientFactory.CreateClient("NutrisAPI");
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();

    return new AuthService(apiClient, jsRuntime, configuration, localStorage);
});
builder.Services.AddScoped<IBoteCapService, BoteCapApiService>();
// Agregar soporte para autorización
builder.Services.AddAuthorizationCore();

// Agregar localización (opcional para .NET localization)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configurar logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Inicializar servicios
try
{
    // IMPORTANTE: Inicializar localización primero
    var localizationService = app.Services.GetRequiredService<ILocalizationService>();
    await localizationService.InitializeAsync();
    Console.WriteLine("Localization service initialized successfully");

    // Luego inicializar autenticación
    var authService = app.Services.GetRequiredService<IAuthService>();
    await authService.InitializeAsync();
    Console.WriteLine("Auth service initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing services: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

await app.RunAsync();