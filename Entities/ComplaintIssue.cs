﻿using System.ComponentModel.DataAnnotations;

namespace SawirahMunicipalityWeb.Entities
{
    public class ComplaintIssue
    {
        [Key]
        public Guid Id{ get; set; }
        public string IssueName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Complaint> Complaints { get; set; }= new List<Complaint>();

    }
}
