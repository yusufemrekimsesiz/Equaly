using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Resources.Strings;
using Equaly.Services;
using Equaly.Views;

namespace Equaly.ViewModels
{
    public class ExpenseListItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PayerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string PayerDisplay => AppStrings.PaidByFormat(PayerName);
    }

    public partial class ExpensesViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<ExpenseListItem> Expenses { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        public ExpensesViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadExpensesAsync()
        {
            IsBusy = true;

            try
            {
                var expenses = await _databaseService.GetExpensesAsync();
                var people = await _databaseService.GetPeopleAsync();

                Expenses.Clear();
                foreach (var expense in expenses)
                {
                    var payer = people.FirstOrDefault(p => p.Id == expense.PayerId);

                    Expenses.Add(new ExpenseListItem
                    {
                        Id = expense.Id,
                        Description = expense.Description,
                        Amount = expense.TotalAmount,
                        PayerName = payer?.Name ?? "-",
                        Date = expense.Date
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task EditExpenseAsync(ExpenseListItem item)
        {
            if (item is null)
                return;

            await Shell.Current.GoToAsync($"{nameof(AddExpensePage)}?expenseId={item.Id}");
        }

        [RelayCommand]
        private async Task DeleteExpenseAsync(ExpenseListItem item)
        {
            if (item is null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                AppStrings.DeleteExpenseTitle,
                AppStrings.DeleteExpenseConfirm(item.Description),
                AppStrings.Delete, AppStrings.Cancel);

            if (!confirm)
                return;

            var expense = new Expense { Id = item.Id };
            await _databaseService.DeleteExpenseAsync(expense);

            await LoadExpensesAsync();
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddExpensePage));
        }
    }
}