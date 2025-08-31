// Program.cs
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using DaApi.Domain;
using System.Xml.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("OrdersCache", b => b
        .Expire(TimeSpan.FromMinutes(120))                 // TTL
        .SetVaryByQuery("from", "to", "page", "pageSize")   // unik cache per kombination
        .Tag("orders"));                                  // tag för invalidation
});

builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseInMemoryDatabase("products-db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    DbSeed.Seed(db);
}

app.UseCors("DevCors");

// --------------------- PRODUCTS ---------------------

//GET all products as JSON or CSV
app.MapGet("/api/products", async (AppDb db, string format = "json") =>
{
    var products = await db.Products.AsNoTracking().ToListAsync();

    if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
    {
        var csv = "Id,Name,Description,Price,Brand,Category,ImageUrl,DateAdded\n" +
          string.Join("\n", products.Select(p =>
              $"{p.Id},{CsvEscape(p.Name)},{CsvEscape(p.Description!)},{CsvNum(p.Price)},{CsvEscape(p.Brand!)},{CsvEscape(p.Category!)},{CsvEscape(p.ImageUrl!)},{p.DateAdded.ToString("O", CultureInfo.InvariantCulture)}"));

        return Results.File(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv)), "text/csv", "products.csv");
    }

    return Results.Ok(products);
}).WithName("GetProducts");

// GET products as Google Merchant Center XML feed
app.MapGet("/api/products/feed.xml", async (AppDb db) =>
{
    var products = await db.Products.AsNoTracking().Where(p => p.Category.ToLower() == "camping").ToListAsync();

    XNamespace g = "http://base.google.com/ns/1.0";

    var rss = new XElement("rss",
        new XAttribute("version", "2.0"),
        new XAttribute(XNamespace.Xmlns + "g", g),
        new XElement("channel",
            new XElement("title", "DaCampingStore"),
            new XElement("link", "https://dacampingstore.com"),
            new XElement("description", "Product feed for Google Merchant Center"),
            products.Select(p => new XElement("item",
                new XElement(g + "id", p.Id),
                new XElement(g + "title", p.Name),
                new XElement(g + "description", p.Description ?? ""),
                new XElement(g + "link", "https://dacampingstore.com/products/" + p.Id),
                new XElement(g + "image_link", $"https://picsum.photos/200/200?random={p.Id}"),
                new XElement(g + "condition", "new"),
                new XElement(g + "availability", "in stock"),
                new XElement(g + "price", $"{p.Price:F2} SEK"),
                new XElement(g + "shipping",
                    new XElement(g + "country", "SE"),
                    new XElement(g + "service", "Standard"),
                    new XElement(g + "price", "59.00 SEK")
                ),
                new XElement(g + "brand", p.Brand ?? ""),
                new XElement(g + "google_product_category", "1013")
            ))
        )
    );

    var xml = "<?xml version=\"1.0\"?>\n" + rss.ToString();
    return Results.Content(xml, "application/rss+xml");
}).WithName("GetGoogleMerchantCenterFeed");

app.MapGet("/api/products/{id:int}", async Task<Results<Ok<Product>, NotFound>> (int id, AppDb db) =>
{
    var p = await db.Products.FindAsync(id);
    return p is null ? TypedResults.NotFound() : TypedResults.Ok(p);
}).WithName("GetProduct");

app.MapPost("/api/products", async Task<Results<Created<Product>, ValidationProblem>>
    (ProductCreateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var product = new Product
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Brand = dto.Brand,
        Category = dto.Category,
        ImageUrl = dto.ImageUrl,
        DateAdded = DateTime.UtcNow
    };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/products/{product.Id}", product);
}).WithName("CreateProduct");

app.MapPut("/api/products/{id:int}", async Task<Results<NoContent, NotFound, ValidationProblem>>
    (int id, ProductUpdateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var p = await db.Products.FindAsync(id);
    if (p is null) return TypedResults.NotFound();

    p.Name = dto.Name;
    p.Description = dto.Description;
    p.Price = dto.Price;
    p.Brand = dto.Brand;
    p.Category = dto.Category;
    p.ImageUrl = dto.ImageUrl;

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}).WithName("UpdateProduct");

app.MapDelete("/api/products/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AppDb db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return TypedResults.NotFound();

    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}).WithName("DeleteProduct");

// --------------------- CUSTOMERS ---------------------

app.MapGet("/api/customers", async (AppDb db) =>
    await db.Customers.AsNoTracking().ToListAsync());

app.MapGet("/api/customers/{id:int}", async Task<Results<Ok<Customer>, NotFound>> (int id, AppDb db) =>
{
    var c = await db.Customers.FindAsync(id);
    return c is null ? TypedResults.NotFound() : TypedResults.Ok(c);
});

app.MapPost("/api/customers", async Task<Results<Created<Customer>, ValidationProblem>>
    (CustomerCreateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var c = new Customer
    {
        Name = dto.Name,
        Email = dto.Email,
        Phone = dto.Phone,
        Address = dto.Address,
        BirthDate = dto.BirthDate
    };
    db.Customers.Add(c);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/customers/{c.Id}", c);
});

// --------------------- ORDERS ---------------------

app.MapGet("/api/orders", async (
    AppDb db,
    HttpContext http,
    DateTime? from,
    DateTime? to,
    int? page,
    int? pageSize
) =>
{
    var query = db.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .AsQueryable();

    if (from.HasValue) query = query.Where(o => o.OrderDateUtc >= from.Value);
    if (to.HasValue) query = query.Where(o => o.OrderDateUtc <= to.Value);

    query = query.OrderByDescending(o => o.OrderDateUtc);

    var total = await query.CountAsync();
    http.Response.Headers["X-Total-Count"] = total.ToString();

    if (page.HasValue || pageSize.HasValue)
    {
        var p = Math.Max(1, page ?? 1);
        var ps = Math.Clamp(pageSize ?? 50, 1, 200);
        var items = await query.Skip((p - 1) * ps).Take(ps).ToListAsync();
        return Results.Ok(items);
    }
    else
    {
        var items = await query.ToListAsync();
        return Results.Ok(items);
    }
})
.CacheOutput("OrdersCache");  // <— aktivera cachen här

//TODO: Invalidera cache om ny order skapas eller order uppdateras

// Hämta en order
app.MapGet("/api/orders/{id:int}", async Task<Results<Ok<Order>, NotFound>> (int id, AppDb db) =>
{
    var o = await db.Orders
        .Include(x => x.Items)
        .FirstOrDefaultAsync(x => x.Id == id);
    return o is null ? TypedResults.NotFound() : TypedResults.Ok(o);
});

// REVIEWS
app.MapGet("/api/products/reviews", async (AppDb db) =>
    await db.Reviews
        .AsNoTracking()
        .ToListAsync());

app.MapGet("/api/products/{productId:int}/reviews", async Task<Results<Ok<List<ProductReview>>, NotFound>> (int productId, AppDb db) =>
{
    var reviews = await db.Reviews
        .Where(r => r.ProductId == productId)
        .AsNoTracking()
        .ToListAsync();

    if (reviews.Count == 0)
        return TypedResults.NotFound();

    return TypedResults.Ok(reviews);
});

app.MapPost("/api/products/{productId:int}/reviews", async Task<Results<Created<ProductReview>, ValidationProblem, NotFound>>
    (int productId, ProductReviewCreateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var product = await db.Products.FindAsync(productId);
    if (product is null) return TypedResults.NotFound();

    var review = new ProductReview
    {
        ProductId = productId,
        CustomerId = dto.CustomerId,
        Rating = dto.Rating,
        Comment = dto.Comment,
        DateAdded = DateTime.UtcNow
    };

    db.Reviews.Add(review);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/products/{productId}/reviews/{review.Id}", review);
});

app.MapDelete("/api/products/{productId:int}/reviews/{reviewId:int}",
    async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>>
    (int productId, int reviewId, AppDb db, HttpContext httpContext) =>
{
    // Kolla om rätt api-nyckel finns i headern
    if (!httpContext.Request.Headers.TryGetValue("X-Api-Key", out var apiKey) || apiKey != "qwerty123456")
        return TypedResults.Unauthorized();

    var review = await db.Reviews
        .FirstOrDefaultAsync(r => r.ProductId == productId && r.Id == reviewId);

    if (review is null) return TypedResults.NotFound();

    db.Reviews.Remove(review);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});

// Hjälpare för data annotations
static Dictionary<string, string[]> Validate<T>(T instance)
{
    var ctx = new ValidationContext(instance!);
    var results = new List<ValidationResult>();
    var ok = Validator.TryValidateObject(instance!, ctx, results, validateAllProperties: true);

    var errors = new Dictionary<string, string[]>();
    if (!ok)
    {
        foreach (var r in results)
        {
            var key = r.MemberNames.FirstOrDefault() ?? "";
            if (!errors.ContainsKey(key)) errors[key] = Array.Empty<string>();
            errors[key] = errors[key].Concat(new[] { r.ErrorMessage ?? "Invalid" }).ToArray();
        }
    }
    return errors;
}

app.UseOutputCache();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

static string CsvEscape(string field)
{
    if (field == null) return "";
    // Dubbelcitat inne i texten ersätts med två dubbla citat
    var needsQuotes = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
    var escaped = field.Replace("\"", "\"\"");
    return needsQuotes ? $"\"{escaped}\"" : escaped;
}

static string CsvNum(decimal n) =>
    n.ToString(CultureInfo.InvariantCulture);
