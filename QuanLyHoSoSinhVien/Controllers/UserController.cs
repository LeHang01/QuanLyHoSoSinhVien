
using BotDetect.Web.UI.Mvc;
using CamTrai.Common;
using CamTrai.Models;
using Model.DAO;
using Model.EF;
using CamTrai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Facebook;
using System.Configuration;

namespace CamTrai.Controllers
{
    public class UserController : Controller
    {
        // GET: User
      
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }
        [HttpPost]
        [CaptchaValidation("CaptchaCode", "registerCapcha", "Mã xác nhận không đúng!")]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var dao = new SinhVienDao();
                if (dao.CheckUserName(model.UserName))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại");
                }
                else if (dao.CheckEmail(model.Email))
                {
                    ModelState.AddModelError("", "Email đã tồn tại");
                }
                else
                {
                    var user = new tb_SinhVien();
                    user.Ho = model.Ho;
                    user.Ten = model.Ten;
                    user.UserName  = model.UserName;
                    user.Password = Encryptor.MD5Hash(model.Password);
                    user.GioiTinh = model.GioiTinh;
                    user.Mssv = model.Mssv;
                    user.DiaChi = model.DiaChi;
                    user.NgaySinh = model.NgaySinh;
                    user.Phone = model.Phone;
                    user.Email = model.Email;
                    user.MaLop = model.MaLop;
                    user.Image = model.Image;
                    user.Status = true;
                    user.CreatedDate = DateTime.Now;
                    
                    

                    var result = dao.Insert(user);
                    if (result > 0)
                    {
                        ViewBag.Success = "Đăng ký thành công";
                        model = new RegisterModel();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Đăng ký không thành công.");
                    }
                }
            }
            return View();
        }
        
        public ActionResult Login()
        {

            return View();
        }
        public ActionResult profile(long id)
        {
            var model = new SinhVienDao().GetByID(id);
            ViewData["sinhvien"] = model;
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var dao = new SinhVienDao();
                var result = dao.Login(model.UserName, Encryptor.MD5Hash(model.Password));
                if (result == 1)
                {
                    var user = dao.GetById(model.UserName);
                    var userSession = new UserLogin();
                    userSession.UserName = user.UserName;
                    userSession.UserID = user.ID;
                    Session.Add(CommonConstants.USER_SESSION, userSession);
                    return RedirectToAction("Index", "Home");
                }
                else if (result == 0)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại.");
                }
                else if (result == -2)
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                }
                else
                {
                    ModelState.AddModelError("", "đăng nhập không đúng.");
                }
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session[CommonConstants.USER_SESSION] = null;
            return RedirectToAction("Index", "Home");
        }
        public ActionResult LoginFacebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = ConfigurationManager.AppSettings["FbAppId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email",
            });

            return Redirect(loginUrl.AbsoluteUri);
        }
        public ActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = ConfigurationManager.AppSettings["FbAppId"],
                client_secret = ConfigurationManager.AppSettings["FbAppSecret"],
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });


            var accessToken = result.access_token;
            if (!string.IsNullOrEmpty(accessToken))
            {
                fb.AccessToken = accessToken;
                // Get the user's information, like email, first name, middle name etc
                dynamic me = fb.Get("me?fields=first_name,middle_name,last_name,id,email");
                string email = me.email;
                string userName = me.email;
                string firstname = me.first_name;
                string middlename = me.middle_name;
                string lastname = me.last_name;

                var user = new tb_SinhVien();
                user.Email = email;
                user.UserName = userName;
                user.Status = true;
                user.Ho = firstname + " " + middlename ;
                user.Ten = lastname;
                user.CreatedDate = DateTime.Now;
                var resultInsert = new SinhVienDao().InsertForFacebook(user);
                if (resultInsert > 0)
                {
                    var userSession = new UserLogin();
                    userSession.UserName = user.UserName;
                    userSession.UserID = user.ID;
                    Session.Add(CommonConstants.USER_SESSION, userSession);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Edit(int id)
        {
            tb_SinhVien sv = new SinhVienDao().ViewDetail(id);
            ViewData["sinhvien"] = sv;
            return View();
        }
        [HttpPost]
        public ActionResult Edit(tb_SinhVien SV)
        {


            if (ModelState.IsValid)
            {
                var dao = new SinhVienDao();
                var result = dao.Update(SV);
                if (result)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Cập nhật thành công");
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}