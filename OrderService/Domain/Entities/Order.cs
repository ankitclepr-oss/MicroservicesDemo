namespace OrderService.Domain.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public double Amount { get; private set; }

        // ✅ Required by EF
        private Order() { }

        // ✅ REMOVE Id from constructor
        public Order(double amount)
        {
            if (amount <= 0)
                throw new Exception("Invalid amount");

            Amount = amount;
        }
    }
}
