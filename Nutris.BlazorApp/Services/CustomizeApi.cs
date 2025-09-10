// ✅ AJUSTA ESTE using AL NAMESPACE REAL DE TU SimpleApiService

using NutrisBlazor.Models;
using NutrisBlazor.Services;
using System.Text.Json;

namespace Nutris.BlazorApp.Features.Customize
{
    public interface ICustomizeApi
    {
        Task<CustomizeRG35Response> GetRG35Async(string id);
        Task<CustomizeRG37Response> GetRG37Async(string id);
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
        public CustomizeApi(SimpleApiService api) => _api = api;

        // === GETs (idénticos al VUE) ===
        public Task<CustomizeRG35Response> GetRG35Async(string id) =>
            _api.GetAsync<CustomizeRG35Response>($"CustomizeRG35?$filter=Code eq '{id}'&expand=Formulation,Recipe,Analytics,Files&tenant=nutris");

        public Task<CustomizeRG37Response> GetRG37Async(string id) =>
            _api.GetAsync<CustomizeRG37Response>($"CustomizeRG37?$filter=Code eq '{id}'&expand=Recipe&tenant=nutris");

        public Task<JsonDocument> GetAtributosAsync() =>
            _api.GetAsync<JsonDocument>("Atributos?tenant=nutris&$expand=valoresAtributos");

        public Task<JsonDocument> GetRelacionBoteAsync() =>
            _api.GetAsync<JsonDocument>("RelacionBote");

        public Task<JsonDocument> GetRelacionTapaAsync() =>
            _api.GetAsync<JsonDocument>("RelacionTapa");

        public Task<JsonDocument> GetLotFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>($"LotFormat?$tenant=nutris");
       
        public Task<JsonDocument> GetBbdFormatAsync(string rg37) =>
            _api.GetAsync<JsonDocument>($"BBDFormat?$tenant=nutris");

        // === PATCHs (idénticos al VUE) ===


        public Task PatchRG35Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG35('{id}')?$expand=Formulation,Recipe,Analytics,Files&tenant=nutris", payload);

        public Task PatchRG37Async(string id, object payload) =>
            _api.PatchAsync($"CustomizeRG37('{id}')?$expand=Recipe&tenant=nutris", payload);

        public Task PatchFormulationAsync(string rg37, object payload) =>
            _api.PatchAsync($"Formulation('{rg37}')", payload);

        // === POST (idéntico al VUE para imágenes de caja/palet) ===
        public Task<HttpResponseMessage> PostModificarImagenCabAsync(object payload) =>
            _api.PostAsync("modificarImagen(1)/Microsoft.NAV.modificarCab", payload);
    }
}
