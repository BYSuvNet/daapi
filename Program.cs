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
        // policy
        //     .AllowAnyHeader()
        //     .AllowAnyMethod()
        //     .SetIsOriginAllowed(_ => true) // allow all origins
        //     .AllowCredentials()            // only if you actually need cookies/auth headers
        //     .SetPreflightMaxAge(TimeSpan.FromHours(1));

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
    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Coffee Mug", Description = "Ceramic mug", Price = 79m },
            new Product { Name = "Notebook", Description = "A5 dotted", Price = 49m }
        );
        await db.SaveChangesAsync();
    }
}


// Order matters: CORS before endpoints
app.UseCors("DevCors");

// Endpoints

app.MapGet("/products", async (AppDb db) =>
    await db.Products.AsNoTracking().ToListAsync())
   .WithName("GetProducts");

app.MapGet("/products/{id:int}", async Task<Results<Ok<Product>, NotFound>> (int id, AppDb db) =>
{
    var p = await db.Products.FindAsync(id);
    return p is null ? TypedResults.NotFound() : TypedResults.Ok(p);
}).WithName("GetProduct");

app.MapPost("/products", async Task<Results<Created<Product>, ValidationProblem>>
    (ProductCreateDto dto, AppDb db) =>
{
    // Manuell validering av data annotations (Minimal API har ingen automatisk)
    var validationErrors = Validate(dto);
    if (validationErrors.Count > 0)
        return TypedResults.ValidationProblem(validationErrors);

    var product = new Product { Name = dto.Name, Description = dto.Description, Price = dto.Price };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/products/{product.Id}", product);
}).WithName("CreateProduct");

app.MapPut("/products/{id:int}", async Task<Results<NoContent, NotFound, ValidationProblem>>
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

    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}).WithName("UpdateProduct");

app.MapDelete("/products/{id:int}", async Task<Results<NoContent, NotFound>> (int id, AppDb db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return TypedResults.NotFound();

    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}).WithName("DeleteProduct");

// Enkel hjälpare för data annotations
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
            if (!errors.ContainsKey(key)) errors[key] = [];
            errors[key] = [.. errors[key], r.ErrorMessage ?? "Invalid"];
        }
    }
    return errors;
}

app.Run();
