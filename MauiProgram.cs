using Microsoft.Extensions.Logging;
using Equaly.Services;
using Equaly.ViewModels;
using Equaly.Views;

namespace Equaly
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<SettlementService>();

            builder.Services.AddTransient<GroupsPage>();
            builder.Services.AddTransient<GroupsViewModel>();

            builder.Services.AddTransient<PeoplePage>();
            builder.Services.AddTransient<PeopleViewModel>();

            builder.Services.AddTransient<AddExpensePage>();
            builder.Services.AddTransient<AddExpenseViewModel>();

            builder.Services.AddTransient<SettlementsPage>();
            builder.Services.AddTransient<SettlementsViewModel>();

            builder.Services.AddTransient<ExpensesPage>();
            builder.Services.AddTransient<ExpensesViewModel>();

            return builder.Build();
        }
    }
}