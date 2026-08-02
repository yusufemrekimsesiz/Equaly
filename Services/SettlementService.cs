using Equaly.Models;

namespace Equaly.Services
{
    // Yardımcı iç sınıf: gerçek Person nesnelerini bozmadan bakiye üzerinde
    // simülasyon yapabilmek için kullanılır.
    internal class BalanceSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class SettlementService
    {
        // Minimum ödeşme (para transferi) listesini üretir.
        public List<Settlement> CalculateSettlements(List<Person> people)
        {
            var settlements = new List<Settlement>();

            // --- ADIM 1: Borçlular ve alacaklıları ayır ---
            // Bakiyesi negatif olanlar borçlu (başkasına ödemesi gereken),
            // bakiyesi pozitif olanlar alacaklıdır (birinden para alacak).
            // Orijinal Person listesini bozmamak için kopya (snapshot) alıyoruz.
            var debtors = people
                .Where(p => p.Balance < 0)
                .Select(p => new BalanceSnapshot { Name = p.Name, Balance = p.Balance })
                .OrderBy(p => p.Balance) // en negatif (en çok borçlu) başta
                .ToList();

            var creditors = people
                .Where(p => p.Balance > 0)
                .Select(p => new BalanceSnapshot { Name = p.Name, Balance = p.Balance })
                .OrderByDescending(p => p.Balance) // en yüksek alacak başta
                .ToList();

            int i = 0; // debtors işaretçisi
            int j = 0; // creditors işaretçisi

            // --- ADIM 2: Açgözlü (Greedy) eşleştirme ---
            // Mantık: Her adımda en büyük borçlu ile en büyük alacaklıyı eşleştir.
            // İkisinden küçük olan tutar kadar bir ödeme işlemi oluştur.
            // Bu, mümkün olan en az sayıda işlemle (transfer) tüm borçları kapatır,
            // çünkü her adımda en az bir kişi (borçlu ya da alacaklı) tamamen "sıfırlanır"
            // ve listeden çıkar.
            while (i < debtors.Count && j < creditors.Count)
            {
                var debtor = debtors[i];
                var creditor = creditors[j];

                // Borçlunun ödeyebileceği miktar (-Balance, pozitif değer)
                // ile alacaklının almayı beklediği miktardan küçük olanı seç.
                decimal amount = Math.Min(-debtor.Balance, creditor.Balance);
                amount = Math.Round(amount, 2);

                if (amount > 0)
                {
                    settlements.Add(new Settlement
                    {
                        FromPersonName = debtor.Name,
                        ToPersonName = creditor.Name,
                        Amount = amount
                    });

                    // İşlemi uygula: borçlunun borcu azalır, alacaklının alacağı azalır.
                    debtor.Balance += amount;
                    creditor.Balance -= amount;
                }

                // Borcu kapanan borçluyu bir sonraki borçluya geç.
                if (Math.Abs(debtor.Balance) < 0.01m)
                    i++;

                // Alacağı kapanan alacaklıyı bir sonraki alacaklıya geç.
                if (Math.Abs(creditor.Balance) < 0.01m)
                    j++;
            }

            return settlements;
        }
    }
}
