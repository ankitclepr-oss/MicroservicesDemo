namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        public int Id { get; private set; } // DB generated
        public int OrderId { get; private set; }
        public double Amount { get; private set; }

        private Payment() { } // EF

        public Payment(int orderId, double amount)
        {
            if (amount <= 0)
                throw new Exception("Invalid payment amount");

            OrderId = orderId;
            Amount = amount;
        }
    }
}
