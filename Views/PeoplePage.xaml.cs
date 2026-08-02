using Equaly.ViewModels;

namespace Equaly.Views
{
    public partial class PeoplePage : ContentPage
    {
        private readonly PeopleViewModel _viewModel;

        public PeoplePage(PeopleViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadPeopleAsync();
        }
    }
}
