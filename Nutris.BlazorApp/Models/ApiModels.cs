using System;
using System.Text.Json.Serialization;

namespace NutrisBlazor.Models
{
    // DTO para PATCH requests (basado en callApiPatch.ts)
    public class PatchProductData
    {
        [JsonPropertyName("Bote_capacidad")]
        public string? BoteCapacidad { get; set; }

        [JsonPropertyName("Bote_boca")]
        public string? BoteBoca { get; set; }

        [JsonPropertyName("Bote_color")]
        public string? BoteColor { get; set; }

        [JsonPropertyName("Bote_material")]
        public string? BoteMaterial { get; set; }

        [JsonPropertyName("Label_size")]
        public string? LabelSize { get; set; }

        [JsonPropertyName("Label_material")]
        public string? LabelMaterial { get; set; }

        [JsonPropertyName("Label_finish")]
        public string? LabelFinish { get; set; }

        [JsonPropertyName("Label_Color")]
        public string? LabelColor { get; set; }

        [JsonPropertyName("Customer_accepted")]
        public bool? CustomerAccepted { get; set; }

        [JsonPropertyName("Tomar_muestra")]
        public bool? TomarMuestra { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("Product_name_2")]
        public string? ProductName2 { get; set; }

        [JsonPropertyName("Pallet_comments")]
        public string? PalletComments { get; set; }

        [JsonPropertyName("Filling_exp_date")]
        public string? FillingExpDate { get; set; }

        [JsonPropertyName("Filling_batch")]
        public string? FillingBatch { get; set; }

        [JsonPropertyName("Label_config")]
        public string? LabelConfig { get; set; }

        [JsonPropertyName("No_analitycs")]
        public bool? NoAnalitycs { get; set; }

        [JsonPropertyName("Filling_batch_others")]
        public string? FillingBatchOthers { get; set; }

        [JsonPropertyName("Filling_exp_date_others")]
        public string? FillingExpDateOthers { get; set; }

        [JsonPropertyName("Filling_batch_others_pack")]
        public string? FillingBatchOthersPack { get; set; }

        [JsonPropertyName("Filling_exp_date_others_pack")]
        public string? FillingExpDateOthersPack { get; set; }
    }

    // DTO para POST requests (basado en callApiPost.ts)
    public class PostImageData
    {
        [JsonPropertyName("numeroRG")]
        public string? NumeroRG { get; set; }

        [JsonPropertyName("tipoImagen")]
        public string? TipoImagen { get; set; }

        [JsonPropertyName("imagenBase64")]
        public string? ImagenBase64 { get; set; }

        [JsonPropertyName("numeroTroquel")]
        public string? NumeroTroquel { get; set; }
    }

    // Respuesta genérica de API
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }

    // Modelo base para respuestas con paginación
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; } = new List<T>();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }
    }
}