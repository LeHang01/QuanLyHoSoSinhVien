using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Model.DAO;
namespace CamTrai.Controllers
{
    public class SinhvienController : Controller
    {
        // GET: Sinhvien
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Detail(long id)
        {
            var model = new SinhVienDao().GetByID(id);
            ViewData["sinhvien"] = model;


            return View();
        }
        [ChildActionOnly]
        public PartialViewResult sinhvien()
        {
            var model = new SVDao().ListAll();
            return PartialView(model);
        }
        public JsonResult ListName(string q)
        {
            try
            {
                var data = new SinhVienDao().ListName(q);
                return Json(new
                {
                    data = data,
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý ngoại lệ
                return Json(new
                {
                    status = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult sv()
        {
            var model = new SVDao().ListAll();
            return PartialView(model);
        }
        public PartialViewResult sv19()
        {
            var model = new SVDao().ListAll();
            return PartialView(model);
        }
        public ActionResult Search(string keyword, int page = 1, int pageSize = 1)
        {
            int totalRecord = 0;
            var model = new SinhVienDao().Search(keyword, ref totalRecord, page, pageSize);

            ViewBag.Total = totalRecord;
            ViewBag.Page = page;
            ViewBag.Keyword = keyword;
            int maxPage = 5;
            int totalPage = 0;

            totalPage = (int)Math.Ceiling((double)(totalRecord / pageSize));
            ViewBag.TotalPage = totalPage;
            ViewBag.MaxPage = maxPage;
            ViewBag.First = 1;
            ViewBag.Last = totalPage;
            ViewBag.Next = page + 1;
            ViewBag.Prev = page - 1;

            return View(model);
        }
    }
}