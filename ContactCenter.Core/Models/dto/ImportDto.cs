using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    public class ImportDto
    {

        public ImportDto(Import import)
        {
            if (import != null)
            {
                foreach (PropertyInfo property in typeof(ImportDto).GetProperties())
                {
                    var x = import.GetType().GetProperty(property.Name).GetValue(import, null);
                    property.SetValue(this, x, null);
                }
            }
        }
        public int Id { get; set; }

        public int BoardId { get; set; }
        public string NewListName { get; set; }
        public virtual Board Board { get; set; }
        public virtual Group Group { get; set; }

        public string FileName { get; set; }
        public string UniqueFileName { get; set; }
        public ImportStatus Status { get; set; }

        public int ProgressPercent { get; set; }

        public string MsgErro { get; set; }

        public int countImported { get; set; }

        public int countErrors { get; set; }

        public int countTotal { get; set; }

        public DateTime ImportDate { get; set; }

    }
}
