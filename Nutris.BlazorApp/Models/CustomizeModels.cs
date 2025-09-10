namespace NutrisBlazor.Models
{
    // Model Classes
    public class InputItem
    {
        public string placeHolder { get; set; } = "";
    }

    public class InputRecipe
    {
        public string Active { get; set; } = "";
        public string Source_used { get; set; } = "";
        public string Quantity_of_active_per_serving { get; set; } = "";
        public string EU_RDA { get; set; } = "";
    }

    public class InputBottle
    {
        public string desc { get; set; } = "";
    }

    public class InputGummyDna
    {
        public string name { get; set; } = "";
        public string desc { get; set; } = "";
    }

    public class InputInformation
    {
        public string title { get; set; } = "";
        public string placeHolder { get; set; } = "";
    }

    public class InputPallet
    {
        public string title { get; set; } = "";
        public string placeHolder { get; set; } = "";
    }

    public class InputAnalytics
    {
        public string Code { get; set; } = "";
        public string Active { get; set; } = "";
        public string PVP { get; set; } = "";
        public string Analitycs { get; set; } = "";
        public string Periodicity { get; set; } = "";
        public string Observations { get; set; } = "";
        public string Quantity { get; set; } = "";
        public string Source { get; set; } = "";
        public decimal Precio_Analiticas { get; set; }
    }

    public class FileItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string File { get; set; } = "";
    }

    public class ListLotes
    {
        public int ID { get; set; }
        public string Format { get; set; } = "";
    }

    public class ListBBD
    {
        public int ID { get; set; }
        public string Format { get; set; } = "";
    }

    public class OptionItem
    {
        public string Value { get; set; } = "";
        public string Color_HEX { get; set; } = "";
    }

    public class SelectedOption
    {
        public string Capacidad { get; set; } = "";
        public string Material { get; set; } = "";
        public string Diametro { get; set; } = "";
        public string Color { get; set; } = "";
        public string Forma { get; set; } = "";
    }

    public class SelectedOptionLabels
    {
        public string label_size { get; set; } = "";
        public string label_material { get; set; } = "";
        public string label_finish { get; set; } = "";
        public string label_colors { get; set; } = "";
    }

    public class SelectedOptionCap
    {
        public string Forma { get; set; } = "";
        public string Diametro { get; set; } = "";
        public string Color { get; set; } = "";
        public bool? Sleever { get; set; }
    }

    // Response Models
    public class CustomizeRG35Response
    {
        public string Code { get; set; }
        public string Product_name { get; set; }
        public string Order { get; set; }
        public string Nutris_Code { get; set; }
        public string Estimated_date { get; set; }
        public string Deadline_date { get; set; }
        public decimal? Unit_price { get; set; }
        public int? MOQ { get; set; }
        public string Product_name_2 { get; set; }
        public string Characteristics { get; set; }
        public string RG37 { get; set; }
        public bool Customer_Accepted_RG37 { get; set; }
        public string Status { get; set; }
        public string Tipo { get; set; }
        public string Country { get; set; }
        public string Country_2 { get; set; }
        public string Country_3 { get; set; }
        public string Logo_Pais { get; set; }
        public string Logo_Pais_2 { get; set; }
        public string Logo_Pais_3 { get; set; }
        public string Box_label_imagen { get; set; }
        public string Label_imagen { get; set; }
        public string Bote_imagen { get; set; }
        public string Pallet_label_imagen { get; set; }
        public string Bote_capacidad { get; set; }
        public string Bote_material { get; set; }
        public string Bote_boca { get; set; }
        public string Bote_color { get; set; }
        public string Bote_forma { get; set; }
        public string Label_size { get; set; }
        public string Label_material { get; set; }
        public string Label_finish { get; set; }
        public string Label_color { get; set; }
        public string Cap_tapa { get; set; }
        public string Cap_Boca { get; set; }
        public string Cap_color { get; set; }
        public bool? Cap_sleever { get; set; }
        public string Trade_name { get; set; }
        public string Filling_batch { get; set; }
        public string Filling_exp_date { get; set; }
        public string Filling_location { get; set; }
        public string Filling_batch_others { get; set; }
        public string Filling_exp_date_others { get; set; }
        public string Pieces_per_container { get; set; }
        public string Pallet_type { get; set; }
        public string Pallet_layers { get; set; }
        public string Pallet_boxes_per_layer { get; set; }
        public string Pallet_boxes_per_pallet { get; set; }
        public string Box_name { get; set; }
        public string Box_units_per { get; set; }
        public string Pallet_comments { get; set; }
        public string Label_config { get; set; }
        public bool No_analitycs { get; set; }
        public List<FormulationData> formulation { get; set; }
        public List<RecipeData> recipe { get; set; }
        public List<AnalyticsData> analytics { get; set; }
        public List<FileData> files { get; set; }
    }

    public class CustomizeRG37Response
    {
        public string Code { get; set; }
        public string Product_name { get; set; }
        public string Product_name_2 { get; set; }
        public string Status { get; set; }
        public bool Suitable_vegetarians { get; set; }
        public bool Suitable_vegans { get; set; }
        public bool Natural_colors { get; set; }
        public bool Natural_flavor { get; set; }
        public string Nutris_comments { get; set; }
        public string Take_sample { get; set; }
        public bool Customer_accepted { get; set; }
        public bool Tomar_muestra { get; set; }
        public string Base { get; set; }
        public string Sugar_composition { get; set; }
        public string Cover { get; set; }
        public string Color { get; set; }
        public string Flavour { get; set; }
        public string Size { get; set; }
        public string Serving { get; set; }
        public string Shape { get; set; }
        public string Imagen { get; set; }
        public List<RecipeData> recipe { get; set; }
    }

    public class FormulationData
    {
        public bool Suitable_vegetarians { get; set; }
        public bool Suitable_vegans { get; set; }
        public bool Natural_colors { get; set; }
        public bool Natural_flavor { get; set; }
        public string Nutris_comments { get; set; }
        public string Take_sample { get; set; }
        public bool Customer_accepted { get; set; }
        public bool Tomar_muestra { get; set; }
        public string Base { get; set; }
        public string Sugar_composition { get; set; }
        public string Cover { get; set; }
        public string Color { get; set; }
        public string Flavour { get; set; }
        public string Size { get; set; }
        public string Serving { get; set; }
        public string Shape { get; set; }
        public string Imagen { get; set; }
    }

    public class RecipeData
    {
        public string Active { get; set; }
        public string Source_used { get; set; }
        public string Quantity_of_active_per_serving { get; set; }
        public string EU_RDA { get; set; }
    }

    public class AnalyticsData
    {
        public string Code { get; set; }
        public string Active { get; set; }
        public string PVP { get; set; }
        public string Analitycs { get; set; }
        public string Periodicity { get; set; }
        public string Observations { get; set; }
        public string Quantity { get; set; }
        public string Source { get; set; }
        public decimal Precio_Analiticas { get; set; }
    }

    public class FileData
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string File { get; set; }
    }

    public class AtributosResponse
    {
        public List<AtributoItem> value { get; set; }
    }

    public class AtributoItem
    {
        public List<OptionItem> valoresAtributos { get; set; }
    }

    public class LotFormatResponse
    {
        public List<ListLotes> value { get; set; }
    }

    public class BBDFormatResponse
    {
        public List<ListBBD> value { get; set; }
    }
}