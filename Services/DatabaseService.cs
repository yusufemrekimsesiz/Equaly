using SQLite;
using Equaly.Models;

namespace Equaly.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

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

        // Bir harcamaya dahil edilen kişilerin Id listesini döner.
        // Boş liste dönerse (eski kayıtlar için) "herkese eşit bölündü" anlamına gelir.
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

            var person = new Person
            {
                Name = name,
                Balance = 0
            };

            await _database.InsertAsync(person);

            // Kişi sayısı değiştiği için mevcut harcamaların payı da değişebilir.
            await RecalculateBalancesAsync();
        }

        // Kişiyi silmeye çalışır. Başarılıysa null, başarısızsa kullanıcıya gösterilecek hata mesajını döner.
        public async Task<string> DeletePersonAsync(Person person)
        {
            await InitAsync();

            // Bakiyesi sıfır değilse (henüz hesaplaşma tamamlanmamışsa) silmeye izin verme.
            if (Math.Abs(person.Balance) > 0.01m)
                return $"{person.Name} kişisinin bakiyesi sıfır değil. Önce hesaplaşma yapılmalı.";

            // Bu kişi herhangi bir harcamayı ödemişse, o kayıtlar yetim (orphan) kalmasın diye silmeye izin verme.
            var paidExpenses = await _database.Table<Expense>().Where(e => e.PayerId == person.Id).ToListAsync();
            if (paidExpenses.Count > 0)
                return $"{person.Name}, bir veya daha fazla harcamayı ödemiş görünüyor. Önce bu harcamaları silin veya düzenleyin.";

            // Kişinin katıldığı diğer harcamalardaki katılımcı bağlantılarını temizle.
            var participantLinks = await _database.Table<ExpenseParticipant>().Where(p => p.PersonId == person.Id).ToListAsync();
            foreach (var link in participantLinks)
                await _database.DeleteAsync(link);

            await _database.DeleteAsync(person);
            await RecalculateBalancesAsync();

            return null;
        }

        public async Task AddExpenseAsync(Expense expense, List<int> participantPersonIds)
        {
            await InitAsync();

            await _database.InsertAsync(expense);
            await SaveParticipantsAsync(expense.Id, participantPersonIds);

            await RecalculateBalancesAsync();
        }

        public async Task UpdateExpenseAsync(Expense expense, List<int> participantPersonIds)
        {
            await InitAsync();

            await _database.UpdateAsync(expense);

            var oldLinks = await _database.Table<ExpenseParticipant>().Where(p => p.ExpenseId == expense.Id).ToListAsync();
            foreach (var link in oldLinks)
                await _database.DeleteAsync(link);

            await SaveParticipantsAsync(expense.Id, participantPersonIds);

            await RecalculateBalancesAsync();
        }

        public async Task DeleteExpenseAsync(Expense expense)
        {
            await InitAsync();

            var links = await _database.Table<ExpenseParticipant>().Where(p => p.ExpenseId == expense.Id).ToListAsync();
            foreach (var link in links)
                await _database.DeleteAsync(link);

            await _database.DeleteAsync(expense);

            await RecalculateBalancesAsync();
        }

        private async Task SaveParticipantsAsync(int expenseId, List<int> participantPersonIds)
        {
            foreach (var personId in participantPersonIds)
            {
                await _database.InsertAsync(new ExpenseParticipant
                {
                    ExpenseId = expenseId,
                    PersonId = personId
                });
            }
        }

        // Tüm harcamaları baştan tarayarak her kişinin net bakiyesini yeniden hesaplar.
        // Her harcama SADECE kendi katılımcı listesindeki kişiler arasında eşit paylaştırılır.
        // Katılımcı listesi boşsa (eski/basit kayıtlar), geriye dönük uyumluluk için
        // gruptaki HERKESE eşit bölünür.
        public async Task RecalculateBalancesAsync()
        {
            var people = await _database.Table<Person>().ToListAsync();
            var expenses = await _database.Table<Expense>().ToListAsync();
            var allLinks = await _database.Table<ExpenseParticipant>().ToListAsync();

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
                    : people; // katılımcı belirtilmemişse herkese eşit bölünür

                if (participants.Count == 0)
                    continue;

                var share = expense.TotalAmount / participants.Count;

                // Sadece katılımcılar kendi payı kadar borçlanır.
                foreach (var participant in participants)
                    participant.Balance -= share;

                // Ödeyen kişi harcamanın tamamı kadar alacaklanır.
                var payer = people.FirstOrDefault(p => p.Id == expense.PayerId);
                if (payer is not null)
                    payer.Balance += expense.TotalAmount;
            }

            foreach (var person in people)
                await _database.UpdateAsync(person);
        }
    }
}
