using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            if (Session["StudentId"] == null)
            {
                Session["StudentId"] = "";
            }
            Session["SelectedPlan"] = "";
            //if (Session["god"] != null)
            //{
            //    Session["god"] = "";
            //}
            //if (Session["coursename"] != null)
            //{
            //    Session["coursename"] = "";
            //}
            ViewBag.StudentID = Session["StudentId"];
            return View();
        }

        public bool Authenticate(string username, string password)
        {
            Session["StudentId"] = username.Substring(1, username.Length-1);
            return true;
        }


    }
}