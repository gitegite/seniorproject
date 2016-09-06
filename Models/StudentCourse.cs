using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class StudentCourse
    {
        public StudentCourse()
        {
            Sections = new List<StudentSection>();
        }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public int Credit { get; set; }
        public int CreditSum { get; set; }
        public List<StudentSection> Sections { get; set; }

        public string remark { get; set; }
    }
}