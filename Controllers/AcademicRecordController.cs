using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class AcademicRecordController : Controller
    {
        //// GET: AcademicRecord
        //public ActionResult Index()
        //{
        //    return View();
        //}

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
                         }).Distinct().ToList().OrderByDescending(x => x.year).ThenByDescending(y => y.semester);
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

        public ViewResult GetAcademicRecord()
        {
            string studentCode = Session["StudentId"].ToString();
            List<AcademicYear> academics = new List<AcademicYear>();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from grade in prz.Grades
                         where grade.StudentCode == studentCode
                         select new
                         {
                             year = grade.Year,
                             semester = grade.Semester
                         }).Distinct().ToList().OrderByDescending(x => x.year).ThenByDescending(y => y.semester); ;
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
            StudentAcademicRecord academicRecords = new StudentAcademicRecord();
            academicRecords.AcademicRecords = academics;
            return View("Index", academicRecords);
        }
    }



}