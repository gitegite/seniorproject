using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace SeniorProject.Controllers
{
    public class ServiceController : Controller
    {

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

        public ActionResult GetStudentCourse(short year, short semester)
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
            return View("SectionView",courseList);
        }

        public JsonResult GetStudentSectionJSON(short year, short semester, string courseCode, string studentCode)
        {
            List<object> sectionList = new List<object>();
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

                    var query = (from detail in prz.SectionDetails
                                 where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
                                 && sec.Year == detail.Year && sec.Semester == detail.Semester
                                 select new
                                 {
                                     Day = detail.Day,
                                     StartTime = detail.StartTime,
                                     StartTimeNum = detail.Start,
                                     EndTime = detail.EndTime,
                                     EndTimeNum = detail.End,
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


                    object section = new
                    {
                        Year = sec.Year,
                        Semester = sec.Semester,
                        CourseCode = sec.CourseCode,
                        CourseName = sec.CourseName,
                        SectionNumber = sec.SectionNumber,
                        IsClosed = sec.IsClosed,
                        SeatAvailable = sec.SeatAvailable,
                        SeatLimit = sec.SeatLimit,
                        SeatUsed = sec.SeatUsed,
                        MidtermDate = sec.MidtermDate,
                        MidtermStartTime = sec.MidtermStartTime,
                        MidtermEndTime = sec.MidtermEndTime,
                        MidtermRoom = sec.MidtermRoom,
                        FinalDate = sec.FinalDate,
                        FinalStartTime = sec.FinalStartTime,
                        FinalEndTime = sec.FinalEndTime,
                        FinalRoom = sec.FinalRoom,
                        Remark = sec.Remark,
                        AllSavedAmount = amount,
                        FriendSavedList = friendSavedList,
                        SectionDetailList = query
                    };

                    sectionList.Add(section);
                }
            }
            return Json(sectionList, JsonRequestBehavior.AllowGet);
        }

        public string GetFriendStatus(string FriendStudID, string MyFacebookID)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                return (from friend in prz.Friends
                        where friend.StudentCode == FriendStudID && friend.FriendFacebookID == MyFacebookID
                        select friend.FriendStatus).SingleOrDefault().ToString() == "A" ? "true" : "false";
            }
        }


        public List<StudentSection> GetStudentSection(short year, short semester, string courseCode, string studentCode)
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
                                                              StartTimeNum = detail.Start,
                                                              EndTime = detail.EndTime,
                                                              EndTimeNum = detail.End,
                                                              InstructorCode = detail.InstructorCode,
                                                              Instructor = (from instructor in prz.Instructors
                                                                            where instructor.InstructorCode == detail.InstructorCode
                                                                            select instructor.FirstNameEN + instructor.LastNameEN).SingleOrDefault(),
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
            return sectionList;
        }

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

        public List<StudentSectionDetail> GetStudentSectionDetail(short year, short semester, string courseCode, short sectionNumber)
        {
            List<StudentSectionDetail> sectionDetailList = new List<StudentSectionDetail>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                sectionDetailList = (from course in prz.Courses
                                     join section in prz.Sections on course.CourseCode equals section.CourseCode
                                     join detail in prz.SectionDetails on new { section.CourseCode, section.SectionNumber }
                                         equals new { detail.CourseCode, detail.SectionNumber }
                                     where section.Year == year && section.Semester == semester &&
                                           section.CourseCode == courseCode && section.SectionNumber == sectionNumber
                                     select new StudentSectionDetail()
                         {
                             Year = section.Year,
                             Semester = section.Semester,
                             CourseCode = course.CourseCode,
                             CourseName = course.NameEN,
                             SectionNumber = section.SectionNumber,
                             DayNum = detail.Day,
                             StartTime = detail.StartTime,
                             StartTimeNum = detail.Start,
                             EndTime = detail.EndTime,
                             EndTimeNum = detail.End,
                             InstructorCode = detail.InstructorCode,
                             IsMorning = detail.IsMorning
                         }).ToList<StudentSectionDetail>();

            }
            return sectionDetailList;
        }

        public JsonResult GetCourseListDetailJSON(short year, short semester, string courseList, string studentCode = "")
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
                                                  FinalRoom = section.FinalRoom,
                                                  
                                              }).SingleOrDefault();

                        sec.FriendListInformation = (from friend in prz.Friends
                                                     join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                                     join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                                                     where friend.StudentCode == studentCode
                                                     && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
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
                                                         GPA = Double.Parse(student.CurrentGPA),
                                                         Credit = Convert.ToInt32(student.CreditComplete),
                                                         FacebookID = student.Facebook_ID
                                                     }).Distinct().ToList();


                        sec.SectionDetails = (from detail in prz.SectionDetails
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
                                                  Instructor = (from instructor in prz.Instructors
                                                                where instructor.InstructorCode == detail.InstructorCode
                                                                select instructor.FirstNameEN + instructor.LastNameEN).SingleOrDefault(),
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

        /*
         *  Function: GetCourseListDetail
         *  Description: To find the course information according to the course and section lists provided
         *               as the parameter. The number of the needed sections can be varied
         *  Input: Year (short), Semester (short), courseList (string)
         *  Output: The list of StudentCourse
         *  Remark: The format for the courseList --> Ex. ACT1600,401,402,403,404;BG1001,401,405,406;BG14036,401,402,403
         */

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
                                                  Instructor = (from instructor in prz.Instructors
                                                                where instructor.InstructorCode == detail.InstructorCode
                                                                select instructor.FirstNameEN + instructor.LastNameEN).SingleOrDefault(),
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

        /*
        public JsonResult GetStudentProfileJSON(string studentID)
        {
            StudentProfile student;
            using(ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                student = (from s in prz.Students
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

                student.FriendList = (from f in prz.Friends
                                     where f.StudentCode == studentID
                                     select f.FriendID).ToList();
            }

            return Json(student, JsonRequestBehavior.AllowGet);
        }
         * 
         * */


        /*
        public StudentProfile GetStudentProfile(string studentID)
        {
            StudentProfile student;
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                student = (from s in prz.Students
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

                student.FriendList = (from f in prz.Friends
                                      where f.StudentCode == studentID
                                      select f.FriendID).ToList();
            }
            return student;
        }
         * 
         * */


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

        public JsonResult GetStudentProfileJSON(String studentCode)
        {
            return Json(GetStudentProfile(studentCode), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCurrentEnrollmentRecordJSON(string studentID)
        {
            List<StudentCourse> enrollmentRecordList = new List<StudentCourse>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var enrollList = (from e in prz.Enrollments
                                  where e.StudentCode == studentID
                                  select new
                                  {
                                      Year = e.Year,
                                      Semester = e.Semester,
                                      CourseCode = e.CourseCode,
                                      SectionNumber = e.SectionNumber
                                  });

                foreach (var e in enrollList)
                {
                    StudentCourse c = new StudentCourse();
                    List<StudentSection> sectionList = new List<StudentSection>();

                    var q = (from course in prz.Courses
                             join section in prz.Sections on course.CourseCode equals section.CourseCode
                             where course.CourseCode == e.CourseCode && section.Year == e.Year
                             && section.Semester == e.Semester && section.SectionNumber == e.SectionNumber
                             select new
                             {
                                 Year = section.Year,
                                 Semester = section.Semester,
                                 CourseCode = course.CourseCode,
                                 CourseName = course.NameEN,
                                 Credit = course.Credit,
                                 CreditSum = course.CreditSum,
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

                             }).ToList();
                    c.CourseName = q[0].CourseName;
                    c.CourseCode = q[0].CourseCode;
                    c.Credit = q[0].Credit;
                    c.CreditSum = q[0].CreditSum;

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
                    c.Sections = sectionList;
                    enrollmentRecordList.Add(c);
                }

            }
            return Json(enrollmentRecordList, JsonRequestBehavior.AllowGet);
        }

        public CurrentEnrollmentRecordList GetCurrentEnrollmentRecord(string studentID)
        {
            List<StudentCourse> enrollmentRecordList = new List<StudentCourse>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var enrollList = (from e in prz.Enrollments
                                  where e.StudentCode == studentID
                                  select new
                                  {
                                      Year = e.Year,
                                      Semester = e.Semester,
                                      CourseCode = e.CourseCode,
                                      SectionNumber = e.SectionNumber
                                  });

                foreach (var e in enrollList)
                {
                    StudentCourse c = new StudentCourse();
                    List<StudentSection> sectionList = new List<StudentSection>();

                    var q = (from course in prz.Courses
                             join section in prz.Sections on course.CourseCode equals section.CourseCode
                             where course.CourseCode == e.CourseCode && section.Year == e.Year
                             && section.Semester == e.Semester && section.SectionNumber == e.SectionNumber
                             select new
                             {
                                 Year = section.Year,
                                 Semester = section.Semester,
                                 CourseCode = course.CourseCode,
                                 CourseName = course.NameEN,
                                 Credit = course.Credit,
                                 CreditSum = course.CreditSum,
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

                             }).ToList();
                    c.CourseName = q[0].CourseName;
                    c.CourseCode = q[0].CourseCode;
                    c.Credit = q[0].Credit;
                    c.CreditSum = q[0].CreditSum;

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
                    c.Sections = sectionList;
                    enrollmentRecordList.Add(c);
                }

            }
            CurrentEnrollmentRecordList currentEnrollmentList = new CurrentEnrollmentRecordList();
            currentEnrollmentList.EnrollmentCourses = enrollmentRecordList;
            return currentEnrollmentList;
        }

        public JsonResult GetAcademicRecordJSON(string studentCode)
        {

            List<AcademicYear> academics = new List<AcademicYear>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from grade in prz.Grades
                         where grade.StudentCode == studentCode
                         select new
                         {
                             year = grade.Year,
                             semester = grade.Semester
                         }).Distinct().ToList();
                foreach (var i in q)
                {
                    AcademicYear academicYear = new AcademicYear();
                    List<AcademicRecord> records = new List<AcademicRecord>();
                    academicYear.Year = i.year;
                    academicYear.Semester = i.semester;
                    records = (from grade in prz.Grades
                               join course in prz.Courses on grade.CourseCode equals course.CourseCode
                               where grade.Year == i.year && grade.Semester == i.semester &&
                                     grade.StudentCode == studentCode

                               select new AcademicRecord
                               {
                                   CourseCode = course.CourseCode,
                                   CourseName = course.NameEN,
                                   FinalGrade = grade.FinalGrade,
                                   Grade = ConvertGrade(grade.FinalGrade),
                                   Credit = course.Credit
                               }).ToList();

                    decimal allSumGrade = 0;
                    decimal allCredit = 0;
                    foreach (AcademicRecord record in records)
                    {
                        allSumGrade += Convert.ToDecimal((record.Grade * record.Credit));
                        allCredit += record.Credit;
                    }
                    if (allCredit > 0)
                    {
                        academicYear.TotalGrade = Math.Round(allSumGrade / allCredit, 2);
                    }
                    academicYear.Records = records;
                    academics.Add(academicYear);
                }
            }
            return Json(academics, JsonRequestBehavior.AllowGet);
        }

        public List<AcademicYear> GetAcademicRecord(string studentCode)
        {
            List<AcademicYear> academics = new List<AcademicYear>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from grade in prz.Grades
                         where grade.StudentCode == studentCode
                         select new
                         {
                             year = grade.Year,
                             semester = grade.Semester
                         }).Distinct().ToList();
                foreach (var i in q)
                {
                    AcademicYear academicYear = new AcademicYear();
                    List<AcademicRecord> records = new List<AcademicRecord>();
                    academicYear.Year = i.year;
                    academicYear.Semester = i.semester;
                    records = (from grade in prz.Grades
                               join course in prz.Courses on grade.CourseCode equals course.CourseCode
                               where grade.Year == i.year && grade.Semester == i.semester &&
                                     grade.StudentCode == studentCode

                               select new AcademicRecord
                               {
                                   CourseCode = course.CourseCode,
                                   CourseName = course.NameEN,
                                   FinalGrade = grade.FinalGrade,
                                   Grade = ConvertGrade(grade.FinalGrade),
                                   Credit = course.Credit
                               }).ToList();

                    decimal allSumGrade = 0;
                    decimal allCredit = 0;
                    foreach (AcademicRecord record in records)
                    {
                        allSumGrade += Convert.ToDecimal((record.Grade * record.Credit));
                        allCredit += record.Credit;
                    }
                    if (allCredit > 0)
                    {
                        academicYear.TotalGrade = Math.Round(allSumGrade / allCredit, 2);
                    }
                    academicYear.Records = records;
                    academics.Add(academicYear);
                }
            }
            return academics;
        }

        public double ConvertGrade(string grade)
        {
            if (grade == "A")
            {
                return 4.0;
            }
            else if (grade == "A-")
            {
                return 3.5;
            }
            else if (grade == "B+")
            {
                return 3.25;
            }
            else if (grade == "B")
            {
                return 3.0;
            }
            else if (grade == "B-")
            {
                return 2.75;
            }
            else if (grade == "C+")
            {
                return 2.50;
            }
            else if (grade == "C")
            {
                return 2.00;
            }
            else if (grade == "C-")
            {
                return 1.75;
            }
            else if (grade == "D")
            {
                return 1.00;
            }
            else
            {
                return 0.00;
            }
        }

        public string SaveFacebookID(string studentID, string facebookID)
        {
            string result;
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from student in prz.Students
                         where student.StudentCode == studentID
                         select student).SingleOrDefault();

                q.Facebook_ID = facebookID;

                try
                {
                    prz.SubmitChanges();
                    result = "Facebook ID Saved Completely!";
                }
                catch (Exception error)
                {
                    result = error.Message;
                }
                return result;
            }
        }


        /*
        public string SaveFriend(string studentID, string friendID)
        {
            string result;
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                prz.Friends.InsertOnSubmit(new Friend
                {
                    StudentCode = studentID,
                    FriendID = friendID
                });
                prz.SubmitChanges();
                result = "Saved Completely!";
            }
            return result;
        }

        public string SaveFacebookFriend(string studentID, List<String> friendIDs)
        {
            string result;
            using(ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                foreach(String i in friendIDs)
                {
                    prz.Friends.InsertOnSubmit(new Friend
                        {
                            StudentCode = studentID,
                            FriendID = i
                        });
                }
                try
                {
                    prz.SubmitChanges();
                    result = "Friend Accounts saved completely!";
                }
                catch (Exception error)
                {
                    result = error.Message;
                }
            }
            return result;
        }

        public JsonResult GetFacebookFriendListJSON(string studentCode)
        {
            List<String> friendList = new List<string>();
            using(ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                friendList = (from f in prz.Friends
                              where f.StudentCode == studentCode
                              select f.FriendID).ToList<String>();
                              
            }

            return Json(friendList, JsonRequestBehavior.AllowGet);
        }

        public List<String> GetFacebookFriendList(string studentCode)
        {
            List<String> friendList = new List<string>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                friendList = (from f in prz.Friends
                              where f.StudentCode == studentCode
                              select f.FriendID).ToList<String>();

            }
            return friendList;
        }
         * 
         * */

        /*
         *  Function: SavePlan2
         *  Description: To save plans along with given schedules to the database
         *  Input: studentID (string), planList (string), scheduleList (string)
         *  Output: A string message showing the result of saving
         *  Remark: planList format --> <CourseCode 1>; <CourseCode 2>; ...; <CourseCode n>
         *          scheduleList format --> <CourseCode 1>, <Section>; <CourseCode 2>, <Section>; ...; <CourseCode n>, <Section>
         */

        public string savePlan2(string studentID, string planList, string scheduleList)
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
                else
                {
                    resultMessage = "This plan has already existed!";
                }

                int scheduleCount = prz.SavedSchedules.Where(x => x.ScheduleList == scheduleList && x.StudentCode == studentID
                    && x.PlanList == planList).Count();

                if (scheduleCount == 0)
                {
                    prz.SavedSchedules.InsertOnSubmit(new SavedSchedule
                    {
                        StudentCode = studentID,
                        PlanList = planList,
                        ScheduleList = scheduleList
                    });

                    prz.SubmitChanges();
                    resultMessage += "Schedule(s) saved successfully";
                }
                else
                {
                    resultMessage += "This schedule has existed";
                }
                return resultMessage;
            }
        }

        public void ModifySavedSchedule(string studentID, string planList, string scheduleList, string newScheduleList)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                SavedSchedule q = (from schedule in prz.SavedSchedules
                         where schedule.StudentCode == studentID && schedule.PlanList == planList && schedule.ScheduleList == scheduleList
                         select schedule).SingleOrDefault();
                prz.SavedSchedules.DeleteOnSubmit(q);
                prz.SubmitChanges();
                savePlan2(studentID, planList, newScheduleList);
            }
        }

        public string SavePlan(string studentID, string planList, List<String> scheduleList)
        {
            string resultMessage = "";
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                try
                {
                    prz.SavedPlans.InsertOnSubmit(new SavedPlan
                    {
                        StudentCode = studentID,
                        PlanList = planList
                    });
                    prz.SubmitChanges();

                }
                catch (ConstraintException constraintError)
                {
                    resultMessage = "Plan already existed: ";
                }

                finally
                {
                    try
                    {
                        foreach (string s in scheduleList)
                        {
                            prz.SavedSchedules.InsertOnSubmit(new SavedSchedule
                                {
                                    StudentCode = studentID,
                                    PlanList = planList,
                                    ScheduleList = s
                                });
                        }
                        prz.SubmitChanges();
                        resultMessage += "Schedule(s) saved successfully";
                    }
                    catch (ConstraintException error)
                    {
                        resultMessage += "Some schedules already existed: " + error.Message;
                    }
                }

            }
            return resultMessage;
        }

        public string DeleteSchedule(string studentID, string planList, string scheduleString)
        {
            string result = "Some problems occurred while the program deletes the schedule";
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                SavedSchedule schedule = (from savedSchedule in prz.SavedSchedules
                                          where savedSchedule.StudentCode == studentID && savedSchedule.PlanList == planList &&
                                          savedSchedule.ScheduleList == scheduleString
                                          select savedSchedule).SingleOrDefault();
                prz.SavedSchedules.DeleteOnSubmit(schedule);
                prz.SubmitChanges();
                if (prz.SavedSchedules.Where(s => s.StudentCode == studentID && s.PlanList == planList) == null)
                {
                    SavedPlan plan = (from savedPlan in prz.SavedPlans
                                      where savedPlan.StudentCode == studentID && savedPlan.PlanList == planList
                                      select savedPlan).SingleOrDefault();
                    prz.SavedPlans.DeleteOnSubmit(plan);
                    prz.SubmitChanges();
                }
                result = "Schedule Deleted Successfully";
            }
            return result;
        }

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

        public List<StudentSavedPlan> GetSavedPlan(string studentID)
        {
            List<StudentSavedPlan> savedPlanList = new List<StudentSavedPlan>();

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                savedPlanList = (from p in prz.SavedPlans
                                 join student in prz.Students on p.StudentCode equals student.StudentCode
                                 where studentID == p.StudentCode
                                 select new StudentSavedPlan
                                 {
                                     StudentCode = p.StudentCode,
                                     StudentName = String.Format("{0} {1}", student.FirstNameEN, student.LastNameEN),
                                     StudentFaculty = student.FacultyNameEN,
                                     StudentDepartment = student.DepartmentNameEN,
                                     PlanList = p.PlanList
                                 }).ToList();

                for (int i = 0; i < savedPlanList.Count; ++i)
                {
                    savedPlanList[i].schedules = (from s in prz.SavedSchedules
                                                  where studentID == s.StudentCode &&
                                                  savedPlanList[i].PlanList == s.PlanList
                                                  select s).ToList();
                }

            }

            return savedPlanList;
        }

        public JsonResult GetSavedPlanJSON(string studentID)
        {
            List<StudentSavedPlan> savedPlanList = new List<StudentSavedPlan>();

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                savedPlanList = (from p in prz.SavedPlans
                                 join student in prz.Students on p.StudentCode equals student.StudentCode
                                 where studentID == p.StudentCode
                                 select new StudentSavedPlan
                                 {
                                     StudentCode = p.StudentCode,
                                     StudentName = String.Format("{0} {1}", student.FirstNameEN, student.LastNameEN),
                                     StudentFaculty = student.FacultyNameEN,
                                     StudentDepartment = student.DepartmentNameEN,
                                     PlanList = p.PlanList
                                 }).ToList();

                for (int i = 0; i < savedPlanList.Count; ++i)
                {
                    savedPlanList[i].schedules = (from s in prz.SavedSchedules
                                                  where studentID == s.StudentCode &&
                                                  savedPlanList[i].PlanList == s.PlanList
                                                  select s).ToList();

                }

            }

            return Json(savedPlanList, JsonRequestBehavior.AllowGet);
        }

        public List<StudentSavedPlan> GetAllFriendSavedPlan(string studentCode)
        {
            List<StudentSavedPlan> friendSavedPlan = new List<StudentSavedPlan>();
            List<string> friendCodeList;
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                friendCodeList = (from friend in prz.Friends
                                  join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                  where friend.StudentCode == studentCode
                                  select student.StudentCode).ToList();

                foreach (string code in friendCodeList)
                {
                    var q = (from plan in prz.SavedPlans
                             join schedule in prz.SavedSchedules on plan.PlanList equals schedule.PlanList
                             where plan.StudentCode == code && plan.StudentCode == schedule.StudentCode
                             select new
                             {
                                 StudentCode = plan.StudentCode,
                                 PlanList = plan.PlanList,
                                 ScheduleList = schedule.ScheduleList
                             });

                    List<SavedSchedule> savedSchedule = new List<SavedSchedule>();
                    if (q.Count() != 0)
                    {
                        foreach (var item in q)
                        {
                            SavedSchedule schedule = new SavedSchedule();
                            schedule.StudentCode = item.StudentCode;
                            schedule.PlanList = item.PlanList;
                            schedule.ScheduleList = item.ScheduleList;
                            savedSchedule.Add(schedule);
                        }

                        StudentSavedPlan p = new StudentSavedPlan();
                        p.StudentCode = savedSchedule[0].StudentCode;
                        p.PlanList = savedSchedule[0].PlanList;
                        p.schedules = savedSchedule;
                        friendSavedPlan.Add(p);
                    }

                }
            }
            return friendSavedPlan;
        }

        public JsonResult GetAllFriendSavedPlanJSON(string studentCode)
        {
            List<StudentSavedPlan> friendSavedPlan = new List<StudentSavedPlan>();
            List<string> friendCodeList;
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                friendCodeList = (from friend in prz.Friends
                                  join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                  where friend.StudentCode == studentCode
                                  select student.StudentCode).ToList();

                foreach (string code in friendCodeList)
                {
                    var q = (from plan in prz.SavedPlans
                             join schedule in prz.SavedSchedules on plan.PlanList equals schedule.PlanList
                             where plan.StudentCode == code && plan.StudentCode == schedule.StudentCode
                             select new
                             {
                                 StudentCode = plan.StudentCode,
                                 PlanList = plan.PlanList,
                                 ScheduleList = schedule.ScheduleList
                             });

                    List<SavedSchedule> savedSchedule = new List<SavedSchedule>();
                    if (q.Count() != 0)
                    {
                        foreach (var item in q)
                        {
                            SavedSchedule schedule = new SavedSchedule();
                            schedule.StudentCode = item.StudentCode;
                            schedule.PlanList = item.PlanList;
                            schedule.ScheduleList = item.ScheduleList;
                            savedSchedule.Add(schedule);
                        }

                        StudentSavedPlan p = new StudentSavedPlan();
                        p.StudentCode = savedSchedule[0].StudentCode;
                        p.PlanList = savedSchedule[0].PlanList;
                        p.schedules = savedSchedule;
                        friendSavedPlan.Add(p);
                    }

                }
            }
            return Json(friendSavedPlan, JsonRequestBehavior.AllowGet);
        }

        public void SetDefaultStudentConstraint()
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                List<String> studentList = (from s in prz.Students
                                            where s.StudentCode.Contains("5411")
                                            select s.StudentCode).ToList();

                foreach (string s in studentList)
                {
                    prz.StudentConstraints.InsertOnSubmit(new StudentConstraint
                        {
                            StudentCode = s,
                            ConstraintList = "SUNAM,1;SUNNOON,1;SUNPM,1;MONAM,1;MONNOON,1;MONPM,1;TUEAM,1;TUENOON,1;TUEPM,1;WEDAM,1;WEDNOON,1;WEDPM,1;THUAM,1;THUNOON,1;THUPM,1;FRIAM,1;FRINOON,1;FRIPM,1;SATAM,1;SATNOON,1;SATPM,1"
                        });
                }
                prz.SubmitChanges();
            }
        }

        public void SetStudentConstraint(string studentID, string constraintList)
        {
            //constraintList.ToUpper();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var query = (from c in prz.StudentConstraints
                             where c.StudentCode == studentID
                             select c).SingleOrDefault();
                if(query == null)
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
        }
        
        public static List<StudentConstraintData> GetStudentConstraint(string studentID)
        {
            List<StudentConstraintData> constraintList = new List<StudentConstraintData>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                string consList = (from c in prz.StudentConstraints
                                   where c.StudentCode == studentID
                                   select c.ConstraintList).SingleOrDefault();
                string[] constraintSet = consList.Split(';');
                foreach (string c in constraintSet)
                {
                    string[] constraint = c.Split(',');
                    StudentConstraintData sc = new StudentConstraintData();
                    sc.ConstraintName = constraint[0];
                    sc.ConstraintValue = constraint[1].Equals("1") ? true : false;
                    constraintList.Add(sc);
                }
            }
            return constraintList;
        }

        public JsonResult GetStudentConstraintJSON(string studentID)
        {
            List<StudentConstraintData> constraintList = new List<StudentConstraintData>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                string consList = (from c in prz.StudentConstraints
                                   where c.StudentCode == studentID
                                   select c.ConstraintList).SingleOrDefault();
                string[] constraintSet = consList.Split(';');
                foreach (string c in constraintSet)
                {
                    string[] constraint = c.Split(',');
                    StudentConstraintData sc = new StudentConstraintData();
                    sc.ConstraintName = constraint[0];
                    sc.ConstraintValue = constraint[1].Equals("1") ? true : false;
                    constraintList.Add(sc);
                }
            }
            return Json(constraintList, JsonRequestBehavior.AllowGet);
        }

        public CourseInstructorList GetInstructorFromCourse(short year, short semester, string courseCode)
        {
            CourseInstructorList instructorList = new CourseInstructorList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                instructorList.courseInstructorList = (from section in prz.Sections
                                                       join detail in prz.SectionDetails on section.SectionNumber equals detail.SectionNumber
                                                       join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                                                       where section.Year == year && section.Semester == semester && section.CourseCode == courseCode &&
                                                       detail.CourseCode == courseCode
                                                       select new CourseInstructor
                                                       {
                                                           instructorCode = instructor.InstructorCode,
                                                           instructorName = instructor.FirstNameEN + " " + instructor.LastNameEN
                                                       }).Distinct().ToList();
            }
            return instructorList;
        }

        public JsonResult GetInstructorFromCourseJSON(short year, short semester, string courseCode)
        {
            CourseInstructorList instructorList = new CourseInstructorList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                instructorList.courseInstructorList = (from section in prz.Sections
                                                       join detail in prz.SectionDetails on section.SectionNumber equals detail.SectionNumber
                                                       join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                                                       where section.Year == year && section.Semester == semester && section.CourseCode == courseCode &&
                                                       detail.CourseCode == courseCode
                                                       select new CourseInstructor
                                                       {
                                                           instructorCode = instructor.InstructorCode,
                                                           instructorName = instructor.FirstNameEN + " " + instructor.LastNameEN
                                                       }).Distinct().ToList();
            }
            return Json(instructorList, JsonRequestBehavior.AllowGet);
        }

        public List<StudentSection> SuggestSection(short year, short semester, string courseCode, short sectionNumber,
            string scheduleList, string studentCode = "")
        {
            List<StudentSection> suggestedSections = new List<StudentSection>();
            string courseList = String.Format("{0},{1}", courseCode, sectionNumber);
            if (scheduleList.Contains(courseList + ";"))
            {
                scheduleList = scheduleList.Replace(courseList + ";", "");
            }
            else if(scheduleList.Contains(";" + courseList))
            {
                scheduleList = scheduleList.Replace(";" + courseList, "");
            }
            StudentSection section = GetCourseListDetail(year, semester, courseList).SingleOrDefault().Sections.SingleOrDefault();
            List<string> instructorCodeList = (from d in section.SectionDetails
                                               where d.CourseCode == courseCode
                                               select d.InstructorCode).Distinct().ToList();

            List<StudentSection> allSections = GetStudentSection(year, semester, courseCode, studentCode);
            allSections.RemoveAll(x => x.SectionNumber == sectionNumber);
            List<StudentSection> allCheckedSections = new List<StudentSection>();
            allCheckedSections.AddRange(allSections);

            List<StudentCourse> scheduleCourses = GetCourseListDetail(year, semester, scheduleList);
            List<StudentSection> scheduleSections = new List<StudentSection>();

            foreach (StudentCourse course in scheduleCourses)
            {
                scheduleSections.Add(course.Sections.SingleOrDefault());
            }

            // Find the similar sections having the same instructor
            foreach (string instructor in instructorCodeList)
            {
                foreach (StudentSection s in allSections)
                {
                    bool isMatched = false;
                    foreach (StudentSectionDetail detail in s.SectionDetails)
                    {
                        if (detail.InstructorCode == instructor)
                        {
                            isMatched = true;
                            break;
                        }
                         
                    }
                    if (isMatched)
                    {
                        if (!IsSectionConflict(s, scheduleSections))
                        {
                            suggestedSections.Add(s);
                            allCheckedSections.RemoveAll(sec => sec.SectionNumber == s.SectionNumber);
                        }

                    }
                }
            }

            allSections.Clear();
            allSections.AddRange(allCheckedSections);

            // If there is no section that has the same instructor, the system tries to find the section that have
            // the same study day and time
            if (suggestedSections.Count() == 0)
            {
                foreach (StudentSection s in allSections)
                {
                    bool isSame = false;
                    for (int i = 0; s.SectionDetails.Count == section.SectionDetails.Count && i < s.SectionDetails.Count; ++i)
                    {
                        isSame = s.SectionDetails[i].DayNum == section.SectionDetails[i].DayNum &&
                                 s.SectionDetails[i].StartTime == section.SectionDetails[i].StartTime &&
                                 s.SectionDetails[i].EndTime == section.SectionDetails[i].EndTime ? true : false;
                    }
                    if (isSame)
                    {
                        suggestedSections.Add(s);
                        allCheckedSections.RemoveAll(sec => sec.SectionNumber == s.SectionNumber);
                    }
                }
            }


            allSections.Clear();
            allSections.AddRange(allCheckedSections);

            // If the system cannot still find a possible section, the system finds the section that does not have 
            // study day and time conflict amongst the other sections in the schedule
            if (suggestedSections.Count == 0)
            {


                foreach (StudentSection s in allSections)
                {
                    bool isConflict = IsSectionConflict(s, scheduleSections);

                    if (!isConflict)
                    {
                        suggestedSections.Add(s);
                        //allSections.RemoveAll(sec => sec.SectionNumber == s.SectionNumber);
                    }
                }
            }

            return suggestedSections;
        }

        public JsonResult SuggestSectionJSON(short year, short semester, string courseCode, short sectionNumber,
             string scheduleList, string studentCode = "")
        {
            return Json(SuggestSection(year, semester, courseCode, sectionNumber, scheduleList, studentCode), JsonRequestBehavior.AllowGet);
        }

        public string RegisterSchedule(short year, short semester, string godSchedule, string studentCode)
        {
            string result = "";
            string[] courseList = godSchedule.Split(';');
            List<string> noSeatSection = new List<string>();
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
                        noSeatSection.Add(subject[0]);
                    }
                }

                if (noSeatSection.Count == 0)
                {
                    result = "Schedule Already Registered";
                }
                else
                {
                    result = "No Seat Available Course(s) \n";
                    foreach (string course in noSeatSection)
                    {
                        result += course + "\n";
                    }
                }
                return result;
            }
        }

        public static bool IsSectionConflict(StudentSection checkSection, List<StudentSection> scheduleSections)
        {
            bool isConflict = false;

            for (int i = 0; i < checkSection.SectionDetails.Count; ++i)
            {
                foreach (StudentSection sections in scheduleSections)
                {
                    foreach (StudentSectionDetail detail in sections.SectionDetails)
                    {
                        if (checkSection.SectionDetails[i].DayNum == detail.DayNum)
                        {
                            if (checkSection.SectionDetails[i].StartTimeNum == detail.StartTimeNum ||
                               checkSection.SectionDetails[i].EndTimeNum == detail.EndTimeNum ||
                                (checkSection.SectionDetails[i].StartTimeNum > detail.StartTimeNum
                                && checkSection.SectionDetails[i].StartTimeNum < detail.EndTimeNum) ||
                                (checkSection.SectionDetails[i].StartTimeNum < detail.StartTimeNum && 
                                checkSection.SectionDetails[i].EndTimeNum > detail.StartTimeNum))
                            {
                                isConflict = true;
                                break;
                            }
                        }
                    }

                    if (isConflict)
                    {
                        break;
                    }

                }
                if (isConflict)
                {
                    break;
                }
            }


            return isConflict;
        }


        public JsonResult TestJSONFunction()
        {
            JsonResult result = GetCurrentEnrollmentRecordJSON("5510138");
            String resultString = new JavaScriptSerializer().Serialize(result);
            var resultJSON = new JavaScriptSerializer().Deserialize<StudentCourse>(resultString);
            return Json(resultJSON, JsonRequestBehavior.AllowGet);
        }

    }
}
