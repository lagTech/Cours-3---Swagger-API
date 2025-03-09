//dotnet add package Microsoft.EntityFrameworkCore.InMemory
//dotnet add package Microsoft.EntityFrameworkCore

using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Data;
using Microsoft.AspNetCore.Mvc;
using MVC.Business;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json;

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

// Add this to your builder configuration in Program.cs
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));


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

//app.Use(async (context, next) =>
//{
//    Console.WriteLine($"Request Content-Type: {context.Request.ContentType}");
//    foreach (var header in context.Request.Headers)
//    {
//        Console.WriteLine($"Header: {header.Key} = {header.Value}");
//    }

//    // Log form data
//    if (context.Request.HasFormContentType)
//    {
//        Console.WriteLine(" Request contains form-data!");
//        var form = await context.Request.ReadFormAsync();
//        foreach (var key in form.Keys)
//        {
//            Console.WriteLine($"Form field: {key} = {form[key]}");
//        }

//        if (form.Files.Count > 0)
//        {
//            foreach (var file in form.Files)
//            {
//                Console.WriteLine($" Uploaded file: {file.FileName} (Size: {file.Length} bytes)");
//            }
//        }
//    }
//    else
//    {
//        Console.WriteLine(" Request does NOT contain form-data!");
//    }

//    await next();
//});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


//API 


// Replace your current /Posts/Add endpoint with this one:
app.MapPost("/Posts/Add", async (HttpContext context, IRepository_mini repo, BlobController blobController) =>
{
    Console.WriteLine("Posts/Add endpoint called with direct form handling");

    try
    {
        if (!context.Request.HasFormContentType)
        {
            Console.WriteLine("Request is not form data");
            return Results.BadRequest("Expected multipart/form-data request");
        }

        var form = await context.Request.ReadFormAsync();

        Console.WriteLine("Form data received:");
        foreach (var key in form.Keys)
        {
            Console.WriteLine($"  {key} = {form[key]}");
        }

        Console.WriteLine("Form files:");
        foreach (var file in form.Files)
        {
            Console.WriteLine($"  {file.Name} = {file.FileName} ({file.Length} bytes)");
        }

        // Check for required fields
        if (!form.ContainsKey("title") || string.IsNullOrEmpty(form["title"]))
        {
            Console.WriteLine("Missing title");
            return Results.BadRequest("Title is required");
        }

        if (!form.ContainsKey("user") || string.IsNullOrEmpty(form["user"]))
        {
            Console.WriteLine("Missing user");
            return Results.BadRequest("User is required");
        }

        // Get the image file
        var imageFile = form.Files.GetFile("image");
        if (imageFile == null)
        {
            // Try to find the file if it has a different field name
            if (form.Files.Count > 0)
            {
                imageFile = form.Files[0]; // Use the first file regardless of its name
                Console.WriteLine($"No file with name 'image' found, using first file: {imageFile.Name}");
            }
            else
            {
                Console.WriteLine("No image file found");
                return Results.BadRequest("Image file is required");
            }
        }

        // Extract form values
        string title = form["title"]!;
        string user = form["user"]!;
        int category = 0;
        if (form.ContainsKey("category") && !string.IsNullOrEmpty(form["category"]))
        {
            int.TryParse(form["category"], out category);
        }

        Console.WriteLine($"Parsed values: Title={title}, Category={category}, User={user}, Image={imageFile.FileName}");

        // Process the upload
        Guid guid = Guid.NewGuid();
        string imageUrl = await blobController.PushImageToBlob(imageFile, guid);

        Post newPost = new Post
        {
            Title = title,
            Category = (Category)category,
            User = user,
            BlobImage = guid,
            Url = imageUrl
        };

        Console.WriteLine("Creating post via repository");
        return await repo.CreateAPIPost(newPost);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in Posts/Add: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.BadRequest($"Error processing request: {ex.Message}");
    }
})
.Produces<PostReadDTO>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("Posts_Add")
.WithOpenApi()
.DisableAntiforgery();

app.MapGet("/Posts/{id}", async (IRepository_mini repo, Guid id) =>
{
    var post = await repo.GetPostById(id);
    return post != null ? Results.Ok(post) 
    : Results.NotFound(new { message = $"No post found with ID: {id}" });
});

app.MapDelete("/Posts/{id}", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.DeletePost(id);
    return result ? Results.Ok(new { message = $"Post with ID {id} was successfully deleted" }) 
    : Results.NotFound(new { message = $"No post found with ID: {id}" });
});

app.MapGet("/Posts/Index", async (IRepository_mini repo, int page = 1, int pageSize = 10) =>
{
    var result = await repo.GetPostIndex(page, pageSize);
    return Results.Ok(result);
});

app.MapGet("/Posts/Count", async (IRepository_mini repo) =>
{
    int count = await repo.GetPostCount();
    return Results.Ok(new { totalPosts = count });
});

// Increment post likes
app.MapPost("/Posts/{id}/Like", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.IncrementPostLike(id);
    return result ? Results.Ok() 
    : Results.NotFound(new { message = $"No post found with ID: {id}" });
});

// Increment post dislikes
app.MapPost("/Posts/{id}/Dislike", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.IncrementPostDislike(id);
    return result ? Results.Ok() : Results.NotFound(new { message = $"No post found with ID: {id}" });
});



//Comments
// Create a new comment
app.MapPost("/Comments/Add", async (IRepository_mini repo, [FromBody] Comment comment) =>
{
    if (comment == null)
    {
        return Results.BadRequest(new { message = "Comment data is required" });
    }

    if (string.IsNullOrEmpty(comment.Commentaire))
    {
        return Results.BadRequest(new { message = "Comment text is required" });
    }

    if (string.IsNullOrEmpty(comment.User))
    {
        return Results.BadRequest(new { message = "User is required" });
    }

    if (comment.PostId == Guid.Empty)
    {
        return Results.BadRequest(new { message = "Post ID is required" });
    }

    try
    {
        return await repo.CreateAPIComment(comment);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
})
.Produces<CommentReadDTO>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("Comments_Add")
.WithOpenApi(operation =>
{
    // Add request body schema
    operation.RequestBody = new OpenApiRequestBody
    {
        Content = new Dictionary<string, OpenApiMediaType>
        {
            ["application/json"] = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["commentaire"] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = "The comment text"
                        },
                        ["user"] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = "The username of the commenter"
                        },
                        ["postId"] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid",
                            Description = "The ID of the post this comment belongs to"
                        }
                    },
                    Required = new HashSet<string> { "commentaire", "user", "postId" }
                },
                Example = new OpenApiObject
                {
                    ["commentaire"] = new OpenApiString("This is a sample comment"),
                    ["user"] = new OpenApiString("JohnDoe"),
                    ["postId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6")
                }
            }
        },
        Required = true
    };

    return operation;
});



// Get a single comment by ID
app.MapGet("/Comments/{id}", async (IRepository_mini repo, Guid id) =>
{
    var comment = await repo.GetCommentById(id);
    return comment != null
        ? Results.Ok(comment)
        : Results.NotFound(new { message = $"No comment found with ID: {id}" });
});

// Delete a comment
app.MapDelete("/Comments/{id}", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.DeleteComment(id);
    return result ? Results.Ok() : Results.NotFound();
});

// Get paginated comments
app.MapGet("/Comments/Index", async (IRepository_mini repo, int page = 1, int pageSize = 10) =>
{
    var result = await repo.GetCommentIndex(page, pageSize);
    return Results.Ok(result);
});

// Get the total number of comments
app.MapGet("/Comments/Count", async (IRepository_mini repo) =>
{
    int count = await repo.GetCommentCount();
    return Results.Ok(new { totalComments = count });
});

// Approve a comment
app.MapPost("/Comments/{id}/Approve", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.ApproveComment(id);
    return result ? Results.Ok() : Results.NotFound();
});

// Increment comment likes
app.MapPost("/Comments/{id}/Like", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.IncrementCommentLike(id);
    return result ? Results.Ok() : Results.NotFound();
});

// Increment comment dislikes
app.MapPost("/Comments/{id}/Dislike", async (IRepository_mini repo, Guid id) =>
{
    var result = await repo.IncrementCommentDislike(id);
    return result ? Results.Ok() : Results.NotFound();
});




app.Run();


