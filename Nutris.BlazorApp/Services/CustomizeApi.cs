using NutrisBlazor.Models;
using NutrisBlazor.Models.Converters;
using NutrisBlazor.Services;
using System.Text.Json;

namespace NutrisBlazor.Services
{
    public interface ICustomizeApi
    {
        Task<CustomizeRG35Response?> GetRG35Async(string id);
        Task<CustomizeRG37Response?> GetRG37Async(string id);
        Task<JsonDocument> GetAtributosAsync();
        Task<JsonDocument> GetRelacionBoteAsync();
        Task<JsonDocument> GetRelacionTapaAsync();
        Task PatchRG35Async(string id, object payload);
        Task PatchRG37Async(string id, object payload);
        Task PatchFormulationAsync(string rg37, object payload);
        Task<JsonDocument> GetLotFormatAsync(string rg37);
        Task<JsonDocument> GetBbdFormatAsync(string rg37);
        Task<JsonDocument> GetTiposCajastAsync();
        Task<HttpResponseMessage> PostModificarImagenCabAsync(object payload);
        Task<User> GetCurrentUserAsync();
    }

    public sealed class CustomizeApi : ICustomizeApi
    {
        private readonly SimpleApiService _api;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IAuthService _authService;
        private User _currentUser;

        public CustomizeApi(SimpleApiService api, IAuthService authService)
        {
            _api = api;
            _authService = authService;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null,
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new FlexibleStringConverter(),
                    new FlexibleIntConverter(),
                    new FlexibleDecimalConverter(),
                    new FlexibleBoolConverter()
                }
            };
        }

        public async Task<User> GetCurrentUserAsync()
        {
            _currentUser ??= await _authService.GetCurrentUserAsync();
            return _currentUser;
        }

        public async Task<CustomizeRG35Response?> GetRG35Async(string id)
        {
            try
            {
                var response = await _api.GetAsync<JsonDocument>($"CustomizeRG35('{id}')?$expand=Formulation,Recipe,Analytics,Files&tenant=nutris");

                if (response?.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var json = response.RootElement.GetRawText();
                    return JsonSerializer.Deserialize<CustomizeRG35Response>(json, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting RG35: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<CustomizeRG37Response?> GetRG37Async(string id)
        {
            try
            {
                var response = await _api.GetAsync<JsonDocument>($"CustomizeRG37('{id}')?$expand=Recipe,Files&tenant=nutris");

                if (response?.RootElement.ValueKind == JsonValueKind.Object)
                {
                    var json = response.RootElement.GetRawText();
                    return JsonSerializer.Deserialize<CustomizeRG37Response>(json, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting RG37: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<JsonDocument> GetAtributosAsync()
        {
            var user = await GetCurrentUserAsync();
            string url;

            if (user.Family)
                url = $"Atributos?$expand=valoresAtributos($filter=Family eq {user.Family.ToString().ToLower()})&tenant=nutris";
            else if (user.Standard)
                url = $"Atributos?$expand=valoresAtributos($filter=Standard eq {user.Standard.ToString().ToLower()})&tenant=nutris";
            else if (user.Premium)
                url = $"Atributos?$expand=valoresAtributos($filter=Premium eq {user.Premium.ToString().ToLower()})&tenant=nutris";
            else
                url = $"Atributos?$expand=valoresAtributos&tenant=nutris";

            return await _api.GetAsync<JsonDocument>(url);
        }

        public async Task<JsonDocument> GetRelacionBoteAsync() 
        {
            
            return await _api.GetAsync<JsonDocument>("RelacionBote?tenant=nutris");
        }
            

        public async Task<JsonDocument> GetRelacionTapaAsync()
        {
            return await _api.GetAsync<JsonDocument>("RelacionTapa?tenant=nutris");
        }
          

        public Task<JsonDocument> GetLotFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>("LotFormat?tenant=nutris");

        public Task<JsonDocument> GetBbdFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>("BBDFormat?tenant=nutris");

        public Task<JsonDocument> GetTiposCajastAsync() =>
            _api.GetAsync<JsonDocument>("ApiTiposCaja?&tenant=nutris");

        public Task PatchRG35Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG35('{id}')?$expand=Formulation,Recipe,Analytics,Files&tenant=nutris", payload);

        public Task PatchRG37Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG37('{id}')?$expand=Recipe&tenant=nutris", payload);

        public Task PatchFormulationAsync(string rg37, object payload) =>
            _api.PatchAsync($"Formulation('{rg37}')?tenant=nutris", payload);

        public Task<HttpResponseMessage> PostModificarImagenCabAsync(object payload) =>
            _api.PostAsync("modificarImagen(1)/Microsoft.NAV.modificarCab", payload);
    }
}