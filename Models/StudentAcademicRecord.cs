using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SeniorProject.Models
{
    public class StudentAcademicRecord
    {
        public List<AcademicYear> AcademicRecords { get; set; }

        public StudentAcademicRecord()
        {
            AcademicRecords = new List<AcademicYear>();
        }
    }
}