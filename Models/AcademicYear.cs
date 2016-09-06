using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class AcademicYear
    {
        public short Year { get; set; }
        public short Semester { get; set; }
        public decimal TotalGrade { get; set; }
        public List<AcademicRecord> Records { get; set; }
    }
}