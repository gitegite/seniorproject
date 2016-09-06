using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;
using SeniorProject.ClassLookUpListModel;
using System.Net;


namespace SeniorProject.Controllers
{
    public class SectionSelectorController : Controller
    {
        // GET: SectionSelector
        public ActionResult Index()
        {
            return View();
        }

        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public JsonResult GetStudentSectionDetailJSON(short year, short semester, string courseCode, short sectionNumber)
        {
            List<object> sectionDetailList = new List<object>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from course in prz.Courses
                         join section in prz.Sections on course.CourseCode equals section.CourseCode
                         join detail in prz.SectionDetails on new { section.CourseCode, section.SectionNumber }
                             equals new { detail.CourseCode, detail.SectionNumber }
                         where section.Year == year && section.Semester == semester &&
                               section.CourseCode == courseCode && section.SectionNumber == sectionNumber
                         select new
                         {
                             Year = section.Year,
                             Semester = section.Semester,
                             CourseCode = course.CourseCode,
                             CourseName = course.NameEN,
                             SectionNumber = section.SectionNumber,
                             SeatAvailable = section.SeatAvailable,
                             DayNum = detail.Day,
                             StartTime = detail.StartTime,
                             EndTime = detail.EndTime,
                             InstructorCode = detail.InstructorCode,
                             IsMorning = detail.IsMorning
                         });

                foreach (var d in q)
                {
                    sectionDetailList.Add(new
                    {
                        Year = d.Year,
                        Semester = d.Semester,
                        CourseCode = d.CourseCode,
                        CourseName = d.CourseName,
                        SectionNumber = d.SectionNumber,
                        SeatAvailable = d.SeatAvailable,
                        Day = d.DayNum,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        InstructorCode = d.InstructorCode,
                        IsMorning = d.IsMorning

                    });
                }
            }

            return Json(sectionDetailList, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(WebRequestMethods.Http.Post)]
        public ActionResult ShowSection(short year, short semester, string courseCode, string studentCode)
        /*Get Available Section for specific course on specific year & semester along with friend id who saved plan on the section */
        {
            List<StudentSection> sectionList = new List<StudentSection>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from course in prz.Courses
                         join section in prz.Sections on course.CourseCode equals section.CourseCode
                         where course.CourseCode == courseCode && section.Year == year
                         && section.Semester == semester
                         select new
                         {
                             Year = section.Year,
                             Semester = section.Semester,
                             CourseCode = course.CourseCode,
                             CourseName = course.NameEN,
                             SectionNumber = section.SectionNumber,
                             IsClosed = section.IsClosed,
                             SeatAvailable = section.SeatAvailable,
                             SeatLimit = section.SeatLimit,
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

                         });

                foreach (var sec in q)
                {

                    List<StudentSectionDetail> details = (from detail in prz.SectionDetails
                                                          where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
                                                          && sec.Year == detail.Year && sec.Semester == detail.Semester
                                                          select new StudentSectionDetail()
                                                          {
                                                              DayNum = detail.Day,
                                                              StartTime = detail.StartTime,
                                                              EndTime = detail.EndTime,
                                                              InstructorCode = detail.InstructorCode,
                                                              Room = detail.RoomCode,
                                                              IsMorning = detail.IsMorning
                                                          }).ToList();


                    int amount = (from schedule in prz.SavedSchedules
                                  where schedule.ScheduleList.Contains(sec.CourseCode + "," +
                                  sec.SectionNumber)
                                  select schedule.StudentCode).Distinct().Count();

                    List<string> friendSavedList = (from friend in prz.Friends
                                                    join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                                    join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                                                    where friend.StudentCode == studentCode
                                                    && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                                                    select student.StudentCode).Distinct().ToList();


                    StudentSection section = new StudentSection();
                    section.Year = sec.Year;
                    section.Semester = sec.Semester;
                    section.CourseCode = sec.CourseCode;
                    section.CourseName = sec.CourseName;
                    section.SectionNumber = sec.SectionNumber;
                    section.IsClosed = sec.IsClosed;
                    section.SeatAvailable = sec.SeatAvailable;
                    section.SeatLimited = sec.SeatLimit;
                    section.SeatUsed = sec.SeatUsed;
                    section.MidtermDate = sec.MidtermDate;
                    section.MidtermStartTime = sec.MidtermStartTime;
                    section.MidtermEndTime = sec.MidtermEndTime;
                    section.MidtermRoom = sec.MidtermRoom;
                    section.FinalDate = sec.FinalDate;
                    section.FinalStartTime = sec.FinalStartTime;
                    section.FinalEndTime = sec.FinalEndTime;
                    section.FinalRoom = sec.FinalRoom;
                    section.Remark = sec.Remark;
                    section.SectionDetails = details;
                    section.AllSavedAmount = amount;
                    section.FriendSavedList = friendSavedList;
                    sectionList.Add(section);
                }
            }


            if (Request.IsAjaxRequest())
            {
                //return Json(sectionList);
                return PartialView("_SectionView", sectionList);
            }
            else
            {
                return View(sectionList);
            }

        }
                [AcceptVerbs(WebRequestMethods.Http.Get)]

        public JsonResult ShowSectionJSON(short year, short semester, string courseCode, string studentCode)
        /*Get Available Section for specific course on specific year & semester along with friend id who saved plan on the section */
        {
            List<StudentSection> sectionList = new List<StudentSection>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from course in prz.Courses
                         join section in prz.Sections on course.CourseCode equals section.CourseCode
                         where course.CourseCode == courseCode && section.Year == year
                         && section.Semester == semester
                         select new
                         {
                             Year = section.Year,
                             Semester = section.Semester,
                             CourseCode = course.CourseCode,
                             CourseName = course.NameEN,
                             SectionNumber = section.SectionNumber,
                             IsClosed = section.IsClosed,
                             SeatAvailable = section.SeatAvailable,
                             SeatLimit = section.SeatLimit,
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

                         });

                foreach (var sec in q)
                {

                    List<StudentSectionDetail> details = (from detail in prz.SectionDetails
                                                          join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode into inst
                                                          from a in inst.DefaultIfEmpty()
                                                          where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
                                                          && sec.Year == detail.Year && sec.Semester == detail.Semester
                                                          select new StudentSectionDetail()
                                                          {
                                                              DayNum = detail.Day,
                                                              DayName = GetDayName(detail.Day),
                                                              StartTime = detail.StartTime,
                                                              EndTime = detail.EndTime,
                                                              StartTimeNum = detail.Start,
                                                              EndTimeNum = detail.End,
                                                              InstructorCode = detail.InstructorCode,
                                                              Instructor = a.FirstNameEN + " " + a.LastNameEN,
                                                              Room = detail.RoomCode,
                                                              IsMorning = detail.IsMorning
                                                          }).ToList();


                    //int amount = (from schedule in prz.SavedSchedules
                    //              where schedule.ScheduleList.Contains(sec.CourseCode + "," +
                    //              sec.SectionNumber)
                    //              select schedule.StudentCode).Distinct().Count();

                    //List<string> friendSavedList = (from friend in prz.Friends
                    //                                join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                    //                                join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                    //                                where friend.StudentCode == studentCode
                    //                                && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                    //                                select student.StudentCode).Distinct().ToList();


                    StudentSection section = new StudentSection();
                    section.Year = sec.Year;
                    section.Semester = sec.Semester;
                    section.CourseCode = sec.CourseCode;
                    section.CourseName = sec.CourseName;
                    section.SectionNumber = sec.SectionNumber;
                    section.IsClosed = sec.IsClosed;
                    section.SeatAvailable = sec.SeatAvailable;
                    section.SeatLimited = sec.SeatLimit;
                    section.SeatUsed = sec.SeatUsed;
                    section.MidtermDate = sec.MidtermDate;
                    section.MidtermStartTime = sec.MidtermStartTime;
                    section.MidtermEndTime = sec.MidtermEndTime;
                    section.MidtermRoom = sec.MidtermRoom;
                    section.FinalDate = sec.FinalDate;
                    section.FinalStartTime = sec.FinalStartTime;
                    section.FinalEndTime = sec.FinalEndTime;
                    section.FinalRoom = sec.FinalRoom;
                    section.Remark = sec.Remark;
                    section.SectionDetails = details;
                    //section.AllSavedAmount = amount;
                    //section.FriendSavedList = friendSavedList;
                    section.isChecked = false;
                    sectionList.Add(section);
                }
            }            
                //return Json(sectionList);
                return Json(sectionList, JsonRequestBehavior.AllowGet);
           

        }
        
        
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public ActionResult GetCourses(short year, short semester)
        {
            ViewBag.Select = Session["SelectedPlan"];
            //ViewBag.PJSON = Session["PlandetailJSON"];
            CoursesList cList = new CoursesList();
            //cList.CourseList = GetStudentCourse(year, semester);
            //return View("SectionView", cList);
            return View("SectionView");
        }

        [AcceptVerbs(WebRequestMethods.Http.Post)]
        public string AddSubject(string CourseCode, string selectedsections)
        // Add or Update CourseCode & Selected Section to Session
        {
            string newString = CourseCode + "," + selectedsections;
            string currentString = Session["SelectedPlan"].ToString();
            string[] SavedPlan = currentString.Split(';');
            bool exist = false;
            int counter = 0;
            for (int i = 0; i < SavedPlan.Length; i++)
            {
                string[] check = SavedPlan[i].Split(',');
                if (check[0].Equals(CourseCode))
                {
                    exist = true;
                    counter = i;
                    break;
                }
            }

            if (!exist)
            {
                if (!currentString.Equals(""))
                {
                    currentString += ";" + newString;
                }
                else
                {
                    currentString += newString;
                }
                Session["SelectedPlan"] = currentString;
            }
            else
            {
                string newPlan = "";
                for (int i = 0; i < SavedPlan.Length; i++)
                {
                    if (i != counter)
                    {
                        newPlan += SavedPlan[i] + ";";
                    }
                    else
                    {
                        newPlan += newString + ";";
                    }
                }
                Session["SelectedPlan"] = newPlan.Substring(0, newPlan.Length - 1);
            }
            //Session["PlandetailJSON"] = GetCourseListDetailJSON(2015, 1, Session["SelectedPlan"].ToString());
            return Session["SelectedPlan"].ToString();
        }

        public List<StudentCourse> GetStudentCourse(short year, short semester)
        {
            List<StudentCourse> courseList = new List<StudentCourse>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from c in prz.Courses
                         join sec in prz.Sections on c.CourseCode equals sec.CourseCode
                         where sec.Year == year && sec.Semester == semester
                         select new
                         {
                             CourseCode = c.CourseCode,
                             CourseName = c.NameEN,
                             Credit = c.Credit,
                             CreditSum = c.CreditSum
                         }).Distinct();

                foreach (var i in q)
                {
                    StudentCourse course = new StudentCourse();
                    course.CourseCode = i.CourseCode;
                    course.CourseName = i.CourseName;
                    course.Credit = i.Credit;
                    course.CreditSum = i.CreditSum;
                    courseList.Add(course);
                }
            }
            return courseList;
        }
        public List<StudentSection> GetStudentSection(short year, short semester, string courseCode)
        {
            List<StudentSection> sectionList = new List<StudentSection>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from course in prz.Courses
                         join section in prz.Sections on course.CourseCode equals section.CourseCode
                         where course.CourseCode == courseCode && section.Year == year
                         && section.Semester == semester
                         select new
                         {
                             Year = section.Year,
                             Semester = section.Semester,
                             CourseCode = course.CourseCode,
                             CourseName = course.NameEN,
                             SectionNumber = section.SectionNumber,
                             IsClosed = section.IsClosed,
                             SeatAvailable = section.SeatAvailable,
                             SeatLimit = section.SeatLimit,
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

                         });

                foreach (var sec in q)
                {

                    List<StudentSectionDetail> details = (from detail in prz.SectionDetails
                                                          where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
                                                          && sec.Year == detail.Year && sec.Semester == detail.Semester
                                                          select new StudentSectionDetail()
                                                          {
                                                              DayNum = detail.Day,
                                                              StartTime = detail.StartTime,
                                                              EndTime = detail.EndTime,
                                                              InstructorCode = detail.InstructorCode,
                                                              Room = detail.RoomCode,
                                                              IsMorning = detail.IsMorning
                                                          }).ToList();


                    StudentSection section = new StudentSection();
                    section.Year = sec.Year;
                    section.Semester = sec.Semester;
                    section.CourseCode = sec.CourseCode;
                    section.CourseName = sec.CourseName;
                    section.SectionNumber = sec.SectionNumber;
                    section.IsClosed = sec.IsClosed;
                    section.SeatAvailable = sec.SeatAvailable;
                    section.SeatLimited = sec.SeatLimit;
                    section.SeatUsed = sec.SeatUsed;
                    section.MidtermDate = sec.MidtermDate;
                    section.MidtermStartTime = sec.MidtermStartTime;
                    section.MidtermEndTime = sec.MidtermEndTime;
                    section.MidtermRoom = sec.MidtermRoom;
                    section.FinalDate = sec.FinalDate;
                    section.FinalStartTime = sec.FinalStartTime;
                    section.FinalEndTime = sec.FinalEndTime;
                    section.FinalRoom = sec.FinalRoom;
                    section.Remark = sec.Remark;
                    section.SectionDetails = details;
                    sectionList.Add(section);
                }
            }
            return sectionList;
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
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public JsonResult GetStudentCourseJSON(short year, short semester)
        {
            List<object> courseList = new List<object>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from c in prz.Courses
                         join sec in prz.Sections on c.CourseCode equals sec.CourseCode
                         where sec.Year == year && sec.Semester == semester
                         select new
                         {
                             CourseCode = c.CourseCode,
                             CourseName = c.NameEN,
                             Credit = c.Credit,
                             CreditSum = c.CreditSum
                         }).Distinct();

                foreach (var i in q)
                {
                    object course = new
                    {
                        CourseCode = i.CourseCode,
                        CourseName = i.CourseName,
                        Credit = i.Credit,
                        CreditSum = i.CreditSum
                    };

                    courseList.Add(course);
                }
            }
            return Json(courseList, JsonRequestBehavior.AllowGet);
        }

        public string GetDayName(int dayNum)
        {
            if (dayNum == 1)
            {
                return "SUNDAY";
            }
            else if (dayNum == 2)
            {
                return "MONDAY";
            }
            else if (dayNum == 3)
            {
                return "TUESDAY";
            }
            else if (dayNum == 4)
            {
                return "WEDNESDAY";
            }
            else if (dayNum == 5)
            {
                return "THURSDAY";
            }
            else if (dayNum == 6)
            {
                return "FRIDAY";
            }
            else
            {
                return "SATURDAY";
            }
        }
    }
}
