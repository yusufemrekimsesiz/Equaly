namespace Equaly.Models
{
    // Kategori anahtarları dil bağımsız sabitlerdir; veritabanında bu anahtarlar saklanır.
    // Görüntü adı ve emoji, AppStrings üzerinden dile göre çözülür.
    public static class ExpenseCategories
    {
        public const string Food = "food";
        public const string Transport = "transport";
        public const string Accommodation = "accommodation";
        public const string Entertainment = "entertainment";
        public const string Shopping = "shopping";
        public const string Bills = "bills";
        public const string Health = "health";
        public const string Other = "other";

        // "Ödendi" işaretlemesinde sistem tarafından otomatik oluşturulan ödeme kaydı için.
        // Kasıtlı olarak All listesine dahil değil — kullanıcı Picker'dan elle seçemez.
        public const string Payment = "payment";

        public static readonly string[] All =
        {
            Food, Transport, Accommodation, Entertainment, Shopping, Bills, Health, Other
        };
    }
}