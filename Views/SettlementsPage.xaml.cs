using Equaly.ViewModels;

namespace Equaly.Views
{
    public partial class SettlementsPage : ContentPage
    {
        private readonly SettlementsViewModel _viewModel;

        public SettlementsPage(SettlementsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadSettlementsAsync();
        }
    }
}
