using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Services;

namespace Equaly.ViewModels
{
    public partial class SettlementsViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;
        private readonly SettlementService _settlementService;

        public ObservableCollection<Settlement> Settlements { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasNoSettlements;

        public SettlementsViewModel(DatabaseService databaseService, SettlementService settlementService)
        {
            _databaseService = databaseService;
            _settlementService = settlementService;
        }

        [RelayCommand]
        public async Task LoadSettlementsAsync()
        {
            IsBusy = true;

            try
            {
                var people = await _databaseService.GetPeopleAsync();
                var result = _settlementService.CalculateSettlements(people);

                Settlements.Clear();
                foreach (var settlement in result)
                    Settlements.Add(settlement);

                HasNoSettlements = Settlements.Count == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
