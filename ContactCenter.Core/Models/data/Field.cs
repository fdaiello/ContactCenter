using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ContactCenter.Core.Models
{
    // Field Type
    public enum FieldType
    {
        Integer,
        Decimal,
        Money,
        Date,
        Time,
        DateTime,
        Text,
        TextArea,
        DataList,
        Image,
        Document
    }

    // Customized Fields diefinition
    // It will be used to configure available fields for Contacts and for Cards
    public class Field
    {
        public int Id { get; set; }                                             // Primary Key
        public bool IsGlobal { get; set; }                                      // Indicate if this field is available for all Groups, or is particular to a group
        public int? GroupId { get; set; }                                       // Foreign Key Group - Nullable - In case this field is not global, Id of the group that owns this field
        [StringLength(256)]
        public string Label { get; set; }                                       // Label of the field, for screen use
        public FieldType FieldType { get; set; }                                // Indicate type of field
        public virtual ICollection<DataListValue> DataListValues { get; set; }  // List of values
        public virtual ICollection<ContactField> ContactFields { get; set; }    // fields enabled to Contacts
        public virtual ICollection<BoardField> BoardFields { get; set; }        // fields enabeld to given Board
        public virtual ICollection<ContactFieldValue> ContactFieldValues { get; set; }
        public void CopyFromDto(FieldDto fieldDto)
        {
            this.Label = fieldDto.Label;
            this.FieldType = fieldDto.FieldType;
            this.DataListValues = fieldDto.DataListValues;
        }
    }

}
