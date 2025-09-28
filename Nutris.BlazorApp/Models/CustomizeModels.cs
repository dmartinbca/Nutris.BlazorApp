using NutrisBlazor.Models.Converters;
using System.Text.Json.Serialization;

namespace NutrisBlazor.Models
{
    public class CustomizeRG35Response
    {
        public string Code { get; set; } = "";
        public string Product_name { get; set; } = "";
        public string Product_name_2 { get; set; } = "";
        public string Order { get; set; } = "";
        public string Nutris_Code { get; set; } = "";
        public string Estimated_date { get; set; } = "";
        public string Deadline_date { get; set; } = "";
        public decimal Unit_price { get; set; }
        public int MOQ { get; set; }
        public string Characteristics { get; set; } = "";
        public string RG37 { get; set; } = "";
        public string Status { get; set; } = "";
        public bool Customer_Accepted_RG37 { get; set; }

        // Países y logos
        public string Country { get; set; } = "";
        public string Country_2 { get; set; } = "";
        public string Country_3 { get; set; } = "";
        public string Logo_Pais { get; set; } = "";
        public string Logo_Pais_2 { get; set; } = "";
        public string Logo_Pais_3 { get; set; } = "";

        // Imágenes
        public string Box_label_imagen { get; set; } = "";
        public string Label_imagen { get; set; } = "";
        public string Bote_imagen { get; set; } = "";
        public double Peso_total { get; set; } = 0;
        public string Cap_imagen { get; set; } = "";
        public string Pallet_label_imagen { get; set; } = "";


        //Formatos Other
        public string Filling_LoteOthers { get; set; } = "";
        public string Box_FormatoLoteCaja { get; set; } = "";
        public string Pallet_FormatoLotePalet { get; set; } = "";
        // Tipo de producto
        public string Tipo { get; set; } = "";

        // Bote/Botella
        public string Bote_capacidad { get; set; } = "";
        public string Bote_material { get; set; } = "";
        public string Bote_boca { get; set; } = "";
        public string Bote_color { get; set; } = "";
        public string Bote_forma { get; set; } = "";
        public string Pieces_per_container { get; set; } = "";

        // Label
        public string Label_size { get; set; } = "";
        public string Label_material { get; set; } = "";
        public string Label_finish { get; set; } = "";
        public string Label_color { get; set; } = "";
        public string Label_config { get; set; } = "";
        public string Label_type { get; set; } = "";

       

        // Cap/Tapa
        public string Cap_tapa { get; set; } = "";
        public string Cap_Boca { get; set; } = "";
        public string Cap_color { get; set; } = "";
        public bool? Cap_sleever { get; set; }

        // Filling
        public string Trade_name { get; set; } = "";
        public string Filling_batch { get; set; } = "";
        public string Filling_exp_date { get; set; } = "";
        public string Filling_location { get; set; } = "";
        public string Filling_batch_others { get; set; } = "";
        public string Filling_exp_date_others { get; set; } = "";

        // Pallet
        public string Pallet_type { get; set; } = "";
        public string Pallet_layers { get; set; } = "";
        public string Pallet_boxes_per_layer { get; set; } = "";
        public string Pallet_boxes_per_pallet { get; set; } = "";
        public string Pallet_comments { get; set; } = "";

        // Box
        public string Box_name { get; set; } = "";
        public string Box_units_per { get; set; } = "";

        public string Box_Nombre_Producto { get; set; } = "";
        public string Box_Lote { get; set; } = "";
        public string Box_BBD { get; set; } = "";
        public string Box_label_config { get; set; } = "";
        public string Box_Quantity { get; set; } = "";
        // Analytics
        public bool No_analitycs { get; set; }

        // Listas relacionadas
        public List<FormulationItem> Formulation { get; set; } = new();
        public List<RecipeItem> Recipe { get; set; } = new();
        public List<AnalyticsItem> Analytics { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();
    }

    public class CustomizeRG37Response
    {
        public string Code { get; set; } = "";
        public string Product_name { get; set; } = "";
        public string Product_name_2 { get; set; } = "";
        public string Status { get; set; } = "";
        public string Country { get; set; } = "";
        public string Country_2 { get; set; } = "";
        public string Country_3 { get; set; } = "";
        public string Logo_Pais { get; set; } = "";
        public string Logo_Pais_2 { get; set; } = "";
        public string Logo_Pais_3 { get; set; } = "";
        // Formulation
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Suitable_vegetarians { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Suitable_vegans { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Natural_colors { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Natural_flavor { get; set; }

        public string Nutris_comments { get; set; } = "";
        public string Take_sample { get; set; } = "";

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Customer_accepted { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Tomar_muestra { get; set; }

        // Gummy DNA
        public string Base { get; set; } = "";
        public string Sugar_composition { get; set; } = "";
        public string Cover { get; set; } = "";
        public string Color { get; set; } = "";
        public string Flavour { get; set; } = "";
        public string Size { get; set; } = "";
        public string Serving { get; set; } = "";
        public string Shape { get; set; } = "";
        public string Imagen { get; set; } = "";

        // Recipe
        public List<RecipeItem> Recipe { get; set; } = new();
        public List<FileItem> Files { get; set; } = new();
    }

    public class FormulationItem
    {
        public bool Suitable_vegetarians { get; set; }
        public bool Suitable_vegans { get; set; }
        public bool Natural_colors { get; set; }
        public bool Natural_flavor { get; set; }
        public string Nutris_comments { get; set; } = "";
        public string Take_sample { get; set; } = "";
        public bool Customer_accepted { get; set; }
        public bool Tomar_muestra { get; set; }
        public string Base { get; set; } = "";
        public string Sugar_composition { get; set; } = "";
        public string Cover { get; set; } = "";
        public string Color { get; set; } = "";
        public string Flavour { get; set; } = "";
        public string Size { get; set; } = "";
        public string Serving { get; set; } = "";
        public string Shape { get; set; } = "";
        public string Imagen { get; set; } = "";
        public string Shape_2 { get; set; } = "";
        public string Imagen_2 { get; set; } = "";
        public string Shape_3 { get; set; } = "";
        public string Imagen_3 { get; set; } = "";
        public string Shape_4 { get; set; } = "";
        public string Imagen_4 { get; set; } = "";
    }

    public class RecipeItem
    {
        public string Active { get; set; } = "";
        public string Source_used { get; set; } = "";
        public string Quantity_of_active_per_serving { get; set; } = "";
        public string EU_RDA { get; set; } = "";
    }

    public class AnalyticsItem
    {
        public string Code { get; set; } = "";
        public string Active { get; set; } = "";
        public string PVP { get; set; } = "";

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool Analitycs { get; set; }

        public string Periodicity { get; set; } = "";
        public string Observations { get; set; } = "";
        public string Quantity { get; set; } = "";
        public string Source { get; set; } = "";

        [JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal Precio_Analiticas { get; set; }
    }

    public class FileItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string File { get; set; } = "";
    }
}