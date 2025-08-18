// Program.cs
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/api/products", async (AppDb db) =>
    await db.Products.AsNoTracking().ToListAsync())
   .WithName("GetProducts");

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
        ImageUrl = dto.ImageUrl
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
        Address = dto.Address
    };
    db.Customers.Add(c);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/api/customers/{c.Id}", c);
});

app.MapPut("/api/customers/{id:int}", async Task<Results<NoContent, NotFound, ValidationProblem>>
    (int id, CustomerUpdateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var c = await db.Customers.FindAsync(id);
    if (c is null) return TypedResults.NotFound();

    c.Name = dto.Name;
    c.Email = dto.Email;
    c.Phone = dto.Phone;
    c.Address = dto.Address;

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});

app.MapDelete("/api/customers/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AppDb db) =>
{
    var c = await db.Customers.FindAsync(id);
    if (c is null) return TypedResults.NotFound();

    db.Customers.Remove(c);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
});

// --------------------- ORDERS ---------------------

// Hämta alla orders
app.MapGet("/api/orders", async (AppDb db) =>
    await db.Orders
        .AsNoTracking()
        .Include(o => o.Items)
        .ToListAsync());

// Hämta en order
app.MapGet("/api/orders/{id:int}", async Task<Results<Ok<Order>, NotFound>> (int id, AppDb db) =>
{
    var o = await db.Orders
        .Include(x => x.Items)
        .FirstOrDefaultAsync(x => x.Id == id);
    return o is null ? TypedResults.NotFound() : TypedResults.Ok(o);
});

// Skapa order
app.MapPost("/api/orders", async Task<Results<Created<Order>, ValidationProblem, NotFound>>
    (OrderCreateDto dto, AppDb db) =>
{
    // Validera DTO
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    // Kontrollera kund
    var customer = await db.Customers.FindAsync(dto.CustomerId);
    if (customer is null) return TypedResults.NotFound();

    // Hämta produkter som ingår
    var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
    var products = await db.Products
        .Where(p => productIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id);

    // Säkerställ att alla produkter finns
    var missing = productIds.Where(id => !products.ContainsKey(id)).ToList();
    if (missing.Any())
    {
        var err = new Dictionary<string, string[]>
        {
            ["Items"] = new[] { $"Unknown ProductId(s): {string.Join(",", missing)}" }
        };
        return TypedResults.ValidationProblem(err);
    }

    var order = new Order
    {
        CustomerId = customer.Id,
        CustomerName = customer.Name,
        CustomerEmail = customer.Email,
        OrderDate = DateTime.UtcNow
    };

    foreach (var i in dto.Items)
    {
        var product = products[i.ProductId];
        order.Items.Add(new OrderItem
        {
            ProductId = product.Id,
            Quantity = i.Quantity,
            Price = product.Price // fryser priset vid ordertillfället
        });
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/orders/{order.Id}", order);
});

// Uppdatera orderstatus (t.ex. markera som Shipped/Delivered/Cancelled)
app.MapPatch("/api/orders/{id:int}/status", async Task<Results<NoContent, NotFound, ValidationProblem>>
    (int id, OrderStatusUpdateDto dto, AppDb db) =>
{
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var o = await db.Orders.FindAsync(id);
    if (o is null) return TypedResults.NotFound();

    o.Status = dto.Status;
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

app.Run();
