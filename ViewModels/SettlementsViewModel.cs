using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equaly.Models;
using Equaly.Resources.Strings;
using Equaly.Services;
using Microsoft.Maui.ApplicationModel.DataTransfer;

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
                var people = await _databaseService.GetPeopleAsync(AppSession.CurrentGroupId);
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

        // Bir hesaplaşma satırını "ödendi" olarak işaretler. Bunu gerçekleştirmek için
        // yeni bir tablo/alan eklemeye gerek yok: borçlu kişinin ödeyen (payer), alacaklı
        // kişinin ise TEK katılımcı olduğu özel bir "ödeme" harcaması oluşturuyoruz.
        // Bu harcama kaydedilince mevcut RecalculateBalancesAsync mantığı otomatik olarak
        // borcu tam olarak kapatıyor (payer tam tutar kadar alacaklanır, tek katılımcı
        // tam tutar kadar borçlanır — net etki: borç sıfırlanır).
        [RelayCommand]
        private async Task MarkAsPaidAsync(Settlement settlement)
        {
            if (settlement is null)
                return;

            bool confirm = await Shell.Current.DisplayAlert(
                AppStrings.MarkAsPaidTitle,
                AppStrings.MarkAsPaidConfirm(settlement.FromPersonName, settlement.ToPersonName, settlement.Amount),
                AppStrings.Ok, AppStrings.Cancel);

            if (!confirm)
                return;

            var paymentExpense = new Expense
            {
                GroupId = AppSession.CurrentGroupId,
                PayerId = settlement.FromPersonId,
                TotalAmount = settlement.Amount,
                Category = ExpenseCategories.Payment,
                Description = AppStrings.PaymentDescription(settlement.FromPersonName, settlement.ToPersonName)
            };

            // Katılımcı listesinde SADECE alacaklı kişi var — bu yüzden tüm tutar
            // yalnızca onun payına yazılır (borcu tam kapanır), ödeyen kişi ise
            // ödediği tutar kadar alacaklanır (kendi borcu o kadar azalır).
            var participantIds = new List<int> { settlement.ToPersonId };

            await _databaseService.AddExpenseAsync(paymentExpense, participantIds);

            await LoadSettlementsAsync();
        }

        // Hesaplaşma listesini düz metin olarak WhatsApp/mesaj/e-posta gibi herhangi bir
        // uygulamaya paylaşır. .NET MAUI'nin yerleşik Share API'si kullanılıyor — ekstra
        // paket ya da dosya izni gerektirmiyor (sadece metin paylaşımı).
        [RelayCommand]
        private async Task ShareSummaryAsync()
        {
            if (Settlements.Count == 0)
                return;

            var builder = new StringBuilder();
            builder.AppendLine(AppStrings.ShareTitle);
            builder.AppendLine();

            foreach (var settlement in Settlements)
                builder.AppendLine(settlement.DisplayText);

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = AppStrings.ShareTitle,
                Text = builder.ToString().Trim()
            });
        }
    }
}