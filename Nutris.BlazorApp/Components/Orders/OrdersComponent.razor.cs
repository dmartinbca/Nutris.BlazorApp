using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NutrisBlazor.Models;
using NutrisBlazor.Services;
using System.Net.Http;
using System.Text.Json;

namespace Nutris.BlazorApp.Components.Orders;

public class OrdersComponentBase : ComponentBase
{
    [Inject] protected IJSRuntime JS { get; set; } = default!; // Agregar esta línea
    [Inject] public ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] protected IApiService Api { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected ILocalizationService Localization { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    // Parámetros desde el padre (Customize.razor)
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public CustomizeRG35Response? RG35 { get; set; }
    [Parameter] public CustomizeRG37Response? RG37 { get; set; }
    [Parameter] public bool HasRG35 { get; set; }
    [Parameter] public bool HasRG37 { get; set; }
    [Parameter] public JsonElement Atributos { get; set; }
    [Parameter] public JsonElement RelacionBote { get; set; }
    [Parameter] public JsonElement RelacionTapa { get; set; }
    [Parameter] public JsonElement LotFormat { get; set; }
    [Parameter] public JsonElement BbdFormat { get; set; }

    // Callbacks hacia el padre
    [Parameter] public EventCallback OnApprove { get; set; }
    [Parameter] public EventCallback<string> OnSaveName { get; set; }
    [Parameter] public EventCallback<(string format, string? other)> OnSaveLotFormat { get; set; }
    [Parameter] public EventCallback<(string format, string? other)> OnSaveBbdFormat { get; set; }
    [Parameter] public EventCallback<object> OnPatchRG35 { get; set; }
    [Parameter] public EventCallback<object> OnPatchRG37 { get; set; }
    [Parameter] public EventCallback<object> OnUploadBoxOrPallet { get; set; }

    // Estado de carga
    protected bool IsLoading { get; set; } = false;

    // Propiedades del Header
    protected string Prefix => Id?.Split('-')[0]?.ToUpper() ?? "";
    protected string Code { get; set; } = "-";
    protected string Name { get; set; } = "-";
    protected string ProductName2 { get; set; } = "-";
    protected string NutrisCode { get; set; } = "-";
    protected int MOQ { get; set; }
    protected decimal UnitPrice { get; set; }
    protected DateTime? EstimatedDate { get; set; }
    protected DateTime? DeadlineDate { get; set; }
    protected string EstimatedDateString => EstimatedDate?.ToString("dd/MM/yyyy") ?? "-";
    protected string DeadlineDateString => DeadlineDate?.ToString("dd/MM/yyyy") ?? "-";
    protected string Status { get; set; } = "-";
    protected bool Status37 { get; set; }
    protected string RG37Code { get; set; } = "-";
    protected string ProductType { get; set; } = "Bote";

    // Países y logos
    protected string CustomerLogoUrl { get; set; } = "";
    protected string Country1 { get; set; } = "-";
    protected string Country2 { get; set; } = "-";
    protected string Country3 { get; set; } = "-";
    protected string CountryFlag1 { get; set; } = "";
    protected string CountryFlag2 { get; set; } = "";
    protected string CountryFlag3 { get; set; } = "";

    // Archivos para descargar
    protected List<FileItem> ReportFiles { get; set; } = new();
    protected bool ShowReports { get; set; }

    // Control de secciones abiertas
    protected Dictionary<int, bool> IsOpen { get; set; } = new()
    {
        { 1, true }, { 2, false }, { 3, false }, { 4, false }, { 5, false }
    };

    // FORMULATION
    protected int PercentFilledFormulation { get; set; }
    protected bool CustomerAccepted { get; set; }
    protected bool TakeSample { get; set; }
    protected decimal TakeSamplePrice { get; set; }
    protected string NutrisComments { get; set; } = "";
    protected string Shape { get; set; } = "-";
    protected string GummyShapeImg { get; set; } = "";

    // Features (vegetarian, vegan, etc.)
    protected bool SuitableVegetarians { get; set; }
    protected bool SuitableVegans { get; set; }
    protected bool NaturalColors { get; set; }
    protected bool NaturalFlavor { get; set; }

    // Listas de Gummy DNA
    protected List<InputItem> GummyListBn { get; set; } = new();
    protected List<InputItem> GummyListB { get; set; } = new();
    protected List<RecipeRow> RecipeRows { get; set; } = new();

    // PACKAGING
    protected int PercentFilledBottle { get; set; }
    protected string BottleType { get; set; } = "-";
    protected string BottleImg { get; set; } = "";
    protected List<InputItem> BottleInfo { get; set; } = new();
    protected string FillingBatch { get; set; } = "";
    protected string FillingBatchOther { get; set; } = "";
    protected string FillingExpDate { get; set; } = "";
    protected string FillingExpDateOther { get; set; } = "";
    protected string FillingLocation { get; set; } = "-";
    protected bool IsSendingBatchOther { get; set; }
    protected bool IsSendingBbdOther { get; set; }
    protected List<FormatOption> BatchFormats { get; set; } = new();
    protected List<FormatOption> BbdFormats { get; set; } = new();

    // LABEL
    protected int PercentFilledLabel { get; set; }
    protected bool NoLabel { get; set; }
    protected string LabelImageUrl { get; set; } = "";
    protected List<InputItem> LabelInfo { get; set; } = new();

    // PALLETIZING
    protected int PercentFilledPalettizing { get; set; }
    protected string BoxLabelImg { get; set; } = "";
    protected string PalletLabelImg { get; set; } = "";
    protected string PalletComments { get; set; } = "";
    protected List<InputItem> PalletInfo { get; set; } = new();
    protected List<InputItem> PalletizingInfo { get; set; } = new();

    // ANALYTICS
    protected int PercentFilledAnalytics { get; set; }
    protected bool NoAnalytics { get; set; }
    protected List<AnalyticsRow> AnalyticsRows { get; set; } = new();
    protected decimal SumAnalytics { get; set; }
    protected string SumAnalyticsFormatted => $"{SumAnalytics:F2}€";

    // Opciones de catálogos
    protected List<AtributoOption> OptionsSize { get; set; } = new();
    protected List<AtributoOption> OptionsSizeLabel { get; set; } = new();
    protected List<AtributoOption> OptionsFinish { get; set; } = new();
    protected List<AtributoOption> OptionsLabelMaterial { get; set; } = new();
    protected List<AtributoOption> OptionsColorLabel { get; set; } = new();
    protected List<AtributoOption> OptionsColorBote { get; set; } = new();
    public string Logo { get; set; } = string.Empty;
    public bool showLanguageMenu = false;
    public string currentLanguage = "es";

    public int i = 0;
    protected override async Task OnParametersSetAsync()
    {
        currentLanguage = Localization.CurrentLanguage ?? "es";

        // Suscribirse a cambios de idioma
        Localization.OnLanguageChanged += OnLanguageChanged;
        await LoadDataAsync();
    }
    public void OnLanguageChanged()
    {
        currentLanguage = Localization.CurrentLanguage;
        InvokeAsync(StateHasChanged);
    }

    public void ToggleLanguageMenu()
    {
        showLanguageMenu = !showLanguageMenu;
    }
    public void GoBack()
    {
        Navigation.NavigateTo("/home");
    }
    public async Task SelectLanguage(string language)
    {
        showLanguageMenu = false;
        await Localization.ChangeLanguageAsync(language);
    }
    protected string GetListTranslation(string baseKey, int index)
    {
        // Construye la clave como "orderView.ListInputBn[0]" 
        var key = $"{baseKey}[{index}]";
        return Localization[key];
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Inicializar tooltips y restaurar estados
            await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.initializeTooltips");
            await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.restoreContainerStates");
        }
    }
    protected async Task HandleFormulationApproved()
    {
        // Actualizar el estado local
        CustomerAccepted = true;
        Status = "Cerrado cliente";

        // Limpiar modals
        await JS.InvokeVoidAsync("modalHelper.cleanupModals");

        // Pequeño delay
        await Task.Delay(200);
         CalculateAllPercentages();
        // Navegar a home
        // Navigation.NavigateTo("/home", forceLoad: true);
    }
    private async Task LoadDataAsync()
    {
        try
        {
            if (HasRG35 && RG35 != null)
            {
                LoadFromRG35(RG35);
            }
            else if (HasRG37 && RG37 != null)
            {
                LoadFromRG37(RG37);
            }

            // Cargar opciones de catálogos
            LoadCatalogOptions();

            // Calcular porcentajes
            CalculateAllPercentages();
            var customerLogo = await LocalStorage.GetItemAsync<string>("logo");

            if (!string.IsNullOrEmpty(customerLogo))
            {
                Logo = $"data:image/png;base64,{customerLogo}";
            }
            else
            {
                Logo = string.Empty;
            }




            // Guardar logo del cliente si existe
            if (!string.IsNullOrEmpty(CustomerLogoUrl))
            {
                await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.setCustomerLogo",
                    CustomerLogoUrl.Replace("data:image/png;base64,", ""));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
    }

    private void LoadFromRG35(CustomizeRG35Response data)
    {
        // Header
        Code = data.Code ?? "-";
        Name = data.Product_name ?? "-";
        ProductName2 = data.Product_name_2 ?? "-";
        MOQ = data.MOQ;
        UnitPrice = data.Unit_price;
        NutrisCode = data.Nutris_Code ?? "-";
        Status = data.Status ?? "-";
        Status37 = data.Customer_Accepted_RG37;
        RG37Code = data.RG37 ?? "-";
        ProductType = data.Tipo ?? "Bote";

        // Fechas
        if (DateTime.TryParse(data.Estimated_date, out var est))
            EstimatedDate = est;
        if (DateTime.TryParse(data.Deadline_date, out var dead))
            DeadlineDate = dead;

        // Logo del cliente (obtener de localStorage)
        CustomerLogoUrl = ""; // Se cargará desde JS

        // Países y banderas
        Country1 = data.Country ?? "-";
        Country2 = data.Country_2 ?? "-";
        Country3 = data.Country_3 ?? "-";

        if (!string.IsNullOrEmpty(data.Logo_Pais))
            CountryFlag1 = $"data:image/png;base64,{data.Logo_Pais}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_2))
            CountryFlag2 = $"data:image/png;base64,{data.Logo_Pais_2}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_3))
            CountryFlag3 = $"data:image/png;base64,{data.Logo_Pais_3}";

        // Archivos
        ReportFiles = data.Files ?? new List<FileItem>();

        // Formulation
        if (data.Formulation != null && data.Formulation.Any())
        {
            var form = data.Formulation.First();
            SuitableVegetarians = form.Suitable_vegetarians;
            SuitableVegans = form.Suitable_vegans;
            NaturalColors = form.Natural_colors;
            NaturalFlavor = form.Natural_flavor;
            NutrisComments = form.Nutris_comments ?? "";
            CustomerAccepted = form.Customer_accepted;
            TakeSample = form.Tomar_muestra;
            TakeSamplePrice = decimal.TryParse(form.Take_sample, out var price) ? price : 0;
            Shape = form.Shape ?? "-";

            if (!string.IsNullOrEmpty(form.Imagen))
                GummyShapeImg = $"data:image/png;base64,{form.Imagen}";

            // Gummy DNA lists
            GummyListBn = new List<InputItem>
            {
                new() { Label = Localization["orderView.ListInputBn[0]"], Value = form.Base ?? "-" },
                new() { Label = Localization["orderView.ListInputBn[1]"], Value = form.Sugar_composition ?? "-" },
                new() { Label = Localization["orderView.ListInputBn[2]"], Value = form.Cover ?? "-" }
            };

            GummyListB = new List<InputItem>
            {
                new() { Label = Localization["orderView.ListInputB[0]"], Value = form.Color ?? "-" },
                new() { Label = Localization["orderView.ListInputB[1]"], Value = form.Flavour ?? "-" },
                new() { Label = Localization["orderView.ListInputB[2]"], Value = form.Size ?? "-" },
                new() { Label = Localization["orderView.ListInputB[3]"], Value = form.Serving ?? "-" }
            };
        }

        // Recipe
        RecipeRows = data.Recipe?.Select(r => new RecipeRow
        {
            Active = r.Active ?? "-",
            SourceUsed = r.Source_used ?? "-",
            QuantityServing = r.Quantity_of_active_per_serving ?? "-",
            RdaEu = r.EU_RDA ?? "-"
        }).ToList() ?? new List<RecipeRow>();

        // Packaging
        if (!string.IsNullOrEmpty(data.Bote_imagen))
            BottleImg = $"data:image/png;base64,{data.Bote_imagen}";

        BottleInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.BottleA[0]"], Value = data.Characteristics ?? "-" },
            new() { Label = Localization["orderView.BottleA[1]"], Value = data.Bote_boca ?? "-" },
            new() { Label = Localization["orderView.BottleA[2]"], Value = data.Pieces_per_container ?? "-" }
        };

        FillingBatch = data.Filling_batch ?? "-";
        FillingExpDate = data.Filling_exp_date?.Replace("_x002F_", "/") ?? "-";
        FillingLocation = data.Filling_location ?? "-";
        FillingBatchOther = data.Filling_batch_others ?? "";
        FillingExpDateOther = data.Filling_exp_date_others ?? "";

        // Label
        NoLabel = data.Label_config == "No label";
        if (!string.IsNullOrEmpty(data.Label_imagen))
            LabelImageUrl = $"data:image/png;base64,{data.Label_imagen}";

        LabelInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.GUMMYDNAL[0]"], Value = data.Label_size ?? "-" },
            new() { Label = Localization["orderView.GUMMYDNAL[1]"], Value = data.Label_material ?? "-" },
            new() { Label = Localization["orderView.GUMMYDNAL[2]"], Value = data.Label_finish ?? "-" },
            new() { Label = Localization["orderView.GUMMYDNAL[3]"], Value = data.Label_color ?? "-" }
        };

        // Palletizing
        if (!string.IsNullOrEmpty(data.Box_label_imagen))
            BoxLabelImg = $"data:image/png;base64,{data.Box_label_imagen}";
        if (!string.IsNullOrEmpty(data.Pallet_label_imagen))
            PalletLabelImg = $"data:image/png;base64,{data.Pallet_label_imagen}";

        PalletComments = data.Pallet_comments ?? "";

        PalletInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.Pallet[0]"], Value = data.Box_name ?? "-" },
            new() { Label = Localization["orderView.Pallet[1]"], Value = data.Box_units_per ?? "-" }
        };

        PalletizingInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[0]"], Value = data.Pallet_type ?? "-" },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[1]"], Value = data.Pallet_layers ?? "-" },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[2]"], Value = data.Pallet_boxes_per_layer ?? "-" },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[3]"], Value = data.Pallet_boxes_per_pallet ?? "-" }
        };

        // Analytics
        NoAnalytics = data.No_analitycs;
        AnalyticsRows = data.Analytics?.Select(a => new AnalyticsRow
        {
            Active = a.Active ?? "-",
            Quantity = a.Quantity ?? "-",
            Source = a.Source ?? "-",
            Analytics = a.Analitycs,
            Periodicity = a.Periodicity ?? "-",
            Observations = a.Observations ?? "-",
            Price = a.Precio_Analiticas
        }).ToList() ?? new List<AnalyticsRow>();

        SumAnalytics = AnalyticsRows.FirstOrDefault()?.Price ?? 0;
    }

    private void LoadFromRG37(CustomizeRG37Response data)
    {
        // Header básico
        Code = data.Code ?? "-";
        Name = data.Product_name ?? "-";
        ProductName2 = data.Product_name_2 ?? "-";
        Status = data.Status ?? "-";
        RG37Code = data.Code ?? "-";

        // Formulation
        SuitableVegetarians = data.Suitable_vegetarians;
        SuitableVegans = data.Suitable_vegans;
        NaturalColors = data.Natural_colors;
        NaturalFlavor = data.Natural_flavor;
        NutrisComments = data.Nutris_comments ?? "";
        CustomerAccepted = data.Customer_accepted;
        TakeSample = data.Tomar_muestra;
        TakeSamplePrice = decimal.TryParse(data.Take_sample, out var price) ? price : 0;
        Shape = data.Shape ?? "-";

        if (!string.IsNullOrEmpty(data.Imagen))
            GummyShapeImg = $"data:image/png;base64,{data.Imagen}";

        // Países y banderas
        Country1 = data.Country ?? "-";
        Country2 = data.Country_2 ?? "-";
        Country3 = data.Country_3 ?? "-";

        if (!string.IsNullOrEmpty(data.Logo_Pais))
            CountryFlag1 = $"data:image/png;base64,{data.Logo_Pais}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_2))
            CountryFlag2 = $"data:image/png;base64,{data.Logo_Pais_2}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_3))
            CountryFlag3 = $"data:image/png;base64,{data.Logo_Pais_3}";
        // Gummy DNA lists
        GummyListBn = new List<InputItem>
        {
            new() { Label = Localization["orderView.ListInputBn[0]"], Value = data.Base ?? "-" },
            new() { Label = Localization["orderView.ListInputBn[1]"], Value = data.Sugar_composition ?? "-" },
            new() { Label = Localization["orderView.ListInputBn[2]"], Value = data.Cover ?? "-" }
        };

        GummyListB = new List<InputItem>
        {
            new() { Label = Localization["orderView.ListInputB[0]"], Value = data.Color ?? "-" },
            new() { Label = Localization["orderView.ListInputB[1]"], Value = data.Flavour ?? "-" },
            new() { Label = Localization["orderView.ListInputB[2]"], Value = data.Size ?? "-" },
            new() { Label = Localization["orderView.ListInputB[3]"], Value = data.Serving ?? "-" }
        };

        // Recipe
        RecipeRows = data.Recipe?.Select(r => new RecipeRow
        {
            Active = r.Active ?? "-",
            SourceUsed = r.Source_used ?? "-",
            QuantityServing = r.Quantity_of_active_per_serving ?? "-",
            RdaEu = r.EU_RDA ?? "-"
        }).ToList() ?? new List<RecipeRow>();

        ReportFiles = data.Files ?? new List<FileItem>();
    }

    private void LoadCatalogOptions()
    {
        try
        {
            // Cargar formatos de lote y BBD
            if (LotFormat.ValueKind == JsonValueKind.Object && LotFormat.TryGetProperty("value", out var lotValues))
            {
                BatchFormats = lotValues.EnumerateArray()
                    .Select(item => new FormatOption
                    {
                        Format = item.TryGetProperty("Format", out var f) ? f.GetString() ?? "" : ""
                    })
                    .ToList();
            }

            if (BbdFormat.ValueKind == JsonValueKind.Object && BbdFormat.TryGetProperty("value", out var bbdValues))
            {
                BbdFormats = bbdValues.EnumerateArray()
                    .Select(item => new FormatOption
                    {
                        Format = item.TryGetProperty("Format", out var f) ? f.GetString() ?? "" : ""
                    })
                    .ToList();
            }

            // Cargar atributos si están disponibles
            if (Atributos.ValueKind == JsonValueKind.Object && Atributos.TryGetProperty("value", out var attrValues))
            {
                var attrArray = attrValues.EnumerateArray().ToList();

                // Aquí podrías cargar las opciones específicas según los índices
                // Por ejemplo: OptionsSize, OptionsSizeLabel, etc.
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading catalog options: {ex.Message}");
        }
    }

    private void CalculateAllPercentages()
    {
        CalculateFormulationPercentage();
        CalculateBottlePercentage();
        CalculateLabelPercentage();
        CalculatePalletizingPercentage();
        CalculateAnalyticsPercentage();
    }

    private void CalculateFormulationPercentage()
    {
        if (CustomerAccepted)
        {
            PercentFilledFormulation = 100;
            return;
        }

        var fields = new List<object?>
        {
            GummyListBn.All(x => x.Value != "-" && !string.IsNullOrEmpty(x.Value)),
            GummyListB.All(x => x.Value != "-" && !string.IsNullOrEmpty(x.Value)),
            Shape != "-",
            !string.IsNullOrEmpty(GummyShapeImg)
        };

        var grupo50Completo = fields.All(x => x is bool b && b);
        var grupo30Completo = RecipeRows.Any();

        var percentage = 0;
        if (grupo50Completo) percentage += 50;
        if (grupo30Completo) percentage += 30;

        PercentFilledFormulation = percentage;
    }

    private void CalculateBottlePercentage()
    {
        if (ProductType == "Bulk")
        {
            PercentFilledBottle = 100;
            return;
        }

        var fields = new List<string>
        {
            ProductName2,
            FillingBatch,
            FillingExpDate,
            FillingLocation
        };

        fields.AddRange(BottleInfo.Select(x => x.Value));

        var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-");
        PercentFilledBottle = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
    }

    private void CalculateLabelPercentage()
    {
        if (NoLabel || ProductType == "Bulk")
        {
            PercentFilledLabel = 100;
            return;
        }

        var filled = LabelInfo.Count(x => !string.IsNullOrEmpty(x.Value) && x.Value != "-");
        PercentFilledLabel = LabelInfo.Count > 0 ? (filled * 100 / LabelInfo.Count) : 0;
    }

    private void CalculatePalletizingPercentage()
    {
        var fields = new List<string>();

        fields.AddRange(PalletInfo.Select(x => x.Value));
        fields.AddRange(PalletizingInfo.Select(x => x.Value));

        if (!string.IsNullOrEmpty(BoxLabelImg)) fields.Add(BoxLabelImg);
        if (!string.IsNullOrEmpty(PalletLabelImg)) fields.Add(PalletLabelImg);

        var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-");
        PercentFilledPalettizing = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
    }

    private void CalculateAnalyticsPercentage()
    {
        PercentFilledAnalytics = (NoAnalytics || AnalyticsRows.Any()) ? 100 : 0;
    }

    // UI Helpers
    protected string FormatPercent(int value) => Math.Clamp(value, 0, 100).ToString();

    protected void ToggleDownloadReports() => ShowReports = !ShowReports;

    protected async Task ToggleSection(int idx)
    {
        if (IsOpen.ContainsKey(idx))
        {
            IsOpen[idx] = !IsOpen[idx];

            // Guardar estado en localStorage
            await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.toggleContainer", $"container-toggle-{idx}");
        }
    }

    protected string GetStepColor(int step)
    {
        const string verde = "#8ED300";
        const string verdeSuave = "#FFA500";
        const string gris = "#E5E5E5";

        if (Prefix == "RG37")
        {
            if (step == 1)
            {
                if (PercentFilledFormulation == 100 && (Status == "Cerrado cliente" || Status == "Cerrado cliente y calidad"))
                    return verde;
                if (PercentFilledFormulation > 0 && Status == "Desarrollo")
                    return verdeSuave;
                return gris;
            }
            if (step == 5)
            {
                if (Status == "Cerrado cliente y calidad") return verde;
                if (Status == "Cerrado cliente") return verdeSuave;
                return gris;
            }
            return gris;
        }

        // RG35
        if (Prefix == "RG35" && step == 1 && Status37)
        {
            return verde;
        }

        if (step == 2)
        {
            if (ProductType == "Bulk")
            {
                if (PercentFilledPalettizing == 100 && (NoAnalytics || PercentFilledAnalytics == 100) &&
                    (Status == "Cerrado cliente" || Status == "Cerrado cliente y calidad"))
                    return verde;
                if (PercentFilledPalettizing > 0 || (NoAnalytics || PercentFilledAnalytics > 0))
                    return verdeSuave;
                return gris;
            }
            else
            {
                if (PercentFilledBottle == 100 && (PercentFilledLabel == 100 || NoLabel) &&
                    PercentFilledPalettizing == 100 && (NoAnalytics || PercentFilledAnalytics == 100) &&
                    (Status == "Cerrado cliente" || Status == "Cerrado cliente y calidad"))
                    return verde;
                if (PercentFilledBottle > 0 || PercentFilledLabel > 0 || PercentFilledPalettizing > 0 ||
                    (NoAnalytics || PercentFilledAnalytics > 0))
                    return verdeSuave;
                return gris;
            }
        }

        if (step == 3)
        {
            if (Status == "Cerrado cliente y calidad") return verde;
            if (Status == "Cerrado cliente") return verdeSuave;
            return gris;
        }

        if (step == 4)
        {
            if (Status == "Cerrado cliente y calidad") return verdeSuave;
            return gris;
        }

        if (step == 5)
        {
            if (Prefix == "RG35" && Status37) return verde;
            return gris;
        }

        return gris;
    }

    protected string GetStepTextColor(int step)
    {
        if (Prefix == "RG37" && step != 1 && step != 5)
            return "#989898";

        var bg = GetStepColor(step);
        if (bg == "#8ED300" || bg == "#FFA500")
            return "#FFFFFF";
        return "#989898";
    }

    protected string GetStepBorder(int step)
    {
        var bg = GetStepColor(step);
        if (bg == "#90EE90")
            return "#228B22"; // Dark green border for soft green phases
        return "none";
    }

    protected bool GetSwitchValue(int idx)
    {
        return idx switch
        {
            0 => SuitableVegetarians,
            1 => SuitableVegans,
            2 => NaturalColors,
            3 => NaturalFlavor,
            _ => false
        };
    }

    protected string GetFeatureImage(int idx)
    {
        return idx switch
        {
            0 => "/img/SUITABLE FOR VEGETARIAN.svg",
            1 => "/img/VEGAN FRIENDLY.svg",
            2 => "/img/NATURAL COLORS.svg",
            3 => "/img/NATURAL FLAVORS.svg",
            _ => ""
        };
    }

    protected string GetFeatureTitle(int idx)
    {
        return Localization[$"orderView.CheckTitle[{idx}]"];
    }

    protected bool CanConfirmAndSign
    {
        get
        {
            if (ProductType == "Bote")
            {
                return PercentFilledFormulation == 100 &&
                       PercentFilledBottle == 100 &&
                       (PercentFilledLabel == 100 || NoLabel) &&
                       PercentFilledPalettizing == 100 &&
                       (NoAnalytics || PercentFilledAnalytics == 100) &&
                       Status != "Cerrado cliente y calidad" &&
                       Status != "Cerrado cliente";
            }
            else if (ProductType == "Bulk")
            {
                return PercentFilledFormulation == 100 &&
                       PercentFilledPalettizing == 100 &&
                       (NoAnalytics || PercentFilledAnalytics == 100) &&
                       Status != "Cerrado cliente y calidad" &&
                       Status != "Cerrado cliente";
            }
            return false;
        }
    }

    protected string GetConfirmButtonClass()
    {
        return CanConfirmAndSign ? "btn-save-confirm RalewayRegular font-20" : "btn-save-confirm-disabled";
    }

    // Acciones
    protected async Task SaveName()
    {
        if (OnSaveName.HasDelegate)
            await OnSaveName.InvokeAsync(ProductName2);
    }

    protected async Task SaveBatch()
    {
        if (OnSaveLotFormat.HasDelegate)
            await OnSaveLotFormat.InvokeAsync((FillingBatch, null));
    }

    protected async Task SaveBatchOther()
    {
        if (string.IsNullOrWhiteSpace(FillingBatchOther)) return;

        IsSendingBatchOther = true;
        try
        {
            if (OnSaveLotFormat.HasDelegate)
                await OnSaveLotFormat.InvokeAsync((FillingBatch, FillingBatchOther));
            FillingBatchOther = "";
        }
        finally
        {
            IsSendingBatchOther = false;
        }
    }

    protected async Task SaveBbd()
    {
        if (OnSaveBbdFormat.HasDelegate)
            await OnSaveBbdFormat.InvokeAsync((FillingExpDate, null));
    }

    protected async Task SaveBbdOther()
    {
        if (string.IsNullOrWhiteSpace(FillingExpDateOther)) return;

        IsSendingBbdOther = true;
        try
        {
            if (OnSaveBbdFormat.HasDelegate)
                await OnSaveBbdFormat.InvokeAsync((FillingExpDate, FillingExpDateOther));
            FillingExpDateOther = "";
        }
        finally
        {
            IsSendingBbdOther = false;
        }
    }

    protected async Task OnTakeSampleChanged()
    {
        if (OnPatchRG37.HasDelegate)
        {
            await OnPatchRG37.InvokeAsync(new { Tomar_muestra = TakeSample });
        }
    }

    protected async Task OnNoLabelChanged()
    {
        if (OnPatchRG35.HasDelegate)
        {
            var labelConfig = NoLabel ? "No label" : "Label";
            await OnPatchRG35.InvokeAsync(new { Label_config = labelConfig });
        }
    }

    protected async Task OnNoAnalyticsChanged()
    {
        if (OnPatchRG35.HasDelegate)
        {
            await OnPatchRG35.InvokeAsync(new { No_analitycs = NoAnalytics });
        }
    }

    protected async Task SavePalletComments()
    {
        if (OnPatchRG35.HasDelegate)
        {
            await OnPatchRG35.InvokeAsync(new { Pallet_comments = PalletComments });
        }
    }

    protected async Task DownloadBase64File(string base64, string name)
    {
        await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.downloadBase64File", base64, name);
    }

    protected void HandleConfirm()
    {
        // Lógica para confirmar y firmar
        StateHasChanged();
    }

    protected void HandlePalletLabelUpdated(string newUrl)
    {
        PalletLabelImg = newUrl;
        StateHasChanged();
    }

    protected void HandleBoxLabelUpdated(string newUrl)
    {
        BoxLabelImg = newUrl;
        StateHasChanged();
    }

    protected void HandleLabelOptionsUpdated(object payload)
    {
        // Actualizar opciones de etiqueta
        StateHasChanged();
    }

    protected void HandleDraftLabelUpdated(object payload)
    {
        // Actualizar borrador de etiqueta
        StateHasChanged();
    }

    // Modelos internos
    public class InputItem
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "-";
    }

    public class RecipeRow
    {
        public string Active { get; set; } = "-";
        public string SourceUsed { get; set; } = "-";
        public string QuantityServing { get; set; } = "-";
        public string RdaEu { get; set; } = "-";
    }

    public class FormatOption
    {
        public string Format { get; set; } = "";
    }

    public class AnalyticsRow
    {
        public string Active { get; set; } = "-";
        public string Quantity { get; set; } = "-";
        public string Source { get; set; } = "-";
        public bool Analytics { get; set; }
        public string Periodicity { get; set; } = "-";
        public string Observations { get; set; } = "-";
        public decimal Price { get; set; }
    }

    public class AtributoOption
    {
        public string Value { get; set; } = "";
        public string Display { get; set; } = "";
    }
}