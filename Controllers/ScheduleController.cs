using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;

namespace SeniorProject.Controllers
{
    public class ScheduleController : Controller
    {
        // GET: Schedule
        public ActionResult Index()
        {
            string StudentCode = Session["StudentId"].ToString();
            CurrentEnrollmentRecordList record = new CurrentEnrollmentRecordList();
            record = GetCurrentEnrollmentRecord(StudentCode);
            return View("Index",record);
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
                                                              join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                                                              where sec.CourseCode == detail.CourseCode && sec.SectionNumber == detail.SectionNumber
                                                              && sec.Year == detail.Year && sec.Semester == detail.Semester
                                                              select new StudentSectionDetail()
                                                              {
                                                                  DayNum = detail.Day,
                                                                  StartTime = detail.StartTime,
                                                                  EndTime = detail.EndTime,
                                                                  StartTimeNum = detail.Start,
                                                                  InstructorCode = detail.InstructorCode,
                                                                  Instructor = instructor.FirstNameEN + " " + instructor.LastNameEN,
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

    }
}