using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class StudentProfileController : Controller
    {

        // GET: StudentProfile
        public ActionResult Index()
        {
            return View();
        }
        public ViewResult GetStudentProfile()
        {
            //if (Session["SelectedPlan"] == null)
            //{
            //    // create new Session
            //    Session["SelectedPlan"] = "";
            //}
            //if (Session["PlandetailJSON"] == null)
            //{
            //    Session["PlandetailJSON"] = "";
            //}
            //if (Session["SelectedFriend"] == null)
            //{
            //    Session["SelectedFriend"] = "";
            //}

            string studentID = Session["StudentId"].ToString();
            StudentProfile user = new StudentProfile();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                user = (from s in prz.Students
                        where s.StudentCode == studentID
                        select new StudentProfile()
                        {
                            ID = studentID,
                            Name = s.FirstNameEN + " " + s.LastNameEN,
                            Nation = s.NationNameEN,
                            BirthDate = "-",
                            Level = s.LevelNameEN,
                            Faculty = s.FacultyNameEN,
                            Major = s.DepartmentNameEN,
                            Gender = s.Gender.ToString(),
                            StudyYear = "-",
                            GPA = Convert.ToDouble(s.CurrentGPA),
                            Credit = Convert.ToInt32(s.CurrentCredit),
                            FacebookID = s.Facebook_ID
                        }).SingleOrDefault();

                Session["StudentProfileName"] = user.Name;
                Session["StudentProfileFaculty"] = user.Faculty;
                Session["StudentProfileMajor"] = user.Major;
                Session["StudentProfileFacebookID"] = user.FacebookID;

                //user.FriendList = (from f in prz.Friends
                //                   where f.StudentCode == studentID
                //                   select f.FriendFacebookID).ToList();
            }

            return View("StudentProfileView", user);
        }
    }
}