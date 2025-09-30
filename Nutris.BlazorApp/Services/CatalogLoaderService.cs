using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutrisBlazor.Services
{
    public interface ICatalogLoaderService
    {
        Task LoadAndCacheCatalogsAsync();
    }

    public class CatalogLoaderService : ICatalogLoaderService
    {
        private readonly SimpleApiService _api;
        private readonly ILocalStorageService _localStorage;
        private readonly IAuthService _authService;

        public CatalogLoaderService(
            SimpleApiService api,
            ILocalStorageService localStorage,
            IAuthService authService)
        {
            _api = api;
            _localStorage = localStorage;
            _authService = authService;
        }

        public async Task LoadAndCacheCatalogsAsync()
        {
            try
            {
                Console.WriteLine("Loading and caching catalogs...");

                // Cargar todos los catálogos en paralelo
                var catalogTasks = new[]
                {
                    LoadCatalog("Atributos", () => GetAtributosAsync()),
                    LoadCatalog("RelacionBote", () => GetRelacionBoteAsync()),
                    LoadCatalog("RelacionTapa", () => GetRelacionTapaAsync()),
                    LoadCatalog("TiposCajas", () => GetTiposCajasAsync())
                };

                await Task.WhenAll(catalogTasks);

                // Guardar timestamp de última actualización
                await _localStorage.SetItemAsync("catalogsTimestamp", DateTime.UtcNow.ToString("o"));

                Console.WriteLine("Catalogs cached successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error caching catalogs: {ex.Message}");
                // No fallar el login si los catálogos no se pueden cargar
            }
        }

        private async Task LoadCatalog(string key, Func<Task<JsonDocument>> loader)
        {
            try
            {
                var data = await loader();
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data.RootElement);
                    await _localStorage.SetItemAsync($"catalog_{key}", json);
                    Console.WriteLine($"Cached catalog: {key}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading catalog {key}: {ex.Message}");
            }
        }

        // Métodos privados para cargar catálogos directamente
        private async Task<JsonDocument> GetAtributosAsync()
        {
            var url = "";
            var user = await _authService.GetCurrentUserAsync();

            if (user == null)
            {
                url = "Atributos?$expand=valoresAtributos&tenant=nutris";
            }
            else if (user.Family)
            {
                url = $"Atributos?$expand=valoresAtributos($filter=Family eq true)&tenant=nutris";
            }
            else if (user.Standard)
            {
                url = $"Atributos?$expand=valoresAtributos($filter=Standard eq true)&tenant=nutris";
            }
            else if (user.Premium)
            {
                url = $"Atributos?$expand=valoresAtributos($filter=Premium eq true)&tenant=nutris";
            }
            else
            {
                url = "Atributos?$expand=valoresAtributos&tenant=nutris";
            }

            return await _api.GetAsync<JsonDocument>(url);
        }

        private Task<JsonDocument> GetRelacionBoteAsync() =>
            _api.GetAsync<JsonDocument>("RelacionBote?tenant=nutris");

        private Task<JsonDocument> GetRelacionTapaAsync() =>
            _api.GetAsync<JsonDocument>("RelacionTapa?tenant=nutris");

        private Task<JsonDocument> GetTiposCajasAsync() =>
            _api.GetAsync<JsonDocument>("ApiTiposCaja?&tenant=nutris");
    }
}