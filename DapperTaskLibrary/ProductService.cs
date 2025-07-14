using Dapper;
using DapperTaskLibrary.Interfaces;

namespace DapperTaskLibrary;

public class ProductService : IProductOperations
{
    private readonly DbHelper _dbHelper;

    public ProductService(string connectionString)
    {
        _dbHelper = new DbHelper(connectionString);
    }

    public void CreateProduct(string name, string description, float weight, float height, float width, float length)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"INSERT INTO Product (Name, Description, Weight, Height, Width, Length)
                              VALUES (@Name, @Description, @Weight, @Height, @Width, @Length)";
            connection.Execute(query, new
            {
                Name = name,
                Description = description,
                Weight = weight,
                Height = height,
                Width = width,
                Length = length
            });

            Console.WriteLine("Product created successfully.");
        }
    }

    public IEnumerable<string> FetchProduct(string name)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"SELECT Name FROM Product WHERE Name = @Name";
            return connection.Query<string>(query, new { Name = name });
        }
    }

    public IEnumerable<string> GetAllProducts()
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"SELECT Name FROM Product";
            return connection.Query<string>(query);
        }
    }

    public void UpdateProduct(int id, string name, string description, float weight, float height, float width, float length)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"UPDATE Product
                              SET Name = @Name, Description = @Description, Weight = @Weight,
                                  Height = @Height, Width = @Width, Length = @Length
                              WHERE Id = @Id";

            int rowsAffected = connection.Execute(query, new
            {
                Id = id,
                Name = name,
                Description = description,
                Weight = weight,
                Height = height,
                Width = width,
                Length = length
            });

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No product with ID {id} was found or updated.");
            }
            else
            {
                Console.WriteLine($"Product with ID {id} was successfully updated.");
            }
        }
    }

    public void DeleteProduct(int id)
    {
        using (var connection = _dbHelper.GetConnection())
        {
            var query = @"DELETE FROM Product WHERE Id = @Id";
            int rowsAffected = connection.Execute(query, new { Id = id });

            if (rowsAffected == 0)
            {
                Console.WriteLine($"No product with ID {id} was found.");
            }
            else
            {
                Console.WriteLine($"Product with ID {id} was successfully deleted.");
            }
        }
    }

}
