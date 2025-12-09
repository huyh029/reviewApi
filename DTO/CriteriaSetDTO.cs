using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
        public int? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public string? abbreviation { get; set; }
        public int fromScore { get; set; }
        public int toScore { get; set; }
    }
    public class CriteriaNodeDTO
    {
        public int? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public int? maxScore { get; set; }
        public string? scoreType { get; set; }
        public List<CriteriaNodeDTO>? children { get; set; } 
    }
    
}
