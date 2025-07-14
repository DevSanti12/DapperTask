using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperTaskLibrary.Interfaces;
public interface IProductOperations
{
    void CreateProduct(string name, string description, float weight, float height, float width, float length);
    IEnumerable<string> FetchProduct(string name);
    IEnumerable<string> GetAllProducts();
    void UpdateProduct(int id, string name, string description, float weight, float height, float width, float length);
    void DeleteProduct(int id);
}

