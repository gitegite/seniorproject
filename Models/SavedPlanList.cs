using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class SavedPlanList
    {
        public List<StudentSavedPlan> SavedPlans { get; set; }

        public SavedPlanList()
        {
            SavedPlans = new List<StudentSavedPlan>();
        }
    }
}