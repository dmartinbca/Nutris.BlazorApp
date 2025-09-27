using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Nutris.BlazorApp.Components.Modals;
using NutrisBlazor.Components.Modals;
using NutrisBlazor.Models;
using NutrisBlazor.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static Nutris.BlazorApp.Components.Modals.BoteCapDataModal;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nutris.BlazorApp.Components.Orders;

public class OrdersComponentBase : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = default!;
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
    protected bool isLabelModalOpen { get; set; }
    protected List<LabelOptionMX> LabelOptionsSize { get; set; } = new();
    protected List<LabelOption> LabelOptionsFinish { get; set; } = new();
    protected List<LabelOption> LabelOptionsMaterial { get; set; } = new();
    protected List<LabelOption> LabelOptionsColor { get; set; } = new();
    protected SelectedLabelOptions selectedLabelOptions { get; set; } = new();
    public string? LabelImageUrl { get; set; } // 
    // Estado de carga
    protected bool IsLoading { get; set; } = false;
    private bool isBoteCapOpen;
    private bool accordionOpen;
    // Propiedades del Header
    protected bool IsProcessingNoAnalytics { get; set; } = false;
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

    protected BoteCapDataModal modalRef;
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
        { 1, false }, { 2, false }, { 3, false }, { 4, false }, { 5, false }
    };
    private bool isDataLoaded = false;
    // FORMULATION
    protected int PercentFilledFormulation { get; set; }
    protected bool CustomerAccepted { get; set; }
    protected bool TakeSample { get; set; }
    protected decimal TakeSamplePrice { get; set; }
    protected string NutrisComments { get; set; } = "";
    protected string Shape { get; set; } = "-";
    protected string GummyShapeImg { get; set; } = "";
    protected string Shape_1 { get; set; } = "-";
    protected string GummyShapeImg_1 { get; set; } = "";
    protected string Shape_2 { get; set; } = "-";
    protected string GummyShapeImg_2 { get; set; } = "";
    protected string Shape_3 { get; set; } = "-";
    protected string GummyShapeImg_3 { get; set; } = "";

    // Features (vegetarian, vegan, etc.)
    protected bool SuitableVegetarians { get; set; }
    protected bool SuitableVegans { get; set; }
    protected bool NaturalColors { get; set; }
    protected bool NaturalFlavor { get; set; }
    protected bool isConfirmModalVisible = false;
    // Listas de Gummy DNA
    protected List<InputItem> GummyListBn { get; set; } = new();
    protected List<InputItem> GummyListB { get; set; } = new();
    protected List<RecipeRow> RecipeRows { get; set; } = new();

    // PACKAGING
    protected int PercentFilledBottle { get; set; }
    protected string BottleType { get; set; } = "-";
    protected string BottleImg { get; set; } = "";
    protected string CapImg { get; set; } = "";
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
    public List<string> capacidades = new();
    public Dictionary<string, List<string>> capacidadToDiametros = new();
    public List<string> materiales = new();

    protected List<BoteCapDataModal.BoteDataItem> boteDataList = new();
    protected List<BoteCapDataModal.CapDataItem> capDataList = new();
    protected List<BoteCapDataModal.ColorOption> boteColorOptions = new();
    protected List<BoteCapDataModal.ColorOption> capColorOptions = new();

    protected BoteCapDataModal.BoteDataItem? selectedBoteOption;
    protected BoteCapDataModal.CapDataItem? selectedCapOption;
    protected string characteristics = "";
    protected bool isLabelOpen;
    private string? _boteResumen;
    private string? _tapaResumen;
    public readonly OptionLookups _opts = new();
    protected bool IsProcessingNoLabel { get; set; } = false;
    protected bool isThankYouModalVisible = false;
    private sealed class LabelOptionsDto
    {
        public string? Label_size { get; set; }
        public string? Label_material { get; set; }
        public string? Label_finish { get; set; }
        public string? Label_Color { get; set; }
    }
    public class LabelOptionsPatch // mismo DTO de arriba
    {
        [JsonPropertyName("Label_size")] public string? LabelSize { get; set; }
        [JsonPropertyName("Label_material")] public string? LabelMaterial { get; set; }
        [JsonPropertyName("Label_finish")] public string? LabelFinish { get; set; }
        [JsonPropertyName("Label_type")] public string? LabelColors { get; set; }
    }
    protected List<NutrisBlazor.Components.Modals.ModalLabel.OptionMX> MapSizeMx(List<AtributoOption> src) =>
    src.Select((x, i) => new NutrisBlazor.Components.Modals.ModalLabel.OptionMX
    {
        ID = i + 1,
        Value = x.Value ?? "",
        Imagen = ""
    }).ToList();

    protected List<NutrisBlazor.Components.Modals.ModalLabel.Option> MapSimple(List<AtributoOption> src) =>
    src.Select((x, i) => new NutrisBlazor.Components.Modals.ModalLabel.Option
    {
        ID = i + 1,
        Value = x.Value ?? ""
    }).ToList();

    protected Task OpenLabelModal() { isLabelModalOpen = true; StateHasChanged(); return Task.CompletedTask; }
    protected Task CloseLabelModal() { isLabelModalOpen = false; StateHasChanged(); return Task.CompletedTask; }
    private async Task OpenLabelModalAsync()
    {
        await ReloadLabelPreviewAsync();
        isLabelOpen = true;
    }
    protected void ShowConfirmModal()
    {
        isConfirmModalVisible = true;
        StateHasChanged();
    }
    protected async Task DownloadImage()
    {
        if (string.IsNullOrEmpty(LabelImageUrl))
        {
            await ShowAlert("No image available to download");
            return;
        }

        try
        {
            // Create download link using JavaScript
            await JSRuntime.InvokeVoidAsync("downloadBase64File",
                LabelImageUrl,
                $"Label-Imagen-{Id}.png",
                "image/png");
        }
        catch (Exception ex)
        {
         
            await ShowAlert("Error downloading image. Please try again.");
        }
    }
    private async Task ShowAlert(string message)
    {
        await JSRuntime.InvokeVoidAsync("alert", message);
    }


    private async Task ReloadLabelPreviewAsync()
    {
        // Si tu API devuelve bytes:
        var bytes = await Http.GetByteArrayAsync(
            $"api/CustomizeRG35('{Code}')/Label?tenant=nutris");
        LabelImageUrl = "data:image/png;base64," + Convert.ToBase64String(bytes);

        // Si tu API devuelve una URL directa, simplemente asigna esa URL:
        // LabelImageUrl = await Http.GetStringAsync(...);
    }
    public async Task OnLabelSave()
    {
        // Aquí no hace nada especial: ya actualizaste en Upload/Delete.
        // Si quieres, fuerza un reload de los datos de la orden.
        await ReloadOrderAsync();
    }
    public Task OnLabelImageChanged(string? newUrl)
    {
        LabelImageUrl = newUrl; // refleja lo que devuelva el modal
        StateHasChanged();
        return Task.CompletedTask;
    }
    protected async Task UploadLabelAsync(IBrowserFile file)
    {
        // Límite razonable (ajusta a tu necesidad)
        const long maxSize = 20 * 1024 * 1024;

        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxSize);
        var sc = new StreamContent(stream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(sc, "file", file.Name);

        // ENDPOINT de subida que ya tengas en OrdersComponent (ajústalo al tuyo real)
        var resp = await Http.PostAsync(
            $"api/CustomizeRG35('{Code}')/Label/Upload?tenant=nutris", content);

        resp.EnsureSuccessStatusCode();
        await ReloadLabelPreviewAsync();   // refresca la miniatura
    }

    protected async Task<Stream> DownloadLabelAsync()
    {
        // ENDPOINT de descarga (ajústalo)
        var resp = await Http.GetAsync(
            $"api/CustomizeRG35('{Code}')/Label/Download?tenant=nutris",
            HttpCompletionOption.ResponseHeadersRead);

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStreamAsync();
    }


    protected async Task DeleteLabelAsync()
    {
        // ENDPOINT de borrado (ajústalo si existe)
        var resp = await Http.DeleteAsync(
            $"api/CustomizeRG35('{Code}')/Label?tenant=nutris");

        resp.EnsureSuccessStatusCode();

        LabelImageUrl = null;
        StateHasChanged();
    }
    
    private async Task ReloadOrderAsync()
    {
        // vuelve a pedir los datos y refresca UI
        // ...
        await InvokeAsync(StateHasChanged);
    }
    private void LoadBoteAndCapData()
    {
        // Convertir RelacionBote a List<BoteDataItem>
        if (RelacionBote.TryGetProperty("value", out var boteArray) &&
            boteArray.ValueKind == JsonValueKind.Array)
        {
            boteDataList = boteArray.EnumerateArray()
                .Select(item => new BoteCapDataModal.BoteDataItem
                {
                    Forma = item.TryGetProperty("Forma", out var f) ? f.GetString() : null,
                    Capacidad = item.TryGetProperty("Capacidad", out var c) ? c.GetString() : null,
                    Diametro = item.TryGetProperty("Diametro", out var d) ? d.GetString() : null,
                    Material = item.TryGetProperty("Material", out var m) ? m.GetString() : null,
                    Color = item.TryGetProperty("Color", out var col) ? col.GetString() : null
                })
                .Where(b => b.Forma != null)
                .ToList();
        }

        // Convertir RelacionTapa a List<CapDataItem>
        if (RelacionTapa.TryGetProperty("value", out var tapaArray) &&
            tapaArray.ValueKind == JsonValueKind.Array)
        {
            capDataList = tapaArray.EnumerateArray()
                .Select(item => new BoteCapDataModal.CapDataItem
                {
                    Forma = item.TryGetProperty("Forma", out var f) ? f.GetString() : null,
                    Diametro = item.TryGetProperty("Diametro", out var d) ? d.GetString() : null,
                    Color = item.TryGetProperty("Color", out var c) ? c.GetString() : null,
                    Sleeve = item.TryGetProperty("Sleeve", out var s) && s.ValueKind == JsonValueKind.True
                })
                .Where(c => c.Forma != null)
                .ToList();
        }
    }

    private void InitializeSelectedOptions()
    {
        if (HasRG35 && RG35 != null)
        {
            selectedBoteOption = new BoteCapDataModal.BoteDataItem
            {
                Forma = RG35.Bote_forma,
                Capacidad = RG35.Bote_capacidad,
                Diametro = RG35.Bote_boca,
                Material = RG35.Bote_material,
                Color = RG35.Bote_color,
                ImagenBote = $"data:image/png;base64,{RG35.Bote_imagen}" 

            };

            selectedCapOption = new BoteCapDataModal.CapDataItem
            {
                Forma = RG35.Cap_tapa,
                Diametro = RG35.Cap_Boca,
                Color = RG35.Cap_color,
                Sleeve = RG35.Cap_sleever ?? false,
                ImagenCap = $"data:image/png;base64,{RG35.Cap_imagen}" 
            };

            characteristics = RG35.Characteristics ?? "";
        }
    }
    public void HandlePackagingUpdated(BoteCapDataModal.UpdatedOptions u)
    {
        // Si quieres reflejar la selección en la pantalla antes del reload:
        selectedBoteOption = new BoteCapDataModal.BoteDataItem
        {
            Forma = u.BoteOption?.BoteForma,
            Capacidad = u.BoteOption?.BoteCapacidad,
            Diametro = u.BoteOption?.BoteBoca,
            Material = u.BoteOption?.BoteMaterial,
            Color = u.BoteOption?.BoteColor
        };

        selectedCapOption = new BoteCapDataModal.CapDataItem
        {
            Forma = u.CapOption?.CapTapa,
            Diametro = u.CapOption?.CapBoca,
            Color = u.CapOption?.CapColor,
            Sleeve = u.CapOption?.CapSleever ?? false
        };

        characteristics = u.Characteristics;

        StateHasChanged();
    }

    protected Task HandleSave(BoteDataItem bote, CapDataItem tapa)
    {
        // lo que necesites al guardar
        return Task.CompletedTask;
    }
    protected async Task HandleSaveData(BoteCapDataModal.BoteDataItem bote,
                                      BoteCapDataModal.CapDataItem cap)
    {
        selectedBoteOption = bote;
        selectedCapOption = cap;

        // TODO: persistir en tu modelo si aplica
        await InvokeAsync(StateHasChanged);
    }
 
    protected Task HandleAccordionOpen((int tabIndex, int stepIndex) indexes)
    {
        // Lógica para manejar el accordion
        Console.WriteLine($"Accordion: Tab {indexes.tabIndex}, Step {indexes.stepIndex}");
        return Task.CompletedTask;
    }
    protected async Task HandleClose()
    {
        isBoteCapOpen = false;
        if (modalRef != null) await modalRef.HideModal();
    }

    protected async Task LoadBoteData()
    {
         
    }

    protected async Task LoadCapData()
    {
         
    }

    protected async Task LoadColorOptions()
    {
         
    }

    protected override async Task OnParametersSetAsync()
    {
        currentLanguage = Localization.CurrentLanguage ?? "es";
         BuildBoteLookups(RelacionBote, out capacidades, out capacidadToDiametros, out materiales);
        LoadBoteAndCapData();  // <-- NUEVA LÍNEA
        InitializeSelectedOptions();
        boteColorOptions = BuildColorOptionsFromRelacion(RelacionBote, isCap: false);
        capColorOptions = BuildColorOptionsFromRelacion(RelacionTapa, isCap: true);
        if (boteColorOptions.Count == 0)
            boteColorOptions = new()
        {
            new() { ID=1, Value="Clear",  ColorHex="#CCCCCC" },
            new() { ID=2, Value="Amber",  ColorHex="#FFBF00" },
            new() { ID=3, Value="Black",  ColorHex="#000000" },
            new() { ID=4, Value="White",  ColorHex="#FFFFFF" },
            new() { ID=5, Value="Blue",   ColorHex="#0D6EFD" },
        };
        if (capColorOptions.Count == 0)
            capColorOptions = new()
        {
            new() { ID=1, Value="White",  ColorHex="#FFFFFF" },
            new() { ID=2, Value="Black",  ColorHex="#000000" },
            new() { ID=3, Value="Gold",   ColorHex="#D4AF37" },
            new() { ID=4, Value="Silver", ColorHex="#C0C0C0" },
        };
        // Suscribirse a cambios de idioma
        Localization.OnLanguageChanged += OnLanguageChanged;
        await LoadDataAsync();
    }
    private  void BuildBoteLookups(
    JsonElement relacionBote,
    out List<string> caps,
    out Dictionary<string, List<string>> capToDia,
    out List<string> mats)
    {
        caps = new(); mats = new(); capToDia = new();

        if (!relacionBote.TryGetProperty("value", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return;

        var capSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var matSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var it in arr.EnumerateArray())
        {
            if (TryGetString(it, "Capacidad", out var cap) && !string.IsNullOrWhiteSpace(cap))
            {
                capSet.Add(cap);

                if (!map.TryGetValue(cap, out var dias))
                {
                    dias = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    map[cap] = dias;
                }

                if (TryGetString(it, "Diametro", out var dia) && !string.IsNullOrWhiteSpace(dia))
                    dias.Add(dia);
            }

            if (TryGetString(it, "Material", out var mat) && !string.IsNullOrWhiteSpace(mat))
                matSet.Add(mat);
        }

        // Ordena capacidades por número si es posible
        caps = capSet
            .OrderBy(c => int.TryParse(c, out var n) ? n : int.MaxValue)
            .ThenBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();

        mats = matSet.OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList();

        capToDia = map.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToList(),
            StringComparer.OrdinalIgnoreCase);

        if (Atributos.ValueKind == JsonValueKind.Object)
            BuildOptionLookupsFromAtributos(Atributos);

    }
    private void BuildOptionLookupsFromAtributos(JsonElement atributos)
    {
        try
        {
            if (!atributos.TryGetProperty("value", out var valueArr) || valueArr.ValueKind != JsonValueKind.Array)
                return;

            // Helpers
            List<string> ListFromIndex(int idx)
            {
                if (idx < 0 || idx >= valueArr.GetArrayLength()) return new();
                var arr = valueArr[idx].GetProperty("valoresAtributos");
                return arr.EnumerateArray()
                          .Select(v => v.TryGetProperty("Value", out var ve) ? ve.GetString() : null)
                          .Where(s => !string.IsNullOrWhiteSpace(s))
                          .Select(s => s!.Trim())
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .ToList();
            }

            List<BoteCapDataModal.ColorOption> ColorListFromIndex(int idx)
            {
                var list = new List<BoteCapDataModal.ColorOption>();
                if (idx < 0 || idx >= valueArr.GetArrayLength()) return list;

                var arr = valueArr[idx].GetProperty("valoresAtributos");
                int id = 1;
                foreach (var v in arr.EnumerateArray())
                {
                    var label = v.TryGetProperty("Value", out var ve) ? ve.GetString() ?? "" : "";
                    if (string.IsNullOrWhiteSpace(label)) continue;
                    var hex = v.TryGetProperty("Color_HEX", out var he) ? (he.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(hex)) hex = ColorToHexFallback(label);

                    // Imagenes
                    var Round = v.TryGetProperty("Round", out var ro) ? (ro.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Round)) Round = "";

                    var Square = v.TryGetProperty("Square", out var sq) ? (sq.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Square)) Square = "";

                    var Cylindrical = v.TryGetProperty("Cylindrical", out var cy) ? (cy.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Cylindrical)) Cylindrical = "";

                    var Simple = v.TryGetProperty("Simple", out var si) ? (si.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Simple)) Simple = "";

                    var Metal = v.TryGetProperty("Metal", out var me) ? (me.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Metal)) Metal = "";

                    var Childproof = v.TryGetProperty("Childproof", out var ch) ? (ch.GetString() ?? "") : "";
                    if (string.IsNullOrWhiteSpace(Childproof)) Childproof = "";

                    list.Add(new BoteCapDataModal.ColorOption
                    {
                        ID = id++,
                        Value = label.Trim(),
                        ColorHex = hex.Trim(),
                        Round = $"data:image/png;base64,{Round}" ,
                        Square = $"data:image/png;base64,{Square}",
                        Cylindrical = $"data:image/png;base64,{Cylindrical}",
                        Simple = $"data:image/png;base64,{Simple}",
                        Metal = $"data:image/png;base64,{Metal}",
                        Childproof = $"data:image/png;base64,{Childproof}"

                    });
                }

                // distinct por Value
                return list.GroupBy(c => c.Value, StringComparer.OrdinalIgnoreCase)
                           .Select(g => g.First())
                           .ToList();
            }

            // Índices iguales a tu VUE (fetchOptions)
            //   optionsSize            = value[23]
            //   optionsSizeLabel       = value[16]  (no lo usamos aquí)
            //   optionsDiameter        = value[5]
            //   optionsMaterial        = value[21]
            //   optionsBoca / DThread  = value[22]  (si lo necesitas, úsalo igual)
            //   optionsForma (bote)    = value[24]
            //   optionsFinish          = value[8]   (no lo usamos aquí)
            //   optionsLabelMaterial   = value[9]   (no lo usamos aquí)
            //   optionsColorLabel      = value[10]  (no lo usamos aquí)
            //   optionsColorBote       = value[20]  + Color_HEX
            //   optionsShapecover      = value[19]  (formas tapa)
            //   optionsColorcover      = value[18]

            _opts.Capacidades = ListFromIndex(23);
            _opts.Diametros = ListFromIndex(5);
            _opts.Materiales = ListFromIndex(21);
            _opts.FormasBote = ListFromIndex(24);     // por si quieres usarlas
            _opts.Bocas = ListFromIndex(22);     // idem
            _opts.FormasTapa = ListFromIndex(19);
         
            _opts.ColorBote = ColorListFromIndex(20);
            _opts.ColorCover = ColorListFromIndex(18);
        }
        catch
        {
            // Silencioso: si algo falla dejamos listas vacías y el modal usa sus defaults.
        }
    }
    private static string ColorToHexFallback(string name)
    {
        var n = (name ?? "").Trim().ToLowerInvariant();
        return n switch
        {
            "clear" => "#CCCCCC",
            "white" => "#FFFFFF",
            "black" => "#000000",
            "amber" => "#FFBF00",
            "red" => "#FF0000",
            "orange" => "#FFA500",
            "blue" or "light blue" => "#0D6EFD",
            "dark blue" => "#00008B",
            "green" or "light green" => "#90EE90",
            "emerald green" => "#50C878",
            "pale green" => "#98FB98",
            "turquoise" => "#40E0D0",
            "purple" or "violet" or "light purple" => "#800080",
            "gold" => "#D4AF37",
            "silver" => "#C0C0C0",
            _ => "#CCCCCC"
        };
    }
    public sealed class OptionLookups
    {
        public List<string> Capacidades { get; set; } = new();
        public List<string> Diametros { get; set; } = new();
        public List<string> Materiales { get; set; } = new();

        public List<string> FormasBote { get; set; } = new();
        public List<string> FormasTapa { get; set; } = new();
        public List<string> Bocas { get; set; } = new();

        public List<BoteCapDataModal.ColorOption> ColorBote { get; set; } = new();
        public List<BoteCapDataModal.ColorOption> ColorCover { get; set; } = new();
    }
    private static bool TryGetString(JsonElement obj, string prop, out string value)
    {
        value = "";
        if (obj.TryGetProperty(prop, out var el))
        {
            value = el.ValueKind == JsonValueKind.String ? (el.GetString() ?? "") : el.ToString();
            return true;
        }
        return false;
    }
    public void OnLanguageChanged()
    {
        currentLanguage = Localization.CurrentLanguage;
        InvokeAsync(StateHasChanged);
    }
    private static List<BoteCapDataModal.ColorOption> BuildColorOptionsFromRelacion(JsonElement relacion, bool isCap)
    {
        var result = new List<BoteCapDataModal.ColorOption>();
        if (relacion.ValueKind != JsonValueKind.Object) return result;
        if (!relacion.TryGetProperty("value", out var arr) || arr.ValueKind != JsonValueKind.Array) return result;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int id = 1;

        foreach (var item in arr.EnumerateArray())
        {
            if (!item.TryGetProperty("Color", out var colEl) || colEl.ValueKind != JsonValueKind.String) continue;
            var raw = colEl.GetString() ?? "";
            var norm = NormalizeColorName(raw);   // p.ej. "yellow (O)" -> "yellow"; "Purple/Violet" -> "purple"

            // dedupe por nombre normalizado
            if (!seen.Add(norm)) continue;

            var hex = ColorToHex(norm, isCap ? "#FFFFFF" : "#CCCCCC"); // fallback diferente para tapa/bote
            result.Add(new BoteCapDataModal.ColorOption
            {
                ID = id++,
                Value = ToTitle(raw),   // mostramos el original bonito (con mayúsculas)
                ColorHex = hex
            });
        }

        // orden alfabético por etiqueta
        result = result.OrderBy(o => o.Value, StringComparer.CurrentCultureIgnoreCase).ToList();
        // re-asigna IDs consecutivos tras ordenar
        for (int i = 0; i < result.Count; i++) result[i].ID = i + 1;

        return result;
    }
    private static string NormalizeColorName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim();

        // quita paréntesis: "white (O)" -> "white"
        s = Regex.Replace(s, @"\s*\([^)]*\)\s*", "", RegexOptions.CultureInvariant);

        // normaliza separadores y espacios
        s = s.Replace(" / ", "/").Replace("  ", " ");
        s = Regex.Replace(s, @"\s+", " ");

        // normaliza variantes habituales
        s = s.Replace("LightBlue", "Light Blue", StringComparison.OrdinalIgnoreCase)
             .Replace("LightGreen", "Light Green", StringComparison.OrdinalIgnoreCase)
             .Replace("Purple/Violet", "Purple", StringComparison.OrdinalIgnoreCase);

        return s.Trim().ToLowerInvariant();
    }
    private static string ColorToHex(string name, string fallback)
    {
        // mapa de colores que aparecen en tus JSON (puedes ampliar cuando quieras)
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["clear"] = "#CCCCCC",
            ["white"] = "#FFFFFF",
            ["black"] = "#000000",
            ["amber"] = "#FFBF00",
            ["blue"] = "#0D6EFD",
            ["light blue"] = "#ADD8E6",
            ["dark blue"] = "#00008B",
            ["harbor blue"] = "#1A4876",
            ["blue stress"] = "#1E90FF",
            ["blue mens"] = "#1E90FF",
            ["green"] = "#008000",
            ["light green"] = "#90EE90",
            ["emerald green"] = "#50C878",
            ["pale green"] = "#98FB98",
            ["turquoise"] = "#40E0D0",
            ["red"] = "#FF0000",
            ["salmon"] = "#FA8072",
            ["orange"] = "#FFA500",
            ["pastel orange"] = "#FFD8B1",
            ["pink"] = "#FFC0CB",
            ["pastel pink"] = "#FFD1DC",
            ["hot pink"] = "#FF69B4",
            ["purple"] = "#800080",
            ["violet"] = "#8A2BE2",
            ["light purple"] = "#D8BFD8",
            ["lila"] = "#C8A2C8",
            ["magenta"] = "#FF00FF",
            ["anthracite"] = "#30363D",
            ["gold"] = "#D4AF37",
            ["silver"] = "#C0C0C0",
            ["yellow"] = "#FFFF00"
        };

        if (map.TryGetValue(name, out var hex)) return hex;

        // si venía "purple/violet", "orange/..." etc → prueba por partes
        if (name.Contains('/'))
            foreach (var part in name.Split('/'))
                if (map.TryGetValue(part.Trim(), out hex)) return hex;

        return fallback;
    }
    private static string ToTitle(string s)
    {
        s = s?.Trim() ?? "";
        if (string.IsNullOrEmpty(s)) return s;
        // deja tal cual si ya viene capitalizado con espacios; solo corrige todo minúscula
        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
    public void ToggleLanguageMenu()
    {
        showLanguageMenu = !showLanguageMenu;
    }
    public async void GoBack()
    {
        await JS.InvokeVoidAsync("history.back");
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
      //  await JSRuntime.InvokeVoidAsync("cleanupModals");
    }
    protected async Task OpenBoteCapModal()
    {
        isBoteCapOpen = true;
        if (modalRef is not null)
        {
            await modalRef.ShowModal();
        }
    }
    protected Task OnSetAccordionOpen(bool open)
    {
        accordionOpen = open;
        StateHasChanged();
        return Task.CompletedTask;
    }
    protected async Task CloseBoteCapModal()
    {
        isBoteCapOpen = false;
        if (modalRef != null)
            await modalRef.HideModal();
    }
    protected Task OnBoteCapClosed()
{
    isBoteCapOpen = false;
    StateHasChanged();
    return Task.CompletedTask;
}
    protected Task OnBoteCapSaved()
    {
        // lo que necesites tras guardar
        return Task.CompletedTask;
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
            Shape_1 = form.Shape_2 ?? "-";
            Shape_2 = form.Shape_3 ?? "-";
            Shape_3 = form.Shape_4 ?? "-";

            if (!string.IsNullOrEmpty(form.Imagen))
                GummyShapeImg = $"data:image/png;base64,{form.Imagen}";
            if (!string.IsNullOrEmpty(form.Imagen_2))
                GummyShapeImg_1 = $"data:image/png;base64,{form.Imagen_2}";
            if (!string.IsNullOrEmpty(form.Imagen_3))
                GummyShapeImg_2 = $"data:image/png;base64,{form.Imagen_3}";
            if (!string.IsNullOrEmpty(form.Imagen_4))
                GummyShapeImg_3 = $"data:image/png;base64,{form.Imagen_4}";

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

        if (!string.IsNullOrEmpty(data.Cap_imagen))
            CapImg = $"data:image/png;base64,{data.Cap_imagen}";

        BottleInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.BottleA[0]"], Value = data.Characteristics ?? "-" },
            new() { Label = Localization["orderView.BottleA[1]"], Value = data.Bote_boca ?? "-" },
            new() { Label = Localization["orderView.BottleA[2]"], Value = FormatNumericValue(data.Pieces_per_container) }
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
            new() { Label = Localization["orderView.GUMMYDNAL[3]"], Value = data.Label_type ?? "-" },
            new() { Label = Localization["orderView.GUMMYDNAL[1]"], Value = data.Label_material ?? "-" },
            new() { Label = Localization["orderView.GUMMYDNAL[2]"], Value = data.Label_finish ?? "-" },
         
        };

        // Palletizing
        if (!string.IsNullOrEmpty(data.Box_label_imagen))
            BoxLabelImg = $"data:image/png;base64,{data.Box_label_imagen}";
        if (!string.IsNullOrEmpty(data.Pallet_label_imagen))
            PalletLabelImg = $"data:image/png;base64,{data.Pallet_label_imagen}";

        PalletComments = data.Pallet_comments ?? "";
        if(data.Box_label_config=="Standard")
        {
            PalletInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.Pallet[5]"], Value = data.Box_label_config ?? "-" },
            new() { Label = Localization["orderView.Pallet[0]"], Value = data.Box_name ?? "-" },
            new() { Label = Localization["orderView.Pallet[1]"], Value = FormatNumericValue(data.Box_units_per) },


        };
        }
        else
        {
            PalletInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.Pallet[5]"], Value = data.Box_label_config ?? "-" },
          

           
        };
        }
      

        PalletizingInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[0]"], Value = data.Pallet_type ?? "-" },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[1]"], Value = FormatNumericValue(data.Pallet_layers) },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[2]"], Value =  FormatNumericValue(data.Pallet_boxes_per_layer ) },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[3]"], Value = FormatNumericValue(data.Pallet_boxes_per_pallet)}
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
        characteristics = data.Characteristics ?? "";
        selectedLabelOptions = new SelectedLabelOptions
        {
            LabelSize = data.Label_size ?? "",
            LabelMaterial = data.Label_material ?? "",
            LabelFinish = data.Label_finish ?? "",
            LabelColors = data.Label_color ?? ""
        };

        // Imagen final de etiqueta (si existe)
        if (!string.IsNullOrEmpty(data.Label_imagen))
            LabelImageUrl = $"data:image/png;base64,{data.Label_imagen}";
        else
            LabelImageUrl = null;


    }
    private static List<LabelOption> MapAttrToLabelOption(JsonElement valueArr, int idx)
    {
        var list = new List<LabelOption>();
        if (idx < 0 || idx >= valueArr.GetArrayLength()) return list;

        int id = 1;
        foreach (var v in valueArr[idx].GetProperty("valoresAtributos").EnumerateArray())
        {
            var val = v.TryGetProperty("Value", out var ve) ? ve.GetString() ?? "" : "";
            if (string.IsNullOrWhiteSpace(val)) continue;
            list.Add(new LabelOption { ID = id++, Value = val.Trim() });
        }
        return list;
    }

    private static List<LabelOptionMX> MapAttrToLabelOptionMX(JsonElement valueArr, int idx)
    {
        var list = new List<LabelOptionMX>();
        if (idx < 0 || idx >= valueArr.GetArrayLength()) return list;

        int id = 1;
        foreach (var v in valueArr[idx].GetProperty("valoresAtributos").EnumerateArray())
        {
            var val = v.TryGetProperty("Value", out var ve) ? ve.GetString() ?? "" : "";
            var img = v.TryGetProperty("Imagen", out var im) ? (im.GetString() ?? "") : "";
            if (string.IsNullOrWhiteSpace(val)) continue;
            list.Add(new LabelOptionMX { ID = id++, Value = val.Trim(), Imagen = img });
        }
        return list;
    }
    private string FormatNumericValue(int? value)
    {
        return value == null || value == 0 ? "-" : value.ToString();
    }
    private string FormatNumericValue(decimal? value)
    {
        return value == null || value == 0 ? "-" : value.ToString();
    }
    private string FormatNumericValue(string? value)
    {
        return value == null || value == "0" ? "-" : value.ToString();
    }
    private void BuildLabelCatalogsFromAtributos(JsonElement atributos)
    {
        if (!atributos.TryGetProperty("value", out var valueArr) || valueArr.ValueKind != JsonValueKind.Array)
            return;

        // Índices (los mismos que usabas en tu VUE):
        // optionsSizeLabel     -> value[16]  (usa MX)
        // optionsFinish        -> value[8]
        // optionsLabelMaterial -> value[9]
        // optionsColorLabel    -> value[10]

        LabelOptionsSize = MapAttrToLabelOptionMX(valueArr, 16);
        LabelOptionsFinish = MapAttrToLabelOption(valueArr, 8);
        LabelOptionsMaterial = MapAttrToLabelOption(valueArr, 9);
        LabelOptionsColor = MapAttrToLabelOption(valueArr, 7);
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
            new() { Label = Localization["orderView.ListInputB[2]"], Value = FormatNumericValue(data.Size) },
            new() { Label = Localization["orderView.ListInputB[3]"], Value = FormatNumericValue(data.Serving) }
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
            // ---- Lote / BBD (ya lo tenías) ----
            if (LotFormat.ValueKind == JsonValueKind.Object && LotFormat.TryGetProperty("value", out var lotValues))
                BatchFormats = lotValues.EnumerateArray().Select(i => new FormatOption { Format = i.GetProperty("Format").GetString() ?? "" }).ToList();

            if (BbdFormat.ValueKind == JsonValueKind.Object && BbdFormat.TryGetProperty("value", out var bbdValues))
                BbdFormats = bbdValues.EnumerateArray().Select(i => new FormatOption { Format = i.GetProperty("Format").GetString() ?? "" }).ToList();

            // ---- Atributos → opciones del modal ----
            if (!(Atributos.ValueKind == JsonValueKind.Object && Atributos.TryGetProperty("value", out var attrValues)))
                return;

            var arr = attrValues.EnumerateArray().ToList();
            Console.WriteLine(arr);
            List<AtributoOption> FromIndex(int idx)
            {
                if (idx < 0 || idx >= arr.Count) return new();
                if (!arr[idx].TryGetProperty("valoresAtributos", out var vals)) return new();
                return vals.EnumerateArray()
                           .Select(v => new AtributoOption
                           {
                               Value = v.TryGetProperty("Value", out var ve) ? (ve.GetString() ?? "") : "",
                               Display = v.TryGetProperty("Value", out var ve2) ? (ve2.GetString() ?? "") : ""
                           })
                           .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                           .ToList();
            }

            // Índices iguales al proyecto Vue:
            //   SizeLabel = [16],  Finish = [8],  LabelMaterial = [9],  ColorLabel = [10]
            OptionsSizeLabel = FromIndex(16);
            OptionsFinish = FromIndex(8);
            OptionsLabelMaterial = FromIndex(9);
            OptionsColorLabel = FromIndex(7);
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
        if(RG35.Box_label_config=="Standard")
        {
            var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-");
            PercentFilledPalettizing = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
        }
        else
        {
            var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-" && RG35.Box_label_imagen!="");
            PercentFilledPalettizing = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
        }
       
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

    private string GetConfirmButtonClass()
    {
        // Si el botón puede ser clickeado (está habilitado)
        if (CanConfirmAndSign)
        {
            return "btn-save-confirm";
        }
        // Si el botón está deshabilitado
        else
        {
            return "btn-save-confirm"; // Usamos la misma clase, el :disabled se encargará del estilo
        }
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
        IsProcessingNoLabel = true;
        StateHasChanged(); // Actualizar UI inmediatamente para deshabilitar el checkbox

        try
        {
            if (OnPatchRG35.HasDelegate)
            {
                var labelConfig = NoLabel ? "No label" : "Label";
                await OnPatchRG35.InvokeAsync(new { Label_config = labelConfig });
            }

            // Recalcular porcentajes después del cambio
            CalculateLabelPercentage();
        }
        finally
        {
            IsProcessingNoLabel = false;
            StateHasChanged(); // Volver a habilitar el checkbox
        }
    }

    protected async Task OnNoAnalyticsChanged()
    {
        IsProcessingNoAnalytics = true;
        StateHasChanged();

        try
        {
            if (OnPatchRG35.HasDelegate)
            {
                await OnPatchRG35.InvokeAsync(new { No_analitycs = NoAnalytics });
            }

            CalculateAnalyticsPercentage();
        }
        finally
        {
            IsProcessingNoAnalytics = false;
            StateHasChanged();
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
    protected async Task DownloadDataSheetAsync()
    {
        try
        {
            // 1) payload como en Vue
            var payload = new { numeroRG = Code };

            // 2) POST al mismo endpoint (con tu prefijo api/)
            var resp = await Api.PostAsync (
                "dataSheet(1)/Microsoft.NAV.Download",
                payload
            );

            resp.EnsureSuccessStatusCode();

            // 3) Leer y extraer base64 (acepta dos formatos)
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            string? b64 = null;
            var root = doc.RootElement;

            if (root.TryGetProperty("data", out var dataEl) &&
                dataEl.ValueKind == JsonValueKind.Object &&
                dataEl.TryGetProperty("value", out var valueEl1))
            {
                b64 = valueEl1.GetString();
            }
            else if (root.TryGetProperty("value", out var valueEl2))
            {
                b64 = valueEl2.GetString();
            }

            if (string.IsNullOrWhiteSpace(b64))
                return;

            // 4) Normalizar: quitar prefijo si viene y espacios/nuevas líneas
            const string prefix = "data:application/pdf;base64,";
            if (b64.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                b64 = b64.Substring(prefix.Length);

            b64 = Regex.Replace(b64, @"\s+", ""); // sin espacios/saltos

            // 5) Descargar (reutilizamos tu helper de JS que ya usas en ReportFiles)
            //    => Le pasamos con prefijo data: para que fuerce "download".
            await DownloadBase64File($"{prefix}{b64}", $"DataSheet-{Code}.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading datasheet: {ex.Message}");
            // Si quieres, muestra un toast/alert aquí.
        }
    }
    protected async Task HandleConfirm()
    {
        // Cerrar el modal de confirmación
        isConfirmModalVisible = false;
        StateHasChanged();

        // Esperar un momento para que se cierre el modal de confirmación
        await Task.Delay(300);

        // Mostrar el modal de agradecimiento
        isThankYouModalVisible = true;
        StateHasChanged();
    }

    protected async Task HandlePalletLabelUpdated(dynamic response)
    {
        try
        {
            if (response != null)
            {
                // Si el response tiene la imagen en base64
                string? base64Image = null;

                // Intentar obtener la imagen del response
                if (response is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey("Pallet_label_imagen"))
                    {
                        base64Image = dict["Pallet_label_imagen"]?.ToString();
                    }
                }
                else
                {
                    // Intentar deserializar si es un objeto anónimo
                    var json = JsonSerializer.Serialize(response);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (data?.ContainsKey("Pallet_label_imagen") == true)
                    {
                        base64Image = data["Pallet_label_imagen"]?.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(base64Image))
                {
                    // Actualizar la imagen del pallet
                    PalletLabelImg = $"data:image/png;base64,{base64Image}";

                    // Actualizar la lista de palletizing info si es necesario
                    if (PalletizingInfo != null && PalletizingInfo.Count > 0)
                    {
                        // Buscar el item de Pallet Label y actualizarlo
                        var palletLabelItem = PalletizingInfo.FirstOrDefault(p => p.Label == Localization["orderView.PalletLabel"]);
                        if (palletLabelItem != null)
                        {
                            palletLabelItem.Value = "Uploaded";
                        }
                    }

                    // Recalcular porcentajes
                    CalculatePalletizingPercentage();

                    // Forzar actualización de la UI
                    await InvokeAsync(StateHasChanged);

                    Console.WriteLine("Pallet label image updated successfully in UI");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Error handling pallet label update");
        }
    }
    protected async Task HandleBoxLabelUpdated(dynamic response)
    {
        try
        {
            if (response != null)
            {
                string? base64Image = null;

                if (response is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey("Box_label_imagen"))
                    {
                        base64Image = dict["Box_label_imagen"]?.ToString();
                    }
                }
                else
                {
                    var json = JsonSerializer.Serialize(response);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (data?.ContainsKey("Box_label_imagen") == true)
                    {
                        base64Image = data["Box_label_imagen"]?.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(base64Image))
                {
                    BoxLabelImg = $"data:image/png;base64,{base64Image}";
                    CalculatePalletizingPercentage();
                    await InvokeAsync(StateHasChanged);
                    Console.WriteLine("Box label image updated successfully");
                }
                CalculateAllPercentages();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Error handling box label update");
        }
    }
    protected async Task HandleLabelOptionsUpdated(ModalLabel.LabelOptionsUpdatedEventArgs e)
    {
        // Si quieres, puedes parsear e.Response o e.SentData para refrescar UI local
        // Por simplicidad, recarga porcentajes y cierra el modal
        if (e.SentData is not null)
        {

            
            var json = JsonSerializer.Serialize(e.SentData);
            var opts = JsonSerializer.Deserialize<LabelOptionsPatch>(json);
            if (opts is null) return;

            LabelInfo = new List<InputItem>
            {
                new() { Label = Localization["orderView.GUMMYDNAL[0]"], Value = opts.LabelSize     ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[3]"], Value =opts.LabelColors ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[1]"], Value = opts.LabelMaterial   ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[2]"], Value = opts.LabelFinish   ?? "-" },

              
            
            };

        }

        CalculateLabelPercentage();
        await InvokeAsync(StateHasChanged);
        isLabelModalOpen = false;
    }
    protected async Task HandleDraftLabelUpdated(ModalLabel.DraftLabelUpdatedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Base64))
        {
            // construimos un data URL y lo mostramos en la UI
            LabelImageUrl = e.DataUrl;   // p.ej. "data:image/png;base64,...."
            CalculateLabelPercentage();  // si tu porcentaje depende de tener imagen
            await InvokeAsync(StateHasChanged);
        }
    }



    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Simular carga de datos
            await Task.Delay(100); // Simular llamada API

            // Inicializar con datos de ejemplo
            //boteDataList = new List<BoteCapDataModal.BoteDataItem>
            //{
            //    new() { Forma = "ROUND", Capacidad = "150", Diametro = "D45", Material = "PET", Color = "Clear" },
            //    new() { Forma = "SQUARE", Capacidad = "200", Diametro = "D45", Material = "PET", Color = "Amber" },
            //    // Agregar más datos según necesites
            //};

            //capDataList = new List<BoteCapDataModal.CapDataItem>
            //{
            //    new() { Forma = "Simple", Diametro = "D45", Color = "White" },
            //    new() { Forma = "Childproof", Diametro = "D45", Color = "Black" },
            //    // Agregar más datos según necesites
            //};

            //boteColorOptions = new List<BoteCapDataModal.ColorOption>
            //{
            //    new() { ID = 1, Value = "Clear", ColorHex = "#FFFFFF" },
            //    new() { ID = 2, Value = "Amber", ColorHex = "#FFBF00" },
            //    // Agregar más colores
            //};

            //capColorOptions = new List<BoteCapDataModal.ColorOption>
            //{
            //    new() { ID = 1, Value = "White", ColorHex = "#FFFFFF" },
            //    new() { ID = 2, Value = "Black", ColorHex = "#000000" },
            //    // Agregar más colores
            //};

            isDataLoaded = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
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