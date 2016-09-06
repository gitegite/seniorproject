using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class CurrentEnrollmentRecordList
    {
        public List<StudentCourse> EnrollmentCourses { get; set; }

        public CurrentEnrollmentRecordList()
        {
            EnrollmentCourses = new List<StudentCourse>();
        }
    }
}