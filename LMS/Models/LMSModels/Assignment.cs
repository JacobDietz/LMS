using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public uint AssignmentId { get; set; }
        public uint ClassId { get; set; }
        public string Name { get; set; } = null!;
        public uint Points { get; set; }
        public string? Content { get; set; }
        public DateTime Due { get; set; }
        public string CategoryName { get; set; } = null!;

        public virtual AssignmentCategory C { get; set; } = null!;
        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
