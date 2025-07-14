using DapperTaskLibrary.Models;
namespace DapperTaskLibrary.Interfaces
{
    public interface IOrderOperations
    {
        public void CreateOrder(OrderStatus status, DateTime createdDate, DateTime updatedDate, int productid);

        public IEnumerable<int> FetchOrdersByStatus(OrderStatus status);

        public IEnumerable<int> FetchOrderById(int id);

        public IEnumerable<int> GetAllOrders();

        public void UpdateOrder(int id, OrderStatus status, DateTime createdDate, DateTime updatedDate, int productid);

        public void DeleteOrder(int id);

        public void DeleteOrdersInBulk(int? year = null, int? month = null, OrderStatus? status = null, int? productId = null);

        public IEnumerable<dynamic> FetchFilteredOrders(int? year = null, int? month = null, OrderStatus? status = null, int? productId = null);
    }
}
