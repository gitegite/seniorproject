using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class TimetableList
    {
        public List<Timetable> TList;

        public TimetableList()
        {
            TList = new List<Timetable>();
        }

        public void add(Timetable a)
        {
            TList.Add(a);
        }

        public List<Timetable> getTimetable()
        {
            return this.TList;
        }
    }
}