using Restaurant_Management.Models.Entities;
using Restaurant_Management.Models.ViewModels;

namespace Restaurant_Management.Services
{
    public interface IPaymentGatewayService
    {
        Task<PaymentTransaction> ProcessPaymentAsync(Order order, CheckoutViewModel checkoutModel);
    }
}
