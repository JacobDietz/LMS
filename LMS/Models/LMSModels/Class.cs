using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Assignments = new HashSet<Assignment>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public uint ClassId { get; set; }
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public uint Year { get; set; }
        public string Season { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public uint Number { get; set; }
        public string Professor { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Professor ProfessorNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Assignment> Assignments { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
