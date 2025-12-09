namespace reviewApi.DTO
{
    public class RequestLoginDTO
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }
    public class ResponseLoginDTO
    {
        public string? token { get; set; }
    }
    public class RequestRegisterDTO
    {
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? DepartmentCode { get; set; }
        public string? RoleCode { get; set; }
        public string? Password { get; set; }
    }
}
