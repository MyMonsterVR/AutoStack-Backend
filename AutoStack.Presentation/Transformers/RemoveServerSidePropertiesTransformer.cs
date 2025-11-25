using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AutoStack.Presentation.Transformers;

internal sealed class RemoveServerSidePropertiesTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Remove UserId from CreateStack request body since it's set server-side from JWT
        foreach (var path in document.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) == true)
                {
                    // Check if this is the CreateStack operation by checking if it has the properties we expect
                    if (mediaType.Schema?.Properties?.ContainsKey("userId") == true &&
                        mediaType.Schema?.Properties?.ContainsKey("name") == true &&
                        mediaType.Schema?.Properties?.ContainsKey("stackInfo") == true)
                    {
                        mediaType.Schema.Properties.Remove("userId");
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}
