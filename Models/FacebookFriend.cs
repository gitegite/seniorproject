using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class FacebookFriend
    {
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string StudentFaculty { get; set; }
        public string StudentMajor { get; set; }
        public char FriendStatus { get; set; }
        public string FacebookID { get; set; }
        public bool IsChecked { get; set; }


    }
}