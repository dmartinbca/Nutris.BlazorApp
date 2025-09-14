using System.Collections.Generic;
using System.Threading.Tasks;

namespace NutrisBlazor.Services
{
    public interface IBoteCapService
    {
        Task<BoteCapLookups> LoadLookupsAsync();
        Task<Dictionary<string, Dictionary<string, BoteDimension>>> LoadDimensionsAsync();
        Task<bool> SaveConfigurationAsync(BoteCapConfiguration config, string codeRG35);
    }

    public class BoteCapLookups
    {
        public List<string> Capacidades { get; set; } = new();
        public Dictionary<string, List<string>> CapacidadToDiametros { get; set; } = new();
        public List<string> Materiales { get; set; } = new();
        public List<ColorOption> ColoresBote { get; set; } = new();
        public List<ColorOption> ColoresTapa { get; set; } = new();
    }

    public class ColorOption
    {
        public string Label { get; set; } = "";
        public string Hex { get; set; } = "";
    }

    public class BoteDimension
    {
        public double Altura { get; set; }
        public double DiametroBase { get; set; }
        public double DiametroBoca { get; set; }
    }

    public class BoteCapConfiguration
    {
        public string BoteForma { get; set; } = "";
        public string BoteCapacidad { get; set; } = "";
        public string BoteDiametro { get; set; } = "";
        public string BoteMaterial { get; set; } = "";
        public string BoteColor { get; set; } = "";

        public string TapaForma { get; set; } = "";
        public string TapaDiametro { get; set; } = "";
        public string TapaColor { get; set; } = "";
        public bool TapaSleeve { get; set; }
    }
}
