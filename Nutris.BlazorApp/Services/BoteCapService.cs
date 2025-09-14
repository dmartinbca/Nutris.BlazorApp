using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NutrisBlazor.Services
{
    public interface IBoteCapService
    {
        Task<List<BoteDataItem>> GetBoteDataAsync(string tenant = "nutris");
        Task<List<CapDataItem>> GetCapDataAsync(string tenant = "nutris");
        Task<List<ColorOption>> GetBoteColorsAsync();
        Task<List<ColorOption>> GetCapColorsAsync();
        Task<bool> SaveConfigurationAsync(string codeRG35, BoteCapConfiguration configuration);
    }

    public class BoteCapService : IBoteCapService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BoteCapService> _logger;

        public BoteCapService(HttpClient httpClient, ILogger<BoteCapService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<BoteDataItem>> GetBoteDataAsync(string tenant = "nutris")
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/BoteData?tenant={tenant}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<BoteDataItem>>() ?? new List<BoteDataItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bote data");
                return new List<BoteDataItem>();
            }
        }

        public async Task<List<CapDataItem>> GetCapDataAsync(string tenant = "nutris")
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/CapData?tenant={tenant}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<CapDataItem>>() ?? new List<CapDataItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cap data");
                return new List<CapDataItem>();
            }
        }

        public async Task<List<ColorOption>> GetBoteColorsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/BoteColors");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ColorOption>>() ?? new List<ColorOption>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching bote colors");
                return new List<ColorOption>();
            }
        }

        public async Task<List<ColorOption>> GetCapColorsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/CapColors");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ColorOption>>() ?? new List<ColorOption>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cap colors");
                return new List<ColorOption>();
            }
        }

        public async Task<bool> SaveConfigurationAsync(string codeRG35, BoteCapConfiguration configuration)
        {
            try
            {
                var url = $"api/CustomizeRG35('{codeRG35}')?$expand=Formulation,Recipe,Analytics&tenant=nutris";
                var response = await _httpClient.PatchAsJsonAsync(url, configuration);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error saving configuration: {error}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                return false;
            }
        }
    }

    // Model classes
    public class BoteDataItem
    {
        public int ID { get; set; }
        public string Forma { get; set; } = "";
        public string Capacidad { get; set; } = "";
        public string Diametro { get; set; } = "";
        public string Material { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public class CapDataItem
    {
        public int ID { get; set; }
        public string Forma { get; set; } = "";
        public string Diametro { get; set; } = "";
        public string Color { get; set; } = "";
        public bool Sleever { get; set; }
    }

    public class ColorOption
    {
        public int ID { get; set; }
        public string Value { get; set; } = "";
        public string ColorHex { get; set; } = "";
    }

    public class BoteCapConfiguration
    {
        [JsonPropertyName("Bote_forma")]
        public string BoteForma { get; set; } = "";

        [JsonPropertyName("Bote_capacidad")]
        public string BoteCapacidad { get; set; } = "";

        [JsonPropertyName("Bote_boca")]
        public string BoteBoca { get; set; } = "";

        [JsonPropertyName("Bote_color")]
        public string BoteColor { get; set; } = "";

        [JsonPropertyName("Bote_material")]
        public string BoteMaterial { get; set; } = "";

        [JsonPropertyName("Cap_tapa")]
        public string CapTapa { get; set; } = "";

        [JsonPropertyName("Cap_Boca")]
        public string CapBoca { get; set; } = "";

        [JsonPropertyName("Cap_color")]
        public string CapColor { get; set; } = "";

        [JsonPropertyName("Cap_sleever")]
        public bool CapSleever { get; set; }

        [JsonPropertyName("Characteristics")]
        public string Characteristics { get; set; } = "";
    }
}