using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CamTrai.Areas.Admin.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Nhập Tên đăng nhập")]
        public string UserName { set; get; }
        [Required(ErrorMessage = "Nhập Mật khẩu")]
        public string Password { set; get; }
        
        public bool RememberMe { set; get; }
    }
}