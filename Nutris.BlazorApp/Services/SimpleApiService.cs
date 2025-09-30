using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NutrisBlazor.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string path);
        Task<HttpResponseMessage> PostAsync(string path, object data);
        Task<HttpResponseMessage> PostAsync2(string path, object data);
        Task<HttpResponseMessage> PatchAsync(string path, object data);
    }

    public class SimpleApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        public SimpleApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "";
            _username = _configuration["ApiSettings:Username"] ?? "";
            _password = _configuration["ApiSettings:Password"] ?? "";

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authToken);

            _httpClient.DefaultRequestHeaders.Add("Isolation", "snapshot");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<T> GetAsync<T>(string path)
        {
            try
            {
                var fullUrl = path.StartsWith("http") ? path : $"{_baseUrl}{path}";
                Console.WriteLine($"API GET: {fullUrl}");

                var sw = System.Diagnostics.Stopwatch.StartNew();

                var response = await _httpClient.GetAsync(fullUrl);
                Console.WriteLine($"⏱️ HTTP response received in {sw.ElapsedMilliseconds}ms");

                response.EnsureSuccessStatusCode();

                if (typeof(T) == typeof(JsonDocument))
                {
                    Console.WriteLine($"📝 Starting JsonDocument.ParseAsync from stream...");
                    using var stream = await response.Content.ReadAsStreamAsync();
                    Console.WriteLine($"📝 Stream obtained in {sw.ElapsedMilliseconds}ms");

                    var doc = await JsonDocument.ParseAsync(stream);
                    Console.WriteLine($"✅ JsonDocument parsed in {sw.ElapsedMilliseconds}ms total");
                    return (T)(object)doc;
                }
                else
                {
                    Console.WriteLine($"📝 Deserializing {typeof(T).Name}...");
                    var result = await response.Content.ReadFromJsonAsync<T>();
                    Console.WriteLine($"✅ {typeof(T).Name} deserialized in {sw.ElapsedMilliseconds}ms total");
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GET request to {path}: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object data)
        {
            try
            {
                var fullUrl = path.StartsWith("http") ? path : $"{_baseUrl}{path}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"API POST: {fullUrl}");
                return await _httpClient.PostAsync(fullUrl, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en POST request: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync2(string path, object data)
        {
            try
            {
                var fullUrl = path.StartsWith("http") ? path : $"{_baseUrl}{path}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
                {
                    Content = content
                };
                Console.WriteLine($"API POST: {fullUrl}");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en POST request: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, object data)
        {
            try
            {
                var fullUrl = path.StartsWith("http") ? path : $"{_baseUrl}{path}";
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Patch, fullUrl)
                {
                    Content = content
                };

                request.Headers.Add("If-Match", "*");

                Console.WriteLine($"API PATCH: {fullUrl}");
                return await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en PATCH request: {ex.Message}");
                throw;
            }
        }
    }
}