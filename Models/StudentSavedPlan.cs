using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class StudentSavedPlan
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string StudentFaculty { get; set; }
        public string StudentDepartment { get; set; }

        public string StudentFacebookID { get; set; }
        public string PlanList { get; set; }
        public List<string> CourseNameList { get; set; }
        public List<SavedSchedule> schedules { get; set; }

        public StudentSavedPlan()
        {
            schedules = new List<SavedSchedule>();
            CourseNameList = new List<string>();
        }
    }
}