using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class AddCourseController : Controller
    {
        // GET: AddCourse
        public ActionResult Index()
        {
            return View("AddAndScheduleView");
        }
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public string SetStudentConstraint(string studentID, string constraintList)
        {//constraintList.ToUpper();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var query = (from c in prz.StudentConstraints
                             where c.StudentCode == studentID
                             select c).SingleOrDefault();
                if (query == null)
                {
                    prz.StudentConstraints.InsertOnSubmit(new StudentConstraint
                    {
                        StudentCode = studentID,
                        ConstraintList = constraintList
                    });
                }
                else
                {
                    query.ConstraintList = constraintList;
                }

                prz.SubmitChanges();
            }
            return "yeah boy";
        }
        [AcceptVerbs(WebRequestMethods.Http.Post)]
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
                Session["PlandetailsJSON"] = GetCourseListDetailJSON(2015, 1, Session["SelectedPlan"].ToString());
            }
            else
            {
                Session["SelectedPlan"] = "";
                Session["PlandetailsJSON"] = "";
            }
            return Session["SelectedPlan"].ToString();
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
        public string savePlan(string studentID, string planList, string scheduleList)
        {
            planList = SortPlanList(planList);
            scheduleList = SortScheduleList(planList, scheduleList);

            string resultMessage = "";

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                int planCount = prz.SavedPlans.Where(x => x.PlanList == planList && x.StudentCode == studentID).Count();
                if (planCount == 0)
                {
                    prz.SavedPlans.InsertOnSubmit(new SavedPlan
                    {
                        StudentCode = studentID,
                        PlanList = planList
                    });
                    prz.SubmitChanges();
                }

                int scheduleCount = prz.SavedSchedules.Where(x => x.ScheduleList == scheduleList && x.StudentCode == studentID
                    && x.PlanList == planList).Count();

                if (scheduleCount == 0)
                {
                    prz.SavedSchedules.InsertOnSubmit(new SavedSchedule
                    {
                        StudentCode = studentID,
                        PlanList = planList,
                        ScheduleList = scheduleList,
                        SavedFrom = studentID
                    });

                    prz.SubmitChanges();
                    resultMessage += "Schedule saved successfully";
                }
                else
                {
                    resultMessage += "This schedule has already existed in your saved plans";
                }
                string resultM = resultMessage;
                return resultMessage;
            }
        }

        //[AcceptVerbs(WebRequestMethods.Http.Post)]
        //public ActionResult GetSavedPlan(string studentID)
        //{
        //    List<StudentSavedPlan> savedPlanList = new List<StudentSavedPlan>();

        //    using (ParazoDBDataContext prz = new ParazoDBDataContext())
        //    {
        //        savedPlanList = (from p in prz.SavedPlans
        //                         where studentID == p.StudentCode
        //                         select new StudentSavedPlan
        //                         {
        //                             StudentCode = p.StudentCode,
        //                             PlanList = p.PlanList
        //                         }).ToList();

        //        for (int i = 0; i < savedPlanList.Count; ++i)
        //        {
        //            savedPlanList[i].schedules = (from s in prz.SavedSchedules
        //                                          where studentID == s.StudentCode &&
        //                                          savedPlanList[i].PlanList == s.PlanList
        //                                          select s).ToList();
        //        }

        //    }
        //    StudentSavedPlanList savedPlan = new StudentSavedPlanList();
        //    savedPlan.SavedPlans.AddRange(savedPlanList);

        //    return PartialView("SavedPlanList", savedPlan);
        //}
        
        public string SortPlanList(string planList)
        {
            string resultPlan = "";
            string[] temp = planList.Split(';');

            for (int i = 0; i < temp.Length; ++i)
            {
                for (int j = 0; j < temp.Length; ++j)
                {
                    if (temp[i].CompareTo(temp[j]) < 0)
                    {
                        string tmp = temp[i];
                        temp[i] = temp[j];
                        temp[j] = tmp;
                    }
                }
            }

            foreach (string t in temp)
            {
                resultPlan += t + ";";
            }
            resultPlan = resultPlan.Substring(0, resultPlan.Length - 1);
            return resultPlan;
        }

        public string SortScheduleList(string planList, string scheduleList)
        {
            string resultScheduleList = "";
            string[] planTemp = planList.Split(';');
            string[] schedulePlanTemp = scheduleList.Split(';');
            foreach (string p in planTemp)
            {
                resultScheduleList += p + ",";
                foreach (string spTemp in schedulePlanTemp)
                {
                    string[] subject = spTemp.Split(',');
                    if (subject[0] == p)
                    {
                        resultScheduleList += subject[1] + ";";
                        break;
                    }
                }
            }
            resultScheduleList = resultScheduleList.Substring(0, resultScheduleList.Length - 1);
            return resultScheduleList;
        }

        public string RegisterSchedule(short year, short semester, string godSchedule, string studentCode)
        {
            string result = "";
            string[] courseList = godSchedule.Split(';');
            List<string> noSeatSection = new List<string>();
            List<string> registeredSection = new List<string>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                foreach (string course in courseList)
                {
                    string[] subject = course.Split(',');
                    short sectionNumber = Int16.Parse(subject[1]);
                    int seat = (from section in prz.Sections
                                where section.Year == year && section.Semester == semester &&
                                section.CourseCode == subject[0] && section.SectionNumber == sectionNumber
                                select section.SeatAvailable).SingleOrDefault();
                    if (seat == 0)
                    {
                        noSeatSection.Add(course);
                    }
                    else
                    {
                        registeredSection.Add(course);
                    }
                }

                if (noSeatSection.Count == 0)
                {
                    result = "Success\n";
                    foreach (string course in registeredSection)
                    {
                        result += course + ";";
                    }
                }
                else
                {
                    result = "Fail\n";
                    foreach (string course in noSeatSection)
                    {
                        result += course + ";";
                    }
                    
                }
                result = result.Substring(0, result.Length - 1);
                result += "\n" + godSchedule;
                return result;
            }
        }

        public string FaveSection(string courseCode, string sectionNumber)
        {
            return courseCode + "," + sectionNumber;
        }


    }
}