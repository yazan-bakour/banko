using System.Text.Json;
using System.Text.Json.Serialization;

namespace Banko.Helpers
{
  public class EnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
  {
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      string? value = reader.GetString();
      return value == null ? default : Enum.Parse<TEnum>(value, true);
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
      writer.WriteStringValue(value.ToString());
    }
  }
}