using System.Text.Json;
using System.Text.Json.Serialization;
using FileBlogSystem.Features;

namespace FileBlogSystem.Features;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [JsonConverter(typeof(RolesListJsonConverter))]
    public List<Roles> Roles { get; set; } = new();
}

public class RolesListJsonConverter : JsonConverter<List<Roles>>
{
    public override List<Roles> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var roles = new List<Roles>();
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;
            var value = reader.GetString();
            if (value != null && Enum.TryParse<Roles>(value, true, out var role))
                roles.Add(role);
            else
                throw new JsonException($"Invalid role value: {value}");
        }
        return roles;
    }

    public override void Write(Utf8JsonWriter writer, List<Roles> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var role in value)
        {
            writer.WriteStringValue(role.ToString());
        }
        writer.WriteEndArray();
    }
}
