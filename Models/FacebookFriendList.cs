using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class FacebookFriendList
    {
        public List<FacebookFriend> friendList { get; set; }

        public FacebookFriendList()
        {
            friendList = new List<FacebookFriend>();
        }
    }
}