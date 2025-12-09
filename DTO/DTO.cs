using DocumentFormat.OpenXml.Drawing.Charts;
using System.Text.Json.Serialization;

namespace reviewApi.DTO
{
    public class RoleDTO
    {
        public string? code { get; set; }
        public string? name { get; set; }
    }
    public class DepartmentDTO
    {
        public string? id { get; set; }
        public string? code { get; set; }
        public string? name { get; set; }
        public List<DepartmentDTO>? children { get; set; }
    }   
    public class responseDTO<T>
    {
        public string? message { get; set; }
        public T? data { get; set; }
        public Pagination? pagination { get; set; }
    }
    public class Pagination
    {
        public int currentPage { get; set; }
        public int totalPages { get; set; }
        public int totalItems { get; set; }
        public int itemsPerPage { get; set; }
        public int? draftTotalItems { get; set; }
    }
    public class DeleteMultiple<T>
    {
        public bool? all { get; set; } = false;
        public List<T>? exclude { get; set; }
        public Filter? filters { get; set; }
        public List<T>? ids { get; set; }
    }
    public class Filter
    {
        public string? search { get; set; }
        [JsonPropertyName("object")]
        public string? _object { get; set; }
        public int? month { get; set; }
        public int? year { get; set; }
        public string? status { get; set; }
        public string? period { get; set; }
    }
    public class UserDTO
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? title { get; set; }
    }
}
