using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace reviewApi.DTO
{
    public class CriteriaSetDTO
    {
          public int? id { get; set; }
          public string? name { get; set; }
          public List<string>? applicableObjects { get; set; }
          public List<PeriodDTO>? applicationPeriods { get; set; }
          public List<ClassficationDTO>? classificationLevels { get; set; }
          public List<CriteriaNodeDTO>? criteriaTree { get; set; }
    }
    public class PeriodDTO
    {
        public int month { get; set; }
        public int year { get; set; }
    }
    public class ClassficationDTO
    {
        [JsonConverter(typeof(NullableIntRelaxedConverter))]
        public int? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? abbreviation { get; set; }
        public int fromScore { get; set; }
        public int toScore { get; set; }
    }
    public class CriteriaNodeDTO
    {
        [JsonConverter(typeof(NullableIntRelaxedConverter))]
        public int? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? maxScore { get; set; }
        public string? scoreType { get; set; }
        public List<CriteriaNodeDTO>? children { get; set; } 
    }

    // Converter cho phép id là null/chuỗi rỗng/chuỗi số
    public class NullableIntRelaxedConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var n)) return n;
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (int.TryParse(s, out var v)) return v;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue) writer.WriteNumberValue(value.Value);
            else writer.WriteNullValue();
        }
    }
    
}
