using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;

namespace SeniorProject.Controllers
{
    public class FriendsPlanController : Controller
    {
        // GET: FriendsPlan
        public ActionResult Index(string studentID)
        {
            StudentProfile StudInfo = new StudentProfile();
            StudInfo = GetStudentProfile(studentID);
            Session["FriendSavePlan"] = StudInfo.AllSavedPlans;
            return View("Index", StudInfo);
        }

        public StudentProfile GetStudentProfile(string studentCode)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var studentProfile = (from student in prz.Students
                                      where studentCode == student.StudentCode
                                      select new StudentProfile
                                      {
                                          ID = student.StudentCode,
                                          Name = String.Format("{0} {1}", student.FirstNameEN, student.LastNameEN),
                                          Nation = student.NationNameEN,
                                          ProgramName = student.ProgramNameEN,
                                          BirthDate = student.BirthDate,
                                          Level = student.LevelNameEN,
                                          Faculty = student.FacultyNameEN,
                                          Major = student.DepartmentNameEN,
                                          Gender = student.Gender.ToString(),
                                          GPA = Convert.ToDouble(student.CurrentGPA),
                                          Credit = Convert.ToInt32(student.CurrentCredit),
                                          FacebookID = student.Facebook_ID,
                                          FriendList = (from friend in prz.Friends
                                                        where friend.StudentCode == studentCode
                                                        select friend.FriendFacebookID).ToList(),
                                          AllSavedPlans = (from plan in prz.SavedPlans
                                                           where plan.StudentCode == studentCode
                                                           select new StudentSavedPlan
                                                           {
                                                               PlanList = plan.PlanList,
                                                               schedules = (from schedule in prz.SavedSchedules
                                                                            where schedule.StudentCode == studentCode && schedule.PlanList == plan.PlanList
                                                                            select schedule).ToList()
                                                           }).ToList()
                                      }).SingleOrDefault();
                return studentProfile;
            }
        }

        public ActionResult GetPlanList()
        {
            List<List<StudentCourse>> AllsavedList = new List<List<StudentCourse>>();
            List<StudentSavedPlan> SavedPlans = (List<StudentSavedPlan>)Session["FriendSavePlan"];
            foreach (StudentSavedPlan saved in SavedPlans)
            {
                List<StudentCourse> savedList = new List<StudentCourse>();
                savedList = GetCourseListDetail(2015, 1, saved.schedules[0].ScheduleList);
                AllsavedList.Add(savedList);
            }
            return PartialView("PlanList", AllsavedList);
        }

        public ActionResult GetSchedule(int index)
        {
            List<List<StudentCourse>> plan = new List<List<StudentCourse>>();
            Session["FriendPlanIndex"] = index;
            List<StudentSavedPlan> SavedPlans = (List<StudentSavedPlan>) Session["FriendSavePlan"];
            foreach (SavedSchedule i in SavedPlans[index].schedules)
            {
                List<StudentCourse> course = new List<StudentCourse>();
                course = GetCourseListDetail(2015, 1, i.ScheduleList);
                plan.Add(course);
            }
            return PartialView("Schedule", plan);
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
                                              }).OrderBy(a => a.DayNum).ThenBy(a => a.StartTimeNum).ToList();
                        c.Sections.Add(sec);

                    }
                    courses.Add(c);
                }
            }
            return courses;
        }

        public string duplicatePlan(int scheduleIndex, string friendID)
        {
            List<StudentSavedPlan> FriendSavedPlans = (List<StudentSavedPlan>)Session["FriendSavePlan"];
            Session["FriendScheduleIndex"] = scheduleIndex;
            return savePlan2(Session["StudentID"].ToString(), FriendSavedPlans[(int)Session["FriendPlanIndex"]].PlanList,
                    FriendSavedPlans[(int)Session["FriendPlanIndex"]].schedules[(int)Session["FriendScheduleIndex"]].ScheduleList,
                    friendID);
        }

        public string savePlan2(string studentID, string planList, string scheduleList, string friendID)
        {

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
                        SavedFrom = friendID
                    });

                    prz.SubmitChanges();
                    resultMessage += "Schedule saved successfully, You can edit it from your saved plan page.";
                }
                else
                {
                    resultMessage += "You already have this schedule in your saved plans.";
                }
                return resultMessage;
            }
        }
    }
}