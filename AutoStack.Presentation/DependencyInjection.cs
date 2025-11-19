using System.Reflection;

namespace AutoStack.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection service)
    {
        service.AddOpenApi();

        return service;
    }
}