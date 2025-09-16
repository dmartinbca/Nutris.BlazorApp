namespace NutrisBlazor.Models
{
    public class LabelOption
    {
        public int ID { get; set; }
        public string Value { get; set; } = "";
    }

    public class LabelOptionMX : LabelOption
    {
        public string Imagen { get; set; } = "";
    }

    public class SelectedLabelOptions
    {
        public string? LabelSize { get; set; }
        public string? LabelMaterial { get; set; }
        public string? LabelFinish { get; set; }
        public string? LabelColors { get; set; }
    }

    public class LabelOptionsUpdatedEventArgs
    {
        public object? Response { get; set; }
        public object? SentData { get; set; }
    }
}
