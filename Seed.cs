// Data/DbSeed.cs
public class DbSeed
{
    public static void Seed(AppDb context)
    {
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Sample Product 1",
                    Description = "This is a sample product.",
                    Price = 19.99m,
                    Brand = "Brand A",
                    Category = "Category X",
                    ImageUrl = "https://example.com/image1.jpg"
                },
                new Product
                {
                    Name = "Sample Product 2",
                    Description = "This is another sample product.",
                    Price = 29.99m,
                    Brand = "Brand B",
                    Category = "Category Y",
                    ImageUrl = "https://example.com/image2.jpg"
                }
            );
            context.SaveChanges();
        }

        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer
                {
                    Name = "John Doe",
                    Email = "john@doe.com",
                    Phone = "123-456-7890",
                    Address = "123 Main St, Anytown, USA"
                },
                new Customer
                {
                    Name = "Jane Smith",
                    Email = "jane@smith.com",
                    Phone = "987-654-3210",
                    Address = "456 Elm St, Othertown, USA"
                });
            context.SaveChanges();
        }

        if (!context.Orders.Any())
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
                    new OrderItem
                    {
                        ProductId = p1.Id,
                        Quantity = 2,
                        Price = p1.Price
                    }
                }
            };

            context.Orders.Add(order);
            context.SaveChanges();
        }
    }
}
