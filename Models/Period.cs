using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class Period
    {
        private String courseId { get; set; }
        private String coursename { get; set; }
        private int days { get; set; }
        private short section { get; set; }
        private String stimes { get; set; }
        private String etimes { get; set; }

        public Period(String id, String n, int d, short s, String st, String et)
        {
            courseId = id;
            coursename = n;
            days = d;
            section = s;
            stimes = st;
            etimes = et;
        }

        public String getId()
        {
            return this.courseId;
        }

        public String getName()
        {
            return this.coursename;
        }

        public int getDay()
        {
            return this.days;
        }

        public String getStime()
        {
            return this.stimes;
        }

        public String getEtime()
        {
            return this.etimes;
        }

        public short getSection()
        {
            return this.section;
        }
    }

}