using System.Globalization;

namespace Equaly.Resources.Strings
{
    // Merkezi yerelleştirme sınıfı. Dil seçimi otomatik: cihazın sistem dili KESİN OLARAK
    // Türkçe ise Türkçe, her türlü diğer durumda (İngilizce, başka bir dil, ya da kültür
    // bilgisi okunamıyorsa) İngilizce gösterilir. İngilizce her zaman güvenli varsayılandır.
    public static class AppStrings
    {
        private static bool IsTurkish
        {
            get
            {
                try
                {
                    // Hem CurrentUICulture hem CurrentCulture kontrol edilir; ikisi de
                    // "tr" değilse (ya da okunamıyorsa) İngilizce'ye düşülür.
                    var uiLang = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
                    var lang = uiLang ?? CultureInfo.CurrentCulture?.TwoLetterISOLanguageName;

                    return string.Equals(lang, "tr", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    // Kültür bilgisi okunurken herhangi bir sorun olursa güvenli varsayılan: İngilizce.
                    return false;
                }
            }
        }

        // Genel / ortak
        public static string Add => IsTurkish ? "Ekle" : "Add";
        public static string Save => IsTurkish ? "Kaydet" : "Save";
        public static string Delete => IsTurkish ? "Sil" : "Delete";
        public static string Cancel => IsTurkish ? "Vazgeç" : "Cancel";
        public static string Ok => IsTurkish ? "Tamam" : "OK";
        public static string Edit => IsTurkish ? "Düzenle" : "Edit";

        // PeoplePage
        public static string GroupBalancesTitle => IsTurkish ? "Grup Bakiyeleri" : "Group Balances";
        public static string NewPersonPlaceholder => IsTurkish ? "Yeni kişi adı" : "New person name";
        public static string NoPeopleYet => IsTurkish ? "Henüz kişi eklenmedi" : "No people added yet";
        public static string ExpensesNav => IsTurkish ? "Harcamalar" : "Expenses";
        public static string AddExpenseNav => IsTurkish ? "Harcama Ekle" : "Add Expense";
        public static string SettleUpNav => IsTurkish ? "Hesaplaş" : "Settle Up";

        public static string DeletePersonTitle => IsTurkish ? "Kişiyi Sil" : "Delete Person";
        public static string DeletePersonConfirm(string name) =>
            IsTurkish ? $"{name} silinsin mi?" : $"Delete {name}?";
        public static string CannotDeleteTitle => IsTurkish ? "Silinemedi" : "Couldn't Delete";
        public static string CannotAddTitle => IsTurkish ? "Eklenemedi" : "Couldn't Add";

        // AddExpensePage
        public static string NewExpenseTitle => IsTurkish ? "Yeni Harcama" : "New Expense";
        public static string EditExpenseTitle => IsTurkish ? "Harcamayı Düzenle" : "Edit Expense";
        public static string PayerLabel => IsTurkish ? "Ödeyen Kişi" : "Paid By";
        public static string SelectPersonPlaceholder => IsTurkish ? "Kişi seçin" : "Select person";
        public static string AmountLabel => IsTurkish ? "Tutar (TL)" : "Amount (TL)";
        public static string DescriptionLabel => IsTurkish ? "Açıklama" : "Description";
        public static string DescriptionPlaceholder => IsTurkish ? "Örn: Market alışverişi" : "e.g. Grocery shopping";
        public static string ParticipantsLabel => IsTurkish
            ? "Katılımcılar (harcamayı kimler paylaşacak)"
            : "Participants (who's splitting this)";
        public static string DefaultExpenseDescription => IsTurkish ? "Harcama" : "Expense";

        public static string SelectPayerError => IsTurkish ? "Lütfen ödeyen kişiyi seçin." : "Please select who paid.";
        public static string InvalidAmountError => IsTurkish ? "Lütfen geçerli bir tutar girin." : "Please enter a valid amount.";
        public static string AmountMustBePositiveError => IsTurkish ? "Tutar sıfırdan büyük olmalıdır." : "Amount must be greater than zero.";
        public static string SelectParticipantError => IsTurkish
            ? "Lütfen en az bir katılımcı seçin."
            : "Please select at least one participant.";

        // ExpensesPage
        public static string ExpensesTitle => IsTurkish ? "Harcamalar" : "Expenses";
        public static string NewExpenseButton => IsTurkish ? "+ Yeni Harcama" : "+ New Expense";
        public static string NoExpensesYet => IsTurkish ? "Henüz harcama yok" : "No expenses yet";
        public static string PaidByFormat(string name) => IsTurkish ? $"Ödeyen: {name}" : $"Paid by: {name}";
        public static string DeleteExpenseTitle => IsTurkish ? "Harcamayı Sil" : "Delete Expense";
        public static string DeleteExpenseConfirm(string description) => IsTurkish
            ? $"\"{description}\" harcamasını silmek istediğinize emin misiniz?"
            : $"Are you sure you want to delete \"{description}\"?";

        // SettlementsPage
        public static string SettlementsTitle => IsTurkish ? "Kim Kime Ne Kadar Ödeyecek?" : "Who Owes What?";
        public static string AllSettledUp => IsTurkish ? "Herkes ödeşmiş durumda!" : "Everyone is settled up!";
        public static string FromLabelFormat(string name) => IsTurkish ? $"Kimden: {name}" : $"From: {name}";
        public static string SettlementDisplay(string from, string to, decimal amount) => IsTurkish
            ? $"{from}, {to}'e {amount:0.##} TL ödemeli"
            : $"{from} owes {to} {amount:0.##} TL";

        // Groups
        public static string GroupsTitle => IsTurkish ? "Gruplarım" : "My Groups";
        public static string NewGroupPlaceholder => IsTurkish ? "Yeni grup adı" : "New group name";
        public static string NoGroupsYet => IsTurkish
            ? "Henüz grup yok. Başlamak için bir tane ekle!"
            : "No groups yet. Add one to get started!";
        public static string DeleteGroupTitle => IsTurkish ? "Grubu Sil" : "Delete Group";
        public static string DeleteGroupConfirm(string name) => IsTurkish
            ? $"\"{name}\" grubu ve içindeki tüm kişi/harcamalar kalıcı olarak silinecek. Emin misiniz?"
            : $"\"{name}\" and all its people/expenses will be permanently deleted. Are you sure?";
        public static string GroupNameEmptyError => IsTurkish ? "Grup adı boş olamaz." : "Group name cannot be empty.";
        public static string BackToGroupsNav => IsTurkish ? "Gruplar" : "Groups";

        // Expense Categories
        public static string CategoryDisplayName(string key) => key switch
        {
            "food" => IsTurkish ? "Yemek" : "Food",
            "transport" => IsTurkish ? "Ulaşım" : "Transport",
            "accommodation" => IsTurkish ? "Konaklama" : "Accommodation",
            "entertainment" => IsTurkish ? "Eğlence" : "Entertainment",
            "shopping" => IsTurkish ? "Alışveriş" : "Shopping",
            "bills" => IsTurkish ? "Fatura" : "Bills",
            "health" => IsTurkish ? "Sağlık" : "Health",
            "payment" => IsTurkish ? "Ödeme" : "Payment",
            _ => IsTurkish ? "Diğer" : "Other"
        };

        public static string CategoryEmoji(string key) => key switch
        {
            "food" => "🍔",
            "transport" => "🚗",
            "accommodation" => "🏠",
            "entertainment" => "🎬",
            "shopping" => "🛍️",
            "bills" => "🧾",
            "health" => "💊",
            "payment" => "💸",
            _ => "📦"
        };

        public static string CategoryLabel => IsTurkish ? "Kategori" : "Category";

        // Mark as Paid
        public static string MarkAsPaid => IsTurkish ? "Ödendi" : "Mark as Paid";
        public static string MarkAsPaidTitle => IsTurkish ? "Ödemeyi Kaydet" : "Record Payment";
        public static string MarkAsPaidConfirm(string from, string to, decimal amount) => IsTurkish
            ? $"{from}, {to}'e {amount:0.##} TL ödedi olarak işaretlensin mi?"
            : $"Mark that {from} paid {to} {amount:0.##} TL?";
        public static string PaymentDescription(string from, string to) => IsTurkish
            ? $"{from} → {to} ödeme"
            : $"{from} → {to} payment";

        // Undo (Geri Al)
        public static string Undo => IsTurkish ? "Geri Al" : "Undo";
        public static string PersonDeletedUndoMessage(string name) => IsTurkish ? $"{name} silindi" : $"{name} deleted";
        public static string ExpenseDeletedUndoMessage(string description) => IsTurkish
            ? $"\"{description}\" silindi"
            : $"\"{description}\" deleted";

        // Share
        public static string ShareSummary => IsTurkish ? "Özeti Paylaş" : "Share Summary";
        public static string ShareTitle => IsTurkish ? "Hesaplaşma Özeti" : "Settlement Summary";

        // DatabaseService doğrulama / hata mesajları
        public static string PersonNameEmptyError => IsTurkish ? "Kişi adı boş olamaz." : "Person name cannot be empty.";
        public static string DuplicatePersonError(string name) => IsTurkish
            ? $"\"{name}\" isimli bir kişi zaten mevcut."
            : $"A person named \"{name}\" already exists.";
        public static string BalanceNotZeroError(string name) => IsTurkish
            ? $"{name} kişisinin bakiyesi sıfır değil. Önce hesaplaşma yapılmalı."
            : $"{name}'s balance is not zero. Please settle up first.";
        public static string PersonHasPaidExpensesError(string name) => IsTurkish
            ? $"{name}, bir veya daha fazla harcamayı ödemiş görünüyor. Önce bu harcamaları silin veya düzenleyin."
            : $"{name} has paid for one or more expenses. Please delete or edit those expenses first.";
    }
}