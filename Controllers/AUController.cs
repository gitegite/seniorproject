using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;

namespace SeniorProject.Controllers
{
    public class AUController : Controller
    {
        // GET: AU
        public ActionResult Index()
        {
            return View();
        }

        public List<StudentCourse> GetCourseListDetailFriends(short year, short semester, string courseList, string studentCode)
        {
            // Format --> ACT1600,401,402,403,404;BG1001,401,405,406;BG14036,401,402,403
            List<StudentCourse> courses = new List<StudentCourse>();
            string[] courseSections = courseList.Split(';');
            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                foreach (string cs in courseSections)
                {
                    string[] courseSection = cs.Split(',');
                    string courseCode = courseSection[0];
                    StudentCourse c = new StudentCourse();

                    var q = (from course in prz.Courses
                             where course.CourseCode == courseCode
                             select new
                             {
                                 CourseName = course.NameEN,
                                 Credit = course.Credit,
                                 CreditSum = course.CreditSum
                             }).SingleOrDefault();
                    c.CourseName = q.CourseName;
                    c.CourseCode = courseCode;
                    c.Credit = q.Credit;
                    c.CreditSum = q.CreditSum;

                    for (int i = 1; i < courseSection.Length; ++i)
                    {
                        short sectionNumber = Convert.ToInt16(courseSection[i]);
                        StudentSection sec = (from course in prz.Courses
                                              join section in prz.Sections on course.CourseCode equals section.CourseCode
                                              where course.CourseCode == courseCode && section.Year == year
                                              && section.Semester == semester && section.SectionNumber == sectionNumber
                                              select new StudentSection()
                                              {
                                                  Year = section.Year,
                                                  Semester = section.Semester,
                                                  CourseCode = course.CourseCode,
                                                  CourseName = course.NameEN,
                                                  SectionNumber = section.SectionNumber,
                                                  IsClosed = section.IsClosed,
                                                  SeatAvailable = section.SeatAvailable,
                                                  SeatLimited = section.SeatLimit,
                                                  SeatUsed = section.SeatUsed,
                                                  Remark = section.Remark,
                                                  MidtermDate = section.MidtermDate,
                                                  MidtermStartTime = section.MidtermStartTime,
                                                  MidtermEndTime = section.MidtermEndTime,
                                                  MidtermRoom = section.MidtermRoom,
                                                  FinalDate = section.FinalDate,
                                                  FinalStartTime = section.FinalStartTime,
                                                  FinalEndTime = section.FinalEndTime,
                                                  FinalRoom = section.FinalRoom,

                                              }).SingleOrDefault();



                        sec.FriendSavedList = (from friend in prz.Friends
                                               join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                               join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                                               where friend.StudentCode == studentCode
                                               && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                                               select student.StudentCode).Distinct().ToList();

                        //sec.FriendFBIDList = (from friend in prz.Friends
                        //                      join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                        //                      join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                        //                      where friend.StudentCode == studentCode
                        //                      && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                        //                      select student.Facebook_ID).Distinct().ToList();

                        //sec.FriendNameSavedList = (from friend in prz.Friends
                        //                           join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                        //                           join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                        //                           where friend.StudentCode == studentCode
                        //                           && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                        //                           select student.FirstNameEN).Distinct().ToList();


                        sec.FriendList = (from friend in prz.Friends
                                          join student in prz.Students on friend.FriendFacebookID equals student.Facebook_ID
                                          join schedule in prz.SavedSchedules on student.StudentCode equals schedule.StudentCode
                                          where friend.StudentCode == studentCode
                                          && schedule.ScheduleList.Contains(sec.CourseCode + "," + sec.SectionNumber)
                                          select new FacebookFriend
                                          {
                                              StudentCode = student.StudentCode,
                                              StudentName = student.FirstNameEN,
                                              StudentFaculty = student.FacultyNameEN,
                                              StudentMajor = student.DepartmentNameEN,
                                              FacebookID = student.Facebook_ID
                                          }).Distinct().ToList();


                        sec.SectionDetails = (from detail in prz.SectionDetails
                                              where detail.Year == year && detail.Semester == semester
                                              && detail.CourseCode == courseCode && detail.SectionNumber == sectionNumber
                                              orderby detail.Day
                                              select new StudentSectionDetail()
                                              {
                                                  Year = year,
                                                  Semester = semester,
                                                  CourseCode = detail.CourseCode,
                                                  CourseName = sec.CourseName,
                                                  DayNum = detail.Day,
                                                  StartTime = detail.StartTime,
                                                  StartTimeNum = detail.Start,
                                                  EndTime = detail.EndTime,
                                                  EndTimeNum = detail.End,
                                                  InstructorCode = detail.InstructorCode,
                                                  Instructor = (from instructor in prz.Instructors
                                                                where instructor.InstructorCode == detail.InstructorCode
                                                                select instructor.FirstNameEN + " " + instructor.LastNameEN).SingleOrDefault(),
                                                  Room = detail.RoomCode,
                                                  IsMorning = detail.IsMorning
                                              }).ToList();
                        c.Sections.Add(sec);

                    }
                    courses.Add(c);
                }
            }
            return courses;
        }

        public void TimeConstraintCheck(List<StudentCourse> inputCourse, List<StudentConstraintData> Constraints /* & Constraints */)
        {
            for (int i = 0; i < inputCourse.Count; i++)
            {
                for (int j = inputCourse[i].Sections.Count; j > 0; j--)
                {
                    Boolean can_insert = true;
                    var tmp = inputCourse[i].Sections[j - 1];
                    for (int k = 0; k < tmp.SectionDetails.Count; k++)
                    {
                        var tmp2 = (tmp.SectionDetails[k].DayNum - 1) * 3;
                        var sT = TimeSpan.Parse(tmp.SectionDetails[k].StartTime + ":00");
                        var eT = TimeSpan.Parse(tmp.SectionDetails[k].EndTime + ":00");

                        if (sT >= TimeSpan.Parse("09:00:00") && eT <= TimeSpan.Parse("12:00:00"))
                        {
                            if (Constraints[tmp2].ConstraintValue == false)
                            {
                                can_insert = false;
                                break;
                            }
                        }
                        else if (sT >= TimeSpan.Parse("12:00:00") && eT <= TimeSpan.Parse("13:30:00"))
                        {
                            if (Constraints[tmp2 + 1].ConstraintValue == false)
                            {
                                can_insert = false;
                                break;
                            }
                        }
                        else if (sT >= TimeSpan.Parse("13:30:00") && eT <= TimeSpan.Parse("16:30:00"))
                        {
                            if (Constraints[tmp2 + 2].ConstraintValue == false)
                            {
                                can_insert = false;
                                break;
                            }
                        }
                        else if (sT >= TimeSpan.Parse("09:00:00") && eT <= TimeSpan.Parse("13:30:00"))
                        {
                            if (Constraints[tmp2].ConstraintValue == false || Constraints[tmp2 + 1].ConstraintValue == false)
                            {
                                can_insert = false;
                                break;
                            }
                        }
                        else
                        {
                            if (Constraints[tmp2 + 1].ConstraintValue == false || Constraints[tmp2 + 2].ConstraintValue == false)
                            {
                                can_insert = false;
                                break;
                            }
                        }
                    }

                    if (can_insert == false)
                    {
                        inputCourse[i].Sections.RemoveAt(j - 1);
                    }
                }
            }
        }

        public void GenSchedule(int index, Timetable T, string friendList, List<StudentCourse> selected_course, TimetableList GenSchedules)
        {
            // for run through every section
            for (int i = 0; i < selected_course[index].Sections.Count; i++)
            {
                var currSec = selected_course[index].Sections[i];
                bool canInsert = true;
                Timetable newT = new Timetable(T);
                newT.TotalTimeformat += currSec.CourseCode;
                newT.TotalAvailableSeat += currSec.SeatAvailable;
                // for run through check on every day of each section
                for (int j = 0; j < currSec.SectionDetails.Count; j++)
                {
                    var currSecDe = currSec.SectionDetails[j];
                    newT.TotalTimeformat += "," + currSecDe.DayNum + "," + currSecDe.StartTime + "," + currSecDe.EndTime;
                    for (int k = 0; k < newT.getT().Count; k++)
                    {
                        var currSub = newT.getT()[k];
                        //check for conflict exam date -> first
                        if (currSec.MidtermDate.Equals(currSub.MidtermDate) || currSec.FinalDate.Equals(currSub.FinalDate))
                        {
                            if (currSec.MidtermStartTime.Equals(currSub.MidtermStartTime) || currSec.FinalStartTime.Equals(currSub.FinalStartTime))
                            {
                                canInsert = false;
                                break;
                            }
                        }

                        //check for time conflict -> second
                        for (int l = 0; l < currSub.SectionDetails.Count; l++)
                        {
                            var currSubDe = currSub.SectionDetails[l];
                            if (currSubDe.DayNum == currSecDe.DayNum) // detect same day
                            {
                                var InsertSubDeST = TimeSpan.Parse(currSubDe.StartTime + ":00");
                                var InsertSubDeET = TimeSpan.Parse(currSubDe.EndTime + ":00");

                                var ScheduleSubDeST = TimeSpan.Parse(currSecDe.StartTime + ":00");
                                var ScheduleSubDeET = TimeSpan.Parse(currSecDe.EndTime + ":00");

                                //check for time interrupt
                                if (InsertSubDeST == ScheduleSubDeST // Same Start Time
                                    || (ScheduleSubDeST < InsertSubDeET && InsertSubDeET <= ScheduleSubDeET) // Have endtime inside other subject
                                    || (ScheduleSubDeST <= InsertSubDeST && InsertSubDeST < ScheduleSubDeET) // Have starttime inside during other subject
                                    || (ScheduleSubDeST > InsertSubDeST && ScheduleSubDeST < InsertSubDeET)  // Have subject inside other subject;
                                    || (InsertSubDeST < ScheduleSubDeST && InsertSubDeET > ScheduleSubDeET)) // Insert Subject over existed Subject
                                {
                                    canInsert = false;
                                    break;
                                }

                            }
                        }

                        if (canInsert == false)
                        {
                            break;
                        }
                    }

                    if (canInsert == false)
                    {
                        break;
                    }
                }

                if (canInsert == true)
                {
                    newT.add(currSec);

                    if (index + 1 != selected_course.Count)
                    {
                        newT.TotalTimeformat += ";";
                        newT.scheduleString += currSec.CourseCode + "," + currSec.SectionNumber + ";";
                        GenSchedule(index + 1, newT, friendList, selected_course, GenSchedules);
                    }
                    else
                    {
                        newT.scheduleString += currSec.CourseCode + "," + currSec.SectionNumber;
                        Findscore(newT, friendList);
                        newT.timetable = newT.timetable.OrderBy(x => x.CourseCode).ToList();
                        GenSchedules.add(newT);
                    }
                }
            }
        }

        public void Findscore(Timetable inlist, string friendlist)
        {
            int SubjectAmt = inlist.getT().Count;
            string[] friendSplit = friendlist.Split(',');
            double bestCase = friendSplit.Length * SubjectAmt;
            int totalfriend = 0;
            int SubjectWithF = 0;
            double score = 0;

            foreach (StudentSection Sec in inlist.getT())
            {
                bool havefriend = false;
                foreach (string friendId in friendSplit)
                {
                    if (Sec.FriendSavedList.Contains(friendId))
                    {
                        totalfriend += 1;
                    }

                    if (!havefriend)
                    {
                        havefriend = true;
                        SubjectWithF += 1;
                    }
                }
            }
            score = 10 * (double)(totalfriend / bestCase) * (double)(SubjectWithF / SubjectAmt);
            inlist.score = score;
        }


        /* using after complete generated schedule
         * GenSchedules -- in GenController
         */
        public AUGenerateSchedules getFriendsameSchedule(TimetableList GenSchedules, string studentCode)
        {
            AUGenerateSchedules toReturn = new AUGenerateSchedules();
            foreach (Timetable schedule in GenSchedules.TList)
            {
                string god = "";
                string scheduleString = "";
                foreach (StudentSection section in schedule.timetable)
                {
                    scheduleString += section.CourseCode + "," + section.SectionNumber + "," + section.SeatAvailable + ";";
                    god += section.CourseCode + "," + section.SectionNumber + ";";
                }
                toReturn.generatedScheduleStrList.Add(scheduleString);
                god = god.Substring(0, god.Length - 1);
                
                /*
                 * query friend same Schedule
                 */

                toReturn.friendsameScheduleList.Add("query result in god");
            }

            return toReturn;
        }
    }
}