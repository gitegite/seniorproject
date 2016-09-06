using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class Timetable
    {
        public List<StudentSection> timetable { get; set; }
        public double score { get; set; }
        public double seatScore { get; set; }
        public string TotalTimeformat { get; set; }
        public string scheduleString { get; set; }
        public string sameScheduleFriend { get; set; }
        public int TotalAvailableSeat { get; set; }

        public Timetable()
        {
            timetable = new List<StudentSection>();
            TotalTimeformat = "";
            scheduleString = "";
            TotalAvailableSeat = 0;
            score = 0;
            seatScore = 0;
        }

        public Timetable(Timetable T)
        {
            this.timetable = new List<StudentSection>(T.getT());
            this.TotalTimeformat = T.TotalTimeformat;
            this.TotalAvailableSeat = T.TotalAvailableSeat;
            this.score = T.score;
            this.seatScore = T.seatScore/100;
        }

        public List<StudentSection> getT()
        {
            return this.timetable;
        }

        public void add(StudentSection newSc)
        {
            timetable.Add(newSc);
        }
    }
}