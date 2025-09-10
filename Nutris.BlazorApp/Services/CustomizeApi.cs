using NutrisBlazor.Models;
using NutrisBlazor.Models.Converters;
using NutrisBlazor.Services;
using System.Text.Json;

namespace Nutris.BlazorApp.Features.Customize
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
        Task<HttpResponseMessage> PostModificarImagenCabAsync(object payload);
    }

    public sealed class CustomizeApi : ICustomizeApi
    {
        private readonly SimpleApiService _api;
        private readonly JsonSerializerOptions _jsonOptions;

        public CustomizeApi(SimpleApiService api)
        {
            _api = api;

            // Configurar opciones de serialización con los convertidores personalizados
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = null, // Mantener los nombres tal cual
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

        public async Task<CustomizeRG35Response?> GetRG35Async(string id)
        {
            try
            {
                // Para RG35, necesitamos usar la sintaxis correcta de OData
                var response = await _api.GetAsync<JsonDocument>($"CustomizeRG35('{id}')?$expand=Formulation,Recipe,Analytics,Files&tenant=nutris");

                if (response?.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // La respuesta directa es el objeto
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
                // Para RG37, también usamos la sintaxis correcta
                var response = await _api.GetAsync<JsonDocument>($"CustomizeRG37('{id}')?$expand=Recipe&tenant=nutris");

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

        public Task<JsonDocument> GetAtributosAsync() =>
            _api.GetAsync<JsonDocument>("Atributos?tenant=nutris&$expand=valoresAtributos");

        public Task<JsonDocument> GetRelacionBoteAsync() =>
            _api.GetAsync<JsonDocument>("RelacionBote");

        public Task<JsonDocument> GetRelacionTapaAsync() =>
            _api.GetAsync<JsonDocument>("RelacionTapa");

        public Task<JsonDocument> GetLotFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>("LotFormat?tenant=nutris");

        public Task<JsonDocument> GetBbdFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>("BBDFormat?tenant=nutris");

        public Task PatchRG35Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG35('{id}')?$expand=Formulation,Recipe,Analytics,Files&tenant=nutris", payload);

        public Task PatchRG37Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG37('{id}')?$expand=Recipe&tenant=nutris", payload);

        public Task PatchFormulationAsync(string rg37, object payload) =>
            _api.PatchAsync($"Formulation('{rg37}')", payload);

        public Task<HttpResponseMessage> PostModificarImagenCabAsync(object payload) =>
            _api.PostAsync("modificarImagen(1)/Microsoft.NAV.modificarCab", payload);
    }
}