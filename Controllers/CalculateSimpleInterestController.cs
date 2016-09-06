using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class CalculateSimpleInterestController : Controller
    {
        // GET: CalculateSimpleInterest
        public ActionResult SimpleInterest()
        {
            return View("View");
        }

        [HttpPost]
        public ActionResult CalculateSimpleInterestResult()
        {
            decimal principle = Convert.ToDecimal(Request["txtAmount"].ToString());
            decimal rate = Convert.ToDecimal(Request["txtRate"].ToString());
            int time = Convert.ToInt32(Request["txtYear"].ToString());

            decimal simpleInteresrt = (principle * time * rate) / 100;

            StringBuilder sbInterest = new StringBuilder();
            sbInterest.Append("<b>Amount :</b> " + principle + "<br/>");
            sbInterest.Append("<b>Rate :</b> " + rate + "<br/>");
            sbInterest.Append("<b>Time(year) :</b> " + time + "<br/>");
            sbInterest.Append("<b>Interest :</b> " + simpleInteresrt);
            return Content(sbInterest.ToString());
        }
    }
}