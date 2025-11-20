using System.Reflection;
using AutoStack.Presentation.Transformers;

namespace AutoStack.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection service)
    {
        service.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return service;
    }
}