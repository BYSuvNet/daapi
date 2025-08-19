// Data/DbSeed.cs
using System.Globalization;
using System.Text.Json;

public class DbSeed
{
    public static void Seed(AppDb context)
    {
        // --- PRODUCTS ---
        if (!context.Products.Any())
        {
            var products = LoadFromJson<Product>("db/products.json");
            if (products.Count > 0)
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }

        // --- CUSTOMERS ---
        if (!context.Customers.Any())
        {
            var customers = LoadFromJson<Customer>("db/customers.json");
            if (customers.Count > 0)
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }
        }

        // --- ORDERS (demo) ---
        if (!context.Orders.Any() && context.Customers.Any() && context.Products.Any())
        {
            var customer = context.Customers.First();
            var p1 = context.Products.First();

            var order = new Order
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                OrderDate = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = p1.Id, Quantity = 2, Price = p1.Price }
                }
            };

            context.Orders.Add(order);
            context.SaveChanges();
        }
    }

    // ---------------- Helpers ----------------
    private static List<T> LoadFromJson<T>(string relativePath)
    {
        try
        {
            var path = Resolve(relativePath);
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var data = JsonSerializer.Deserialize<List<T>>(json, options);
            return data ?? new List<T>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbSeed] Kunde inte läsa {relativePath}: {ex.Message}");
            return new List<T>();
        }
    }

    private static string Resolve(string relativePath)
    {
        var p1 = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(p1)) return p1;

        var p2 = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        if (File.Exists(p2)) return p2;

        return relativePath;
    }
}
