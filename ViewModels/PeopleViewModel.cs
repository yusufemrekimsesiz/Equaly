using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
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

        public PeopleViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadPeopleAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                var people = await _databaseService.GetPeopleAsync();

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

            await _databaseService.AddPersonAsync(NewPersonName.Trim());

            NewPersonName = string.Empty;

            await LoadPeopleAsync();
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
        private async Task DeletePersonAsync(Person person)
        {
            if (person is null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Kişiyi Sil",
                $"{person.Name} silinsin mi?",
                "Sil", "Vazgeç");

            if (!confirm)
                return;

            var error = await _databaseService.DeletePersonAsync(person);

            if (error is not null)
            {
                await Shell.Current.DisplayAlert("Silinemedi", error, "Tamam");
                return;
            }

            await LoadPeopleAsync();
        }
    }
}
