using LampStoreProjects.Data;
using Dapper;
using System.Data;

namespace LampStoreProjects.Repositories
{
    public class ProductStoreManage : IProductStoreManage
    {
        private readonly IDbConnection _dbConnection;

        public ProductStoreManage(IDbConnection dbConnection)
        {
                _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Product>> GetListProductsync()
        {
            var query = "GetListProduct";
            return await _dbConnection.QueryAsync<Product>(
                query,
                commandType: CommandType.StoredProcedure);
        }
    }
}