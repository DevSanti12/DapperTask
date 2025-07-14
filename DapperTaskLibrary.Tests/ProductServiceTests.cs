using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DapperTaskLibrary;

namespace DapperTaskLibrary.Tests;

public class ProductServiceTests : IDisposable
{
    private readonly string _connectionString;
    private readonly ProductService _ProductService;

    public ProductServiceTests()
    {
        // Load appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Get the connection string
        _connectionString = config.GetConnectionString("TestDatabase");

        // Initialize ProductService with the Test Database connection
        _ProductService = new ProductService(_connectionString);

        // Clear test database before running tests
        ClearDatabase();
    }

    [Fact]
    public void CreateProduct_Should_Add_Products_To_Database()
    {
        // Arrange
        string name = "Test Products";
        string description = "Test Description";
        float weight = 1.5f;
        float height = 2.0f;
        float width = 3.0f;
        float length = 4.0f;

        // Act
        _ProductService.CreateProduct(name, description, weight, height, width, length);

        // Assert - Verify the Products exists in the database
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT COUNT(*) FROM Product WHERE Name = @Name", connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                var count = (int)command.ExecuteScalar();
                Assert.Equal(1, count); // Ensure one Products exists
            }
        }
    }

    [Fact]
    public void GetAllProducts_Should_Return_All_Productss()
    {
        // Arrange - Add Test Data
        _ProductService.CreateProduct("Products 1", "Description 1", 1f, 2f, 3f, 4f);
        _ProductService.CreateProduct("Products 2", "Description 2", 2f, 3f, 4f, 5f);

        // Act
        var Productss = _ProductService.GetAllProducts();

        // Assert
        Assert.NotNull(Productss);
        Assert.Equal(2, Productss.Count());
    }

    [Fact]
    public void Should_Return_SpecificProducts()
    {
        // Arrange - Add Test Data
        _ProductService.CreateProduct("Products 1", "Description 1", 1f, 2f, 3f, 4f);

        // Act
        var Productss = _ProductService.FetchProduct("Products 1");

        // Assert
        Assert.NotNull(Productss);
        Assert.Single(Productss);
        Assert.Equal("Products 1", Productss.First());
    }

    [Fact]
    public void UpdateProduct_Should_Update_Products_Details()
    {
        // Arrange - Add a Products
        var ProductsName = "Original Products";
        _ProductService.CreateProduct(ProductsName, "Description", 1f, 1f, 1f, 1f);

        // Fetch the ID for the inserted Products
        int ProductsId;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT Id FROM Product WHERE Name = @Name", connection))
            {
                command.Parameters.AddWithValue("@Name", ProductsName);
                ProductsId = (int)command.ExecuteScalar();
            }
        }

        // Act - Update the Products
        _ProductService.UpdateProduct(ProductsId, "Updated Products", "Updated Description", 2f, 3f, 4f, 5f);

        // Assert - Ensure the changes were made
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("SELECT * FROM Product WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", ProductsId);
                using (var reader = command.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal("Updated Products", reader["Name"]);
                    Assert.Equal("Updated Description", reader["Description"]);
                    Assert.Equal(2f, Convert.ToSingle(reader["Weight"]));
                }
            }
        }
    }

    // Clean all Productss before or after a test
    private void ClearDatabase()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand("DELETE FROM dbo.Product", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    // For IDisposable pattern to clean up if necessary
    public void Dispose()
    {
        ClearDatabase();
    }
}