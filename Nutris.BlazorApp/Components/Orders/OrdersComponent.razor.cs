using Microsoft.AspNetCore.Components;
using NutrisBlazor.Models;
using NutrisBlazor.Services;
using System.Text.Json;

namespace Nutris.BlazorApp.Components.Orders;

public class OrdersComponentBase : ComponentBase
{
    [Inject] protected SimpleApiService Api { get; set; } = default!;

    // Identificador (opcional si lo usas dentro del componente)
    [Parameter] public string Id { get; set; } = string.Empty;

    // Datos que le pasas desde Customize.razor
    [Parameter] public CustomizeRG35Response RG35 { get; set; }   // puede venir "Undefined" si no aplica
    [Parameter] public CustomizeRG37Response RG37 { get; set; }   // idem

    [Parameter] public JsonElement Atributos { get; set; }
    [Parameter] public JsonElement RelacionBote { get; set; }
    [Parameter] public JsonElement RelacionTapa { get; set; }
    [Parameter] public JsonElement LotFormat { get; set; }
    [Parameter] public JsonElement BbdFormat { get; set; }

    [Parameter] public Boolean HasRG35 { get; set; }
    [Parameter] public Boolean HasRG37 { get; set; }

    // Callbacks hacia el padre (Customize.razor)
    [Parameter] public EventCallback OnApprove { get; set; }
    [Parameter] public EventCallback<string> OnSaveName { get; set; }

    // Si quieres pasar "formato" y "other" en uno solo, puedes usar tupla:
    [Parameter] public EventCallback<(string format, string? other)> OnSaveLotFormat { get; set; }
    [Parameter] public EventCallback<(string format, string? other)> OnSaveBbdFormat { get; set; }

    [Parameter] public EventCallback<object> OnPatchRG35 { get; set; }
    [Parameter] public EventCallback<object> OnPatchRG37 { get; set; }
    [Parameter] public EventCallback<object> OnUploadBoxOrPallet { get; set; }

    protected bool IsLoading { get; set; } = true;
    protected string OrderId { get; set; } = "";
    // Header / resumen
    protected string CustomerLogoUrl { get; set; } = "/img/logo.svg";
    protected string CountryFlag1 { get; set; } = "/img/flags/es.svg";
    protected string CountryFlag2 { get; set; } = "/img/flags/gb.svg";
    protected string CountryFlag3 { get; set; } = "/img/flags/fr.svg";
    protected string Country1 { get; set; } = "-";
    protected string Country2 { get; set; } = "-";
    protected string Country3 { get; set; } = "-";
    protected string Code { get; set; } = "-";
    protected string Name { get; set; } = "-";
    protected string ProductName2 { get; set; } = "-";
    protected int? MOQ { get; set; }
    protected decimal? UnitPrice { get; set; }
    protected string NutrisCode { get; set; } = "-";
    protected DateTime? EstimatedDate { get; set; }
    protected DateTime? DeadlineDate { get; set; }
    protected string EstimatedDateString => EstimatedDate?.ToString("dd/MM/yyyy") ?? "-";
    protected string DeadlineDateString => DeadlineDate?.ToString("dd/MM/yyyy") ?? "-";

    protected bool ShowReports { get; set; }
    protected List<ApiFile> ReportFiles { get; set; } = new();

    // Secciones abiertas
    protected Dictionary<int, bool> IsOpen { get; set; } = new() { { 1, true }, { 2, true }, { 3, true }, { 4, true }, { 5, true } };

    // FORMULATION
    protected int PercentFilledFormulation { get; set; }
    protected string FeatureTagImg { get; set; } = "/img/tag.svg";
    protected string FeatureCheckTitle { get; set; } = "Checks";
    protected string Selectingtheseoptions { get; set; } = "Selecting these options...";
    protected string GummyListInputBn { get; set; } = "BN";
    protected string GummyListInputBnValue { get; set; } = "-";
    protected string GummyListInputB { get; set; } = "B";
    protected string GummyListInputBValue { get; set; } = "-";
    protected string Shape { get; set; } = "-";
    protected string GummyShapeImg { get; set; } = "/img/gummy.png";
    protected List<RecipeRow> RecipeRows { get; set; } = new();
    protected string NutrisComments { get; set; } = string.Empty;
    protected bool TakeSample { get; set; }
    protected decimal TakeSamplePrice { get; set; }
    protected string Approved { get; set; } = "Approved";
    protected string Edit { get; set; } = "Edit";

    // PACKAGING
    protected int PercentFilledBottle { get; set; }
    protected string BottleType { get; set; } = "-";
    protected string BottleDesc { get; set; } = "-";
    protected string BottleImg { get; set; } = "/img/bottle.png";
    protected string FillingBatch { get; set; } = string.Empty;
    protected string FillingBatchOther { get; set; } = string.Empty;
    protected bool IsSendingBatchOther { get; set; }
    protected string FillingExpDate { get; set; } = string.Empty;
    protected string FillingExpDateOther { get; set; } = string.Empty;
    protected bool IsSendingBbdOther { get; set; }
    protected string FillingLocation { get; set; } = "-";
    protected List<FormatOption> BatchFormats { get; set; } = new();
    protected List<FormatOption> BbdFormats { get; set; } = new();

    // LABEL
    protected int PercentFilledLabel { get; set; }
    protected bool NoLabel { get; set; }
    protected string GummyDnaDesc { get; set; } = "-";
    protected string LabelImageUrl { get; set; } = "/img/label.png";

    // PALLETIZING
    protected int PercentFilledPalettizing { get; set; }
    protected string PalletBoxName { get; set; } = "-";
    protected string BoxLabelImg { get; set; } = "/img/boxlabel.png";
    protected string PalletizingInfo { get; set; } = "-";
    protected string PalletLabelImg { get; set; } = "/img/palletlabel.png";
    protected string PalletComments { get; set; } = string.Empty;

    // ANALYTICS
    protected int PercentFilledAnalytics { get; set; }
    protected bool NoAnalytics { get; set; }
    protected List<AnalyticsRow> AnalyticsRows { get; set; } = new();
    protected decimal SumAnalytics { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadOrderAsync();
    }

    protected async Task LoadOrderAsync()
    {
        IsLoading = true;
        try
        {
            // TODO: Ajusta la ruta exacta a tu backend (basada en OrdersComponent.vue / OrderComponents.vue)
            // Ejemplo genérico:
            // var dto = await Api.GetAsync<OrderDto>($"orders/{OrderId ?? "current"}");

            // Mock inicial para no reventar la UI si aún no tienes endpoint:
            if (RG35 is not null)
            {
                var dto = new OrderDto
                {
                    Code = RG35.Code,
                    Name = RG35.Product_name,
                    ProductName2 = RG35.Product_name_2,
                    MOQ = RG35.MOQ,
                    UnitPrice = RG35.Unit_price,
                    NutrisCode = RG35.Nutris_Code,
                    EstimatedDate = Convert.ToDateTime( RG35.Estimated_date),
                    DeadlineDate = Convert.ToDateTime(RG35.Deadline_date),
                    Country1 = RG35.Country,
                    Country2 = RG35.Country_2,
                    Country3 = RG35.Country_3,

                    ReportFiles = new() { new ApiFile { Name = "ClientSummary.pdf", File = "" } }
                };

                MapHeader(dto);
                MapFormulation(dto);
                MapPackaging(dto);
                MapLabel(dto);
                MapPalletizing(dto);
                MapAnalytics(dto);
            }
            if (RG37 is not null)
            {
                var dto = new OrderDto
                {
                    Code = RG37.Code,
                    Name = RG37.Product_name,
                    ProductName2 = RG37.Product_name_2,
                   
                   
                  
                    ReportFiles = new() { new ApiFile { Name = "ClientSummary.pdf", File = "" } }
                };

                MapHeader(dto);
                MapFormulation(dto);
                MapPackaging(dto);
                MapLabel(dto);
                MapPalletizing(dto);
                MapAnalytics(dto);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    void MapHeader(OrderDto dto)
    {
        Code = dto.Code ?? "-";
        Name = dto.Name ?? "-";
        ProductName2 = dto.ProductName2 ?? "-";
        MOQ = dto.MOQ;
        UnitPrice = dto.UnitPrice;
        NutrisCode = dto.NutrisCode ?? "-";
        EstimatedDate = dto.EstimatedDate;
        DeadlineDate = dto.DeadlineDate;
        Country1 = dto.Country1 ?? "-";
        Country2 = dto.Country2 ?? "-";
        Country3 = dto.Country3 ?? "-";
        ReportFiles = dto.ReportFiles ?? new();
    }

    void MapFormulation(OrderDto dto)
    {
        PercentFilledFormulation = dto.PercentFilledFormulation;
        GummyListInputBnValue = dto.GummyBn ?? "-";
        GummyListInputBValue = dto.GummyB ?? "-";
        Shape = dto.Shape ?? "-";
        RecipeRows = dto.Recipe ?? new();
        NutrisComments = dto.NutrisComments ?? "";
        TakeSample = dto.TakeSample;
        TakeSamplePrice = dto.TakeSamplePrice;
    }

    void MapPackaging(OrderDto dto)
    {
        PercentFilledBottle = dto.PercentFilledBottle;
        BottleType = dto.BottleType ?? "-";
        BottleDesc = dto.BottleDesc ?? "-";
        BottleImg = dto.BottleImg ?? BottleImg;
        BatchFormats = dto.BatchFormats ?? new();
        BbdFormats = dto.BbdFormats ?? new();
        FillingBatch = dto.FillingBatch ?? "";
        FillingExpDate = dto.FillingExpDate ?? "";
        FillingLocation = dto.FillingLocation ?? "-";
    }

    void MapLabel(OrderDto dto)
    {
        PercentFilledLabel = dto.PercentFilledLabel;
        NoLabel = dto.NoLabel;
        GummyDnaDesc = dto.GummyDnaDesc ?? "-";
        LabelImageUrl = dto.LabelImageUrl ?? LabelImageUrl;
    }

    void MapPalletizing(OrderDto dto)
    {
        PercentFilledPalettizing = dto.PercentFilledPalettizing;
        PalletBoxName = dto.PalletBoxName ?? "-";
        BoxLabelImg = dto.BoxLabelImg ?? BoxLabelImg;
        PalletizingInfo = dto.PalletizingInfo ?? "-";
        PalletLabelImg = dto.PalletLabelImg ?? PalletLabelImg;
        PalletComments = dto.PalletComments ?? "";
    }

    void MapAnalytics(OrderDto dto)
    {
        PercentFilledAnalytics = dto.PercentFilledAnalytics;
        NoAnalytics = dto.NoAnalytics;
        AnalyticsRows = dto.Analytics ?? new();
        SumAnalytics = dto.AnalyticsTotal;
    }

    // Helpers UI
    protected string FormatPercent(int v) => Math.Clamp(v, 0, 100).ToString();

    protected void ToggleDownloadReports() => ShowReports = !ShowReports;

    protected void ToggleSection(int idx)
    {
        if (IsOpen.ContainsKey(idx))
            IsOpen[idx] = !IsOpen[idx];
    }

    // Acciones
    protected async Task SaveName()
    {
        // TODO endpoint real
        await Api.PatchAsync("orders/update-name", new { productName2 = ProductName2, orderId = OrderId });
    }

    protected async Task SaveBatch()
    {
        // TODO endpoint real
        await Api.PatchAsync("orders/update-batch", new { orderId = OrderId, format = FillingBatch });
    }

    protected async Task SaveBatchOther()
    {
        if (string.IsNullOrWhiteSpace(FillingBatchOther)) return;
        IsSendingBatchOther = true;
        try
        {
            await Api.PatchAsync("orders/update-batch-other", new { orderId = OrderId, other = FillingBatchOther });
            FillingBatchOther = string.Empty;
        }
        finally { IsSendingBatchOther = false; }
    }

    protected async Task SaveBbd()
    {
        // TODO endpoint real
        await Api.PatchAsync("orders/update-bbd", new { orderId = OrderId, format = FillingExpDate });
    }

    protected async Task SaveBbdOther()
    {
        if (string.IsNullOrWhiteSpace(FillingExpDateOther)) return;
        IsSendingBbdOther = true;
        try
        {
            await Api.PatchAsync("orders/update-bbd-other", new { orderId = OrderId, other = FillingExpDateOther });
            FillingExpDateOther = string.Empty;
        }
        finally { IsSendingBbdOther = false; }
    }

    protected async Task OnTakeSampleChanged()
    {
        await Api.PatchAsync("orders/take-sample", new { orderId = OrderId, take = TakeSample });
    }

    protected async Task OnNoLabelChanged()
    {
        await Api.PatchAsync("orders/no-label", new { orderId = OrderId, noLabel = NoLabel });
    }

    protected async Task OnNoAnalyticsChanged()
    {
        await Api.PatchAsync("orders/no-analytics", new { orderId = OrderId, noAnalytics = NoAnalytics });
    }

    protected async Task SavePalletComments()
    {
        await Api.PatchAsync("orders/pallet-comments", new { orderId = OrderId, comments = PalletComments });
    }

    protected void HandleConfirm() { /* continuar flujo firma */ }
    protected void HandlePalletLabelUpdated(string newUrl) { PalletLabelImg = newUrl; StateHasChanged(); }
    protected void HandleBoxLabelUpdated(string newUrl) { BoxLabelImg = newUrl; StateHasChanged(); }
    protected void HandleLabelOptionsUpdated(object _payload) { /* refrescar label */ }
    protected void HandleDraftLabelUpdated(object _payload) { /* guardar draft */ }

    protected void DownloadBase64File(string base64, string name)
    {
        // Sugerencia: usa un JS interop para forzar descarga si lo necesitas
    }

    // ----- Modelos mínimos -----
    protected record ApiFile { public string Name { get; set; } = ""; public string File { get; set; } = ""; }
    protected record RecipeRow { public string Active { get; set; } = "-"; public string SourceUsed { get; set; } = "-"; public string QuantityServing { get; set; } = "-"; public string RdaEu { get; set; } = "-"; }
    protected record FormatOption { public string Format { get; set; } = ""; }
    protected record AnalyticsRow
    {
        public string Active { get; set; } = "-";
        public string Quantity { get; set; } = "-";
        public string Source { get; set; } = "-";
        public bool Analytics { get; set; }
        public string Periodicity { get; set; } = "-";
        public string Observations { get; set; } = "-";
    }

    protected record OrderDto
    {
        // Header
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? ProductName2 { get; set; }
        public int? MOQ { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? NutrisCode { get; set; }
        public DateTime? EstimatedDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public string? Country1 { get; set; }
        public string? Country2 { get; set; }
        public string? Country3 { get; set; }
        public List<ApiFile>? ReportFiles { get; set; }

        // Formulation
        public int PercentFilledFormulation { get; set; }
        public string? GummyBn { get; set; }
        public string? GummyB { get; set; }
        public string? Shape { get; set; }
        public List<RecipeRow>? Recipe { get; set; }
        public string? NutrisComments { get; set; }
        public bool TakeSample { get; set; }
        public decimal TakeSamplePrice { get; set; }

        // Packaging
        public int PercentFilledBottle { get; set; }
        public string? BottleType { get; set; }
        public string? BottleDesc { get; set; }
        public string? BottleImg { get; set; }
        public List<FormatOption>? BatchFormats { get; set; }
        public List<FormatOption>? BbdFormats { get; set; }
        public string? FillingBatch { get; set; }
        public string? FillingExpDate { get; set; }
        public string? FillingLocation { get; set; }

        // Label
        public int PercentFilledLabel { get; set; }
        public bool NoLabel { get; set; }
        public string? GummyDnaDesc { get; set; }
        public string? LabelImageUrl { get; set; }

        // Palettizing
        public int PercentFilledPalettizing { get; set; }
        public string? PalletBoxName { get; set; }
        public string? BoxLabelImg { get; set; }
        public string? PalletizingInfo { get; set; }
        public string? PalletLabelImg { get; set; }
        public string? PalletComments { get; set; }

        // Analytics
        public int PercentFilledAnalytics { get; set; }
        public bool NoAnalytics { get; set; }
        public List<AnalyticsRow>? Analytics { get; set; }
        public decimal AnalyticsTotal { get; set; }
    }
}
