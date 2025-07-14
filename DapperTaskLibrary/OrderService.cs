using System.Data;
using Dapper;
using DapperTaskLibrary.Interfaces;
using DapperTaskLibrary.Models;

namespace DapperTaskLibrary;

public class OrderService : IOrderOperations
{
    private readonly DbHelper _dbHelper;

    public OrderService(string connectionString)
    {
        _dbHelper = new DbHelper(connectionString);
    }

    // Create an order
    public void CreateOrder(OrderStatus status, DateTime createdDate, DateTime updatedDate, int productId)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"INSERT INTO [Order] (Status, CreatedDate, UpdatedDate, ProductId)
                          VALUES (@Status, @CreatedDate, @UpdatedDate, @ProductId)";

            connection.Execute(query, new
            {
                Status = (int)status, // Convert enum to integer for storage
                CreatedDate = createdDate,
                UpdatedDate = updatedDate,
                ProductId = productId
            });

            Console.WriteLine($"Order created successfully with status: {status}");
        }
    }

    // Delete an order by ID
    public void DeleteOrder(int id)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"DELETE FROM [Order] WHERE Id = @Id";
            var rowsAffected = connection.Execute(query, new { Id = id });

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No order found with ID {id} to delete.");
            }
            else
            {
                Console.WriteLine($"Order with ID {id} deleted successfully.");
            }
        }
    }

    // Fetch orders by a specific status
    public IEnumerable<int> FetchOrdersByStatus(OrderStatus status)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"SELECT Id FROM [Order] WHERE Status = @Status";
            return connection.Query<int>(query, new { Status = (int)status }); // Use integer value of enum
        }
    }

    // Fetch an order by its ID
    public IEnumerable<int> FetchOrderById(int id)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"SELECT Id FROM [Order] WHERE Id = @Id";
            return connection.Query<int>(query, new { Id = id });
        }
    }

    // Fetch all orders
    public IEnumerable<int> GetAllOrders()
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"SELECT Id FROM [Order]";
            return connection.Query<int>(query);
        }
    }

    // Update an existing order
    public void UpdateOrder(int id, OrderStatus status, DateTime createdDate, DateTime updatedDate, int productId)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"UPDATE [Order]
                          SET Status = @Status, CreatedDate = @CreatedDate, UpdatedDate = @UpdatedDate, ProductId = @ProductId
                          WHERE Id = @Id";

            var rowsAffected = connection.Execute(query, new
            {
                Id = id,
                Status = (int)status, // Convert enum to integer for storage
                CreatedDate = createdDate,
                UpdatedDate = updatedDate,
                ProductId = productId
            });

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No order found with ID {id} to update.");
            }
            else
            {
                Console.WriteLine($"Order with ID {id} updated successfully.");
            }
        }
    }

    // Fetch filtered orders using a stored procedure
    public IEnumerable<dynamic> FetchFilteredOrders(int? year = null, int? month = null, OrderStatus? status = null, int? productId = null)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Year", year, DbType.Int32);
            parameters.Add("@Month", month, DbType.Int32);
            parameters.Add("@Status", status.HasValue ? (int)status.Value : (object)DBNull.Value, DbType.Int32);
            parameters.Add("@ProductId", productId, DbType.Int32);

            return connection.Query("GetFilteredOrders", parameters, commandType: CommandType.StoredProcedure);
        }
    }

    // Bulk delete orders using filters (via a stored procedure)
    public void DeleteOrdersInBulk(int? year = null, int? month = null, OrderStatus? status = null, int? productId = null)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Year", year, DbType.Int32);
                    parameters.Add("@Month", month, DbType.Int32);
                    parameters.Add("@Status", status.HasValue ? (int)status.Value : (object)DBNull.Value, DbType.Int32); // Nullable enum
                    parameters.Add("@ProductId", productId, DbType.Int32);

                    var rowsAffected = connection.Execute("BulkDeleteOrders", parameters, transaction, commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                    Console.WriteLine($"{rowsAffected} orders deleted successfully.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    throw;
                }
            }
        }
    }
}