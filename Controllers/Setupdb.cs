using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;
using reviewApi.Models;

namespace reviewApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly IUnitOfWork _iUnitOfWork;

        public SetupController(
           IUnitOfWork iUnitOfWork
        )
        {
            _iUnitOfWork = iUnitOfWork;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Setup()
        {
            List<Department> departments = new List<Department>
            {
               new Department { Code = "G00", Name = "Tất cả đơn vị", ParentCode = null },
               new Department { Code = "G01.000.000", Name = "Bộ Công an", ParentCode = "G00" },
               new Department { Code = "G01.501.000", Name = "Văn Phòng Bộ Công an", ParentCode = "G01.000.000" },
               new Department { Code = "G01.501.001.000", Name = "Phòng 1", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.002.000", Name = "Phòng 2", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.003.000", Name = "Phòng 3", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.004.000", Name = "Phòng 4", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.005.000", Name = "Phòng 5", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.006.000", Name = "Phòng 6", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.007.000", Name = "Phòng 7", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.008.000", Name = "Phòng 8", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.008.0001", Name = "Đội Kế hoạch", ParentCode = "G01.501.008.000" },
               new Department { Code = "G01.501.008.0002", Name = "Đội Kế toán, tài vụ", ParentCode = "G01.501.008.000" },
               new Department { Code = "G01.501.008.0003", Name = "Đội xe", ParentCode = "G01.501.008.000" },
               new Department { Code = "G01.501.008.0005", Name = "Đội Hậu cần phía nam", ParentCode = "G01.501.008.000" },
               new Department { Code = "G01.501.012.000", Name = "Trung tâm Thông tin chỉ huy", ParentCode = "G01.501.000" },
               new Department { Code = "G01.501.012.001", Name = "Ban 1 - TTTTCH", ParentCode = "G01.501.012.000" },
               new Department { Code = "G01.501.012.002", Name = "Ban 2 - TTTTCH", ParentCode = "G01.501.012.000" },
               new Department { Code = "G01.501.012.003", Name = "Ban 3 - TTTTCH", ParentCode = "G01.501.012.000" },
               new Department { Code = "G01.501.012.004", Name = "Ban 4 - TTTTCH", ParentCode = "G01.501.012.000" }
            };
            _iUnitOfWork.Department.AddRange(departments);

            List<Role> roles = new List<Role>
            {
               new Role { Code = "BO_TRUONG", Name = "Bộ trưởng" },
               new Role { Code = "THU_TRUONG", Name = "Thứ trưởng" },
               new Role { Code = "THU_KY_LANH_DAO_BO", Name = "Thư ký lãnh đạo bộ" },
               new Role { Code = "TONG_CUC_TRUONG", Name = "Tổng cục trưởng" },
               new Role { Code = "PHO_TONG_CUC_TRUONG", Name = "Phó Tổng cục trưởng" },
               new Role { Code = "VU_TRUONG", Name = "Vụ trưởng" },
               new Role { Code = "PHO_VU_TRUONG", Name = "Phó Vụ trưởng" },
               new Role { Code = "CHANH_VAN_PHONG", Name = "Chánh Văn phòng" },
               new Role { Code = "CUC_TRUONG", Name = "Cục trưởng" },
               new Role { Code = "GIAM_DOC", Name = "Giám đốc" },
               new Role { Code = "HIEU_TRUONG", Name = "Hiệu Trưởng" },
               new Role { Code = "TU_LENH", Name = "Tư Lệnh" },
               new Role { Code = "VIEN_TRUONG", Name = "Viện Trưởng" },
               new Role { Code = "PHO_CHANH_VAN_PHONG", Name = "Phó Chánh Văn phòng" },
               new Role { Code = "PHO_CUC_TRUONG", Name = "Phó Cục trưởng" },
               new Role { Code = "PHO_GIAM_DOC", Name = "Phó Giám đốc" },
               new Role { Code = "PHO_HIEU_TRUONG", Name = "Phó Hiệu trưởng" },
               new Role { Code = "PHO_TU_LENH", Name = "Phó Tư lệnh" },
               new Role { Code = "PHO_VIEN_TRUONG", Name = "Phó Viện Trưởng" },
               new Role { Code = "GIAM_DOC_CONG", Name = "Giám Đốc CTTDT" },
               new Role { Code = "GIAM_DOC_TTTTCH", Name = "Giám đốc TTTTCH" },
               new Role { Code = "TRUONG_CONG_AN_XA", Name = "Trưởng Công an xã/phường" },
               new Role { Code = "TRUONG_PHONG", Name = "Trưởng phòng" },
               new Role { Code = "PHO_GIAM_DOC_CONG", Name = "Phó Giám đốc CTTDT" },
               new Role { Code = "PHO_GIAM_DOC_TTTTCH", Name = "Phó Giám đốc TTTTCH" },
               new Role { Code = "PHO_TRUONG_CONG_AN_XA", Name = "Phó Trưởng Công an xã/phường" },
               new Role { Code = "PHO_TRUONG_PHONG", Name = "Phó Trưởng phòng" },
               new Role { Code = "DOI_TRUONG", Name = "Đội trưởng" },
               new Role { Code = "TRUONG_BAN", Name = "Trưởng Ban" },
               new Role { Code = "PHO_DOI_TRUONG", Name = "Phó Đội trưởng" },
               new Role { Code = "PHO_TRUONG_BAN", Name = "Phó Trưởng ban" },
               new Role { Code = "TO_TRUONG", Name = "Tổ trưởng" },
               new Role { Code = "PHO_TO_TRUONG", Name = "Phó Tổ trưởng" },
               new Role { Code = "CAN_BO", Name = "Cán bộ" },
               new Role { Code = "VAN_THU", Name = "Văn thư" },
               new Role { Code = "CHIEN_SI", Name = "Chiến sĩ" },
               new Role { Code = "THU_KY", Name = "Thư ký" },
               new Role { Code = "KE_TOAN", Name = "Kế toán" },
               new Role { Code = "TRO_LY", Name = "Trợ lý" },
               new Role { Code = "NHAN_VIEN", Name = "Nhân viên" }
            };
            _iUnitOfWork.Role.AddRange(roles);

            _iUnitOfWork.User.Add(new User
            {
               Username = "admin",
               FullName = "Administrator",
               DepartmentCode = "G00",
               RoleCode = "BO_TRUONG",
               Password = "Admin@123"
            });
            _iUnitOfWork.User.Add(new User
            {
                Username = "canBoPhong1",
                FullName = "Cán Bộ Phòng 1",
                DepartmentCode = "G01.501.001.000",
                RoleCode = "CAN_BO",
                Password = "canBoPhong1@123"
            });
            _iUnitOfWork.User.Add(new User
            {
                Username = "truongPhongPhong1",
                FullName = "Trưởng Phòng Phòng 1",
                DepartmentCode = "G01.501.001.000",
                RoleCode = "TRUONG_PHONG",
                Password = "truongPhongPhong1@123"
            });
            _iUnitOfWork.User.Add(new User
            {
                Username = "phoTruongPhongPhong1",
                FullName = "Phó Trưởng Phòng Phòng 1",
                DepartmentCode = "G01.501.001.000",
                RoleCode = "PHO_TRUONG_PHONG",
                Password = "phoTruongPhongPhong1@123"
            });
            var res = _iUnitOfWork.Complete();

            return Ok(new { message = $"Hoàn thành setup data: {res}" });
        }
    }
}
