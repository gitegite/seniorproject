using SeniorProject.Models;
using SeniorProject.ClassLookUpListModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class ClassLookupController : Controller
    {
        // GET: ClassLookup
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpGet]
        public PartialViewResult GetClassLookup(short year, short semester, string courseCode)
        {
            StudentCourse course = new StudentCourse();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {

                var q1 = (from c in prz.Courses
                          join sec in prz.Sections on c.CourseCode equals sec.CourseCode
                          where sec.Year == year && sec.Semester == semester && sec.CourseCode == courseCode
                          select new
                          {
                              Year = year,
                              Semester = semester,
                              CourseCode = courseCode,
                              CourseName = c.NameEN,
                              Credit = c.Credit,
                              CreditSum = c.CreditSum,
                              SectionNumber = sec.SectionNumber,
                              IsClosed = sec.IsClosed,
                              SeatAvailable = sec.SeatAvailable,
                              SeatUsed = sec.SeatUsed,
                              SeatLimited = sec.SeatLimit,
                              Remark = sec.Remark,
                              MidtermDate = sec.MidtermDate,
                              MidtermStartTime = sec.MidtermStartTime,
                              MidtermEndTime = sec.MidtermEndTime,
                              MidtermRoom = sec.MidtermRoom,
                              FinalDate = sec.FinalDate,
                              FinalStartTime = sec.FinalStartTime,
                              FinalEndTime = sec.FinalEndTime,
                              FinalRoom = sec.FinalRoom,
                          }).Distinct().ToList();

                course.CourseCode = q1[0].CourseCode;
                course.CourseName = q1[0].CourseName;
                course.Credit = q1[0].Credit;
                course.CreditSum = q1[0].CreditSum;
                List<StudentSection> sectionList = new List<StudentSection>();
                foreach (var sec in q1)
                {
                    StudentSection section = new StudentSection();
                    section.CourseCode = sec.CourseCode;
                    section.CourseName = sec.CourseName;
                    section.SectionNumber = sec.SectionNumber;
                    section.IsClosed = sec.IsClosed;
                    section.SeatAvailable = sec.SeatAvailable;
                    section.SeatUsed = sec.SeatUsed;
                    section.SeatLimited = sec.SeatLimited;
                    section.Remark = sec.Remark;
                    section.MidtermDate = sec.MidtermDate;
                    section.MidtermStartTime = sec.MidtermStartTime;
                    section.MidtermEndTime = sec.MidtermEndTime;
                    section.MidtermRoom = sec.MidtermRoom;
                    section.FinalDate = sec.FinalDate;
                    section.FinalStartTime = sec.FinalStartTime;
                    section.FinalEndTime = sec.FinalEndTime;
                    section.FinalRoom = sec.FinalRoom;

                    var q2 = (from detail in prz.SectionDetails
                              join instructor in prz.Instructors on detail.InstructorCode equals instructor.InstructorCode
                              where detail.CourseCode == section.CourseCode && detail.SectionNumber == section.SectionNumber
                                  && detail.Year == year && detail.Semester == semester
                              select new
                              {
                                  DayNum = detail.Day,
                                  StudyStart = detail.StartTime,
                                  StudyEnd = detail.EndTime,
                                  InstructorCode = detail.InstructorCode,
                                  Room = detail.RoomCode,
                                  IsMorning = detail.IsMorning,
                                  Instructor = instructor.FirstNameEN + " " + instructor.LastNameEN
                              }).ToList();

                    List<StudentSectionDetail> detailList = new List<StudentSectionDetail>();
                    foreach (var d in q2)
                    {
                        StudentSectionDetail detail = new StudentSectionDetail();
                        detail.Year = year;
                        detail.Semester = semester;
                        detail.CourseCode = courseCode;
                        detail.SectionNumber = section.SectionNumber;
                        detail.DayNum = d.DayNum;
                        detail.StartTime = d.StudyStart;
                        detail.EndTime = d.StudyEnd;
                        detail.Instructor = d.Instructor;
                        detail.Room = d.Room;
                        detail.IsMorning = d.IsMorning;
                        detailList.Add(detail);
                    }
                    section.SectionDetails = detailList;
                    sectionList.Add(section);
                }
                course.Sections = sectionList;

            }
            return PartialView("_ResultView", course);
        }

        public JsonResult GetClassLookupResult(short year, short semester, string courseCode)
        {
            StudentCourse course = new StudentCourse();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                
                var q1 = (from c in prz.Courses
                          join sec in prz.Sections on c.CourseCode equals sec.CourseCode
                          where sec.Year == year && sec.Semester == semester && sec.CourseCode == courseCode
                          select new
                          {
                              Year = year,
                              Semester = semester,
                              CourseCode = courseCode,
                              CourseName = c.NameEN,
                              Credit = c.Credit,
                              CreditSum = c.CreditSum,
                              SectionNumber = sec.SectionNumber,
                              IsClosed = sec.IsClosed,
                              SeatAvailable = sec.SeatAvailable,
                              SeatUsed = sec.SeatUsed,
                              SeatLimited = sec.SeatLimit,
                              Remark = sec.Remark,
                              MidtermDate = sec.MidtermDate,
                              MidtermStartTime = sec.MidtermStartTime,
                              MidtermEndTime = sec.MidtermEndTime,
                              MidtermRoom = sec.MidtermRoom,
                              FinalDate = sec.FinalDate,
                              FinalStartTime = sec.FinalStartTime,
                              FinalEndTime = sec.FinalEndTime,
                              FinalRoom = sec.FinalRoom,
                          }).Distinct().ToList();

                course.CourseCode = q1[0].CourseCode;
                course.CourseName = q1[0].CourseName;
                course.Credit = q1[0].Credit;
                course.CreditSum = q1[0].CreditSum;
                List<StudentSection> sectionList = new List<StudentSection>();
                foreach (var sec in q1)
                {
                    StudentSection section = new StudentSection();
                    section.CourseCode = sec.CourseCode;
                    section.SectionNumber = sec.SectionNumber;
                    section.IsClosed = sec.IsClosed;
                    section.SeatAvailable = sec.SeatAvailable;
                    section.SeatUsed = sec.SeatUsed;
                    section.SeatLimited = sec.SeatLimited;
                    section.Remark = sec.Remark;
                    section.MidtermDate = sec.MidtermDate;
                    section.MidtermStartTime = sec.MidtermStartTime;
                    section.MidtermEndTime = sec.MidtermEndTime;
                    section.MidtermRoom = sec.MidtermRoom;
                    section.FinalDate = sec.FinalDate;
                    section.FinalStartTime = sec.FinalStartTime;
                    section.FinalEndTime = sec.FinalEndTime;
                    section.FinalRoom = sec.FinalRoom;

                    var q2 = (from detail in prz.SectionDetails
                              where detail.CourseCode == section.CourseCode && detail.SectionNumber == section.SectionNumber
                                  && detail.Year == year && detail.Semester == semester
                              select new
                              {
                                  DayNum = detail.Day,
                                  StudyStart = detail.StartTime,
                                  StudyEnd = detail.EndTime,
                                  InstructorCode = detail.InstructorCode,
                                  Room = detail.RoomCode,
                                  IsMorning = detail.IsMorning
                              }).ToList();

                    List<StudentSectionDetail> detailList = new List<StudentSectionDetail>();
                    foreach(var d in q2)
                    {
                        StudentSectionDetail detail = new StudentSectionDetail();
                        detail.Year = year;
                        detail.Semester = semester;
                        detail.CourseCode = courseCode;
                        detail.SectionNumber = section.SectionNumber;
                        detail.DayNum = d.DayNum;
                        detail.StartTime = d.StudyStart;
                        detail.EndTime = d.StudyEnd;
                        detail.InstructorCode = d.InstructorCode;
                        detail.Room = d.Room;
                        detail.IsMorning = d.IsMorning;
                        detailList.Add(detail);
                    }
                    section.SectionDetails = detailList;
                    sectionList.Add(section);
                }
                course.Sections = sectionList;

            }

            return Json(course, JsonRequestBehavior.AllowGet);
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

    }
}