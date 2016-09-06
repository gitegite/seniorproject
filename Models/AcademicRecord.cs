using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class AcademicRecord
    {
        public short Year { get; set; }
        public short Semester { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string FinalGrade { get; set; }
        public double Grade { get; set; }
        public int Credit { get; set; }
        public string SurveyStatus { get; set; }
    }
}