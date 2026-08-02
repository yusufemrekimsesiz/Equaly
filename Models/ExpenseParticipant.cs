using SQLite;

namespace Equaly.Models
{
    public class ExpenseParticipant
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ExpenseId { get; set; }

        public int PersonId { get; set; }
    }
}
