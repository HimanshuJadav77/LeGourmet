using Restaurant_Management.Data;
using Restaurant_Management.Models.Entities;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly RestaurantDbContext _context;

        public PaymentGatewayService(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentTransaction> ProcessPaymentAsync(Order order, CheckoutViewModel checkoutModel)
        {
            // Simulate brief gateway authorization
            await Task.Delay(300);

            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var reference = $"TXN-{timestamp:yyyyMMddHHmmss}-{random.Next(1000, 9999)}";
            var authCode = $"AUTH{random.Next(10000, 99999)}";

            string? lastFour = null;
            if (!string.IsNullOrWhiteSpace(checkoutModel.CardNumber))
            {
                var cleanCard = checkoutModel.CardNumber.Replace(" ", "").Replace("-", "");
                lastFour = cleanCard.Length >= 4 ? cleanCard[^4..] : cleanCard;
            }

            var transaction = new PaymentTransaction
            {
                OrderId = order.OrderId,
                TransactionReference = reference,
                PaymentMethod = checkoutModel.PaymentMethod,
                Amount = order.TotalAmount,
                PaymentDate = timestamp,
                Status = "Completed",
                CardLastFour = lastFour,
                UpiId = checkoutModel.UpiId,
                BankName = checkoutModel.BankName,
                GatewayAuthCode = authCode
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }
    }
}
