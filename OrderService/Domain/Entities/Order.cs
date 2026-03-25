namespace OrderService.Domain.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public double Amount { get; private set; }

        // ✅ Required by EF
        private Order() { }

        public Order(int id, double amount)
        {
            if (amount <= 0)
                throw new Exception("Invalid amount");

            Id = id;
            Amount = amount;
        }
    }
}
