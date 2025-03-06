//dotnet add package Microsoft.EntityFrameworkCore.InMemory
//dotnet add package Microsoft.EntityFrameworkCore

using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using MVC.Business;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// BD InMemory ...
builder.Services.AddDbContext<ApplicationDbContextInMemory>(options => options.UseInMemoryDatabase("InMemoryDb"));
builder.Services.AddScoped<IRepository_mini, EFRepository_mini_InMemory>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50MB limit
});


// Register BlobController as a service
builder.Services.AddScoped<BlobController>();

// Ajouter le service pour Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.OperationFilter<FileUploadOperationFilter>() // Add custom operation filter
);

var app = builder.Build();

// Use CORS before endpoints
app.UseCors("AllowAllOrigins");

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Content-Type: {context.Request.ContentType}");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"Header: {header.Key} = {header.Value}");
    }

    // Log form data
    if (context.Request.HasFormContentType)
    {
        Console.WriteLine(" Request contains form-data!");
        var form = await context.Request.ReadFormAsync();
        foreach (var key in form.Keys)
        {
            Console.WriteLine($"Form field: {key} = {form[key]}");
        }

        if (form.Files.Count > 0)
        {
            foreach (var file in form.Files)
            {
                Console.WriteLine($" Uploaded file: {file.FileName} (Size: {file.Length} bytes)");
            }
        }
    }
    else
    {
        Console.WriteLine(" Request does NOT contain form-data!");
    }

    await next();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//API 
app.MapPost("/Posts/Add", async (IRepository_mini repo, BlobController blobController, [FromForm] PostCreateDTO post) =>
{
    if (post == null)
    {
        Console.WriteLine(" post is NULL");
        return Results.BadRequest("Invalid request. Ensure the form data is correctly sent.");
    }

    Console.WriteLine($" Title: {post.Title}");
    Console.WriteLine($" Category: {post.Category}");
    Console.WriteLine($" User: {post.User}");
    Console.WriteLine($" Image: {(post.Image != null ? post.Image.FileName : "NULL")}");

    if (post.Image == null)
    {
        return Results.BadRequest("Image file is required.");
    }

    try
    {
        Guid guid = Guid.NewGuid();
        string imageUrl = await blobController.PushImageToBlob(post.Image, guid);

        Post newPost = new Post
        {
            Title = post.Title,
            Category = post.Category,
            User = post.User,
            BlobImage = guid,
            Url = imageUrl
        };

        return await repo.CreateAPIPost(newPost);
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Error: {ex}");
        return Results.BadRequest("An error occurred while processing the request.");
    }
}).DisableAntiforgery();


app.MapGet("/Posts", async (IRepository_mini repo) => await repo.GetPosts());

app.MapGet("/Posts/{id}", async (IRepository_mini repo, Guid id) =>
{
    var post = await repo.GetPostById(id);
    return post != null ? Results.Ok(post) : Results.NotFound();
});

app.MapDelete("/Posts/{id}", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.DeletePost(id);
    return result ? Results.Ok() : Results.NotFound();
});


app.Run();


