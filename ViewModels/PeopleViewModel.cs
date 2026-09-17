using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Resources.Strings;
using Equaly.Services;
using Equaly.Views;

namespace Equaly.ViewModels
{
    public partial class PeopleViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Person> People { get; } = new();

        [ObservableProperty]
        private string newPersonName = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool showUndoBanner;

        [ObservableProperty]
        private string undoMessage = string.Empty;

        private string _lastDeletedPersonName;
        private CancellationTokenSource _undoCts;

        public PeopleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadPeopleAsync()
        {
            if (IsBusy)
                return;

            if (!AppSession.HasSelectedGroup)
            {
                await Shell.Current.GoToAsync($"//{nameof(GroupsPage)}");
                return;
            }

            IsBusy = true;

            try
            {
                var people = await _databaseService.GetPeopleAsync(AppSession.CurrentGroupId);

                People.Clear();
                foreach (var person in people)
                    People.Add(person);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddPersonAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPersonName))
                return;

            try
            {
                await _databaseService.AddPersonAsync(AppSession.CurrentGroupId, NewPersonName);
                NewPersonName = string.Empty;
                await LoadPeopleAsync();
            }
            catch (InvalidOperationException ex)
            {
                await Shell.Current.DisplayAlert(AppStrings.CannotAddTitle, ex.Message, AppStrings.Ok);
            }
        }

        [RelayCommand]
        private async Task GoToAddExpenseAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddExpensePage));
        }

        [RelayCommand]
        private async Task GoToSettlementsAsync()
        {
            await Shell.Current.GoToAsync(nameof(SettlementsPage));
        }

        [RelayCommand]
        private async Task GoToExpensesAsync()
        {
            await Shell.Current.GoToAsync(nameof(ExpensesPage));
        }

        [RelayCommand]
        private async Task GoToGroupsAsync()
        {
            await Shell.Current.GoToAsync($"//{nameof(GroupsPage)}");
        }

        [RelayCommand]
        private async Task DeletePersonAsync(Person person)
        {
            if (person is null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                AppStrings.DeletePersonTitle,
                AppStrings.DeletePersonConfirm(person.Name),
                AppStrings.Delete, AppStrings.Cancel);

            if (!confirm)
                return;

            var error = await _databaseService.DeletePersonAsync(person);

            if (error is not null)
            {
                await Shell.Current.DisplayAlert(AppStrings.CannotDeleteTitle, error, AppStrings.Ok);
                return;
            }

            // Geri Al bandını göster ve birkaç saniye sonra otomatik gizle.
            _lastDeletedPersonName = person.Name;
            UndoMessage = AppStrings.PersonDeletedUndoMessage(person.Name);
            ShowUndoBanner = true;

            _undoCts?.Cancel();
            _undoCts = new CancellationTokenSource();
            _ = HideUndoBannerAfterDelayAsync(_undoCts.Token);

            await LoadPeopleAsync();
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
                // Kullanıcı Geri Al'a bastı ya da başka bir silme işlemi zamanlayıcıyı iptal etti.
            }
        }

        [RelayCommand]
        private async Task UndoDeletePersonAsync()
        {
            _undoCts?.Cancel();
            ShowUndoBanner = false;

            if (string.IsNullOrEmpty(_lastDeletedPersonName))
                return;

            var nameToRestore = _lastDeletedPersonName;
            _lastDeletedPersonName = null;

            try
            {
                await _databaseService.AddPersonAsync(AppSession.CurrentGroupId, nameToRestore);
                await LoadPeopleAsync();
            }
            catch (InvalidOperationException ex)
            {
                await Shell.Current.DisplayAlert(AppStrings.CannotAddTitle, ex.Message, AppStrings.Ok);
            }
        }
    }
}