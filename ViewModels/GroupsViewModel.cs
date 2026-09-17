using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Resources.Strings;
using Equaly.Services;
using Equaly.Views;
using System.Collections.ObjectModel;

namespace Equaly.ViewModels
{
    public partial class GroupsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Group> Groups { get; } = new();

        [ObservableProperty]
        private string newGroupName = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public GroupsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadGroupsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                var groups = await _databaseService.GetGroupsAsync();

                Groups.Clear();
                foreach (var group in groups)
                    Groups.Add(group);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(NewGroupName))
                return;

            try
            {
                var group = await _databaseService.AddGroupAsync(NewGroupName);
                NewGroupName = string.Empty;
                await LoadGroupsAsync();

                
                await SelectGroupAsync(group);
            }
            catch (InvalidOperationException ex)
            {
                await Shell.Current.DisplayAlert(AppStrings.CannotAddTitle, ex.Message, AppStrings.Ok);
            }
        }

        [RelayCommand]
        private async Task SelectGroupAsync(Group group)
        {
            if (group is null)
                return;

            AppSession.CurrentGroupId = group.Id;
            await Shell.Current.GoToAsync(nameof(PeoplePage));
        }

        [RelayCommand]
        private async Task DeleteGroupAsync(Group group)
        {
            if (group is null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                AppStrings.DeleteGroupTitle,
                AppStrings.DeleteGroupConfirm(group.Name),
                AppStrings.Delete, AppStrings.Cancel);

            if (!confirm)
                return;

            await _databaseService.DeleteGroupAsync(group);

            
            if (AppSession.CurrentGroupId == group.Id)
                AppSession.CurrentGroupId = 0;

            await LoadGroupsAsync();
        }
    }
}
