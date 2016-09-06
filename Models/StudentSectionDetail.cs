using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeniorProject.Models
{
    public class StudentSectionDetail
    {
        public int Year { get; set; }
        public int Semester { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public short SectionNumber { get; set; }
        public int DayNum { get; set; }
        public string DayName { get; set; }
        public string StartTime { get; set; }
        public short StartTimeNum { get; set; }
        public string EndTime { get; set; }
        public short EndTimeNum { get; set; }
        public string Room { get; set; }
        public string InstructorCode { get; set; }
        public string Instructor { get; set; }
        public bool IsMorning { get; set; }
    }
}
