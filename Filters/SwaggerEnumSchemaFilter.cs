using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace Banko.Filters
{
  public class SwaggerEnumSchemaFilter : ISchemaFilter
  {
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
      if (context.Type.IsEnum)
      {
        var enumValues = Enum.GetNames(context.Type);
        schema.Enum.Clear();
        schema.Type = "string";
        schema.Format = null;

        foreach (var value in enumValues)
        {
          var field = context.Type.GetField(value);
          var descriptionAttribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
              .FirstOrDefault() as DescriptionAttribute;

          var description = descriptionAttribute?.Description ?? value;
          schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(description));
        }
      }
    }
  }
}