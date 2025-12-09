namespace reviewApi.DTO
{
    public class EvaluationObjectsDTO
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }
    public class DepartmentNodeDTO
    {
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public List<DepartmentNodeDTO> children { get; set; }
        public List<IndividualDTO> individuals { get; set; }
    }
    public class IndividualDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public List<string>? selectedObjectIds { get; set; }
    }
    public class assigmentDTO
    {
        public int individualId { get; set; }
        public List<string> selectedObjectIds { get; set; }
    }
}
