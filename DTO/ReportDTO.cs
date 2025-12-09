namespace reviewApi.DTO
{
    public class ReportDTO
    {
        public List<Row> data { get; set; }
        public ReportInfor reportInfo { get; set; }
        public Pagination pagination { get; set; }
    }
    public class Row
    {
        public int stt { get; set; }
        public string canBo { get; set; }
        public string nguoiDanhGia { get; set; }
        public string donVi { get; set; }
        public int diemCaNhan { get; set; }
        public int diemDonVi { get; set; }
        public string phanLoaiCaNhan { get; set; }
    }
    public class ReportInfor
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public string date { get; set; }
    }
    
}
