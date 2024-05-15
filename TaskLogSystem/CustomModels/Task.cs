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
            public string TaskName { get; set; }
            public string TaskDescription { get; set; }

            [DisplayFormat(DataFormatString = "{0:d MMM yyyy}")]
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
