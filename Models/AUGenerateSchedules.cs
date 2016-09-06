using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class AUGenerateSchedules
    {
        public List<String> generatedScheduleStrList { get; set; }
        public string result { get; set; }
        public string message { get; set; }
        public List<String> friendsameScheduleList { get; set; }
    }
}