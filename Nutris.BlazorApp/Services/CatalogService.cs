using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutrisBlazor.Services
{
    public interface ICatalogService
    {
        Task<JsonDocument?> GetCachedCatalogAsync(string catalogName);
        Task<bool> AreCatalogsValidAsync();
        Task InvalidateCatalogsAsync();
    }

    public class CatalogService : ICatalogService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

        public CatalogService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<JsonDocument?> GetCachedCatalogAsync(string catalogName)
        {
            try
            {
                var json = await _localStorage.GetItemAsync<string>($"catalog_{catalogName}");

                if (string.IsNullOrEmpty(json))
                {
                    Console.WriteLine($"Catalog {catalogName} not found in cache");
                    return null;
                }

                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cached catalog {catalogName}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AreCatalogsValidAsync()
        {
            try
            {
                var timestampStr = await _localStorage.GetItemAsync<string>("catalogsTimestamp");

                if (string.IsNullOrEmpty(timestampStr))
                    return false;

                if (!DateTime.TryParse(timestampStr, out var timestamp))
                    return false;

                var age = DateTime.UtcNow - timestamp;
                return age < _cacheExpiration;
            }
            catch
            {
                return false;
            }
        }

        public async Task InvalidateCatalogsAsync()
        {
            try
            {
                await _localStorage.RemoveItemAsync("catalogsTimestamp");
                await _localStorage.RemoveItemAsync("catalog_Atributos");
                await _localStorage.RemoveItemAsync("catalog_RelacionBote");
                await _localStorage.RemoveItemAsync("catalog_RelacionTapa");
                await _localStorage.RemoveItemAsync("catalog_TiposCajas");

                Console.WriteLine("Catalogs cache invalidated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invalidating catalogs: {ex.Message}");
            }
        }
    }
}