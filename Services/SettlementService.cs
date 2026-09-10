using Equaly.Models;

namespace Equaly.Services
{
    internal class BalanceSnapshot
    {
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class SettlementService
    {
        public List<Settlement> CalculateSettlements(List<Person> people)
        {
            var settlements = new List<Settlement>();

            var debtors = people
                .Where(p => p.Balance < 0)
                .Select(p => new BalanceSnapshot { Name = p.Name, Balance = p.Balance })
                .OrderBy(p => p.Balance)
                .ToList();

            var creditors = people
                .Where(p => p.Balance > 0)
                .Select(p => new BalanceSnapshot { Name = p.Name, Balance = p.Balance })
                .OrderByDescending(p => p.Balance)
                .ToList();

            int i = 0;
            int j = 0;

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
                        FromPersonName = debtor.Name,
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