using CreditReporting.Application.DTOs;
using System.Threading.Tasks;

namespace CreditReporting.Application.Interfaces
{
    public interface ICustomerServiceClient
    {
        Task<CustomerDto?> GetCustomerByIdAsync(int customerId);
    }
}
