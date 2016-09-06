using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class ConstraintController : Controller
    {
        // GET: Contraint
        public ActionResult Index()
        {
            return View("ConstraintView");
        }
       [AcceptVerbs(WebRequestMethods.Http.Get)]
        public PartialViewResult DayAndTime()
        {
            if (Request.IsAjaxRequest()){
            
                return PartialView("_DayTimeView");
            }
            else
            {
                return PartialView("_DayTimeView");
            }
        }

       public string SaveFriend(string friendlist)
       {
           Session["SelectedFriend"] = friendlist;
           return "Saved.";
       }
    }
}