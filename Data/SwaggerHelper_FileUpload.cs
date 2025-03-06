using Microsoft.OpenApi.Models;
using MVC.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetParameters().Any(p => p.ParameterType == typeof(PostCreateDTO)))
        {
            operation.Parameters.Clear();

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
                                ["title"] = new OpenApiSchema { Type = "string" },
                                ["category"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                                ["user"] = new OpenApiSchema { Type = "string" },
                                ["image"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            },
                            Required = new HashSet<string> { "title", "category", "user", "image" }
                        }
                    }
                }
            };
        }
    }
}
