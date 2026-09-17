using Equaly.Models;

namespace Equaly.Services
{
    internal class BalanceSnapshot
    {
        public int Id { get; set; }
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
            var debtors = people
                .Where(p => p.Balance < 0)
                .Select(p => new BalanceSnapshot { Id = p.Id, Name = p.Name, Balance = p.Balance })
                .OrderBy(p => p.Balance)
                .ToList();

            var creditors = people
                .Where(p => p.Balance > 0)
                .Select(p => new BalanceSnapshot { Id = p.Id, Name = p.Name, Balance = p.Balance })
                .OrderByDescending(p => p.Balance)
                .ToList();

            int i = 0;
            int j = 0;

            // --- ADIM 2: Açgözlü (Greedy) eşleştirme ---
            while (i < debtors.Count && j < creditors.Count)
            {
                var debtor = debtors[i];
                var creditor = creditors[j];

                decimal amount = Math.Min(-debtor.Balance, creditor.Balance);
                amount = Math.Round(amount, 2);

                if (amount > 0)
                {
                    settlements.Add(new Settlement
                    {
                        FromPersonId = debtor.Id,
                        FromPersonName = debtor.Name,
                        ToPersonId = creditor.Id,
                        ToPersonName = creditor.Name,
                        Amount = amount
                    });

                    debtor.Balance += amount;
                    creditor.Balance -= amount;
                }

                if (Math.Abs(debtor.Balance) < 0.01m)
                    i++;

                if (Math.Abs(creditor.Balance) < 0.01m)
                    j++;
            }

            return settlements;
        }
    }
}