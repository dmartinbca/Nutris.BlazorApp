using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NutrisBlazor.Services
{
    public class BoteCapApiService : IBoteCapService
    {
        private readonly HttpClient _http;
        public BoteCapApiService(HttpClient http) => _http = http;

        public async Task<BoteCapLookups> LoadLookupsAsync()
        {
            try
            {
                // AJUSTA estos endpoints a tu API real
                var lookups = await _http.GetFromJsonAsync<BoteCapLookups>("/api/bote/lookups");
                if (lookups != null) return lookups;
            }
            catch { /* fallback */ }
            return FallbackLookups();
        }

        public async Task<Dictionary<string, Dictionary<string, BoteDimension>>> LoadDimensionsAsync()
        {
            try
            {
                var dims = await _http.GetFromJsonAsync<Dictionary<string, Dictionary<string, BoteDimension>>>("/api/bote/dimensions");
                if (dims != null) return dims;
            }
            catch { /* fallback */ }
            return FallbackDimensions();
        }

        public async Task<bool> SaveConfigurationAsync(BoteCapConfiguration config, string codeRG35)
        {
            try
            {
                var url = $"CustomizeRG35('{codeRG35}')";
                var payload = JsonSerializer.Serialize(config);
                var req = new HttpRequestMessage(HttpMethod.Patch, url)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                var resp = await _http.SendAsync(req);
                return resp.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // Fallbacks por si la API no responde
        private static BoteCapLookups FallbackLookups() => new()
        {
            Capacidades = new() { "150", "200", "250", "300", "400", "500", "600", "1000" },
            CapacidadToDiametros = new()
            {
                ["150"] = new() { "D45" },
                ["200"] = new() { "D45" },
                ["250"] = new() { "D45" },
                ["300"] = new() { "D45" },
                ["400"] = new() { "D45" },
                ["500"] = new() { "D45" },
                ["600"] = new() { "D45" },
                ["1000"] = new() { "D45" },
            },
            Materiales = new() { "PET", "HDPE", "PP" },
            ColoresBote = new()
            {
                new ColorOption{ Label="Clear", Hex="#CCCCCC" },
                new ColorOption{ Label="Amber", Hex="#7A4E1D" },
                new ColorOption{ Label="Black", Hex="#000000" },
            },
            ColoresTapa = new()
            {
                new ColorOption{ Label="White", Hex="#FFFFFF" },
                new ColorOption{ Label="Black", Hex="#000000" },
                new ColorOption{ Label="Blue",  Hex="#0D6EFD" },
            }
        };

        private static Dictionary<string, Dictionary<string, BoteDimension>> FallbackDimensions() => new()
        {
            ["150"] = new() { ["D45"] = new BoteDimension { Altura = 88.3, DiametroBase = 55.3, DiametroBoca = 43.82 } },
            ["200"] = new() { ["D45"] = new BoteDimension { Altura = 105.86, DiametroBase = 60.0, DiametroBoca = 43.82 } },
            ["250"] = new() { ["D45"] = new BoteDimension { Altura = 111.3, DiametroBase = 63.91, DiametroBoca = 43.82 } },
        };
    }
}
