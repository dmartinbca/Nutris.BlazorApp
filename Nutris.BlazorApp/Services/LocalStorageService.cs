using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace NutrisBlazor.Services
{
    public interface ILocalStorageService
    {
        Task<T> GetItemAsync<T>(string key);
        Task SetItemAsync<T>(string key, T value);
        Task RemoveItemAsync(string key);
        Task ClearAsync();
        Task<bool> ContainsKeyAsync(string key);
        Task<string[]> GetKeysAsync();
    }

    public class LocalStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsonSerializerOptions _jsonOptions;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<T> GetItemAsync<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

                if (string.IsNullOrEmpty(json))
                    return default(T);

                // Si T es string, devolver directamente
                if (typeof(T) == typeof(string))
                    return (T)(object)json;

                // Intentar deserializar como JSON
                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting item from localStorage: {ex.Message}");
                return default(T);
            }
        }

        public async Task SetItemAsync<T>(string key, T value)
        {
            try
            {
                string json;

                // Si es string, guardar directamente
                if (value is string stringValue)
                {
                    json = stringValue;
                }
                else
                {
                    json = JsonSerializer.Serialize(value, _jsonOptions);
                }

                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting item in localStorage: {ex.Message}");
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing item from localStorage: {ex.Message}");
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing localStorage: {ex.Message}");
            }
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking key in localStorage: {ex.Message}");
                return false;
            }
        }

        public async Task<string[]> GetKeysAsync()
        {
            try
            {
                var length = await _jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
                var keys = new string[length];

                for (int i = 0; i < length; i++)
                {
                    keys[i] = await _jsRuntime.InvokeAsync<string>("localStorage.key", i);
                }

                return keys;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting keys from localStorage: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}