using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Nutris.BlazorApp;
using Nutris.BlazorApp.Components.Layout;
using NutrisBlazor;
using NutrisBlazor.Services;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ==========================================
// 1. CONFIGURACIÓN
// ==========================================
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

using var response = await httpClient.GetAsync("appsettings.json");
if (response.IsSuccessStatusCode)
{
    using var stream = await response.Content.ReadAsStreamAsync();
    builder.Configuration.AddJsonStream(stream);
}

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ??
    "http://nutris.bca-365.com:7048/BC/api/beta/companies(26671de8-ca21-4927-a4e7-4bd8b962a35d)/";

// ==========================================
// 2. HTTP CLIENTS
// ==========================================

// HttpClient por defecto para localización y otros usos
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// HttpClient para la API con autenticación
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
    client.DefaultRequestHeaders.Add("Isolation", "snapshot");
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// ==========================================
// 3. SERVICIOS BASE (sin dependencias complejas)
// ==========================================

// LocalStorage (no depende de nadie)
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// LocalizationService (depende de HttpClient y LocalStorage)
builder.Services.AddScoped<ILocalizationService>(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    return new LocalizationService(httpClient, localStorage);
});

// SimpleApiService (depende de HttpClient)
builder.Services.AddScoped<SimpleApiService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var apiClient = factory.CreateClient("NutrisAPI");
    var config = sp.GetRequiredService<IConfiguration>();
    return new SimpleApiService(apiClient, config);
});

// IApiService apunta a SimpleApiService
builder.Services.AddScoped<IApiService>(sp => sp.GetRequiredService<SimpleApiService>());

// ==========================================
// 4. SERVICIOS DE AUTENTICACIÓN Y CATÁLOGOS
// ==========================================

// AuthService (NO depende de ICustomizeApi)
builder.Services.AddScoped<IAuthService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var apiClient = factory.CreateClient("NutrisAPI");
    var jsRuntime = sp.GetRequiredService<IJSRuntime>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var localStorage = sp.GetRequiredService<ILocalStorageService>();

    return new AuthService(apiClient, jsRuntime, configuration, localStorage);
});

// CatalogLoaderService (depende de SimpleApiService, LocalStorage y AuthService)
builder.Services.AddScoped<ICatalogLoaderService, CatalogLoaderService>();

// CatalogService (solo depende de LocalStorage)
builder.Services.AddScoped<ICatalogService, CatalogService>();

// ==========================================
// 5. SERVICIOS DE NEGOCIO
// ==========================================

// CustomizeApi (depende de SimpleApiService y AuthService)
builder.Services.AddScoped<ICustomizeApi, CustomizeApi>();

// BoteCapService
builder.Services.AddScoped<IBoteCapService, BoteCapApiService>();

// ==========================================
// 6. OTROS SERVICIOS
// ==========================================

builder.Services.AddAuthorizationCore();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ==========================================
// 7. INICIALIZACIÓN
// ==========================================

var app = builder.Build();

try
{
    Console.WriteLine("Initializing services...");

    // Inicializar localización primero
    var localizationService = app.Services.GetRequiredService<ILocalizationService>();
    await localizationService.InitializeAsync();
    Console.WriteLine("✓ Localization service initialized");

    // Inicializar autenticación
    var authService = app.Services.GetRequiredService<IAuthService>();
    await authService.InitializeAsync();
    Console.WriteLine("✓ Auth service initialized");

    Console.WriteLine("All services initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error initializing services: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

await app.RunAsync();