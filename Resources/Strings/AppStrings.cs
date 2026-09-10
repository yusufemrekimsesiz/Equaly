using System.Globalization;

namespace Equaly.Resources.Strings
{
    
    public static class AppStrings
    {
        private static bool IsTurkish =>
            CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase);

        
        public static string Add => IsTurkish ? "Ekle" : "Add";
        public static string Save => IsTurkish ? "Kaydet" : "Save";
        public static string Delete => IsTurkish ? "Sil" : "Delete";
        public static string Cancel => IsTurkish ? "Vazgeç" : "Cancel";
        public static string Ok => IsTurkish ? "Tamam" : "OK";
        public static string Edit => IsTurkish ? "Düzenle" : "Edit";

        
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

       
        public static string ExpensesTitle => IsTurkish ? "Harcamalar" : "Expenses";
        public static string NewExpenseButton => IsTurkish ? "+ Yeni Harcama" : "+ New Expense";
        public static string NoExpensesYet => IsTurkish ? "Henüz harcama yok" : "No expenses yet";
        public static string PaidByFormat(string name) => IsTurkish ? $"Ödeyen: {name}" : $"Paid by: {name}";
        public static string DeleteExpenseTitle => IsTurkish ? "Harcamayı Sil" : "Delete Expense";
        public static string DeleteExpenseConfirm(string description) => IsTurkish
            ? $"\"{description}\" harcamasını silmek istediğinize emin misiniz?"
            : $"Are you sure you want to delete \"{description}\"?";

        
        public static string SettlementsTitle => IsTurkish ? "Kim Kime Ne Kadar Ödeyecek?" : "Who Owes What?";
        public static string AllSettledUp => IsTurkish ? "Herkes ödeşmiş durumda!" : "Everyone is settled up!";
        public static string FromLabelFormat(string name) => IsTurkish ? $"Kimden: {name}" : $"From: {name}";
        public static string SettlementDisplay(string from, string to, decimal amount) => IsTurkish
            ? $"{from}, {to}'e {amount:0.##} TL ödemeli"
            : $"{from} owes {to} {amount:0.##} TL";

        
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