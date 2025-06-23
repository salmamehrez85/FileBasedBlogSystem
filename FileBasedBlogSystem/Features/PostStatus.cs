using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileBlogSystem.Features
{
    [JsonConverter(typeof(PostStatusJsonConverter))]
    public enum PostStatus
    {
        [EnumMember(Value = "draft")]
        Draft,
        [EnumMember(Value = "published")]
        Published,
        [EnumMember(Value = "archived")]
        Archived
    }

    public class PostStatusJsonConverter : JsonConverter<PostStatus>
    {
        public override PostStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null)
                throw new JsonException();

            foreach (var field in typeof(PostStatus).GetFields())
            {
                var attr = (EnumMemberAttribute?)Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute));
                if (attr != null && string.Equals(attr.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return (PostStatus)field.GetValue(null)!;
                }
            }

            throw new JsonException($"Invalid PostStatus value: {value}");
        }

        public override void Write(Utf8JsonWriter writer, PostStatus value, JsonSerializerOptions options)
        {
            var field = typeof(PostStatus).GetField(value.ToString());
            var attr = (EnumMemberAttribute?)Attribute.GetCustomAttribute(field!, typeof(EnumMemberAttribute));
            writer.WriteStringValue(attr?.Value ?? value.ToString().ToLower());
        }
    }
} 