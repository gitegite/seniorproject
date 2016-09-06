using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class StudentProfile
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Nation { get; set; }
        public string ProgramName { get; set; }
        public string BirthDate { get; set; }
        public string Level { get; set; }
        public string Faculty { get; set; }
        public string Major { get; set; }
        public string Gender { get; set; }
        public string StudyYear { get; set; }
        public double GPA { get; set; }
        public int Credit { get; set; }
        public string FacebookID { get; set; }
        public List<String> FriendList { get; set; }
        public List<StudentSavedPlan> AllSavedPlans { get; set; }

    }
}