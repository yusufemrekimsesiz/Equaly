using SQLite;

namespace Equaly.Models
{
    public class Expense
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PayerId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
