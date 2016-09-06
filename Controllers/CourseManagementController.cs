using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;

namespace SeniorProject.Controllers
{
    public class CourseManagementController : Controller
    {
        short Year = 2015;
        short Semester = 2;
        // GET: AddCourse
        public ActionResult Index()
        {
            if (Session["SelectedPlan"] == null)
            {
                // create new Session
                Session["SelectedPlan"] = "";
            }
            if (Session["PlandetailJSON"] == null)
            {
                Session["PlandetailJSON"] = "";
            }
            ViewBag.Select = Session["SelectedPlan"];
            ViewBag.PJSON = Session["PlandetailJSON"];
            return View("Index");
        }

        public PartialViewResult CurrentCourse()
        {
            string currentCourse = Session["SelectedPlan"].ToString();
            List<StudentCourse> courselist = new List<StudentCourse>();
            if (!currentCourse.Equals(""))
            {
                courselist = GetCourseListDetail(Year, Semester, currentCourse);
            }
            return PartialView("_SelectCourseListView", courselist);
        }

        public PartialViewResult GeneratedSchedule()
        {
            List<Timetable> genSchedule;
            return PartialView("_GenerateSchedulesView");
        }

        public string DeleteSubject(string CourseCode)
        // Delete Specific Subject from Session
        {
            string currentString = Session["SelectedPlan"].ToString();
            string[] SavedPlan = currentString.Split(';');
            string newPlan = "";
            for (int i = 0; i < SavedPlan.Length; i++)
            {
                if (!SavedPlan[i].Split(',')[0].Equals(CourseCode))
                    newPlan += SavedPlan[i] + ";";
            }
            if (!newPlan.Equals(""))
            {
                Session["SelectedPlan"] = newPlan.Substring(0, newPlan.Length - 1);
                Session["PlandetailsJSON"] = GetCourseListDetailJSON(Year, Semester, Session["SelectedPlan"].ToString());
            }
            else
            {
                Session["SelectedPlan"] = "";
                Session["PlandetailsJSON"] = "";
            }
            return "Complete: Delete " + CourseCode + " from the list.";
        }

        public List<StudentCourse> GetCourseListDetail(short year, short semester, string courseList)
        {
            // Format --> ACT1600,401,402,403,404;BG1001,401,405,406;BG14036,401,402,403
            List<StudentCourse> courses = new List<StudentCourse>();
            string[] courseSections = courseList.Split(';');
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                foreach (string cs in courseSections)
                {
                    string[] courseSection = cs.Split(',');
                    string courseCode = courseSection[0];
                    StudentCourse c = new StudentCourse();

                    var q = (from course in prz.Courses
                             where course.CourseCode == courseCode
                             select new
                             {
                                 CourseName = course.NameEN,
                                 Credit = course.Credit,
                                 CreditSum = course.CreditSum
                             }).SingleOrDefault();
                    c.CourseName = q.CourseName;
                    c.CourseCode = courseCode;
                    c.Credit = q.Credit;
                    c.CreditSum = q.CreditSum;

                    for (int i = 1; i < courseSection.Length; ++i)
                    {
                        short sectionNumber = Convert.ToInt16(courseSection[i]);
                        StudentSection sec = (from course in prz.Courses
                                              join section in prz.Sections on course.CourseCode equals section.CourseCode
                                              where course.CourseCode == courseCode && section.Year == year
                                              && section.Semester == semester && section.SectionNumber == sectionNumber
                                              select new StudentSection()
                                              {
                                                  Year = section.Year,
                                                  Semester = section.Semester,
                                                  CourseCode = course.CourseCode,
                                                  CourseName = course.NameEN,
                                                  SectionNumber = section.SectionNumber,
                                                  IsClosed = section.IsClosed,
                                                  SeatAvailable = section.SeatAvailable,
                                                  SeatLimited = section.SeatLimit,
                                                  SeatUsed = section.SeatUsed,
                                                  Remark = section.Remark,
                                                  MidtermDate = section.MidtermDate,
                                                  MidtermStartTime = section.MidtermStartTime,
                                                  MidtermEndTime = section.MidtermEndTime,
                                                  MidtermRoom = section.MidtermRoom,
                                                  FinalDate = section.FinalDate,
                                                  FinalStartTime = section.FinalStartTime,
                                                  FinalEndTime = section.FinalEndTime,
                                                  FinalRoom = section.FinalRoom
                                              }).SingleOrDefault();

                        sec.SectionDetails = (from detail in prz.SectionDetails
                                              join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                                              where detail.Year == year && detail.Semester == semester
                                              && detail.CourseCode == courseCode && detail.SectionNumber == sectionNumber
                                              select new StudentSectionDetail()
                                              {
                                                  Year = year,
                                                  Semester = semester,
                                                  CourseCode = detail.CourseCode,
                                                  CourseName = sec.CourseName,
                                                  DayNum = detail.Day,
                                                  StartTime = detail.StartTime,
                                                  StartTimeNum = detail.Start,
                                                  EndTime = detail.EndTime,
                                                  EndTimeNum = detail.End,
                                                  InstructorCode = detail.InstructorCode,
                                                  Instructor = instructor.FirstNameEN + " " + instructor.LastNameEN,
                                                  Room = detail.RoomCode,
                                                  IsMorning = detail.IsMorning
                                              }).ToList();
                        c.Sections.Add(sec);

                    }
                    courses.Add(c);
                }
            }
            return courses;
        }

        public JsonResult GetCourseListDetailJSON(short year, short semester, string courseList)
        {
            // Format --> ACT1600,401,402,403,404;BG1001,401,405,406;BG14036,401,402,403
            List<StudentCourse> courses = new List<StudentCourse>();
            string[] courseSections = courseList.Split(';');
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                foreach (string cs in courseSections)
                {
                    string[] courseSection = cs.Split(',');
                    string courseCode = courseSection[0];
                    StudentCourse c = new StudentCourse();

                    var q = (from course in prz.Courses
                             where course.CourseCode == courseCode
                             select new
                             {
                                 CourseName = course.NameEN,
                                 Credit = course.Credit,
                                 CreditSum = course.CreditSum
                             }).SingleOrDefault();
                    c.CourseName = q.CourseName;
                    c.CourseCode = courseCode;
                    c.Credit = q.Credit;
                    c.CreditSum = q.CreditSum;

                    for (int i = 1; i < courseSection.Length; ++i)
                    {
                        short sectionNumber = Convert.ToInt16(courseSection[i]);
                        StudentSection sec = (from course in prz.Courses
                                              join section in prz.Sections on course.CourseCode equals section.CourseCode
                                              where course.CourseCode == courseCode && section.Year == year
                                              && section.Semester == semester && section.SectionNumber == sectionNumber
                                              select new StudentSection()
                                              {
                                                  Year = section.Year,
                                                  Semester = section.Semester,
                                                  CourseCode = course.CourseCode,
                                                  CourseName = course.NameEN,
                                                  SectionNumber = section.SectionNumber,
                                                  IsClosed = section.IsClosed,
                                                  SeatAvailable = section.SeatAvailable,
                                                  SeatLimited = section.SeatLimit,
                                                  SeatUsed = section.SeatUsed,
                                                  Remark = section.Remark,
                                                  MidtermDate = section.MidtermDate,
                                                  MidtermStartTime = section.MidtermStartTime,
                                                  MidtermEndTime = section.MidtermEndTime,
                                                  MidtermRoom = section.MidtermRoom,
                                                  FinalDate = section.FinalDate,
                                                  FinalStartTime = section.FinalStartTime,
                                                  FinalEndTime = section.FinalEndTime,
                                                  FinalRoom = section.FinalRoom
                                              }).SingleOrDefault();

                        sec.SectionDetails = (from detail in prz.SectionDetails
                                              join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                                              where detail.Year == year && detail.Semester == semester
                                              && detail.CourseCode == courseCode && detail.SectionNumber == sectionNumber
                                              select new StudentSectionDetail()
                                              {
                                                  Year = year,
                                                  Semester = semester,
                                                  CourseCode = detail.CourseCode,
                                                  CourseName = sec.CourseName,
                                                  DayNum = detail.Day,
                                                  StartTime = detail.StartTime,
                                                  EndTime = detail.EndTime,
                                                  InstructorCode = detail.InstructorCode,
                                                  Instructor = instructor.FirstNameEN + " " + instructor.LastNameEN,
                                                  Room = detail.RoomCode,
                                                  IsMorning = detail.IsMorning
                                              }).ToList();
                        c.Sections.Add(sec);

                    }
                    courses.Add(c);
                }
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }
    }
}