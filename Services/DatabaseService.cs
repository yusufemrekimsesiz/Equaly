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

            await _database.CreateTableAsync<Group>();
            await _database.CreateTableAsync<Person>();
            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<ExpenseParticipant>();
        }

        // ---------------- GRUPLAR ----------------

        public async Task<List<Group>> GetGroupsAsync()
        {
            await InitAsync();
            var groups = await _database.Table<Group>().ToListAsync();
            return groups.OrderBy(g => g.CreatedDate).ToList();
        }

        public async Task<Group> AddGroupAsync(string name)
        {
            await InitAsync();

            var trimmedName = name.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                throw new InvalidOperationException(AppStrings.GroupNameEmptyError);

            var group = new Group { Name = trimmedName };

            await _writeLock.WaitAsync();
            try
            {
                await _database.InsertAsync(group);
            }
            finally
            {
                _writeLock.Release();
            }

            return group;
        }

        // Grubu ve içindeki TÜM kişi/harcama/katılımcı kayıtlarını kalıcı olarak siler.
        public async Task DeleteGroupAsync(Group group)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    var expenseIds = conn.Table<Expense>()
                        .Where(e => e.GroupId == group.Id)
                        .ToList()
                        .Select(e => e.Id)
                        .ToList();

                    foreach (var expenseId in expenseIds)
                    {
                        var links = conn.Table<ExpenseParticipant>().Where(p => p.ExpenseId == expenseId).ToList();
                        foreach (var link in links)
                            conn.Delete(link);
                    }

                    conn.Table<Expense>().Delete(e => e.GroupId == group.Id);
                    conn.Table<Person>().Delete(p => p.GroupId == group.Id);
                    conn.Delete(group);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // ---------------- KİŞİLER (gruba göre) ----------------

        public async Task<List<Person>> GetPeopleAsync(int groupId)
        {
            await InitAsync();
            return await _database.Table<Person>().Where(p => p.GroupId == groupId).ToListAsync();
        }

        public async Task<List<Expense>> GetExpensesAsync(int groupId)
        {
            await InitAsync();
            var expenses = await _database.Table<Expense>().Where(e => e.GroupId == groupId).ToListAsync();
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

        public async Task AddPersonAsync(int groupId, string name)
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
                    var people = conn.Table<Person>().Where(p => p.GroupId == groupId).ToList();

                    var duplicate = people.Any(p =>
                        string.Equals(p.Name.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));

                    if (duplicate)
                        throw new InvalidOperationException(AppStrings.DuplicatePersonError(trimmedName));

                    conn.Insert(new Person { GroupId = groupId, Name = trimmedName, Balance = 0 });

                    RecalculateBalancesSync(conn, groupId);
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

                    RecalculateBalancesSync(conn, person.GroupId);
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

                    RecalculateBalancesSync(conn, expense.GroupId);
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

                    RecalculateBalancesSync(conn, expense.GroupId);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // Not: dışarıdan gelen 'expense' parametresi sadece Id taşıyan eksik bir nesne olabilir
        // (örn. UI listesinden silme). GroupId'yi HER ZAMAN veritabanından tazeden okuyoruz,
        // aksi halde yanlış (sıfır/varsayılan) bir GroupId ile bakiyeler yanlış grup için
        // yeniden hesaplanır ve gerçek grubun bakiyeleri hiç güncellenmemiş kalır.
        public async Task DeleteExpenseAsync(Expense expense)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn =>
                {
                    var actualExpense = conn.Table<Expense>().Where(e => e.Id == expense.Id).FirstOrDefault();
                    if (actualExpense is null)
                        return;

                    var links = conn.Table<ExpenseParticipant>()
                        .Where(p => p.ExpenseId == actualExpense.Id)
                        .ToList();

                    foreach (var link in links)
                        conn.Delete(link);

                    conn.Delete(actualExpense);

                    RecalculateBalancesSync(conn, actualExpense.GroupId);
                });
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task RecalculateBalancesAsync(int groupId)
        {
            await InitAsync();

            await _writeLock.WaitAsync();
            try
            {
                await _database.RunInTransactionAsync(conn => RecalculateBalancesSync(conn, groupId));
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // Artık SADECE bir grubun içindeki kişi/harcamaları tarar (gruplar birbirinden
        // tamamen izole, bir gruptaki harcama başka bir grubun bakiyesini etkilemez).
        private static void RecalculateBalancesSync(SQLiteConnection conn, int groupId)
        {
            var people = conn.Table<Person>().Where(p => p.GroupId == groupId).ToList();
            var expenses = conn.Table<Expense>().Where(e => e.GroupId == groupId).ToList();

            if (people.Count == 0)
                return;

            var expenseIds = expenses.Select(e => e.Id).ToList();
            var allLinks = conn.Table<ExpenseParticipant>().ToList()
                .Where(l => expenseIds.Contains(l.ExpenseId))
                .ToList();

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