using System.Reflection;
using AutoStack.Presentation.Middleware;
using AutoStack.Presentation.Transformers;

namespace AutoStack.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection service)
    {
        service.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddDocumentTransformer<RemoveServerSidePropertiesTransformer>();
        });

        service.AddGlobalExceptionHandler();

        return service;
    }
}