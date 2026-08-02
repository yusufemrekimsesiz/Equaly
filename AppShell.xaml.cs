using Equaly.Views;

namespace Equaly
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AddExpensePage), typeof(AddExpensePage));
            Routing.RegisterRoute(nameof(SettlementsPage), typeof(SettlementsPage));
            Routing.RegisterRoute(nameof(ExpensesPage), typeof(ExpensesPage));
        }
    }
}
