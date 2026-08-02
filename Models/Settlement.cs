namespace Equaly.Models
{
    public class Settlement
    {
        public string FromPersonName { get; set; } = string.Empty;

        public string ToPersonName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string DisplayText => $"{FromPersonName}, {ToPersonName}'e {Amount:0.##} TL ödemeli";
    }
}
