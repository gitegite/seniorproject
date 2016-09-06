using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class FriendList
    {
        public List<Student> Friends { get; set; }
        public string Status { get; set; }

        public FriendList()
        {
            Friends = new List<Student>();
            Status = "";
        }
    }
}