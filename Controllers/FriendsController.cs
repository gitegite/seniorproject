using SeniorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SeniorProject.Controllers
{
    public class FriendsController : Controller
    {
        // GET: Friends
        public ActionResult Index()
        {
            return View("TestFriendView");
        }

        /*
         * Function Name : SaveFacebookID
         * Responsibility : Save FacebookID of current user
         * Input : user StudentID & user FacebookID
         * Step :   1) Find current user Info in Student Table (as ST)
         *          2) Add facebookID to facebookID field of ST
         *          3) Submit Change
         */
        public void SaveFacebookID(string studentID, string facebookID)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var q = (from student in prz.Students
                         where student.StudentCode == studentID
                         select student).SingleOrDefault();

                q.Facebook_ID = facebookID;

                try
                {
                    prz.SubmitChanges();
                }
                catch (Exception error)
                {
                }
            }
        }

        /*
         * Function Name : SaveFacebookFriend2
         * Responsibility : Update & Delete friend based on FB of current user
         * Input : user StudentID & FacebookFriendList (Format: facebookID,facebookID,facebookID,...,facebookID )
         * Step :   1) get existing Friends data ( FL )
         *          2) Delete a friend who doesn't friend or block on FB
         *          3) Add a new friend who doesn't exist in current friends to database
         *          4) Submit Changes
         * Requirement : user must log on with their facebook on using computer
         */
        public void SaveFacebookFriend2(string studentID, string friendList)
        {
            string[] friends = friendList.Split(',');
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var FL = (from fr in prz.Friends
                          where fr.StudentCode == studentID
                          select fr).ToList();

                //Delete friend who wasn't friend / get Blocked on FB
                foreach (var f in FL)
                {
                    if (!friends.Contains(f.FriendFacebookID))
                    {
                        prz.Friends.DeleteOnSubmit(f);
                    }
                }

                //Update friend based on FB
                foreach (string f in friends)
                {
                    int friendCount = FL.Where(x => x.FriendFacebookID == f).Count();
                    if (friendCount == 0)
                    {
                        prz.Friends.InsertOnSubmit(new Friend
                        {
                            StudentCode = studentID,
                            FriendFacebookID = f,
                            FriendStatus = 'P'
                        });
                    }
                }
                prz.SubmitChanges();
            }
        }

        /*
         * Function Name : HasFacebookID
         * Responsibility : Check current user that he/she already has facebookID in database or not
         * Input : user StudentID
         * Step :   1) get Student Info or current user
         *          2) return the facebookID is exist or not
         */
        public int HasFacebookID(string studentcode)
        {
            using (ParazoDBDataContext pdc = new ParazoDBDataContext())
            {
                return pdc.Students.Where(s => s.StudentCode == studentcode).SingleOrDefault().Facebook_ID != "" ? 1 : 0;
            }
        }

        /*
         * Function Name : GetFacebookID
         * Responsibility : Get current user facebook ID
         * Input : user StudentID
         * Step :   1) get Student Info of current user
         *          2) return facebookID
         */
        public string GetFacebookID(string studentcode)
        {
            using (ParazoDBDataContext pdc = new ParazoDBDataContext())
            {
                return (from student in pdc.Students
                        where student.StudentCode == studentcode
                        select student.Facebook_ID).SingleOrDefault();
            }
        }

        /*
         * Function Name : GetFriendStatus
         * Responsibility : Check that friend already allow us or not
         * Input : Friend StudentID & user FacebookID
         * Step :   1) connect to Friends table of specific StudentID & Friend FacebookID
         *          2) return that FriendStatus is "A" or not
         */
        public bool GetFriendStatus(string FriendStudID, string MyFacebookID)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                return (from friend in prz.Friends
                        where friend.StudentCode == FriendStudID && friend.FriendFacebookID == MyFacebookID
                        select friend.FriendStatus).SingleOrDefault().ToString() == "A" ? true : false;
            }
        }

        /*
         * Fucntion Name : GetFacebookFriendList2JSON
         * Responsibility : Get Information of friends which existing on database which friend has to 
         *                  Allow current user to see their save plans and return as JSON
         * Input : user StudentID
         * Step :   1) get FriendFBID and FriendStatus from friends Table
         *          2) get FriendInformation using FBID and check for friend allow to user and assign to FacebookFriend Object
         *          3) return FacebookFriendList in JSONform
         */
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public JsonResult GetFacebookFriendList2JSON(string studentCode)
        {
            List<String> friendList = new List<string>();
            FacebookFriendList friends = new FacebookFriendList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                string myFacebookID = (from student in prz.Students
                                       where student.StudentCode == studentCode
                                       select student.Facebook_ID).SingleOrDefault();
                var fList = (from f in prz.Friends
                             where f.StudentCode == studentCode
                             select new
                             {
                                 f.FriendFacebookID,
                                 f.FriendStatus
                             }).ToList();

                foreach (var f in fList)
                {
                    FacebookFriend friend = (from s in prz.Students
                                             where s.Facebook_ID == f.FriendFacebookID &&
                                             (from fbFriend in prz.Friends
                                              where s.StudentCode == fbFriend.StudentCode &&
                                                     fbFriend.FriendFacebookID == myFacebookID
                                              select fbFriend.FriendStatus).SingleOrDefault() == 'A'
                                             select new FacebookFriend
                                             {
                                                 StudentCode = s.StudentCode,
                                                 StudentName = s.FirstNameEN,
                                                 StudentFaculty = s.FacultyNameEN,
                                                 StudentMajor = s.DepartmentNameEN,
                                                 FriendStatus = f.FriendStatus,
                                                 FacebookID = f.FriendFacebookID,
                                                 IsChecked = false
                                             }).SingleOrDefault();
                    if (friend != null)
                    {
                        friends.friendList.Add(friend);
                    }
                }
            }

            return Json(friends, JsonRequestBehavior.AllowGet);
        }

        /*
         * Fucntion Name : GetFacebookFriendListJSON
         * Responsibility : Get Information of friends which existing on database and return as JSON
         * Input : user StudentID
         * Step :   1) get FriendFBID and FriendStatus from friends Table
         *          2) get FriendInformation using FBID from above and assign to FacebookFriend Object
         *          3) return FacebookFriendList in JSONform
         */
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public JsonResult GetFacebookFriendListJSON(string studentCode)
        {
            List<String> friendList = new List<string>();
            FacebookFriendList friends = new FacebookFriendList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var fList = (from f in prz.Friends
                             where f.StudentCode == studentCode
                             select new
                             {
                                 f.FriendFacebookID,
                                 f.FriendStatus
                             }).ToList();

                foreach (var f in fList)
                {
                    FacebookFriend friend = (from s in prz.Students
                                             where s.Facebook_ID == f.FriendFacebookID
                                             select new FacebookFriend
                                             {
                                                 StudentCode = s.StudentCode,
                                                 StudentName = s.FirstNameEN,
                                                 StudentFaculty = s.FacultyNameEN,
                                                 StudentMajor = s.DepartmentNameEN,
                                                 FriendStatus = f.FriendStatus,
                                                 FacebookID = f.FriendFacebookID,
                                                 IsChecked = false

                                             }).SingleOrDefault();
                    if (friend != null)
                    {
                        friends.friendList.Add(friend);
                    }

                }

            }

            return Json(friends, JsonRequestBehavior.AllowGet);
        }

        /*
         * Fucntion Name : GetFacebookFriendList
         * Responsibility : Get Information of friends which existing on database
         * Input : user StudentID
         * Step :   1) get FriendFBID and FriendStatus from friends Table
         *          2) get FriendInformation using FBID from above and assign to FacebookFriend Object
         *          3) return FacebookFriendList to view
         */
        [AcceptVerbs(WebRequestMethods.Http.Post)]
        public ActionResult GetFacebookFriendList(string studentCode)
        {
            List<String> friendList = new List<string>();
            FacebookFriendList friends = new FacebookFriendList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var fList = (from f in prz.Friends
                             where f.StudentCode == studentCode
                             select new
                             {
                                 f.FriendFacebookID,
                                 f.FriendStatus
                             }).ToList();

                foreach (var f in fList)
                {
                    FacebookFriend friend = (from s in prz.Students
                                             where s.Facebook_ID == f.FriendFacebookID
                                             select new FacebookFriend
                                             {
                                                 StudentCode = s.StudentCode,
                                                 StudentName = s.FirstNameEN,
                                                 StudentFaculty = s.FacultyNameEN,
                                                 StudentMajor = s.DepartmentNameEN,
                                                 FriendStatus = f.FriendStatus,
                                                 FacebookID = s.Facebook_ID
                                             }).FirstOrDefault();

                    if (friend != null)
                    {
                        friends.friendList.Add(friend);
                    }

                }

            }
            return PartialView("FriendView", friends);
        }

        /*
         * Function Name : ChangeFriendStatus
         * Responsibility : Change Friend Status on specific record to A (Allow)
         * Input : user StudentID & friend Facebook ID
         * Step :   1) get specific record in Friend table with Input
         *          2) set Friend Status to A (Allow)
         *          3) Update on submit
         */
        [AcceptVerbs(WebRequestMethods.Http.Post)]
        public void ChangeFriendStatus(string StudentCode, string FacebookId)
        {

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {

                Friend q = (from student in prz.Friends
                            where student.StudentCode == StudentCode && student.FriendFacebookID == FacebookId
                            select student).SingleOrDefault();
                q.FriendStatus = 'A';
                prz.SubmitChanges();

            }
        }

        /*
         * Function Name : ChangeFriendStatusB
         * Responsibility : Change Friend Status on specific record to B (Block)
         * Input : user StudentID & friend Facebook ID
         * Step :   1) get specific record in Friend table with Input
         *          2) set Friend Status to B (Block)
         *          3) Update on submit
         */
        [AcceptVerbs(WebRequestMethods.Http.Post)]
        public void ChangeFriendStatusB(string StudentCode, string FacebookId)
        {
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {

                Friend q = (from student in prz.Friends
                            where student.StudentCode == StudentCode && student.FriendFacebookID == FacebookId
                            select student).SingleOrDefault();

                //Friend q  = prz.Friends.Where(s => s.StudentCode == StudentCode && s.FriendFacebookID == FacebookId).SingleOrDefault();

                q.FriendStatus = 'B';
                prz.SubmitChanges();

            }
        }

        /*CURRENTLY UNKNOWN FUNCTION*/
        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public ActionResult FriendConstraint(string studentCode)
        {
            List<String> friendList = new List<string>();
            FacebookFriendList friends = new FacebookFriendList();
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                var fList = (from f in prz.Friends
                             where f.StudentCode == studentCode && f.FriendStatus == 'A'
                             select new
                             {
                                 f.FriendFacebookID,
                                 f.FriendStatus
                             }).ToList();

                foreach (var f in fList)
                {
                    FacebookFriend friend = (from s in prz.Students
                                             where s.Facebook_ID == f.FriendFacebookID && s.StudentCode != studentCode
                                             select new FacebookFriend
                                             {
                                                 StudentCode = s.StudentCode,
                                                 StudentName = s.FirstNameEN,
                                                 StudentFaculty = s.FacultyNameEN,
                                                 StudentMajor = s.DepartmentNameEN,
                                                 FriendStatus = f.FriendStatus,
                                                 FacebookID = s.Facebook_ID
                                             }).SingleOrDefault();

                    if (friend != null)
                    {
                        friends.friendList.Add(friend);
                    }

                }

            }

            return PartialView("_PickFriendView", friends);
        }
    }

}
