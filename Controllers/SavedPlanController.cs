using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class SavedPlanController : Controller
    {
        static List<StudentSavedPlan> UAllsaved = new List<StudentSavedPlan>();
        static int Uselectplan = 0;
        static int Uselectschedule = 0;
        // GET: SavedPlan
        public ActionResult Index()
        {
            string StudentID = Session["StudentId"].ToString();
            //StudentProfile Userprofile = GetStudentProfile(StudentID);
            //if (Userprofile.AllSavedPlans != null)
            //{
            //    UAllsaved = Userprofile.AllSavedPlans;
            //}
            UAllsaved = GetSavedPlan(StudentID);
            return View("Index", UAllsaved);
        }

        public List<StudentSavedPlan> GetSavedPlan(string studentCode)
        {
            List<StudentSavedPlan> savedPlans = new List<StudentSavedPlan>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                savedPlans = (from plan in prz.SavedPlans
                              where plan.StudentCode == studentCode
                              select new StudentSavedPlan
                              {
                                  PlanList = plan.PlanList,
                                  schedules = (from schedule in prz.SavedSchedules
                                               where schedule.StudentCode == studentCode && schedule.PlanList == plan.PlanList
                                               select schedule).ToList()
                              }).ToList();
                for (int i = 0; i < savedPlans.Count; ++i )
                {
                    string[] courseCodes = savedPlans[i].PlanList.Split(';');
                    foreach (string courseCode in courseCodes)
                    {
                        savedPlans[i].CourseNameList.Add((from course in prz.Courses
                                                          where course.CourseCode == courseCode
                                                          select course.NameEN).SingleOrDefault());
                    }

                }
            }
            return savedPlans;
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
            foreach (StudentSavedPlan saved in UAllsaved)
            {
                List<StudentCourse> savedList = new List<StudentCourse>();
                savedList = GetCourseListDetailFriends(2015, 1, saved.schedules[0].ScheduleList, Session["StudentId"].ToString(), saved.schedules[0].SavedFrom);
                AllsavedList.Add(savedList);
            }
            return PartialView("PlanList", AllsavedList);
        }

        public ActionResult GetSchedule(int index)
        {
            List<List<StudentCourse>> plan = new List<List<StudentCourse>>();
            Uselectplan = index;
            foreach (SavedSchedule i in UAllsaved[index].schedules)
            {
                List<StudentCourse> course = new List<StudentCourse>();
                course = GetCourseListDetailFriends(2015, 1, i.ScheduleList, Session["StudentId"].ToString(), i.SavedFrom);
                plan.Add(course);
            }
            return PartialView("Schedule", plan);
        }

        public List<StudentCourse> GetCourseListDetailFriends(short year, short semester, string courseList, string studentCode, string remark)
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
                    c.remark = remark != studentCode ? (from student in prz.Students
                                                        where student.StudentCode == remark
                                                        select student.FirstNameEN).SingleOrDefault() + " (" + remark + ")" : "";

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
                                                  FinalRoom = section.FinalRoom,

                                              }).SingleOrDefault();

                        sec.FriendList = (from friend in prz.Friends
                                          join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                          join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                                          where friend.StudentCode == studentCode
                                          && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                                          select new FacebookFriend
                                          {
                                              StudentCode = student.StudentCode,
                                              StudentName = student.FirstNameEN,
                                              StudentFaculty = student.FacultyNameEN,
                                              StudentMajor = student.DepartmentNameEN,
                                              FacebookID = student.Facebook_ID
                                          }).Distinct().ToList();

                        sec.SectionDetails = (from detail in prz.SectionDetails
                                              join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode into inst
                                              from a in inst.DefaultIfEmpty()
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
                                                  Instructor = a.FirstNameEN + " " + a.LastNameEN,
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

        public string DeleteSchedule(int index)
        {
            string studentID = Session["StudentId"].ToString();
            string planList = UAllsaved[Uselectplan].PlanList;
            string scheduleString = UAllsaved[Uselectplan].schedules[index - 1].ScheduleList;
            UAllsaved[Uselectplan].schedules.RemoveAt(index - 1);

            string result = "Some problems occurred while the program deletes the schedule";
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                SavedSchedule schedule = (from savedSchedule in prz.SavedSchedules
                                          where savedSchedule.StudentCode == studentID && savedSchedule.PlanList == planList &&
                                          savedSchedule.ScheduleList == scheduleString
                                          select savedSchedule).SingleOrDefault();
                prz.SavedSchedules.DeleteOnSubmit(schedule);
                prz.SubmitChanges();
                if (prz.SavedSchedules.Where(s => s.StudentCode == studentID && s.PlanList == planList).ToList().Count() == 0)
                {
                    SavedPlan plan = (from savedPlan in prz.SavedPlans
                                      where savedPlan.StudentCode == studentID && savedPlan.PlanList == planList
                                      select savedPlan).SingleOrDefault();
                    prz.SavedPlans.DeleteOnSubmit(plan);
                    prz.SubmitChanges();
                    UAllsaved.RemoveAt(Uselectplan);
                }
                result = "Schedule Deleted Successfully";
            }
            return result;
        }

        public string DeletePlan(int index)
        {
            string studentID = Session["StudentId"].ToString();
            string planList = UAllsaved[index].PlanList;
            string result = "Some problems occurred while the program deletes plan List";
            UAllsaved.RemoveAt(index);
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                SavedPlan plan = (from savedPlan in prz.SavedPlans
                                  where savedPlan.StudentCode == studentID && savedPlan.PlanList == planList
                                  select savedPlan).SingleOrDefault();
                prz.SavedPlans.DeleteOnSubmit(plan);

                var schedule = (from savedSchedule in prz.SavedSchedules
                                where savedSchedule.StudentCode == studentID && savedSchedule.PlanList == planList
                                select savedSchedule).ToList();

                prz.SavedSchedules.DeleteAllOnSubmit(schedule);
                prz.SubmitChanges();

                result = "Plan & Schedules Deleted Successfully";
            }
            return result;
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
    }
}