using Equaly.ViewModels;

namespace Equaly.Views
{
    public partial class AddExpensePage : ContentPage
    {
        private readonly AddExpenseViewModel _viewModel;

        public AddExpensePage(AddExpenseViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }
    }
}
