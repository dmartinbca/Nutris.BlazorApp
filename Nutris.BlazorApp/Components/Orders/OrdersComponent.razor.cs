// OrdersComponent.razor.cs - VERSIÓN CORREGIDA COMPLETA - PARTE 1

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Nutris.BlazorApp.Components.Modals;
using NutrisBlazor.Components.Modals;
using NutrisBlazor.Models;
using NutrisBlazor.Services;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static Nutris.BlazorApp.Components.Modals.BoteCapDataModal;

namespace Nutris.BlazorApp.Components.Orders;

public class OrdersComponentBase : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] public ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] protected IApiService Api { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected ILocalizationService Localization { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;

    // Parámetros desde el padre
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
    [Parameter] public JsonElement TiposCajas { get; set; }

    // Callbacks
    [Parameter] public EventCallback OnApprove { get; set; }
    [Parameter] public EventCallback<string> OnSaveName { get; set; }
    [Parameter] public EventCallback<(string format, string? other)> OnSaveLotFormat { get; set; }
    [Parameter] public EventCallback<(string format, string? other)> OnSaveBbdFormat { get; set; }
    [Parameter] public EventCallback<object> OnPatchRG35 { get; set; }
    [Parameter] public EventCallback<object> OnPatchRG37 { get; set; }
    [Parameter] public EventCallback<object> OnUploadBoxOrPallet { get; set; }

    // ===== CACHÉ OPTIMIZADA (sin hashing costoso) =====
    private string? _lastProcessedId;
    private bool _catalogsProcessed = false;

    // Caché de datos procesados
    private List<BoteCapDataModal.BoteDataItem>? _cachedBoteData;
    private List<BoteCapDataModal.CapDataItem>? _cachedCapData;
    private List<BoteCapDataModal.ColorOption>? _cachedBoteColors;
    private List<BoteCapDataModal.ColorOption>? _cachedCapColors;
    private List<string>? _cachedCapacidades;
    private Dictionary<string, List<string>>? _cachedCapacidadToDiametros;
    private List<string>? _cachedMateriales;

    // Estado
    protected bool IsLoading { get; set; } = false;
    protected bool IsProcessingNoAnalytics { get; set; } = false;
    protected bool IsProcessingNoLabel { get; set; } = false;
    protected bool isConfirmModalVisible = false;
    protected bool isThankYouModalVisible = false;
    protected bool isLabelModalOpen { get; set; }
    private bool isBoteCapOpen;
    private bool accordionOpen;
    protected bool showLanguageMenu = false;

    // Header
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
    public string Logo { get; set; } = string.Empty;
    public string currentLanguage = "es";

    // Archivos
    protected List<FileItem> ReportFiles { get; set; } = new();
    protected bool ShowReports { get; set; }

    // Secciones
    protected Dictionary<int, bool> IsOpen { get; set; } = new()
    {
        { 1, false }, { 2, false }, { 3, false }, { 4, false }, { 5, false }
    };

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
    protected bool SuitableVegetarians { get; set; }
    protected bool SuitableVegans { get; set; }
    protected bool NaturalColors { get; set; }
    protected bool NaturalFlavor { get; set; }
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
    protected List<TipoCajaOption> TiposCajaOptions { get; set; } = new();
    protected string SelectedTipoCaja { get; set; } = "-";
    protected bool IsLoadingTipoCaja { get; set; } = false;

    // LABEL
    protected int PercentFilledLabel { get; set; }
    protected bool NoLabel { get; set; }
    protected List<InputItem> LabelInfo { get; set; } = new();
    public string? LabelImageUrl { get; set; }
    protected List<LabelOptionMX> LabelOptionsSize { get; set; } = new();
    protected List<LabelOption> LabelOptionsFinish { get; set; } = new();
    protected List<LabelOption> LabelOptionsMaterial { get; set; } = new();
    protected List<LabelOption> LabelOptionsColor { get; set; } = new();
    protected SelectedLabelOptions selectedLabelOptions { get; set; } = new();

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

    // Opciones
    protected List<AtributoOption> OptionsSize { get; set; } = new();
    protected List<AtributoOption> OptionsSizeLabel { get; set; } = new();
    protected List<AtributoOption> OptionsFinish { get; set; } = new();
    protected List<AtributoOption> OptionsLabelMaterial { get; set; } = new();
    protected List<AtributoOption> OptionsColorLabel { get; set; } = new();
    protected List<AtributoOption> OptionsColorBote { get; set; } = new();

    // Bote y Cap
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
    protected BoteCapDataModal modalRef;
    public readonly OptionLookups _opts = new();

    private const string NO_IMAGE_AVAILABLE = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMgAAADICAYAAACtWK6eAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAZJSURBVHgB7d3dcdswEAXQq5n8ZzpIB04F6sCpIKkgqSCpIK4gqSCuIKkgqSCuIKkAb8gZjmyJokiCBPB9Z3Y8tmRLFnkFLBYLAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgP9qrV+I6JZMaa2/6vW1GcHx1BqVUrb+3imlvpERB1FKfSWib0S0YGTMQdy/Nm6JaMJo3M6rICJaSI9qIYRer/MEQRAROLg2C0bhIPa7JAiiiQhCRJTG4yDqICJkiWAwj4OoY4gQIggGc0HMgwiCwVwQU99LEGsIRoMsVrvJWgg/hHa/CTBKj0HESZCjE2S6EVqeJAgSBINJG0Rc8N5aGLRBYIDkQayfQizJ0H+FEeJGTRxEhWz9HcL2QZIg0eDOPYkMJYKgD2mzWF5JzWIBQhMnQYhonRGx7oFBxEmQKRG9MBqWOEFm+3pZDOMb4mGy78HrNEb4VnrxSJgY/z7bz5SINp1eKyUgCCJOcIa0DKLGy10WqyJOqZIEQXDoJvdqMBW2kxCc7L8rlOXa4GAqPGchwhJc2Rf2E4k5C5xzEJm6J4gQCOcgKtGQPCJBJHJJECkQISLgQIJIgQiBcFmsjjhsOHQQ9b2VfiDzUm6HJCqGCCGdK0VIY+AgCpLOlRJBMJSLINYQPLRGEHziLIJYQ/DQGkH6xkH8OM6H1gjSN9c7rrDnQDlIQSUhQhCZmCcIEQQpOJkfwyRI31CEcgezBCEiQg7rK84FCnc6OYYQQfAx5utjCBGkb3qkO5e2xrx8CSFI1zhIdz7nCyuWiOYIglN8XD8vhTCJHT/YGNJbCJH8rHHNgqiQDv+tNYKkRBOxlUJknUJE8f0XIkg2WooM3USWo+hziSg/mEKkRObiAKn7Zy9ER0sRQiQBU2h0FQStOSFEJrEFQbIg8RQiHYQgHJiqECKQo8iMCCE6MoUIlCiykDJACBGAKUTg48F8aFdCiIUeHswIIZwwhOgA6JEOJqJ1w6wdgjQPtI4IIejGnz0aSr/YROVAq9Zaa/0kfQnIiZA/tD5DwmeSMhRE5D8OoqRJEPJMlvRPKUQgEz1AHEQJkyAi8nqOxfxMSULJhvRnCQeRnO1rlrDPGGOiOEhfJJxzqPYaJxICqRAiEBRBkpAggiBFIKNBCCJVCvFsEkGkSiFYFQdRJYJglCwBQaRINfGG7rWJQkkikH9BCiJJAsUIIilJ+kMQqRTpBxYOokqCYJQ0QRAkzsSJBAjiT6LMJ4KI5aJULgQVxJ9gQ/dwJKgcMdAgCkGk2uT+G34uy75AEJ9pJG1f7C9B1eEFhJEyGkbfRu7hvZQOCCe1cBAVQvQM8V6GhtXvpUg8dwiCCAJvmE7O9Yo8CDJy4YdwXJQCEG2klD5cKAkiGOoYfQ6XxUIJgRBSBe+zWAEQQiT8F6u7bLg1bZITciRxBLnhfm0m4C9JBHkoyfX4y5VH+LIRxB8zzLIcXKuFQBC/XBZL9iCCuJOKLLKH13lcUoZzTi5BCYJhShKkO0PxBdOVJQS3yATJg4iBJIFJgsAVQiDw+kBJFusUlCQsE8qI+HQMIYJIMjNIdOgddHhZ3JdEQAiJgjUfSj5JJg9bFyoYxCZUILlGBJECIQJJiMG+TsBuRGQT9mGRMAQRQsgQRMJwQdSQIXxNMgN/3GIIhBwOhBbcbLi/OiSIb3K0Q6/iN/v3JkNJQLzheJ/o1cOt9M1I/xnvT5nCUP7JfGiOEP05cxgtQwD3Q3PfPRqYDOUQRLLEpMkQMmQeJfqH6bH3BPrz5kCn+8BTGMYYX4h8GJhz8DhBVKJJIYSQo/CY8qWUekdEFx1f7ksS82+l1Ju0lQsCCRHJSqk8nLz8S0TvZOydj4gQ+XcGiQjKh3R3h5FiTLFxnb3xEJE7ifVWWxCVGvtIhzGvlmrJy/csiDOxk3aZOmGrpe5IHc+CyKJhzq2YJvJBQ21RHQkQxJkkCUJEUmcII8nJbJTsWQJBCiCMPPHPWvUWRBV2V9ZwJzGBSG2DYS7+B0P8V5WKLZ3fAAAAAElFTkSuQmCC";

    // ===== LIFECYCLE METHODS =====
 
  

    protected override async Task OnInitializedAsync()
    {
        // Este método SOLO se ejecuta UNA VEZ por instancia del componente
        Console.WriteLine($"🟢 OrdersComponent.OnInitializedAsync - ID: {Id}");

        currentLanguage = Localization.CurrentLanguage ?? "es";
        Localization.OnLanguageChanged += OnLanguageChanged;

        await Task.CompletedTask;
    }

    private int _lastRG35HashCode;
    private int _lastRG37HashCode;
 
    private bool _initialLoadComplete = false;

    protected override async Task OnParametersSetAsync()
    {
        // Este se ejecuta cuando cambian los parámetros
        // Solo procesar si los catálogos no se han procesado aún
        if (!_catalogsProcessed && RelacionBote.ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine($"🔄 ProcessCatalogsOnce - Primera vez");
            ProcessCatalogsOnce();
            _catalogsProcessed = true;
        }
        else if (_catalogsProcessed)
        {
            Console.WriteLine($"⏭️ Catálogos ya procesados, saltando");
        }

        // LoadDataAsync debe ejecutarse siempre que cambien los parámetros
        await LoadDataAsync();

        Console.WriteLine($"✅ OnParametersSetAsync completado " +DateTime.Now.ToString());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.initializeTooltips");
            await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.restoreContainerStates");
        }
    }

    // ===== PROCESAMIENTO DE CATÁLOGOS (OPTIMIZADO) =====

    private void ProcessCatalogsOnce()
    {
        ProcessRelacionBoteOnce();
        ProcessRelacionTapaOnce();

        if (Atributos.ValueKind == JsonValueKind.Object)
        {
            BuildOptionLookupsFromAtributos(Atributos);
        }

        // Guardar en caché
        _cachedBoteData = boteDataList;
        _cachedCapData = capDataList;
        _cachedBoteColors = boteColorOptions;
        _cachedCapColors = capColorOptions;
        _cachedCapacidades = capacidades;
        _cachedCapacidadToDiametros = capacidadToDiametros;
        _cachedMateriales = materiales;
    }

    private void ProcessRelacionBoteOnce()
    {
        if (RelacionBote.ValueKind != JsonValueKind.Object) return;
        if (!RelacionBote.TryGetProperty("value", out var boteArray)) return;
        if (boteArray.ValueKind != JsonValueKind.Array) return;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var totalItems = boteArray.GetArrayLength();
        var capsSet = new HashSet<string>(totalItems / 4, StringComparer.OrdinalIgnoreCase);
        var diasDict = new Dictionary<string, HashSet<string>>(totalItems / 4, StringComparer.OrdinalIgnoreCase);
        var matsSet = new HashSet<string>(totalItems / 8, StringComparer.OrdinalIgnoreCase);
        var boteList = new List<BoteCapDataModal.BoteDataItem>(totalItems);
        var colorsSeen = new HashSet<string>(20, StringComparer.OrdinalIgnoreCase);
        var boteColors = new List<BoteCapDataModal.ColorOption>(20);

        int colorId = 1;

        foreach (var item in boteArray.EnumerateArray())
        {
            string? cap = null, dia = null, mat = null, forma = null, color = null;

            // ⚡ NO extraer ImagenBote (es lo que mata el rendimiento)
            if (item.TryGetProperty("Capacidad", out var capEl) && capEl.ValueKind == JsonValueKind.String)
                cap = capEl.GetString();

            if (item.TryGetProperty("Diametro", out var diaEl) && diaEl.ValueKind == JsonValueKind.String)
                dia = diaEl.GetString();

            if (item.TryGetProperty("Material", out var matEl) && matEl.ValueKind == JsonValueKind.String)
                mat = matEl.GetString();

            if (item.TryGetProperty("Forma", out var formaEl) && formaEl.ValueKind == JsonValueKind.String)
                forma = formaEl.GetString();

            if (item.TryGetProperty("Color", out var colEl) && colEl.ValueKind == JsonValueKind.String)
                color = colEl.GetString();

            // Capacidades y diámetros
            if (!string.IsNullOrWhiteSpace(cap))
            {
                capsSet.Add(cap);
                if (!string.IsNullOrWhiteSpace(dia))
                {
                    if (!diasDict.TryGetValue(cap, out var diaSet))
                    {
                        diaSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        diasDict[cap] = diaSet;
                    }
                    diaSet.Add(dia);
                }
            }

            if (!string.IsNullOrWhiteSpace(mat))
                matsSet.Add(mat);

            // ⚡ BoteDataItem SIN imagen
            if (!string.IsNullOrWhiteSpace(forma))
            {
                ushort pesoMax = 0;
                if (item.TryGetProperty("Peso_Maximo", out var pmEl) && pmEl.ValueKind == JsonValueKind.Number)
                    pesoMax = (ushort)pmEl.GetInt32();

                boteList.Add(new BoteCapDataModal.BoteDataItem
                {
                    Forma = forma,
                    Capacidad = cap,
                    Diametro = dia,
                    Material = mat,
                    Color = color,
                    ImagenBote = null,  // ⚡ NO cargar ahora
                    PesoMaximo = pesoMax
                });
            }

            // Colores
            if (!string.IsNullOrWhiteSpace(color))
            {
                var norm = NormalizeColorName(color);
                if (colorsSeen.Add(norm))
                {
                    boteColors.Add(new BoteCapDataModal.ColorOption
                    {
                        ID = colorId++,
                        Value = ToTitle(color),
                        ColorHex = ColorToHex(norm, "#CCCCCC")
                    });
                }
            }
        }

        capacidades = capsSet.OrderBy(c => {
            if (int.TryParse(c, out var n)) return n;
            return int.MaxValue;
        }).ThenBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();

        materiales = matsSet.OrderBy(m => m, StringComparer.OrdinalIgnoreCase).ToList();

        capacidadToDiametros = diasDict.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderBy(d => d, StringComparer.OrdinalIgnoreCase).ToList(),
            StringComparer.OrdinalIgnoreCase);

        boteDataList = boteList;
        boteColorOptions = boteColors.OrderBy(o => o.Value, StringComparer.CurrentCultureIgnoreCase).ToList();

        for (int i = 0; i < boteColorOptions.Count; i++)
            boteColorOptions[i].ID = i + 1;

        Console.WriteLine($"✅ ProcessRelacionBoteOnce: {sw.ElapsedMilliseconds}ms ({boteList.Count} botes)");
    }

    // Continúo con la parte 2...

    // OrdersComponent.razor.cs - PARTE 2

    private void ProcessRelacionTapaOnce()
    {
        if (RelacionTapa.ValueKind != JsonValueKind.Object) return;
        if (!RelacionTapa.TryGetProperty("value", out var tapaArray)) return;
        if (tapaArray.ValueKind != JsonValueKind.Array) return;

        var sw = System.Diagnostics.Stopwatch.StartNew();

        var totalItems = tapaArray.GetArrayLength();
        var capList = new List<BoteCapDataModal.CapDataItem>(totalItems);
        var colorsSeen = new HashSet<string>(20, StringComparer.OrdinalIgnoreCase);
        var capColors = new List<BoteCapDataModal.ColorOption>(20);

        int colorId = 1;

        foreach (var item in tapaArray.EnumerateArray())
        {
            string? forma = null, dia = null, color = null;
            bool sleeve = false;

            if (item.TryGetProperty("Forma", out var formaEl) && formaEl.ValueKind == JsonValueKind.String)
                forma = formaEl.GetString();

            if (item.TryGetProperty("Diametro", out var diaEl) && diaEl.ValueKind == JsonValueKind.String)
                dia = diaEl.GetString();

            if (item.TryGetProperty("Color", out var colEl) && colEl.ValueKind == JsonValueKind.String)
                color = colEl.GetString();

            if (item.TryGetProperty("Sleeve", out var slEl))
                sleeve = slEl.ValueKind == JsonValueKind.True;

            // ⚡ NO extraer ImagenCap
            if (!string.IsNullOrWhiteSpace(forma))
            {
                capList.Add(new BoteCapDataModal.CapDataItem
                {
                    Forma = forma,
                    Diametro = dia,
                    Color = color,
                    Sleeve = sleeve,
                    ImagenCap = null  // ⚡ Lazy load
                });
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                var norm = NormalizeColorName(color);
                if (colorsSeen.Add(norm))
                {
                    capColors.Add(new BoteCapDataModal.ColorOption
                    {
                        ID = colorId++,
                        Value = ToTitle(color),
                        ColorHex = ColorToHex(norm, "#FFFFFF")
                    });
                }
            }
        }

        capDataList = capList;
        capColorOptions = capColors.OrderBy(o => o.Value, StringComparer.CurrentCultureIgnoreCase).ToList();

        for (int i = 0; i < capColorOptions.Count; i++)
            capColorOptions[i].ID = i + 1;

        Console.WriteLine($"✅ ProcessRelacionTapaOnce: {sw.ElapsedMilliseconds}ms ({capList.Count} tapas)");
    }
    // ===== LAZY LOAD DE IMÁGENES =====

    protected string? GetBoteImageLazy(string? forma, string? capacidad, string? diametro, string? material, string? color)
    {
        if (string.IsNullOrWhiteSpace(forma)) return NO_IMAGE_AVAILABLE;

        if (RelacionBote.ValueKind != JsonValueKind.Object) return NO_IMAGE_AVAILABLE;
        if (!RelacionBote.TryGetProperty("value", out var boteArray)) return NO_IMAGE_AVAILABLE;

        foreach (var item in boteArray.EnumerateArray())
        {
            if (TryGetString(item, "Forma", out var f) && f == forma &&
                TryGetString(item, "Capacidad", out var c) && c == capacidad &&
                TryGetString(item, "Diametro", out var d) && d == diametro &&
                TryGetString(item, "Material", out var m) && m == material &&
                TryGetString(item, "Color", out var col) && col == color)
            {
                if (TryGetString(item, "ImagenBote", out var img) && !string.IsNullOrWhiteSpace(img))
                {
                    return $"data:image/png;base64,{img}";
                }
            }
        }

        return NO_IMAGE_AVAILABLE;
    }

    protected string? GetCapImageLazy(string? forma, string? diametro, string? color, bool sleeve)
    {
        if (string.IsNullOrWhiteSpace(forma)) return NO_IMAGE_AVAILABLE;

        if (RelacionTapa.ValueKind != JsonValueKind.Object) return NO_IMAGE_AVAILABLE;
        if (!RelacionTapa.TryGetProperty("value", out var tapaArray)) return NO_IMAGE_AVAILABLE;

        foreach (var item in tapaArray.EnumerateArray())
        {
            if (TryGetString(item, "Forma", out var f) && f == forma &&
                TryGetString(item, "Diametro", out var d) && d == diametro &&
                TryGetString(item, "Color", out var c) && c == color)
            {
                bool itemSleeve = item.TryGetProperty("Sleeve", out var slEl) && slEl.ValueKind == JsonValueKind.True;

                if (itemSleeve == sleeve)
                {
                    if (TryGetString(item, "ImagenCap", out var img) && !string.IsNullOrWhiteSpace(img))
                    {
                        return $"data:image/png;base64,{img}";
                    }
                }
            }
        }

        return NO_IMAGE_AVAILABLE;
    }
    private void BuildOptionLookupsFromAtributos(JsonElement atributos)
    {
        try
        {
            if (!atributos.TryGetProperty("value", out var valueArr) || valueArr.ValueKind != JsonValueKind.Array)
                return;

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

                    var Round = v.TryGetProperty("Round", out var ro) ? (ro.GetString() ?? "") : "";
                    var Square = v.TryGetProperty("Square", out var sq) ? (sq.GetString() ?? "") : "";
                    var Cylindrical = v.TryGetProperty("Cylindrical", out var cy) ? (cy.GetString() ?? "") : "";
                    var Simple = v.TryGetProperty("Simple", out var si) ? (si.GetString() ?? "") : "";
                    var Metal = v.TryGetProperty("Metal", out var me) ? (me.GetString() ?? "") : "";
                    var Childproof = v.TryGetProperty("Childproof", out var ch) ? (ch.GetString() ?? "") : "";

                    list.Add(new BoteCapDataModal.ColorOption
                    {
                        ID = id++,
                        Value = label.Trim(),
                        ColorHex = hex.Trim(),
                        Round = !string.IsNullOrWhiteSpace(Round) ? $"data:image/png;base64,{Round}" : NO_IMAGE_AVAILABLE,
                        Square = !string.IsNullOrWhiteSpace(Square) ? $"data:image/png;base64,{Square}" : NO_IMAGE_AVAILABLE,
                        Cylindrical = !string.IsNullOrWhiteSpace(Cylindrical) ? $"data:image/png;base64,{Cylindrical}" : NO_IMAGE_AVAILABLE,
                        Simple = !string.IsNullOrWhiteSpace(Simple) ? $"data:image/png;base64,{Simple}" : NO_IMAGE_AVAILABLE,
                        Metal = !string.IsNullOrWhiteSpace(Metal) ? $"data:image/png;base64,{Metal}" : NO_IMAGE_AVAILABLE,
                        Childproof = !string.IsNullOrWhiteSpace(Childproof) ? $"data:image/png;base64,{Childproof}" : NO_IMAGE_AVAILABLE
                    });
                }

                return list.GroupBy(c => c.Value, StringComparer.OrdinalIgnoreCase)
                           .Select(g => g.First())
                           .ToList();
            }

            _opts.Capacidades = ListFromIndex(23);
            _opts.Diametros = ListFromIndex(5);
            _opts.Materiales = ListFromIndex(21);
            _opts.FormasBote = ListFromIndex(24);
            _opts.Bocas = ListFromIndex(22);
            _opts.FormasTapa = ListFromIndex(19);
            _opts.ColorBote = ColorListFromIndex(20);
            _opts.ColorCover = ColorListFromIndex(18);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building option lookups: {ex.Message}");
        }
    }

    // ===== HELPERS DE COLOR =====

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

    private static string NormalizeColorName(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.Trim();
        s = Regex.Replace(s, @"\s*\([^)]*\)\s*", "", RegexOptions.CultureInvariant);
        s = s.Replace(" / ", "/").Replace("  ", " ");
        s = Regex.Replace(s, @"\s+", " ");
        s = s.Replace("LightBlue", "Light Blue", StringComparison.OrdinalIgnoreCase)
             .Replace("LightGreen", "Light Green", StringComparison.OrdinalIgnoreCase)
             .Replace("Purple/Violet", "Purple", StringComparison.OrdinalIgnoreCase);
        return s.Trim().ToLowerInvariant();
    }

    private static string ColorToHex(string name, string fallback)
    {
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

        if (name.Contains('/'))
            foreach (var part in name.Split('/'))
                if (map.TryGetValue(part.Trim(), out hex)) return hex;

        return fallback;
    }

    private static string ToTitle(string s)
    {
        s = s?.Trim() ?? "";
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s.Substring(1);
    }

    // ===== CARGA DE DATOS =====

    private async Task LoadDataAsync()
    {
        try
        {
            if (HasRG35 && RG35 != null)
            {
                LoadFromRG35(RG35);
                LoadCatalogOptions();

                InitializeSelectedOptions();
            }
            else if (HasRG37 && RG37 != null)
            {
                LoadFromRG37(RG37);
            }



            // Calcular porcentajes síncronamente (es rápido)
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

        if (DateTime.TryParse(data.Estimated_date, out var est))
            EstimatedDate = est;
        if (DateTime.TryParse(data.Deadline_date, out var dead))
            DeadlineDate = dead;

        CustomerLogoUrl = "";

        Country1 = data.Country ?? "-";
        Country2 = data.Country_2 ?? "-";
        Country3 = data.Country_3 ?? "-";

        if (!string.IsNullOrEmpty(data.Logo_Pais))
            CountryFlag1 = $"data:image/png;base64,{data.Logo_Pais}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_2))
            CountryFlag2 = $"data:image/png;base64,{data.Logo_Pais_2}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_3))
            CountryFlag3 = $"data:image/png;base64,{data.Logo_Pais_3}";

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

        if (FillingBatchOther.Contains(":"))
            FillingBatchOther = FillingBatchOther.Substring(FillingBatchOther.IndexOf(":") + 1);
        if (FillingExpDateOther.Contains(":"))
            FillingExpDateOther = FillingExpDateOther.Substring(FillingExpDateOther.IndexOf(":") + 1);
        Console.WriteLine($"📋 Loaded from RG35:");
        Console.WriteLine($"   FillingBatch: {FillingBatch}");
        Console.WriteLine($"   FillingBatchOther: {FillingBatchOther}");
        Console.WriteLine($"   FillingExpDate: {FillingExpDate}");
        Console.WriteLine($"   FillingExpDateOther: {FillingExpDateOther}");
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

        if (data?.Box_label_config == "Standard")
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
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[2]"], Value = FormatNumericValue(data.Pallet_boxes_per_layer) },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[3]"], Value = FormatNumericValue(data.Pallet_boxes_per_pallet) }
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

        if (!string.IsNullOrEmpty(data.Label_imagen))
            LabelImageUrl = $"data:image/png;base64,{data.Label_imagen}";
        else
            LabelImageUrl = null;

        LoadTiposCajaOptions();
    }

    private void LoadFromRG37(CustomizeRG37Response data)
    {
        Code = data.Code ?? "-";
        Name = data.Product_name ?? "-";
        ProductName2 = data.Product_name_2 ?? "-";
        Status = data.Status ?? "-";
        RG37Code = data.Code ?? "-";

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

        Country1 = data.Country ?? "-";
        Country2 = data.Country_2 ?? "-";
        Country3 = data.Country_3 ?? "-";

        if (!string.IsNullOrEmpty(data.Logo_Pais))
            CountryFlag1 = $"data:image/png;base64,{data.Logo_Pais}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_2))
            CountryFlag2 = $"data:image/png;base64,{data.Logo_Pais_2}";
        if (!string.IsNullOrEmpty(data.Logo_Pais_3))
            CountryFlag3 = $"data:image/png;base64,{data.Logo_Pais_3}";

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
            new() { Label = Localization["orderView.ListInputB[2]"], Value = data.Size?.ToString() ?? "-" },
            new() { Label = Localization["orderView.ListInputB[3]"], Value = data.Serving?.ToString() ?? "-" }
            };

        RecipeRows = data.Recipe?.Select(r => new RecipeRow
        {
            Active = r.Active ?? "-",
            SourceUsed = r.Source_used ?? "-",
            QuantityServing = r.Quantity_of_active_per_serving ?? "-",
            RdaEu = r.EU_RDA ?? "-"
        }).ToList() ?? new List<RecipeRow>();

        ReportFiles = data.Files ?? new List<FileItem>();
        BottleInfo = new List<InputItem>();
        LabelInfo = new List<InputItem>();
        PalletInfo = new List<InputItem>();
        PalletizingInfo = new List<InputItem>();
    }

    // Continúo con la parte 3...

    // OrdersComponent.razor.cs - PARTE 3

    // ===== CARGA DE CATÁLOGOS Y OPCIONES =====

    private void LoadCatalogOptions()
    {
        try
        {
            if (LotFormat.ValueKind == JsonValueKind.Object && LotFormat.TryGetProperty("value", out var lotValues))
                BatchFormats = lotValues.EnumerateArray()
                    .Select(i => new FormatOption { Format = i.GetProperty("Format").GetString() ?? "" })
                    .ToList();

            if (BbdFormat.ValueKind == JsonValueKind.Object && BbdFormat.TryGetProperty("value", out var bbdValues))
                BbdFormats = bbdValues.EnumerateArray()
                    .Select(i => new FormatOption { Format = i.GetProperty("Format").GetString() ?? "" })
                    .ToList();

            if (!(Atributos.ValueKind == JsonValueKind.Object && Atributos.TryGetProperty("value", out var attrValues)))
                return;

            var arr = attrValues.EnumerateArray().ToList();

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

    private void LoadTiposCajaOptions()
    {
        try
        {
            TiposCajaOptions.Clear();

            if (TiposCajas.ValueKind != JsonValueKind.Object) return;
            if (!TiposCajas.TryGetProperty("value", out var cajasArray)) return;
            if (cajasArray.ValueKind != JsonValueKind.Array) return;

            var boteForma = RG35?.Bote_forma?.ToUpperInvariant() ?? "";
            var boteCapacidad = RG35?.Bote_capacidad ?? "";
            var boteBoca = RG35?.Bote_boca ?? "";

            if (string.IsNullOrEmpty(boteForma) || string.IsNullOrEmpty(boteCapacidad) || string.IsNullOrEmpty(boteBoca))
            {
                Console.WriteLine("No hay datos de bote para filtrar tipos de caja");
                return;
            }

            Console.WriteLine($"Filtrando cajas por: Forma={boteForma}, Capacidad={boteCapacidad}, Boca={boteBoca}");

            int id = 1;
            foreach (var item in cajasArray.EnumerateArray())
            {
                var forma = item.TryGetProperty("Forma", out var f) ? (f.GetString() ?? "").ToUpperInvariant() : "";
                var capacidad = item.TryGetProperty("Capacidad", out var c) ? c.GetString() ?? "" : "";
                var boca = item.TryGetProperty("Boca", out var b) ? b.GetString() ?? "" : "";
                var tipoCaja = item.TryGetProperty("Tipo_de_caja", out var tc) ? tc.GetString() ?? "" : "";

                if (forma == boteForma && capacidad == boteCapacidad && boca == boteBoca)
                {
                    var option = new TipoCajaOption
                    {
                        Id = id++,
                        Forma = forma,
                        Capacidad = capacidad,
                        Boca = boca,
                        Tipo_de_caja = tipoCaja,
                        Unidades_por_caja = item.TryGetProperty("Unidades_por_caja", out var upc) ? upc.GetInt32() : 0,
                        Pallet_EU_Alturas = item.TryGetProperty("Pallet_EU_Alturas", out var euA) ? euA.GetInt32() : 0,
                        Pallet_EU_Base = item.TryGetProperty("Pallet_EU_Base", out var euB) ? euB.GetInt32() : 0,
                        Pallet_Americano_Alturas = item.TryGetProperty("Pallet_Americano_Alturas", out var amA) ? amA.GetInt32() : 0,
                        Pallet_Americano_Base = item.TryGetProperty("Pallet_Americano_Base", out var amB) ? amB.GetInt32() : 0
                    };

                    TiposCajaOptions.Add(option);
                    Console.WriteLine($"Caja añadida: {tipoCaja} - {option.Unidades_por_caja} unidades");
                }
            }

            if (RG35 != null && !string.IsNullOrEmpty(RG35.Box_name))
            {
                SelectedTipoCaja = RG35.Box_name;
            }
            else if (TiposCajaOptions.Any())
            {
                SelectedTipoCaja = TiposCajaOptions.First().Tipo_de_caja;
            }

            Console.WriteLine($"Total de cajas filtradas: {TiposCajaOptions.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tipos de caja: {ex.Message}");
        }
    }

    private void InitializeSelectedOptions()
    {
        if (HasRG35 && RG35 == null) return;

        // ⚡ Cargar imágenes bajo demanda
        var boteImg = GetBoteImageLazy(
            RG35.Bote_forma,
            RG35.Bote_capacidad,
            RG35.Bote_boca,
            RG35.Bote_material,
            RG35.Bote_color
        );

        var capImg = GetCapImageLazy(
            RG35.Cap_tapa,
            RG35.Cap_Boca,
            RG35.Cap_color,
            RG35.Cap_sleever ?? false
        );

        selectedBoteOption = new BoteCapDataModal.BoteDataItem
        {
            Forma = RG35.Bote_forma,
            Capacidad = RG35.Bote_capacidad,
            Diametro = RG35.Bote_boca,
            Material = RG35.Bote_material,
            Color = RG35.Bote_color,
            ImagenBote = boteImg  // ⚡ Lazy loaded
        };

        selectedCapOption = new BoteCapDataModal.CapDataItem
        {
            Forma = RG35.Cap_tapa,
            Diametro = RG35.Cap_Boca,
            Color = RG35.Cap_color,
            Sleeve = RG35.Cap_sleever ?? false,
            ImagenCap = capImg  // ⚡ Lazy loaded
        };

        characteristics = RG35.Characteristics ?? "";
    }

    // ===== CÁLCULOS DE PORCENTAJES (SÍNCRONO) =====

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

        if (RG35?.Box_label_config == "Standard")
        {
            var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-" && TiposCajaOptions.Any());
            PercentFilledPalettizing = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
        }
        else
        {
            var filled = fields.Count(f => !string.IsNullOrEmpty(f) && f != "-" && !string.IsNullOrEmpty(RG35?.Box_label_imagen));
            PercentFilledPalettizing = fields.Count > 0 ? (filled * 100 / fields.Count) : 0;
        }
    }

    private void CalculateAnalyticsPercentage()
    {
        PercentFilledAnalytics = (NoAnalytics || AnalyticsRows.Any()) ? 100 : 0;
    }

    // ===== HELPERS DE FORMATO =====

    protected string FormatPercent(int value) => Math.Clamp(value, 0, 100).ToString();

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

    // ===== UI HELPERS =====

    protected void ToggleDownloadReports() => ShowReports = !ShowReports;

    protected async Task ToggleSection(int idx)
    {
        if (IsOpen.ContainsKey(idx))
        {
            IsOpen[idx] = !IsOpen[idx];
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
            return "#228B22";
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

    // ===== ACCIONES =====

    protected async Task SaveName()
    {
        StateHasChanged();
        if (OnSaveName.HasDelegate)
            await OnSaveName.InvokeAsync(ProductName2);
    }

    protected async Task SaveBatch()
    {
        try
        {
            Console.WriteLine($"💾 SaveBatch called: {FillingBatch}");

            if (OnSaveLotFormat.HasDelegate)
            {
                await OnSaveLotFormat.InvokeAsync((FillingBatch, null));

                // ✅ Esperar un poco para que el padre actualice
                await Task.Delay(100);

                // ✅ Forzar re-render
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in SaveBatch: {ex.Message}");
        }
    }

    protected async Task SaveBatchOther()
    {
        if (string.IsNullOrWhiteSpace(FillingBatchOther))
        {
            Console.WriteLine("⚠️ FillingBatchOther is empty, skipping save");
            return;
        }

        IsSendingBatchOther = true;
        StateHasChanged();

        try
        {
            Console.WriteLine($"💾 SaveBatchOther: format={FillingBatch}, other={FillingBatchOther}");

            if (OnSaveLotFormat.HasDelegate)
            {
                // ✅ Enviar AMBOS valores: el formato del dropdown Y el texto custom
                await OnSaveLotFormat.InvokeAsync((FillingBatch, FillingBatchOther));

                // ✅ NO limpiar el campo aquí - mantener el valor visible
                Console.WriteLine($"✅ Batch other saved: {FillingBatchOther}");

                // Esperar actualización del padre
                await Task.Delay(150);
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in SaveBatchOther: {ex.Message}");
        }
        finally
        {
            IsSendingBatchOther = false;
            StateHasChanged();
        }
    }

    protected async Task SaveBbd()
    {
        try
        {
            Console.WriteLine($"💾 SaveBbd called: {FillingExpDate}");

            if (OnSaveBbdFormat.HasDelegate)
            {
                await OnSaveBbdFormat.InvokeAsync((FillingExpDate, null));

                // ✅ Esperar un poco para que el padre actualice
                await Task.Delay(100);

                // ✅ Forzar re-render
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in SaveBbd: {ex.Message}");
        }
    }

    protected async Task SaveBbdOther()
    {
        if (string.IsNullOrWhiteSpace(FillingExpDateOther))
        {
            Console.WriteLine("⚠️ FillingExpDateOther is empty, skipping save");
            return;
        }

        IsSendingBbdOther = true;
        StateHasChanged();

        try
        {
            Console.WriteLine($"💾 SaveBbdOther: format={FillingExpDate}, other={FillingExpDateOther}");

            if (OnSaveBbdFormat.HasDelegate)
            {
                // ✅ Enviar AMBOS valores: el formato del dropdown Y el texto custom
                await OnSaveBbdFormat.InvokeAsync((FillingExpDate, FillingExpDateOther));

                // ✅ NO limpiar el campo aquí - mantener el valor visible
                Console.WriteLine($"✅ BBD other saved: {FillingExpDateOther}");

                // Esperar actualización del padre
                await Task.Delay(150);
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in SaveBbdOther: {ex.Message}");
        }
        finally
        {
            IsSendingBbdOther = false;
            StateHasChanged();
        }
    }

    protected async Task OnTakeSampleChanged()
    {
        StateHasChanged();
        if (OnPatchRG37.HasDelegate)
        {
            await OnPatchRG37.InvokeAsync(new { Tomar_muestra = TakeSample });
        }
    }

    protected async Task OnNoLabelChanged()
    {
        StateHasChanged();
        IsProcessingNoLabel = true;
        StateHasChanged();

        try
        {
            if (OnPatchRG35.HasDelegate)
            {
                var labelConfig = NoLabel ? "No label" : "Label";
                await OnPatchRG35.InvokeAsync(new { Label_config = labelConfig });
            }

            CalculateLabelPercentage();
        }
        finally
        {
            IsProcessingNoLabel = false;
            StateHasChanged();
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
        StateHasChanged();
        if (OnPatchRG35.HasDelegate)
        {
            await OnPatchRG35.InvokeAsync(new { Pallet_comments = PalletComments });
        }
    }

    protected async Task OnTipoCajaChanged(ChangeEventArgs e)
    {
        if (e.Value == null) return;

        var newTipoCaja = e.Value.ToString();
        if (string.IsNullOrEmpty(newTipoCaja) || newTipoCaja == SelectedTipoCaja) return;

        IsLoadingTipoCaja = true;
        SelectedTipoCaja = newTipoCaja;
        StateHasChanged();

        try
        {
            var selectedCaja = TiposCajaOptions.FirstOrDefault(c => c.Tipo_de_caja == newTipoCaja);
            if (selectedCaja == null)
            {
                Console.WriteLine($"No se encontró la caja seleccionada: {newTipoCaja}");
                return;
            }

            var palletType = RG35?.Pallet_type ?? "";
            var isEuropean = palletType.Contains("EUR", StringComparison.OrdinalIgnoreCase) ||
                            palletType.Contains("Europeo", StringComparison.OrdinalIgnoreCase) ||
                            palletType.Contains("European", StringComparison.OrdinalIgnoreCase);

            int alturas = isEuropean ? selectedCaja.Pallet_EU_Alturas : selectedCaja.Pallet_Americano_Alturas;
            int cajasXAltura = isEuropean ? selectedCaja.Pallet_EU_Base : selectedCaja.Pallet_Americano_Base;
            int cajasPorPallet = alturas * cajasXAltura;

            Console.WriteLine($"Caja seleccionada: {selectedCaja.Tipo_de_caja}");
            Console.WriteLine($"Tipo de pallet: {palletType} (Europeo: {isEuropean})");
            Console.WriteLine($"Alturas: {alturas}, Cajas x Altura: {cajasXAltura}, Total: {cajasPorPallet}");

            var payload = new
            {
                Box_name = selectedCaja.Tipo_de_caja,
                Box_units_per = selectedCaja.Unidades_por_caja,
                Pallet_layers = alturas,
                Pallet_boxes_per_layer = cajasXAltura,
                Pallet_boxes_per_pallet = cajasPorPallet
            };

            if (OnPatchRG35.HasDelegate)
            {
                await OnPatchRG35.InvokeAsync(payload);

                if (RG35 != null)
                {
                    RG35.Box_name = selectedCaja.Tipo_de_caja;
                    RG35.Box_units_per = selectedCaja.Unidades_por_caja.ToString();
                    RG35.Pallet_layers = alturas.ToString();
                    RG35.Pallet_boxes_per_layer = cajasXAltura.ToString();
                    RG35.Pallet_boxes_per_pallet = cajasPorPallet.ToString();
                }

                UpdatePalletInfo();
                CalculatePalletizingPercentage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cambiar tipo de caja: {ex.Message}");
        }
        finally
        {
            IsLoadingTipoCaja = false;
            StateHasChanged();
        }
    }

    private void UpdatePalletInfo()
    {
        if (RG35?.Box_label_config == "Standard")
        {
            PalletInfo = new List<InputItem>
            {
                new() { Label = Localization["orderView.Pallet[5]"], Value = RG35.Box_label_config ?? "-" },
                new() { Label = Localization["orderView.Pallet[0]"], Value = RG35.Box_name ?? "-" },
                new() { Label = Localization["orderView.Pallet[1]"], Value = FormatNumericValue(RG35.Box_units_per) },
            };
        }
        else
        {
            PalletInfo = new List<InputItem>
            {
                new() { Label = Localization["orderView.Pallet[5]"], Value = RG35.Box_label_config ?? "-" },
            };
        }

        PalletizingInfo = new List<InputItem>
        {
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[0]"], Value = RG35.Pallet_type ?? "-" },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[1]"], Value = FormatNumericValue(RG35.Pallet_layers) },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[2]"], Value = FormatNumericValue(RG35.Pallet_boxes_per_layer) },
            new() { Label = Localization["orderView.PALLETIZINGINFORMATION[3]"], Value = FormatNumericValue(RG35.Pallet_boxes_per_pallet) }
        };
    }

    protected async Task DownloadBase64File(string base64, string name)
    {
        await JSRuntime.InvokeVoidAsync("OrdersComponentHelper.downloadBase64File", base64, name);
    }

    // Continúo con la parte 4 (modales y eventos finales)...

    // OrdersComponent.razor.cs - PARTE 4 FINAL

    // ===== MÉTODOS DE MODALES Y EVENTOS =====

    protected async Task HandleFormulationApproved()
    {
        CustomerAccepted = true;
        Status = "Cerrado cliente";

        await JS.InvokeVoidAsync("modalHelper.cleanupModals");
        await Task.Delay(200);

        CalculateFormulationPercentage();
        StateHasChanged();
    }

    protected void ShowConfirmModal()
    {
        isConfirmModalVisible = true;
        StateHasChanged();
    }

    protected async Task HandleConfirm()
    {
        isConfirmModalVisible = false;
        StateHasChanged();

        await Task.Delay(300);

        isThankYouModalVisible = true;
        StateHasChanged();
    }

    protected async Task HandlePalletLabelUpdated(dynamic response)
    {
        try
        {
            if (response != null)
            {
                string? base64Image = null;

                if (response is IDictionary<string, object> dict)
                {
                    if (dict.ContainsKey("Pallet_label_imagen"))
                    {
                        base64Image = dict["Pallet_label_imagen"]?.ToString();
                    }
                }
                else
                {
                    var json = JsonSerializer.Serialize(response);
                    var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (data?.ContainsKey("Pallet_label_imagen") == true)
                    {
                        base64Image = data["Pallet_label_imagen"]?.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(base64Image))
                {
                    PalletLabelImg = $"data:image/png;base64,{base64Image}";

                    if (PalletizingInfo != null && PalletizingInfo.Count > 0)
                    {
                        var palletLabelItem = PalletizingInfo.FirstOrDefault(p => p.Label == Localization["orderView.PalletLabel"]);
                        if (palletLabelItem != null)
                        {
                            palletLabelItem.Value = "Uploaded";
                        }
                    }

                    CalculatePalletizingPercentage();
                    await InvokeAsync(StateHasChanged);

                    Console.WriteLine("Pallet label image updated successfully in UI");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling pallet label update: {ex.Message}");
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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling box label update: {ex.Message}");
        }
    }

    protected async Task HandleLabelOptionsUpdated(ModalLabel.LabelOptionsUpdatedEventArgs e)
    {
        if (e.SentData is not null)
        {
            var json = JsonSerializer.Serialize(e.SentData);
            var opts = JsonSerializer.Deserialize<LabelOptionsPatch>(json);
            if (opts is null) return;

            LabelInfo = new List<InputItem>
            {
                new() { Label = Localization["orderView.GUMMYDNAL[0]"], Value = opts.LabelSize ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[3]"], Value = opts.LabelColors ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[1]"], Value = opts.LabelMaterial ?? "-" },
                new() { Label = Localization["orderView.GUMMYDNAL[2]"], Value = opts.LabelFinish ?? "-" },
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
            LabelImageUrl = e.DataUrl;
            CalculateLabelPercentage();
            await InvokeAsync(StateHasChanged);
        }
    }

    protected Task OpenLabelModal()
    {
        isLabelModalOpen = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected Task CloseLabelModal()
    {
        isLabelModalOpen = false;
        StateHasChanged();
        return Task.CompletedTask;
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
            await JSRuntime.InvokeVoidAsync("downloadBase64File",
                LabelImageUrl,
                $"Label-Imagen-{Id}.png",
                "image/png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading image: {ex.Message}");
            await ShowAlert("Error downloading image. Please try again.");
        }
    }

    private async Task ShowAlert(string message)
    {
        await JSRuntime.InvokeVoidAsync("alert", message);
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

    protected async Task HandleSaveData(BoteCapDataModal.BoteDataItem bote, BoteCapDataModal.CapDataItem cap)
    {
        selectedBoteOption = bote;
        selectedCapOption = cap;
        await InvokeAsync(StateHasChanged);
    }

    public void HandlePackagingUpdated(BoteCapDataModal.UpdatedOptions u)
    {
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

    // ===== MÉTODOS DE LENGUAJE =====

    public void OnLanguageChanged()
    {
        currentLanguage = Localization.CurrentLanguage;
        InvokeAsync(StateHasChanged);
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
        var key = $"{baseKey}[{index}]";
        return Localization[key];
    }

    // ===== MAPEO DE OPCIONES =====

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

    // ===== CLASES AUXILIARES =====

    private sealed class LabelOptionsPatch
    {
        [JsonPropertyName("Label_size")] public string? LabelSize { get; set; }
        [JsonPropertyName("Label_material")] public string? LabelMaterial { get; set; }
        [JsonPropertyName("Label_finish")] public string? LabelFinish { get; set; }
        [JsonPropertyName("Label_type")] public string? LabelColors { get; set; }
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

    // ===== MODELOS INTERNOS =====

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

    public class TipoCajaOption
    {
        public int Id { get; set; }
        public string Forma { get; set; } = "";
        public string Capacidad { get; set; } = "";
        public string Boca { get; set; } = "";
        public string Tipo_de_caja { get; set; } = "";
        public int Unidades_por_caja { get; set; }
        public int Pallet_EU_Alturas { get; set; }
        public int Pallet_EU_Base { get; set; }
        public int Pallet_Americano_Alturas { get; set; }
        public int Pallet_Americano_Base { get; set; }
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

    public class LabelOption
    {
        public int ID { get; set; }
        public string Value { get; set; } = "";
    }

    public class LabelOptionMX
    {
        public int ID { get; set; }
        public string Value { get; set; } = "";
        public string Imagen { get; set; } = "";
    }

    public class SelectedLabelOptions
    {
        public string LabelSize { get; set; } = "";
        public string LabelMaterial { get; set; } = "";
        public string LabelFinish { get; set; } = "";
        public string LabelColors { get; set; } = "";
    }
}