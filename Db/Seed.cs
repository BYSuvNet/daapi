using System.Text.Json;
using System.Globalization;
using System.IO.Compression;
using DaApi.Domain;

public class DbSeed
{
    public static void Seed(AppDb context)
    {
        // --- PRODUCTS ---
        if (!context.Products.Any())
        {
            var products = LoadFromJson<Product>("Db/products.json");
            if (products.Count > 0)
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }

        // --- CUSTOMERS ---
        if (!context.Customers.Any())
        {
            var customers = LoadFromJson<Customer>("Db/customers.json");
            if (customers.Count > 0)
            {
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }
        }

        // --- ORDERS ---
        if (!context.Orders.Any() && context.Customers.Any() && context.Products.Any())
        {
            var json = File.ReadAllText("Db/allorders.json");
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("[DbSeed] Hittade ingen orders-fil i Db/. Skippade order-seed.");
                return;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var raw = JsonSerializer.Deserialize<List<JsonElement>>(json, options) ?? new();

            // Snabba uppslag
            var validCustomerIds = context.Customers.Select(c => c.Id).ToHashSet();
            var validProductIds = context.Products.Select(p => p.Id).ToHashSet();

            var toInsert = new List<Order>(capacity: raw.Count);
            const int batchSize = 500;

            foreach (var o in raw)
            {
                // customerId (tillåt null om inte hittas)
                int? customerId = null;
                if (o.TryGetProperty("customerId", out var cEl) && cEl.ValueKind == JsonValueKind.Number)
                {
                    var cid = cEl.GetInt32();
                    if (validCustomerIds.Contains(cid)) customerId = cid;
                }

                // orderDateUtc
                string? dateStr = o.TryGetProperty("orderDateUtc", out var dEl) && dEl.ValueKind == JsonValueKind.String
                    ? dEl.GetString()
                    : null;
                var orderDateUtc = ParseUtcOrNow(dateStr);

                // currency
                string currency = (o.TryGetProperty("currency", out var curEl) && curEl.ValueKind == JsonValueKind.String)
                    ? (curEl.GetString() ?? "SEK")
                    : "SEK";

                // status (0..4 i din JSON: Pending, Paid, Shipped, Delivered, Cancelled)
                int statusCode = (o.TryGetProperty("status", out var sEl) && sEl.ValueKind == JsonValueKind.Number)
                    ? sEl.GetInt32()
                    : 0;
                var status = MapStatus(statusCode); // 1 => Processing i din enum

                // items
                var items = new List<OrderItem>();
                if (o.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var it in itemsEl.EnumerateArray())
                    {
                        if (!it.TryGetProperty("productId", out var pidEl) || pidEl.ValueKind != JsonValueKind.Number)
                            continue;

                        int productId = pidEl.GetInt32();
                        if (!validProductIds.Contains(productId)) continue;

                        int qty = 1;
                        if (it.TryGetProperty("quantity", out var qEl) && qEl.ValueKind == JsonValueKind.Number)
                            qty = Math.Clamp(qEl.GetInt32(), 1, 3);

                        decimal price = 0m;
                        if (it.TryGetProperty("price", out var pEl) && pEl.ValueKind == JsonValueKind.Number)
                            price = Math.Round(pEl.GetDecimal(), 2);

                        // Vill du istället alltid låsa pris från Product-tabellen? Byt ut raden ovan mot:
                        // var prodPrice = context.Products.Where(p => p.Id == productId).Select(p => p.Price).First();
                        // price = Math.Round(prodPrice, 2);

                        items.Add(new OrderItem
                        {
                            ProductId = productId,
                            Quantity = qty,
                            Price = price
                        });
                    }
                }

                if (items.Count == 0) continue; // hoppa order utan giltiga rader

                toInsert.Add(new Order
                {
                    CustomerId = customerId, // kan vara null om kund saknas
                    OrderDateUtc = orderDateUtc,
                    Currency = string.IsNullOrWhiteSpace(currency) ? "SEK" : currency,
                    Status = status,
                    Items = items
                });

                if (toInsert.Count >= batchSize)
                {
                    context.Orders.AddRange(toInsert);
                    context.SaveChanges();
                    toInsert.Clear();
                }
            }

            if (toInsert.Count > 0)
            {
                context.Orders.AddRange(toInsert);
                context.SaveChanges();
            }

            Console.WriteLine($"[DbSeed] Seedade {context.Orders.Count()} ordrar.");
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

    private static OrderStatus MapStatus(int code) => code switch
    {
        0 => OrderStatus.Pending,
        1 => OrderStatus.Processing, // "Paid" → Processing i din enum
        2 => OrderStatus.Shipped,
        3 => OrderStatus.Delivered,
        4 => OrderStatus.Cancelled,
        _ => OrderStatus.Pending
    };

    private static DateTime ParseUtcOrNow(string? iso)
    {
        if (!string.IsNullOrWhiteSpace(iso) &&
            DateTime.TryParse(iso,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var dt))
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        }
        return DateTime.UtcNow;
    }
}
