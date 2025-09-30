using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Nutris.BlazorApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NutrisBlazor.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password, bool rememberMe = false);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<User> GetCurrentUserAsync();
        Task InitializeAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly IConfiguration _configuration;
        private readonly ILocalStorageService _localStorage;
        private User _currentUser;

        public AuthService(
            HttpClient httpClient,
            IJSRuntime jsRuntime,
            IConfiguration configuration,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _configuration = configuration;
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                if (!string.IsNullOrEmpty(token))
                {
                    var userData = await _localStorage.GetItemAsync<User>("userT");
                    if (userData != null)
                    {
                        _currentUser = userData;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth: {ex.Message}");
            }
        }

        public async Task<bool> LoginAsync(string username, string password, bool rememberMe = false)
        {
            try
            {
                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var loginEndpoint = _configuration["ApiSettings:Endpoints:Login"] ?? "RG35Login";
                var tenant = _configuration["ApiSettings:Tenant"] ?? "nutris";
                var apiUsername = _configuration["ApiSettings:Username"] ?? "API";
                var apiPassword = _configuration["ApiSettings:Password"] ?? "Sb3cBC2n8r7F+Puk6aokQ8m5vJ0OUIgRO6QzXLkkGXs=";

                string filter = BuildLoginFilter(username, password);
                var url = $"{baseUrl}{loginEndpoint}?tenant={tenant}&$filter={Uri.EscapeDataString(filter)}";

                Console.WriteLine($"Login URL: {url}");

                using var client = new HttpClient();
                var authString = $"{apiUsername}:{apiPassword}";
                var authBytes = Encoding.UTF8.GetBytes(authString);
                var authHeader = Convert.ToBase64String(authBytes);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");

                HttpResponseMessage response;

                try
                {
                    response = await client.GetAsync(url);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("CORS") || ex.Message.Contains("fetch"))
                {
                    Console.WriteLine("CORS error detected. Trying alternative approach...");

                    if (IsDevelopment())
                    {
                        return await SimulateLogin(username, rememberMe);
                    }
                    throw;
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login response: {content}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = JsonSerializer.Deserialize<ApiResponse<User>>(content, options);

                    if (result?.Value?.Count > 0)
                    {
                        var user = result.Value[0];
                        _currentUser = user;

                        await SaveUserToStorageAsync(user, rememberMe);

                        // NOTA: Los catálogos se cargarán en segundo plano desde Login.razor
                        // No los cargamos aquí para no bloquear el login

                        try
                        {
                            await LoadUserAdditionalDataAsync(user.Customer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not load additional data: {ex.Message}");
                        }

                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"Login failed with status: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (IsDevelopment() && (username == "demo" || username == "test@test.com"))
                {
                    return await SimulateLogin(username, rememberMe);
                }
            }

            return false;
        }

        private bool IsDevelopment()
        {
            var isDev = _configuration["Environment"] ?? "Development";
            return isDev == "Development";
        }

        private async Task<bool> SimulateLogin(string username, bool rememberMe)
        {
            Console.WriteLine("Using simulated login for development...");

            var simulatedUser = new User
            {
                SystemId = Guid.NewGuid().ToString(),
                Customer = "DEMO001",
                Name = "Demo User",
                user = username,
                Email = username.Contains("@") ? username : $"{username}@demo.com",
            };

            _currentUser = simulatedUser;
            await SaveUserToStorageAsync(simulatedUser, rememberMe);

            return true;
        }

        private string BuildLoginFilter(string username, string password)
        {
            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            if (Regex.IsMatch(username, emailRegex))
            {
                return $"email eq '{username}' and pass eq '{password}'";
            }
            else
            {
                return $"user eq '{username}' and pass eq '{password}'";
            }
        }

        private async Task SaveUserToStorageAsync(User user, bool rememberMe)
        {
            try
            {
                var storageOptions = rememberMe ? "permanent" : "session";

                await _localStorage.SetItemAsync("token", "authenticated");
                await _localStorage.SetItemAsync("userT", user);
                await _localStorage.SetItemAsync("name", user.Name);
                await _localStorage.SetItemAsync("No_", user.Customer);
                await _localStorage.SetItemAsync("user", user.user);
                await _localStorage.SetItemAsync("email", user.Email);
                await _localStorage.SetItemAsync("logo", user.Logo);
                await _localStorage.SetItemAsync("nombrevendedor", user.Nombre_Vendedor);
                await _localStorage.SetItemAsync("salessupport", user.Sales_Support);
                await _localStorage.SetItemAsync("address", user.Address);
                await _localStorage.SetItemAsync("address2", user.Address_2);
                await _localStorage.SetItemAsync("city", user.City);
                await _localStorage.SetItemAsync("county", user.County);
                await _localStorage.SetItemAsync("country", user.Country);
                await _localStorage.SetItemAsync("postcode", user.Post_Code);
                await _localStorage.SetItemAsync("phone", user.Phone);
                await _localStorage.SetItemAsync("mobilephone", user.Mobile_Phone);
                await _localStorage.SetItemAsync("contact", user.Contact);
                await _localStorage.SetItemAsync("shipmentmethodcode", user.Shipment_Method_Code);
                await _localStorage.SetItemAsync("family", user.Family);
                await _localStorage.SetItemAsync("standard", user.Standard);
                await _localStorage.SetItemAsync("premium", user.Premium);
                await _localStorage.SetItemAsync("storageType", storageOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to storage: {ex.Message}");
            }
        }

        private async Task LoadUserAdditionalDataAsync(string customerNo)
        {
            try
            {
                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var userDataEndpoint = _configuration["ApiSettings:Endpoints:UserData"] ?? "RG37";
                var filter = $"Customer_No eq '{customerNo}'";
                var url = $"{baseUrl}{userDataEndpoint}?$expand=formulacionRG37,RG35&$filter={Uri.EscapeDataString(filter)}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await _localStorage.SetItemAsync("data", content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading additional data: {ex.Message}");
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync("token");
                await _localStorage.RemoveItemAsync("userT");
                await _localStorage.RemoveItemAsync("name");
                await _localStorage.RemoveItemAsync("No_");
                await _localStorage.RemoveItemAsync("user");
                await _localStorage.RemoveItemAsync("email");
                await _localStorage.RemoveItemAsync("logo");
                await _localStorage.RemoveItemAsync("nombrevendedor");
                await _localStorage.RemoveItemAsync("salessupport");
                await _localStorage.RemoveItemAsync("address");
                await _localStorage.RemoveItemAsync("address2");
                await _localStorage.RemoveItemAsync("city");
                await _localStorage.RemoveItemAsync("county");
                await _localStorage.RemoveItemAsync("country");
                await _localStorage.RemoveItemAsync("postcode");
                await _localStorage.RemoveItemAsync("phone");
                await _localStorage.RemoveItemAsync("mobilephone");
                await _localStorage.RemoveItemAsync("contact");
                await _localStorage.RemoveItemAsync("shipmentmethodcode");
                await _localStorage.RemoveItemAsync("family");
                await _localStorage.RemoveItemAsync("standard");
                await _localStorage.RemoveItemAsync("premium");
                await _localStorage.RemoveItemAsync("storageType");

                // Limpiar catálogos también
                await _localStorage.RemoveItemAsync("catalogsTimestamp");
                await _localStorage.RemoveItemAsync("catalog_Atributos");
                await _localStorage.RemoveItemAsync("catalog_RelacionBote");
                await _localStorage.RemoveItemAsync("catalog_RelacionTapa");
                await _localStorage.RemoveItemAsync("catalog_TiposCajas");

                _currentUser = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("token");
                return !string.IsNullOrEmpty(token) && token == "authenticated";
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            if (_currentUser == null)
            {
                _currentUser = await _localStorage.GetItemAsync<User>("userT");
            }
            return _currentUser;
        }
    }

    public class ApiResponse<T>
    {
        public List<T> Value { get; set; } = new List<T>();
    }

    public class User
    {
        public string SystemId { get; set; }
        public string Customer { get; set; }
        public string Name { get; set; }
        public string user { get; set; }
        public string Pass { get; set; }
        public string Logo { get; set; }
        public string CIF { get; set; }
        public string Nombre_Vendedor { get; set; }
        public string Sales_Support { get; set; }
        public string Address { get; set; }
        public string Address_2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string Post_Code { get; set; }
        public string Phone { get; set; }
        public string Mobile_Phone { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Shipment_Method_Code { get; set; }
        public bool Family { get; set; }
        public bool Standard { get; set; }
        public bool Premium { get; set; }
    }
}