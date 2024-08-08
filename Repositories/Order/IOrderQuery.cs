using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories
{
    public interface IOrderQuery
    {
        public interface IOrderQuery
        {
            Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        }
    }
}