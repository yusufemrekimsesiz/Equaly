using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Resources.Strings;
using Equaly.Services;

namespace Equaly.ViewModels
{
    // Category Picker için gösterim modeli: Key veritabanına yazılır, DisplayName ekranda görünür.
    public class CategoryOption
    {
        public string Key { get; set; } = ExpenseCategories.Other;
        public string DisplayName { get; set; } = string.Empty;

        public override string ToString() => DisplayName;
    }

    [QueryProperty(nameof(ExpenseId), "expenseId")]
    public partial class AddExpenseViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Person> People { get; } = new();

        public ObservableCollection<object> SelectedParticipants { get; } = new();

        public ObservableCollection<CategoryOption> Categories { get; } = new();

        [ObservableProperty]
        private Person selectedPayer;

        [ObservableProperty]
        private CategoryOption selectedCategory;

        [ObservableProperty]
        private string amount = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string pageTitle = AppStrings.NewExpenseTitle;

        [ObservableProperty]
        private string saveButtonText = AppStrings.Add;

        [ObservableProperty]
        private int expenseId;

        private int _loadedExpenseId;

        public AddExpenseViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            foreach (var key in ExpenseCategories.All)
            {
                Categories.Add(new CategoryOption
                {
                    Key = key,
                    DisplayName = $"{AppStrings.CategoryEmoji(key)} {AppStrings.CategoryDisplayName(key)}"
                });
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            var people = await _databaseService.GetPeopleAsync(AppSession.CurrentGroupId);

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
            SelectedCategory = Categories.FirstOrDefault(c => c.Key == ExpenseCategories.Other);

            SelectedParticipants.Clear();
            foreach (var person in People)
                SelectedParticipants.Add(person);

            PageTitle = AppStrings.NewExpenseTitle;
            SaveButtonText = AppStrings.Add;
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
            SelectedCategory = Categories.FirstOrDefault(c => c.Key == expense.Category)
                                ?? Categories.FirstOrDefault(c => c.Key == ExpenseCategories.Other);

            var participantIds = await _databaseService.GetParticipantIdsAsync(id);

            SelectedParticipants.Clear();
            var participantsToSelect = participantIds.Count > 0
                ? People.Where(p => participantIds.Contains(p.Id))
                : People;

            foreach (var person in participantsToSelect)
                SelectedParticipants.Add(person);

            PageTitle = AppStrings.EditExpenseTitle;
            SaveButtonText = AppStrings.Save;
        }

        [RelayCommand]
        private async Task SaveExpenseAsync()
        {
            ErrorMessage = string.Empty;

            if (SelectedPayer is null)
            {
                ErrorMessage = AppStrings.SelectPayerError;
                return;
            }

            if (!decimal.TryParse(Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedAmount)
                && !decimal.TryParse(Amount, out parsedAmount))
            {
                ErrorMessage = AppStrings.InvalidAmountError;
                return;
            }

            if (parsedAmount <= 0)
            {
                ErrorMessage = AppStrings.AmountMustBePositiveError;
                return;
            }

            if (SelectedParticipants.Count == 0)
            {
                ErrorMessage = AppStrings.SelectParticipantError;
                return;
            }

            var participantIds = SelectedParticipants.OfType<Person>().Select(p => p.Id).ToList();
            var categoryKey = SelectedCategory?.Key ?? ExpenseCategories.Other;

            if (_loadedExpenseId > 0)
            {
                var expense = new Expense
                {
                    Id = _loadedExpenseId,
                    GroupId = AppSession.CurrentGroupId,
                    PayerId = SelectedPayer.Id,
                    TotalAmount = parsedAmount,
                    Category = categoryKey,
                    Description = string.IsNullOrWhiteSpace(Description) ? AppStrings.DefaultExpenseDescription : Description.Trim()
                };

                await _databaseService.UpdateExpenseAsync(expense, participantIds);
            }
            else
            {
                var expense = new Expense
                {
                    GroupId = AppSession.CurrentGroupId,
                    PayerId = SelectedPayer.Id,
                    TotalAmount = parsedAmount,
                    Category = categoryKey,
                    Description = string.IsNullOrWhiteSpace(Description) ? AppStrings.DefaultExpenseDescription : Description.Trim()
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