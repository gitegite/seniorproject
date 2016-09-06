using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class ClassLookup
    {
        public int Year { get; set; }
        public int Semester { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credit { get; set; }
        public int CreditSum { get; set; }
        public short SectionNumber { get; set; }
        public bool IsClosed { get; set; }
        public int SeatAvailable { get; set; }
        public int SeatLimited { get; set; }
        public int SeatUsed { get; set; }
        public string Remark { get; set; }
        public string MidtermDate { get; set; }
        public string MidtermStartTime { get; set; }
        public string MidtermEndTime { get; set; }
        public string MidtermDateTime { get; set; }
        public string MidtermRoom { get; set; }
        public string FinalDate { get; set; }
        public string FinalStartTime { get; set; }
        public string FinalEndTime { get; set; }
        public string FinalRoom { get; set; }
        public int DayNum { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Room { get; set; }
        public string InstructorCode { get; set; }
        public bool IsMorning { get; set; }
    }
}