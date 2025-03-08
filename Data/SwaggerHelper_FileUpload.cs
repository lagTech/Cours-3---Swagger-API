using Microsoft.OpenApi.Models;
using MVC.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using System.Reflection;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Log the method being examined for debugging
        Console.WriteLine($"Examining method: {context.MethodInfo.Name} for operation {operation.OperationId}");

        // Examine all parameters for this method
        foreach (var parameter in context.MethodInfo.GetParameters())
        {
            Console.WriteLine($"  Parameter: {parameter.Name}, Type: {parameter.ParameterType.Name}");

            // Check attributes on parameter
            foreach (var attr in parameter.GetCustomAttributes())
            {
                Console.WriteLine($"    Attribute: {attr.GetType().Name}");
            }
        }

        // Check if this endpoint has a parameter that is PostCreateDTO or has [FromForm]
        var hasFileUpload = context.MethodInfo.GetParameters().Any(p =>
            p.ParameterType == typeof(PostCreateDTO) ||
            p.ParameterType == typeof(IFormFile) ||
            p.GetCustomAttributes<FromFormAttribute>().Any());

        Console.WriteLine($"Has file upload: {hasFileUpload}");

        // Check specific endpoint - always configure /Posts/Add endpoint
        var isPostsAddEndpoint = operation.OperationId != null &&
                                operation.OperationId.EndsWith("Posts_Add");

        Console.WriteLine($"Is Posts/Add endpoint: {isPostsAddEndpoint}");

        if (hasFileUpload || isPostsAddEndpoint || context.MethodInfo.Name.Contains("Posts_Add"))
        {
            Console.WriteLine($"Configuring Swagger for file upload endpoint: {operation.OperationId}");

            // Clear existing parameters
            operation.Parameters.Clear();

            // Define the multipart/form-data request
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["title"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Title of the post"
                                },
                                ["category"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Enum = new List<IOpenApiAny> {
                                        new OpenApiInteger(0), // Humour
                                        new OpenApiInteger(1), // Nouvelle
                                        new OpenApiInteger(2)  // Inconnue
                                    },
                                    Description = "Category (0=Humour, 1=Nouvelle, 2=Inconnue)"
                                },
                                ["user"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Username of the poster"
                                },
                                ["image"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Image file to upload (max 40MB)"
                                }
                            },
                            Required = new HashSet<string> { "title", "user", "image" }
                        },
                        // Add example to help users
                        Example = new OpenApiObject
                        {
                            ["title"] = new OpenApiString("Example Post Title"),
                            ["category"] = new OpenApiInteger(0),
                            ["user"] = new OpenApiString("example_user")
                            // Can't provide example for binary file
                        }
                    }
                },
                Required = true
            };
        }
    }
}