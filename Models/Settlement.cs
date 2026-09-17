using Equaly.Resources.Strings;

namespace Equaly.Models
{
    public class Settlement
    {
        public int FromPersonId { get; set; }
        public string FromPersonName { get; set; } = string.Empty;

        public int ToPersonId { get; set; }
        public string ToPersonName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string DisplayText => AppStrings.SettlementDisplay(FromPersonName, ToPersonName, Amount);

        public string FromLabelDisplay => AppStrings.FromLabelFormat(FromPersonName);
    }
}