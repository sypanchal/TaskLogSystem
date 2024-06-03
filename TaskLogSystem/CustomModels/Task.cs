using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskLogSystem.Models
{
    [MetadataType(typeof(TaskMetadata))]
    public partial class Task
    {
        internal class TaskMetadata
        {
            [Key]
            public int TaskID { get; set; }

            [Required(ErrorMessage = "Task Name is required")]
            [Display(Name = "Task Name")]
            [RegularExpression(@"^[a-zA-Z ]{2,50}$", ErrorMessage = "Task Name must contain only with Alphabets upto 50 characters")]
            public string TaskName { get; set; }

            [Required(ErrorMessage = "Task Description is required")]
            [Display(Name = "Task Description")]
            [StringLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
            public string TaskDescription { get; set; }

            [Required(ErrorMessage = "Task Date is required")]
            [DisplayFormat(DataFormatString = "{0:d MMM yyyy}")]
            [Display(Name = "Task Date")]
            public DateTime TaskDate { get; set; }
            public int ApproverID { get; set; }
            public int ApprovedorRejectedBy { get; set; }
            public DateTime ApprovedorRejectedOn { get; set; }
            public string Status { get; set; }

            [DisplayFormat(DataFormatString = "{0:d MMM yyyy HH:mm:ss}")]
            public DateTime CreatedOn { get; set; }

            [DisplayFormat(DataFormatString = "{0:d MMM yyyy HH:mm:ss}")]
            public DateTime ModifiedOn { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}
