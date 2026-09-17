using Equaly.ViewModels;

namespace Equaly.Views
{
    public partial class GroupsPage : ContentPage
    {
        private readonly GroupsViewModel _viewModel;

        public GroupsPage(GroupsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadGroupsAsync();
        }
    }
}