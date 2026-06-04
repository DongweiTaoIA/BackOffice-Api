namespace BackOffice.Domain.Interfaces;

public interface IProductRegistry
{
    string? ResolveProduct(string input);
    List<string> GetAllProducts();
}
