using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Services;

namespace Equaly.ViewModels
{
    [QueryProperty(nameof(ExpenseId), "expenseId")]
    public partial class AddExpenseViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        // Ödeyen kişi seçimi için Picker'a bağlanan liste
        public ObservableCollection<Person> People { get; } = new();

        // Katılımcı çoklu-seçim CollectionView'ine bağlanan liste (SelectedItems object gerektirir)
        public ObservableCollection<object> SelectedParticipants { get; } = new();

        [ObservableProperty]
        private Person selectedPayer;

        [ObservableProperty]
        private string amount = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string pageTitle = "Yeni Harcama";

        [ObservableProperty]
        private string saveButtonText = "Ekle";

        // Shell navigasyonundan gelen sorgu parametresi.
        // 0 ise yeni harcama, 0'dan farklıysa düzenleme modu.
        [ObservableProperty]
        private int expenseId;

        private int _loadedExpenseId;

        public AddExpenseViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            var people = await _databaseService.GetPeopleAsync();

            People.Clear();
            foreach (var person in people)
                People.Add(person);

            if (ExpenseId > 0)
            {
                await LoadForEditAsync(ExpenseId);
            }
            else
            {
                ResetForNewExpense();
            }
        }

        private void ResetForNewExpense()
        {
            _loadedExpenseId = 0;
            SelectedPayer = null;
            Amount = string.Empty;
            Description = string.Empty;
            ErrorMessage = string.Empty;

            // Yeni harcamada varsayılan olarak herkes katılımcı kabul edilir.
            SelectedParticipants.Clear();
            foreach (var person in People)
                SelectedParticipants.Add(person);

            PageTitle = "Yeni Harcama";
            SaveButtonText = "Ekle";
        }

        private async Task LoadForEditAsync(int id)
        {
            var expense = await _databaseService.GetExpenseByIdAsync(id);
            if (expense is null)
                return;

            _loadedExpenseId = expense.Id;

            SelectedPayer = People.FirstOrDefault(p => p.Id == expense.PayerId);
            Amount = expense.TotalAmount.ToString(CultureInfo.InvariantCulture);
            Description = expense.Description;

            var participantIds = await _databaseService.GetParticipantIdsAsync(id);

            SelectedParticipants.Clear();
            var participantsToSelect = participantIds.Count > 0
                ? People.Where(p => participantIds.Contains(p.Id))
                : People; // eski kayıt: katılımcı belirtilmemişse herkes seçili gösterilir

            foreach (var person in participantsToSelect)
                SelectedParticipants.Add(person);

            PageTitle = "Harcamayı Düzenle";
            SaveButtonText = "Kaydet";
        }

        [RelayCommand]
        private async Task SaveExpenseAsync()
        {
            ErrorMessage = string.Empty;

            if (SelectedPayer is null)
            {
                ErrorMessage = "Lütfen ödeyen kişiyi seçin.";
                return;
            }

            if (!decimal.TryParse(Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount)
                && !decimal.TryParse(Amount, out parsedAmount))
            {
                ErrorMessage = "Lütfen geçerli bir tutar girin.";
                return;
            }

            if (parsedAmount <= 0)
            {
                ErrorMessage = "Tutar sıfırdan büyük olmalıdır.";
                return;
            }

            if (SelectedParticipants.Count == 0)
            {
                ErrorMessage = "Lütfen en az bir katılımcı seçin.";
                return;
            }

            var participantIds = SelectedParticipants.OfType<Person>().Select(p => p.Id).ToList();

            if (_loadedExpenseId > 0)
            {
                var expense = new Expense
                {
                    Id = _loadedExpenseId,
                    PayerId = SelectedPayer.Id,
                    TotalAmount = parsedAmount,
                    Description = string.IsNullOrWhiteSpace(Description) ? "Harcama" : Description.Trim()
                };

                await _databaseService.UpdateExpenseAsync(expense, participantIds);
            }
            else
            {
                var expense = new Expense
                {
                    PayerId = SelectedPayer.Id,
                    TotalAmount = parsedAmount,
                    Description = string.IsNullOrWhiteSpace(Description) ? "Harcama" : Description.Trim()
                };

                await _databaseService.AddExpenseAsync(expense, participantIds);
            }

            ExpenseId = 0;
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            ExpenseId = 0;
            await Shell.Current.GoToAsync("..");
        }
    }
}
