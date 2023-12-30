using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CamTrai.Areas.Admin.Controllers
{
    public class HomeController : AllController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}