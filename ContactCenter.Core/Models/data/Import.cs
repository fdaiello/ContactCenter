using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactCenter.Core.Models
{
    public enum ImportStatus
    {
        queued,
        importing,
        imported,
        canceled,
        error
    }
    public class Import
    {
        [Key]
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int BoardId { get; set; }
        [StringLength(255)]
        public string NewListName { get; set; }
        public virtual Board Board { get; set; }
        public virtual Group Group { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        [StringLength(255)]
        public string UniqueFileName { get; set; }
        public ImportStatus Status { get; set; }

        public int ProgressPercent { get; set; }

        [StringLength(1024)]
        public string MsgErro { get; set; }

        public int countImported { get; set; }

        public int countErrors { get; set; }

        public int countTotal { get; set; }

        public DateTime ImportDate { get; set; }

    }
}
