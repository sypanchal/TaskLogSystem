using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskLogSystem.Models
{
    [MetadataType(typeof(EmployeeMetadata))]
    public partial class Employee
    {
        internal class EmployeeMetadata
        {
            [Key]
            public int EmployeeID { get; set; }

            public string EmployeeCode { get; set; }

            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required]
            public string Gender { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }

            [Required]
            public string MobileNumber { get; set; }

            public int RoleID { get; set; }

            public int ReportingPerson { get; set; }

            public bool IsDeleted { get; set; }
        }
    }
}