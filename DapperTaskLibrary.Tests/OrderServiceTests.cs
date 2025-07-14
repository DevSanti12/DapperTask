using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using DapperTaskLibrary.Models;

namespace DapperTaskLibrary.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly string _connectionString;
    private readonly OrderService _orderService;
    private readonly ProductService _productService;
    private readonly string _description = "Test Description";
    private readonly float _weight = 1.5f;
    private readonly float _height = 2.0f;
    private readonly float _width = 3.0f;
    private readonly float _length = 4.0f;

    public OrderServiceTests()
    {
        // Load appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Get the connection string
        _connectionString = config.GetConnectionString("TestDatabase");

        // Initialize ProductService with the Test Database connection
        _orderService = new OrderService(_connectionString);

        // Initialize ProductService with the Test Database connection
        _productService = new ProductService(_connectionString);

        // Clear test database before running tests
        ClearDatabase();
    }

    [Fact]
    public void CreateOrder_Should_Insert_Order_Into_Database()
    {
        // Arrange
        var status = OrderStatus.InProgress; // Enum status
        var createdDate = DateTime.Now;
        var updatedDate = createdDate.AddMinutes(5);

        // First, create a product to associate with the order
        var productIds = CreateProducts(1);

        // Act
        _orderService.CreateOrder(status, createdDate, updatedDate, productIds.ElementAt(0));

        // Assert - Verify that the order exists in the database
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT COUNT(*) FROM [Order] WHERE Status = @Status", connection))
            {
                command.Parameters.AddWithValue("@Status", (int)status); // Cast enum to integer
                var count = (int)command.ExecuteScalar();
                Assert.Equal(1, count); // Ensure the order was inserted
            }
        }
    }

    [Fact]
    public void FetchOrdersByStatus_Should_Return_Orders_For_Status()
    {
        // Arrange Test data for Products 
        var productIds = CreateProducts(3);

        // Add sample orders
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(1));

        // Act
        var orders = _orderService.FetchOrdersByStatus(OrderStatus.InProgress);

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(2, orders.Count());
    }

    [Fact]
    public void GetAllOrders_Should_Return_All_Orders()
    {
        // Arrange - Add sample orders
        var productIds = CreateProducts(3);
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(1));
        _orderService.CreateOrder(OrderStatus.Done, DateTime.Now, DateTime.Now, productIds.ElementAt(2));

        // Act
        var orders = _orderService.GetAllOrders();

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(3, orders.Count());
    }

    [Fact]
    public void UpdateOrder_Should_Update_Order_In_Database()
    {
        // Arrange
        var productIds = CreateProducts(1);
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(0));

        // Fetch the ID of the inserted order
        int orderId;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Id FROM [Order] WHERE Status = @Status", connection))
            {
                command.Parameters.AddWithValue("@Status", (int)OrderStatus.InProgress);
                orderId = (int)command.ExecuteScalar();
            }
        }

        // Act
        _orderService.UpdateOrder(orderId, OrderStatus.Done, DateTime.Now, DateTime.Now, productIds.ElementAt(0));

        // Assert - Verify the order was updated
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Status FROM [Order] WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", orderId);
                var status = (OrderStatus)(int)command.ExecuteScalar(); // Convert from int to enum
                Assert.Equal(OrderStatus.Done, status);
            }
        }
    }

    [Fact]
    public void DeleteOrder_Should_Remove_Order_From_Database()
    {
        // Arrange - Add a sample order
        var productIds = CreateProducts(1);
        _orderService.CreateOrder(OrderStatus.InProgress, DateTime.Now, DateTime.Now, productIds.ElementAt(0));

        // Fetch the ID of the inserted order
        int orderId;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Id FROM [Order] WHERE Status = @Status", connection))
            {
                command.Parameters.AddWithValue("@Status", (int)OrderStatus.InProgress);
                orderId = (int)command.ExecuteScalar();
            }
        }

        // Act
        _orderService.DeleteOrder(orderId);

        // Assert - Ensure the order no longer exists
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT COUNT(*) FROM [Order] WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", orderId);
                var count = (int)command.ExecuteScalar();
                Assert.Equal(0, count);
            }
        }
    }

    [Fact]
    public void FetchFilteredOrders_Should_Return_Filtered_Orders()
    {
        // Arrange - Add sample orders
        var productIds = CreateProducts(1);
        _orderService.CreateOrder(OrderStatus.InProgress, new DateTime(2023, 1, 15), DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.Done, new DateTime(2023, 2, 20), DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.InProgress, new DateTime(2022, 8, 10), DateTime.Now, productIds.ElementAt(0));

        // Act - Filter by year and status
        var filteredOrders = _orderService.FetchFilteredOrders(year: 2023, status: OrderStatus.InProgress);

        // Assert
        Assert.NotNull(filteredOrders);
        Assert.Equal(1, filteredOrders.Count());
    }

    [Fact]
    public void DeleteOrdersInBulk_Should_Delete_Filtered_Orders()
    {
        // Arrange - Add sample orders
        var productIds = CreateProducts(1);
        _orderService.CreateOrder(OrderStatus.InProgress, new DateTime(2023, 1, 15), DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.Done, new DateTime(2023, 2, 20), DateTime.Now, productIds.ElementAt(0));
        _orderService.CreateOrder(OrderStatus.InProgress, new DateTime(2022, 8, 10), DateTime.Now, productIds.ElementAt(0));

        // Act - Delete orders by year
        _orderService.DeleteOrdersInBulk(year: 2023);

        // Assert - Verify remaining orders
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT COUNT(*) FROM [Order]", connection))
            {
                var count = (int)command.ExecuteScalar();
                Assert.Equal(1, count); // Only the 2022 order should remain
            }
        }
    }

    internal List<int> CreateProducts(int numberOfProducts)
    {
        List<int> productIds = new List<int>();
        for (int i = 0; i < numberOfProducts; i++)
        {
            _productService.CreateProduct($"Test Product {i + 1}", _description, _weight, _height, _width, _length);
            int productId;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT TOP 1 Id FROM Product", connection))
                {
                    productId = (int)command.ExecuteScalar();
                    productIds.Add(productId);
                }
            }
        }
        return productIds;
    }

    // Utility method to clear the Orders table
    private void ClearDatabase()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("DELETE FROM \"Order\"", connection))
            {
                command.ExecuteNonQuery();
            }
            using (var command = new SqlCommand("DELETE FROM Product", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    // Clean up after all tests (optional)
    public void Dispose()
    {
        ClearDatabase();
    }
}