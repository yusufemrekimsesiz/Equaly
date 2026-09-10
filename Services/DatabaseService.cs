using SQLite;
using Equaly.Models;
using Equaly.Resources.Strings;

namespace Equaly.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private async Task InitAsync()
        {
            if (_database is not null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "equaly.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<Person>();
            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<ExpenseParticipant>();
        }

        public async Task<List<Person>> GetPeopleAsync()
        {
            await InitAsync();
            return await _database.Table<Person>().ToListAsync();
        }

        public async Task<List<Expense>> GetExpensesAsync()
        {
            await InitAsync();
            var expenses = await _database.Table<Expense>().ToListAsync();
            return expenses.OrderByDescending(e => e.Date).ToList();
        }

        public async Task<Expense> GetExpenseByIdAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Expense>().Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetParticipantIdsAsync(int expenseId)
        {
            await InitAsync();
            var rows = await _database.Table<ExpenseParticipant>()
                .Where(p => p.ExpenseId == expenseId)
                .ToListAsync();

            return rows.Select(r => r.PersonId).ToList();
        }

        public async Task AddPersonAsync(string name)
        {
            await InitAsync();

            var trimmedName = name.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new InvalidOperationException(AppStrings.PersonNameEmptyError);

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    var people = conn.Table<Person>().ToList();

                    var duplicate = people.Any(p =>
                        string.Equals(p.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));

                    if (duplicate)
                        throw new InvalidOperationException(AppStrings.DuplicatePersonError(trimmedName));

                    conn.Insert(new Person { Name = trimmedName, Balance = 0 });

                    RecalculateBalancesSync(conn);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<string> DeletePersonAsync(Person person)
        {
            await InitAsync();

            if (Math.Abs(person.Balance) > 0.01m)
                return AppStrings.BalanceNotZeroError(person.Name);

            var paidExpenses = await _database.Table<Expense>().Where(e => e.PayerId == person.Id).ToListAsync();
            if (paidExpenses.Count > 0)
                return AppStrings.PersonHasPaidExpensesError(person.Name);

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    var participantLinks = conn.Table<ExpenseParticipant>()
                        .Where(p => p.PersonId == person.Id)
                        .ToList();

                    foreach (var link in participantLinks)
                        conn.Delete(link);

                    conn.Delete(person);

                    RecalculateBalancesSync(conn);
                });
            }
            finally
            {
                _writeLock.Release();
            }

            return null;
        }

        public async Task AddExpenseAsync(Expense expense, List<int> participantPersonIds)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    conn.Insert(expense);

                    foreach (var personId in participantPersonIds)
                    {
                        conn.Insert(new ExpenseParticipant
                        {
                            ExpenseId = expense.Id,
                            PersonId = personId
                        });
                    }

                    RecalculateBalancesSync(conn);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task UpdateExpenseAsync(Expense expense, List<int> participantPersonIds)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    conn.Update(expense);

                    var oldLinks = conn.Table<ExpenseParticipant>()
                        .Where(p => p.ExpenseId == expense.Id)
                        .ToList();

                    foreach (var link in oldLinks)
                        conn.Delete(link);

                    foreach (var personId in participantPersonIds)
                    {
                        conn.Insert(new ExpenseParticipant
                        {
                            ExpenseId = expense.Id,
                            PersonId = personId
                        });
                    }

                    RecalculateBalancesSync(conn);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task DeleteExpenseAsync(Expense expense)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    var links = conn.Table<ExpenseParticipant>()
                        .Where(p => p.ExpenseId == expense.Id)
                        .ToList();

                    foreach (var link in links)
                        conn.Delete(link);

                    conn.Delete(expense);

                    RecalculateBalancesSync(conn);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task RecalculateBalancesAsync()
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn => RecalculateBalancesSync(conn));
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private static void RecalculateBalancesSync(SQLiteConnection conn)
        {
            var people = conn.Table<Person>().ToList();
            var expenses = conn.Table<Expense>().ToList();
            var allLinks = conn.Table<ExpenseParticipant>().ToList();

            if (people.Count == 0)
                return;

            foreach (var person in people)
                person.Balance = 0;

            foreach (var expense in expenses)
            {
                var participantIds = allLinks
                    .Where(l => l.ExpenseId == expense.Id)
                    .Select(l => l.PersonId)
                    .ToList();

                var participants = participantIds.Count > 0
                    ? people.Where(p => participantIds.Contains(p.Id)).ToList()
                    : people;

                if (participants.Count == 0)
                    continue;

                var share = expense.TotalAmount / participants.Count;

                foreach (var participant in participants)
                    participant.Balance -= share;

                var payer = people.FirstOrDefault(p => p.Id == expense.PayerId);
                if (payer is not null)
                    payer.Balance += expense.TotalAmount;
            }

            foreach (var person in people)
                conn.Update(person);
        }
    }
}