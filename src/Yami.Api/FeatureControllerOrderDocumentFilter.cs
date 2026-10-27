using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FeatureControllerOrderDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var descriptions = context.ApiDescriptions
            .Select(description => description.ActionDescriptor)
            .OfType<ControllerActionDescriptor>()
            .ToList();

        var orderedTags = descriptions
            .Select(GetFeatureName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(featureName => featureName, StringComparer.Ordinal)
            .Select(featureName => new OpenApiTag { Name = featureName })
            .ToList();

        var tagOrder = orderedTags
            .Select((tag, index) => (Name: tag.Name ?? string.Empty, index))
            .ToDictionary(tag => tag.Name, tag => tag.index, StringComparer.Ordinal);

        swaggerDoc.Tags = new SortedSet<OpenApiTag>(
            orderedTags,
            Comparer<OpenApiTag>.Create((first, second) =>
            {
                var firstName = first.Name ?? string.Empty;
                var secondName = second.Name ?? string.Empty;
                var order = tagOrder[firstName].CompareTo(tagOrder[secondName]);
                return order != 0 ? order : StringComparer.Ordinal.Compare(firstName, secondName);
            }));

        foreach (var description in context.ApiDescriptions)
        {
            if (description.ActionDescriptor is not ControllerActionDescriptor controller ||
                string.IsNullOrWhiteSpace(description.HttpMethod))
            {
                continue;
            }

            var path = $"/{(description.RelativePath ?? string.Empty).TrimStart('/')}";
            if (!swaggerDoc.Paths.TryGetValue(path, out var pathItem) || pathItem is null ||
                pathItem.Operations is null ||
                !pathItem.Operations.TryGetValue(new HttpMethod(description.HttpMethod), out var operation))
            {
                continue;
            }

            var featureName = GetFeatureName(controller);
            operation.Tags = new HashSet<OpenApiTagReference>
            {
                new(featureName, swaggerDoc)
            };

            var controllerName = controller.ControllerName;
            var actionName = controller.ActionName;
            var operationLabel = controllerName == actionName
                ? controllerName
                : $"{controllerName}: {actionName}";

            operation.Summary = string.IsNullOrWhiteSpace(operation.Summary)
                ? operationLabel
                : $"{controllerName}: {operation.Summary}";
        }
    }

    private static string GetFeatureName(ControllerActionDescriptor descriptor)
    {
        var assemblyName = descriptor.ControllerTypeInfo.Assembly.GetName().Name ?? string.Empty;
        var featureName = assemblyName.Split('.').Last();

        return featureName.Length == 0
            ? featureName
            : char.ToUpperInvariant(featureName[0]) + featureName[1..];
    }
}