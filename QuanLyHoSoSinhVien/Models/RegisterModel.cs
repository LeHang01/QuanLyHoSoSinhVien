using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamTrai.Models
{
    public class RegisterModel
    {
        [Key]
        public int ID { set; get; }

        [Display(Name = "Họ")]
    
        public string Ho { set; get; }
        [Display(Name = "Tên")]
     
        public string Ten { set; get; }

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Yêu cầu nhập tên đăng nhập")]

        public string UserName { set; get; }

        [Display(Name = "Mật khẩu")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Độ dài mật khẩu ít nhất 6 ký tự.")]
        [Required(ErrorMessage = "Yêu cầu nhập mật khẩu")]
        public string Password { set; get; }

        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu không đúng.")]
        public string ConfirmPassword { set; get; }


        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }
        [Display(Name = "MSSV")]
        public string Mssv { get; set; }
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }
        [Display(Name = "Điện thoại")]
        public string Phone { set; get; }

        [Required(ErrorMessage = "Yêu cầu nhập email")]
        [Display(Name = "Email")]

        public string Email { set; get; }

        [Display(Name = "Lớp")]
        public string MaLop { get; set; }

        [Display(Name = "Ảnh thẻ sinh viên")]
        public string Image { get; set; }

    }
}