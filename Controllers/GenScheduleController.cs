using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SeniorProject.Models;
using SeniorProject.Controllers;
using System.Net;

namespace SeniorProject.Controllers
{
    public class GenScheduleController : Controller
    {
        short Year = 2015;
        short Sem = 1;
        private List<double> Scores = new List<double>();

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


        public ActionResult GetScheduleWithFavoriteSection(string friendList, string godMode)
        {
            int generatedScheduleNum = 30;
            string studentCode = Session["StudentId"].ToString();
            TimetableList GenSchedules = new TimetableList();
            List<StudentCourse> selected_course = (List<StudentCourse>)Session["CurrentCourseSectionDetail"];
            List<StudentCourse> newSelectedCourse = new List<StudentCourse>();
            string[] courseSectionList = godMode.Split(';');
            foreach (string courseSections in courseSectionList)
            {
                string[] courseSection = courseSections.Split(',');
                StudentCourse course = selected_course.Where(x => x.CourseCode == courseSection[0]).SingleOrDefault();
                List<StudentSection> selectedSections = new List<StudentSection>();
                for (int i = 1; i < courseSection.Count(); ++i)
                {
                    short section = Convert.ToInt16(courseSection[i]);
                    selectedSections.Add(course.Sections.Where(x => x.SectionNumber == section).SingleOrDefault());
                }
                course.Sections = selectedSections;
                newSelectedCourse.Add(course);
            }


            //cut section that conflict with time constraint first
            TimeConstraintCheck(newSelectedCourse, ServiceController.GetStudentConstraint(studentCode));
            //sort remaining course respect by amount
            newSelectedCourse.Sort((a, b) => a.Sections.Count - b.Sections.Count);
            Timetable newT = new Timetable();
            TimetableList scheduleList = new TimetableList();

            GenSchedule(0, newT, friendList, newSelectedCourse, GenSchedules);

            if (!friendList.Equals("")) //have selected Friend
            {
                var i = GenSchedules.getTimetable().OrderByDescending(x => x.score).ThenByDescending(x => x.TotalAvailableSeat).ToList();
                GenSchedules.TList = i.Take(generatedScheduleNum).ToList();
            }
            else
            {
                GenSchedules.TList = GenSchedules.getTimetable().OrderByDescending(x => x.TotalAvailableSeat).Take(generatedScheduleNum).ToList();
            }
            var grouping = (from schedule in GenSchedules.TList
                            group schedule by schedule.TotalTimeformat into g
                            select new TimetableList()
                            {
                                TList = g.ToList()
                            }).ToList();

            List<TimetableList> FinalList = new List<TimetableList>(grouping);

            return PartialView("_GenView", FinalList);
            
        }


        [AcceptVerbs(WebRequestMethods.Http.Get)]
        public ActionResult Test(string friendList, string godMode)
        {
            int generatedScheduleNum = 30;
            TimetableList GenSchedules = new TimetableList();
            string studentCode = Session["StudentId"].ToString();

            List<StudentCourse> selected_course = GetCourseListDetailFriends(Year, Sem, godMode, studentCode);
            Session["CurrentCourseSectionDetail"] = selected_course;

            //cut section that conflict with time constraint first
            TimeConstraintCheck(selected_course, ServiceController.GetStudentConstraint(studentCode));
            //sort remaining course respect by amount
            selected_course.Sort((a, b) => a.Sections.Count - b.Sections.Count);

            Timetable newT = new Timetable();
            TimetableList scheduleList = new TimetableList();

            GenSchedule(0, newT, friendList, selected_course, GenSchedules);

            if (!friendList.Equals("")) //have selected Friend
            {
                var i = GenSchedules.getTimetable().OrderByDescending(x => x.score).ThenByDescending(x => x.seatScore).ToList();
                GenSchedules.TList = i.ToList();
            }
            else
            {

                GenSchedules.TList = GenSchedules.getTimetable().OrderByDescending(x => x.seatScore).ToList();
            }
            
            var grouping = (from schedule in GenSchedules.TList
                            group schedule by schedule.TotalTimeformat into g
                            select new TimetableList()
                            {
                                TList = g.Take(10).ToList()
                            }).Take(30).ToList();

            List<TimetableList> FinalList = new List<TimetableList>(grouping);
            
            return PartialView("_GenView", FinalList);

        }

        /* GenSchedule (int index , Timetable T) old version
        public void GenSchedule(int index, Timetable T)
        {
            // for run through every section
            for (int i = 0; i < selected_course[index].Sections.Count; i++)
            {
                Boolean can_insert = true;
                Timetable newT = new Timetable(T.getT());
                // for run through check on every day of each section
                for (int j = 0; j < selected_course[index].Sections[i].SectionDetails.Count; j++)
                {
                    for (int k = 0; k < newT.getT().Count; k++)
                    {
                        //check for time intersection
                        var tmp = selected_course[index].Sections[i].SectionDetails[j];
                        if (newT.getT()[k].getDay() == tmp.DayNum)
                        {
                            TimeSpan periodStart = TimeSpan.Parse(newT.getT()[k].getStime() + ":00");
                            TimeSpan periodEnd = TimeSpan.Parse(newT.getT()[k].getEtime() + ":00");
                            TimeSpan StartT = TimeSpan.Parse(tmp.StartTime + ":00");
                            TimeSpan EndT = TimeSpan.Parse(tmp.EndTime + ":00");

                            if ((StartT == periodStart) || (EndT == periodEnd) 
                                || (EndT < periodEnd && EndT > periodStart)
                                || (StartT < periodEnd && StartT > periodStart))
                            {
                                can_insert = false;
                                break;
                            }
                        }
                    }

                    if (can_insert == false)
                    {
                        break;
                    }
                }

                if (can_insert == true)
                {
                    for (int j = 0; j < selected_course[index].Sections[i].SectionDetails.Count; j++)
                    {
                        var tmp2 = selected_course[index].Sections[i];
                        Period newP = new Period(tmp2.CourseCode, tmp2.CourseName, tmp2.SectionDetails[j].DayNum
                            , tmp2.SectionNumber, tmp2.SectionDetails[j].StartTime, tmp2.SectionDetails[j].EndTime);
                        newT.add(newP);
                    }
                    // recursive
                    if (index + 1 != selected_course.Count)
                    {
                        GenSchedule(index + 1, newT);
                    }
                    else
                    {
                        newT.getT().Sort((a, b) => int.Parse(a.getStime().Substring(0, 2)) - int.Parse(b.getStime().Substring(0, 2)));
                        newT.getT().Sort((a, b) => a.getDay() - b.getDay());
                        GenSchedules.add(newT);
                    }
                }
            }
        }
        */

        //normal Generator with out friends concerning
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

        //generator that take select friend and find friend in schedule before put to result
        public void GenScheduleWithF(int index, Timetable T, string Friendlist, List<StudentCourse> selected_course, TimetableList GenSchedules)
        {
            // for run through every section
            for (int i = 0; i < selected_course[index].Sections.Count; i++)
            {
                var currSec = selected_course[index].Sections[i];
                bool canInsert = true;
                Timetable newT = new Timetable(T);
                // for run through check on every day of each section
                for (int j = 0; j < currSec.SectionDetails.Count; j++)
                {
                    var currSecDe = currSec.SectionDetails[j];
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
                                //check for time interrupt
                                if (TimeSpan.Parse(currSubDe.StartTime + ":00") == TimeSpan.Parse(currSecDe.StartTime + ":00")
                                    || TimeSpan.Parse(currSubDe.StartTime + ":00") == TimeSpan.Parse(currSecDe.EndTime + ":00")
                                    || (TimeSpan.Parse(currSubDe.EndTime + ":00") > TimeSpan.Parse(currSecDe.EndTime + ":00")
                                        && TimeSpan.Parse(currSubDe.StartTime + ":00") < TimeSpan.Parse(currSecDe.EndTime + ":00"))
                                    || (TimeSpan.Parse(currSecDe.StartTime + ":00") < TimeSpan.Parse(currSubDe.EndTime + ":00") &&
                                        TimeSpan.Parse(currSecDe.StartTime + ":00") > TimeSpan.Parse(currSubDe.StartTime + ":00")))
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
                        GenScheduleWithF(index + 1, newT, Friendlist, selected_course, GenSchedules);
                    }
                    else
                    {
                        bool havefriend = false;
                        string[] selectFriends = Friendlist.Split(',');
                        Console.WriteLine(selectFriends.Length);

                        //run through every section and search for friends
                        foreach (StudentSection currentSec in newT.getT())
                        {
                            foreach (string friendId in currentSec.FriendSavedList)
                            {
                                // Having friend in at least 1 section
                                if (selectFriends.Contains(friendId) && friendId != "")
                                {
                                    havefriend = true;
                                    break;
                                }
                            }
                            if (havefriend == true)
                            {
                                break;
                            }
                        }

                        //not take section that has no friend or if none select friend put it in to result
                        if (havefriend == true || selectFriends[0].Equals(""))
                        {
                            GenSchedules.add(newT);
                        }
                    }
                }
            }
        }

        /*
         * Function Name : Findscore
         * Input : Timetable (Schedule) & Friendlist (friends studentID seperated by comma)
         * Step :   1) Calculate bestcase (selectFriends * subject amounts)
         *          2) Count real totalNumber of friends & subjects
         *          3) Calculate score by Formula
         * Formula : 10 * (realtotalFriends / bestcase) * (realtotalSubjectWithFriends / SubjectAmount)
         */
        public void Findscore(Timetable inlist, string friendlist)
        {
            int SubjectAmt = inlist.getT().Count;
            string[] friendSplit = friendlist.Split(',');
            double bestCase = friendSplit.Length * SubjectAmt;
            int totalfriend = 0;
            int SubjectWithF = 0;
            double score = 0;
            double seatScore = 0;

            foreach (StudentSection Sec in inlist.getT())
            {
                double SecseatScore = (((double)Sec.SeatAvailable / (double)Sec.SeatLimited) * 100);
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
                seatScore += SecseatScore;
            }
            score = 10 * (double)(totalfriend / bestCase) * (double)(SubjectWithF / SubjectAmt);
            inlist.score = score;
            inlist.seatScore = seatScore;
        }

        // check time constraint before generated
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

        // Find min day after generated
        public void MinDayConstraint(TimetableList inList)
        {
            TimetableList result = new TimetableList();
            int min = 999;
            for (int i = 0; i < inList.getTimetable().Count; ++i)
            {
                var currSchedule = inList.getTimetable()[i];
                List<int> days = new List<int>();
                for (int j = 0; j < currSchedule.getT().Count; ++j)
                {
                    var currScheSecDe = currSchedule.getT()[j].SectionDetails;
                    for (int k = 0; k < currScheSecDe.Count; k++)
                    {
                        if (days.Contains(currScheSecDe[k].DayNum) == false)
                        {
                            days.Add(currScheSecDe[k].DayNum);
                        }
                    }
                }
                if (min > days.Count)
                {
                    min = days.Count;
                    result.getTimetable().Clear();
                    result.getTimetable().Add(currSchedule);
                }
                else if (min == days.Count)
                {
                    result.getTimetable().Add(currSchedule);
                }
            }
            inList = result;
        }

        // Find max day after generated
        public void MaxDayConstraint(TimetableList inList)
        {
            TimetableList result = new TimetableList();
            int max = 0;
            for (int i = 0; i < inList.getTimetable().Count; ++i)
            {
                var currSchedule = inList.getTimetable()[i];
                List<int> days = new List<int>();
                for (int j = 0; j < currSchedule.getT().Count; ++j)
                {
                    var currScheSecDe = currSchedule.getT()[j].SectionDetails;
                    for (int k = 0; k < currScheSecDe.Count; k++)
                    {
                        if (days.Contains(currScheSecDe[k].DayNum) == false)
                        {
                            days.Add(currScheSecDe[k].DayNum);
                        }
                    }
                }
                if (max < days.Count)
                {
                    max = days.Count;
                    result.getTimetable().Clear();
                    result.getTimetable().Add(currSchedule);
                }
                else if (max == days.Count)
                {
                    result.getTimetable().Add(currSchedule);
                }
            }
            inList = result;
        }

        // Find nongap Schedule after generated
        public void NonGapConstraint(TimetableList inList)
        {
            TimetableList result = new TimetableList();
            for (int i = 0; i < inList.getTimetable().Count; ++i)
            {
                //current schedule
                var currSche = inList.getTimetable()[i];
                bool havingGap = false;
                //current sections
                for (int j = 0; j < currSche.getT().Count - 1; ++j)
                {
                    //List currSection
                    var currSection = currSche.getT()[j];
                    //compare section
                    for (int k = j + 1; k < currSche.getT().Count; ++k)
                    {
                        //List compare section Details
                        var compareSection = currSche.getT()[k];
                        //run through current schedule section detail
                        for (int l = 0; l < currSection.SectionDetails.Count; l++)
                        {
                            bool betweenGap = true;
                            //run through compare schedule section detail
                            for (int m = 0; m < compareSection.SectionDetails.Count; m++)
                            {
                                //having same day
                                if (currSection.SectionDetails[l].DayNum == compareSection.SectionDetails[m].DayNum)
                                {
                                    //check for which course come first starttime
                                    if (TimeSpan.Parse(currSection.SectionDetails[l].StartTime + ":00") < TimeSpan.Parse(compareSection.SectionDetails[m].StartTime + ":00"))
                                    {
                                        // if endtime != start time => having gap
                                        if (TimeSpan.Parse(currSection.SectionDetails[l].EndTime + ":00") != TimeSpan.Parse(compareSection.SectionDetails[m].StartTime + ":00"))
                                        {
                                            betweenGap = true;
                                        }
                                        else
                                        {
                                            betweenGap = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (TimeSpan.Parse(compareSection.SectionDetails[m].EndTime + ":00") != TimeSpan.Parse(currSection.SectionDetails[l].StartTime + ":00"))
                                        {
                                            betweenGap = true;
                                        }
                                        else
                                        {
                                            betweenGap = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (betweenGap == false)
                            {
                                break;
                            }
                        }
                        if (havingGap == true)
                        {
                            break;
                        }
                    }
                    if (havingGap == true)
                    {
                        break;
                    }
                }

                if (havingGap == false)
                {
                    result.getTimetable().Add(currSche);
                }
            }

            inList = result;
        }

        //Version 1.1
        public void NonGapConstraint2(TimetableList inList)
        {
            TimetableList result = new TimetableList();
            for (int i = 0; i < inList.getTimetable().Count; ++i)
            {
                bool haveGap = false;
                List<StudentSectionDetail> timelist = new List<StudentSectionDetail>();
                //current schedule
                var currSche = inList.getTimetable()[i];
                for (int j = 0; j < currSche.getT().Count; j++)
                {
                    var currSecDe = currSche.getT()[j].SectionDetails;
                    for (int k = 0; k < currSecDe.Count; k++)
                    {
                        timelist.Add(currSecDe[k]);
                    }
                }

                timelist.OrderBy(a => TimeSpan.Parse(a.StartTime + ":00")).ThenBy(a => a.DayNum);
                for (int j = 0; j < timelist.Count - 1; j++)
                {
                    if (timelist[j].DayNum == timelist[j + 1].DayNum)
                    {

                        if (!timelist[j].EndTime.Equals(timelist[j + 1].StartTime))
                        {
                            haveGap = true;
                            break;
                        }

                    }
                }

                if (haveGap == false)
                {
                    result.add(currSche);
                }
            }
            inList = result;
        }


        // Find gapping Schedule after generated
        public void HaveGapConstraint(TimetableList inList)
        {
            TimetableList result = new TimetableList();
            for (int i = 0; i < inList.getTimetable().Count; ++i)
            {
                bool haveGap = false;
                List<StudentSectionDetail> timelist = new List<StudentSectionDetail>();
                //current schedule
                var currSche = inList.getTimetable()[i];
                for (int j = 0; j < currSche.getT().Count; j++)
                {
                    var currSecDe = currSche.getT()[j].SectionDetails;
                    for (int k = 0; k < currSecDe.Count; k++)
                    {
                        timelist.Add(currSecDe[k]);
                    }
                }

                timelist.OrderBy(a => a.DayNum).ThenBy(a => TimeSpan.Parse(a.StartTime + ":00"));
                for (int j = 0; j < timelist.Count - 1; j++)
                {
                    if (timelist[j].DayNum == timelist[j + 1].DayNum)
                    {
                        if (!timelist[j].EndTime.Equals(timelist[j + 1].StartTime))
                        {
                            haveGap = true;
                            break;
                        }
                    }
                }

                if (haveGap == true)
                {
                    result.add(currSche);
                }
            }
            inList = result;
        }

        public string savePlan2(string planList, string scheduleList)
        {
            string studentID = Session["StudentId"].ToString();
            planList = SortPlanList(planList);
            scheduleList = SortScheduleList(planList, scheduleList);
            string resultMessage = "";

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                int planCount = prz.SavedPlans.Where(x => x.PlanList == planList).Count();
                if (planCount == 0)
                {
                    prz.SavedPlans.InsertOnSubmit(new SavedPlan
                    {
                        StudentCode = studentID,
                        PlanList = planList
                    });
                    prz.SubmitChanges();
                }
                else
                {
                    resultMessage = "This plan has already existed!";
                }

                int scheduleCount = prz.SavedSchedules.Where(x => x.ScheduleList == scheduleList).Count();

                if (scheduleCount == 0)
                {
                    prz.SavedSchedules.InsertOnSubmit(new SavedSchedule
                    {
                        StudentCode = studentID,
                        PlanList = planList,
                        ScheduleList = scheduleList
                    });

                    prz.SubmitChanges();
                    resultMessage += "Schedule(s) saved successfully";
                }
                else
                {
                    resultMessage += "This schedule has existed";
                }
                return resultMessage;
            }
        }

        public string SortPlanList(string planList)
        {
            string resultPlan = "";
            string[] temp = planList.Split(';');

            for (int i = 0; i < temp.Length; ++i)
            {
                for (int j = 0; j < temp.Length; ++j)
                {
                    if (temp[i].CompareTo(temp[j]) < 0)
                    {
                        string tmp = temp[i];
                        temp[i] = temp[j];
                        temp[j] = tmp;
                    }
                }
            }

            foreach (string t in temp)
            {
                resultPlan += t + ";";
            }
            resultPlan = resultPlan.Substring(0, resultPlan.Length - 1);
            return resultPlan;
        }

        public string SortScheduleList(string planList, string scheduleList)
        {
            string resultScheduleList = "";
            string[] planTemp = planList.Split(';');
            string[] schedulePlanTemp = scheduleList.Split(';');
            foreach (string p in planTemp)
            {
                resultScheduleList += p + ",";
                foreach (string spTemp in schedulePlanTemp)
                {
                    string[] subject = spTemp.Split(',');
                    if (subject[0] == p)
                    {
                        resultScheduleList += subject[1] + ";";
                        break;
                    }
                }
            }
            resultScheduleList = resultScheduleList.Substring(0, resultScheduleList.Length - 1);
            return resultScheduleList;
        }

        public string savePlan(string studentID , string planList, string scheduleList)
        {
            planList = SortPlanList(planList);
            scheduleList = SortScheduleList(planList, scheduleList);
            string resultMessage = "";

            using (ParazoDBDataContext prz = new ParazoDBDataContext())
            {
                int planCount = prz.SavedPlans.Where(x => x.PlanList == planList).Count();
                if (planCount == 0)
                {
                    prz.SavedPlans.InsertOnSubmit(new SavedPlan
                    {
                        StudentCode = studentID,
                        PlanList = planList
                    });
                    prz.SubmitChanges();
                }
                else
                {
                    resultMessage = "This plan has already existed!";
                }

                int scheduleCount = prz.SavedSchedules.Where(x => x.ScheduleList == scheduleList).Count();

                if (scheduleCount == 0)
                {
                    prz.SavedSchedules.InsertOnSubmit(new SavedSchedule
                    {
                        StudentCode = studentID,
                        PlanList = planList,
                        ScheduleList = scheduleList
                    });

                    prz.SubmitChanges();
                    resultMessage += "Schedule(s) saved successfully";
                }
                else
                {
                    resultMessage += "This schedule has existed";
                }
                return resultMessage;
            }
        }
    }

}