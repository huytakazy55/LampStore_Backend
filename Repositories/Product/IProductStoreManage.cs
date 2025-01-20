using LampStoreProjects.Data;

namespace LampStoreProjects.Repositories
{
    public interface IProductStoreManage
    {
        Task<IEnumerable<Product>> GetListProductsync();
    }
}