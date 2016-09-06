using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;

namespace SeniorProject.Controllers
{
    public class RecommendController : Controller
    {
        short year = 2015;
        short sem = 1;
        string god = "";
        List<String> notavailableSubject = new List<String>();

        public ActionResult Index()
        {
            return View("Index");
        }

        // GET: Recommend
        public JsonResult NotAvailableCourse(string godMode)
        {
            Session["god-recommend"] = godMode;
            Session["god-update"] = godMode;
            List<StudentCourse> PreRegisSchedule = GetCourseListDetail(year, sem, godMode);
            int colorIndex = 0;
            notavailableSubject.Clear();

            foreach (StudentCourse subject in PreRegisSchedule)
            {
                if (subject.Sections[0].SeatAvailable == 0)
                {
                    notavailableSubject.Add(subject.CourseCode + "," + subject.CourseName + "," + subject.Sections[0].SectionNumber + "," + colorIndex);
                    ++colorIndex;
                }
            }
            return Json(notavailableSubject, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AvailableSection(string CourseCode, string Section)
        {
            List<StudentSection> suggestion = new List<StudentSection>();
            suggestion = SuggestSection(2015, 1, CourseCode, short.Parse(Section), Session["god-recommend"].ToString());
            return Json(suggestion, JsonRequestBehavior.AllowGet);
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
                                                                select instructor.FirstNameEN + " " + instructor.LastNameEN).SingleOrDefault(),
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
            return Json(GetCourseListDetail(year, semester, courseList), JsonRequestBehavior.AllowGet);
        }

        //public List<StudentSectionDetail> GetStudentSectionDetail(short year, short semester, string courseCode, short sectionNumber)
        //{
        //    List<StudentSectionDetail> sectionDetailList = new List<StudentSectionDetail>();
        //    using (ParazoDBDataContext prz = new ParazoDBDataContext())
        //    {
        //        sectionDetailList = (from course in prz.Courses
        //                             join section in prz.Sections on course.CourseCode equals section.CourseCode
        //                             join detail in prz.SectionDetails on new { section.CourseCode, section.SectionNumber }
        //                                 equals new { detail.CourseCode, detail.SectionNumber }
        //                             where section.Year == year && section.Semester == semester &&
        //                                   section.CourseCode == courseCode && section.SectionNumber == sectionNumber
        //                             select new StudentSectionDetail()
        //                             {
        //                                 Year = section.Year,
        //                                 Semester = section.Semester,
        //                                 CourseCode = course.CourseCode,
        //                                 CourseName = course.NameEN,
        //                                 SectionNumber = section.SectionNumber,
        //                                 DayNum = detail.Day,
        //                                 StartTime = detail.StartTime,
        //                                 EndTime = detail.EndTime,
        //                                 InstructorCode = detail.InstructorCode,
        //                                 IsMorning = detail.IsMorning
        //                             }).ToList<StudentSectionDetail>();

        //    }
        //    return sectionDetailList;
        //}

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
                                                              EndTime = detail.EndTime,
                                                              InstructorCode = detail.InstructorCode,
                                                              Instructor = (from instructor in prz.Instructors
                                                                            where detail.InstructorCode == instructor.InstructorCode
                                                                            select instructor.FirstNameEN + " " +
                                                                            instructor.LastNameEN.Substring(0, 1) + ".").SingleOrDefault(),
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
                    sectionList.Add(section);
                }
            }
            return sectionList;
        }

        public JsonResult GetStudentSectionJSON(short year, short semester, string courseCode, string studentCode)
        {
            return Json(GetStudentSection(year, semester, courseCode, studentCode), JsonRequestBehavior.AllowGet);
        }

        public List<StudentSection> SuggestSection(short year, short semester, string courseCode, short sectionNumber,
    string scheduleList, string studentCode = "")
        {
            List<StudentSection> suggestedSections = new List<StudentSection>();
            string courseList = String.Format("{0},{1}", courseCode, sectionNumber);
            if (scheduleList.Contains(courseList + ";"))
            {
                scheduleList = scheduleList.Replace(courseList + ";", ""); // Replace in case not the last subject
            }
            else if (scheduleList.Contains(";" + courseList))
            {
                scheduleList = scheduleList.Replace(";" + courseList, ""); // Replace in case of the last subject
            }
            StudentSection section = GetCourseListDetail(year, semester, courseList).SingleOrDefault().Sections.SingleOrDefault(); // Get Detail of new selected Secton
            List<string> instructorCodeList = (from d in section.SectionDetails
                                               where d.CourseCode == courseCode
                                               select d.InstructorCode).Distinct().ToList();

            List<StudentSection> allSections = GetStudentSection(year, semester, courseCode, studentCode); // Get all section of selected subject
            allSections.RemoveAll(x => x.SectionNumber == sectionNumber); // Remove selected number out of subject
            List<StudentSection> allCheckedSections = new List<StudentSection>();
            allCheckedSections.AddRange(allSections);

            List<StudentCourse> scheduleCourses = GetCourseListDetail(year, semester, scheduleList); // Get Detail of remaining courses in schedule
            List<StudentSection> scheduleSections = new List<StudentSection>();

            foreach (StudentCourse course in scheduleCourses)
            {
                scheduleSections.Add(course.Sections.SingleOrDefault()); // Convert from student course to student section
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

            foreach (StudentSection s in allSections)
            {
                bool isSame = false;
                for (int i = 0; s.SectionDetails.Count == section.SectionDetails.Count && i < s.SectionDetails.Count; ++i)
                {
                    isSame = s.SectionDetails[i].DayNum == section.SectionDetails[i].DayNum &&
                             s.SectionDetails[i].StartTime == section.SectionDetails[i].StartTime &&
                             s.SectionDetails[i].EndTime == section.SectionDetails[i].EndTime ? true : false;
                }
                if (isSame && !IsSectionConflict(s, scheduleSections))
                {
                    suggestedSections.Add(s);
                    allCheckedSections.RemoveAll(sec => sec.SectionNumber == s.SectionNumber);
                }
            }


            allSections.Clear();
            allSections.AddRange(allCheckedSections);

            // If the system cannot still find a possible section, the system finds the section that does not have 
            // study day and time conflict amongst the other sections in the schedule

            foreach (StudentSection s in allSections)
            {
                bool isConflict = IsSectionConflict(s, scheduleSections);

                if (!isConflict)
                {
                    suggestedSections.Add(s);
                }
            }


            suggestedSections.RemoveAll(x => x.SeatAvailable == 0);
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


        public static bool IsSectionConflict(StudentSection checkSection, List<StudentSection> scheduleSections)
        {
            bool isConflict = false;

            for (int i = 0; i < checkSection.SectionDetails.Count; ++i)
            {
                foreach (StudentSection sections in scheduleSections) // run through all available section
                {
                    foreach (StudentSectionDetail detail in sections.SectionDetails) // run through all section detail of each section
                    {
                        if (checkSection.SectionDetails[i].DayNum == detail.DayNum)
                        {
                            var scheduleSecST = detail.StartTimeNum;
                            var scheduleSecET = detail.EndTimeNum;
                            var checkST = checkSection.SectionDetails[i].StartTimeNum;
                            var checkET = checkSection.SectionDetails[i].EndTimeNum;

                            if (checkST == scheduleSecST || //Same starttime
                                checkET == scheduleSecET || //Same Endtime
                                checkST > scheduleSecST && checkST < scheduleSecET ||// Have starttime inside existing subject 
                                checkET > scheduleSecST && checkET < scheduleSecET ||// Have endtime inside existing subject 
                                checkST < scheduleSecST && checkET < scheduleSecET ||// Inside existing subject
                                checkST > scheduleSecST && checkET > scheduleSecET) // Overlap existing subject
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

        //public JsonResult GetStudentSectionJSON(short year, short semester, string courseCode, string studentCode)
        //{
        //    List<object> sectionList = new List<object>();
        //    using (ParazoDBDataContext prz = new ParazoDBDataContext())
        //    {

        //        var q = (from course in prz.Courses
        //                 join section in prz.Sections on course.CourseCode equals section.CourseCode
        //                 where course.CourseCode == courseCode && section.Year == year
        //                 && section.Semester == semester
        //                 select new
        //                 {
        //                     Year = section.Year,
        //                     Semester = section.Semester,
        //                     CourseCode = course.CourseCode,
        //                     CourseName = course.NameEN,
        //                     SectionNumber = section.SectionNumber,
        //                     IsClosed = section.IsClosed,
        //                     SeatAvailable = section.SeatAvailable,
        //                     SeatLimit = section.SeatLimit,
        //                     SeatUsed = section.SeatUsed,
        //                     Remark = section.Remark,
        //                     MidtermDate = section.MidtermDate,
        //                     MidtermStartTime = section.MidtermStartTime,
        //                     MidtermEndTime = section.MidtermEndTime,
        //                     MidtermRoom = section.MidtermRoom,
        //                     FinalDate = section.FinalDate,
        //                     FinalStartTime = section.FinalStartTime,
        //                     FinalEndTime = section.FinalEndTime,
        //                     FinalRoom = section.FinalRoom

        //                 });

        //        foreach (var sec in q)
        //        {

        //            var query = (from detail in prz.SectionDetails
        //                         where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
        //                         && sec.Year == detail.Year && sec.Semester == detail.Semester
        //                         select new
        //                         {
        //                             Day = detail.Day,
        //                             StartTime = detail.StartTime,
        //                             StartTimeNum = detail.Start,
        //                             EndTime = detail.EndTime,
        //                             EndTimeNum = detail.End,
        //                             InstructorCode = detail.InstructorCode,
        //                             Room = detail.RoomCode,
        //                             IsMorning = detail.IsMorning
        //                         }).ToList();

        //            int amount = (from schedule in prz.SavedSchedules
        //                          where schedule.ScheduleList.Contains(sec.CourseCode + "," +
        //                          sec.SectionNumber)
        //                          select schedule.StudentCode).Distinct().Count();

        //            List<string> friendSavedList = (from friend in prz.Friends
        //                                            join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
        //                                            join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
        //                                            where friend.StudentCode == studentCode
        //                                            && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
        //                                            select student.StudentCode).Distinct().ToList();


        //            object section = new
        //            {
        //                Year = sec.Year,
        //                Semester = sec.Semester,
        //                CourseCode = sec.CourseCode,
        //                CourseName = sec.CourseName,
        //                SectionNumber = sec.SectionNumber,
        //                IsClosed = sec.IsClosed,
        //                SeatAvailable = sec.SeatAvailable,
        //                SeatLimit = sec.SeatLimit,
        //                SeatUsed = sec.SeatUsed,
        //                MidtermDate = sec.MidtermDate,
        //                MidtermStartTime = sec.MidtermStartTime,
        //                MidtermEndTime = sec.MidtermEndTime,
        //                MidtermRoom = sec.MidtermRoom,
        //                FinalDate = sec.FinalDate,
        //                FinalStartTime = sec.FinalStartTime,
        //                FinalEndTime = sec.FinalEndTime,
        //                FinalRoom = sec.FinalRoom,
        //                Remark = sec.Remark,
        //                AllSavedAmount = amount,
        //                FriendSavedList = friendSavedList,
        //                SectionDetailList = query
        //            };

        //            sectionList.Add(section);
        //        }
        //    }
        //    return Json(sectionList, JsonRequestBehavior.AllowGet);
        //}

    }
}