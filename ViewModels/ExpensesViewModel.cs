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
        public int GroupId { get; set; }
        public int PayerId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PayerName { get; set; } = string.Empty;
        public string Category { get; set; } = ExpenseCategories.Other;
        public DateTime Date { get; set; }

        public string PayerDisplay => AppStrings.PaidByFormat(PayerName);
        public string CategoryEmoji => AppStrings.CategoryEmoji(Category);
        public string CategoryDisplayName => AppStrings.CategoryDisplayName(Category);
    }

    // Silinen bir harcamayı Geri Al ile aynen yeniden oluşturabilmek için gereken
    // tüm bilgileri (katılımcılar dahil) geçici olarak bellekte tutan anlık görüntü.
    internal class DeletedExpenseSnapshot
    {
        public int GroupId { get; set; }
        public int PayerId { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; } = ExpenseCategories.Other;
        public string Description { get; set; } = string.Empty;
        public List<int> ParticipantIds { get; set; } = new();
    }

    public partial class ExpensesViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<ExpenseListItem> Expenses { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool showUndoBanner;

        [ObservableProperty]
        private string undoMessage = string.Empty;

        private DeletedExpenseSnapshot _lastDeletedExpense;
        private CancellationTokenSource _undoCts;

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
                var expenses = await _databaseService.GetExpensesAsync(AppSession.CurrentGroupId);
                var people = await _databaseService.GetPeopleAsync(AppSession.CurrentGroupId);

                Expenses.Clear();
                foreach (var expense in expenses)
                {
                    var payer = people.FirstOrDefault(p => p.Id == expense.PayerId);

                    Expenses.Add(new ExpenseListItem
                    {
                        Id = expense.Id,
                        GroupId = expense.GroupId,
                        PayerId = expense.PayerId,
                        Description = expense.Description,
                        Amount = expense.TotalAmount,
                        PayerName = payer?.Name ?? "-",
                        Category = string.IsNullOrWhiteSpace(expense.Category) ? ExpenseCategories.Other : expense.Category,
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

            // Silmeden ÖNCE katılımcı listesini kaydet — silindikten sonra bu bilgi
            // veritabanından tamamen kalkıyor, Geri Al için buna ihtiyacımız var.
            var participantIds = await _databaseService.GetParticipantIdsAsync(item.Id);

            _lastDeletedExpense = new DeletedExpenseSnapshot
            {
                GroupId = item.GroupId,
                PayerId = item.PayerId,
                Amount = item.Amount,
                Category = item.Category,
                Description = item.Description,
                ParticipantIds = participantIds
            };

            var expense = new Expense { Id = item.Id };
            await _databaseService.DeleteExpenseAsync(expense);

            UndoMessage = AppStrings.ExpenseDeletedUndoMessage(item.Description);
            ShowUndoBanner = true;

            _undoCts?.Cancel();
            _undoCts = new CancellationTokenSource();
            _ = HideUndoBannerAfterDelayAsync(_undoCts.Token);

            await LoadExpensesAsync();
        }

        private async Task HideUndoBannerAfterDelayAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(5000, token);
                ShowUndoBanner = false;
            }
            catch (TaskCanceledException)
            {
            }
        }

        [RelayCommand]
        private async Task UndoDeleteExpenseAsync()
        {
            _undoCts?.Cancel();
            ShowUndoBanner = false;

            if (_lastDeletedExpense is null)
                return;

            var snapshot = _lastDeletedExpense;
            _lastDeletedExpense = null;

            var expense = new Expense
            {
                GroupId = snapshot.GroupId,
                PayerId = snapshot.PayerId,
                TotalAmount = snapshot.Amount,
                Category = snapshot.Category,
                Description = snapshot.Description
            };

            await _databaseService.AddExpenseAsync(expense, snapshot.ParticipantIds);
            await LoadExpensesAsync();
        }

        [RelayCommand]
        private async Task AddNewExpenseAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddExpensePage));
        }
    }
}