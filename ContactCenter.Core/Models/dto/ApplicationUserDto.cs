using System;
using System.Collections.Generic;
using DBConverter.Models.data.Interfaces;
using Microsoft.AspNetCore.Identity;
using ContactCenter.Core.Models;
using System.Reflection;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ContactCenter.Core.Models
{
    public class ApplicationUserDto
    {
        public ApplicationUserDto(ApplicationUser applicationUser)
        {
            foreach (PropertyInfo property in typeof(ApplicationUserDto).GetProperties().Where(p => p.CanWrite))
            {
                if (applicationUser.GetType().GetProperty(property.Name) != null)
                {
                    var x = applicationUser.GetType().GetProperty(property.Name).GetValue(applicationUser, null);
                    property.SetValue(this, x, null);
                }
            }
        }
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set;  }
        public string JobTitle { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime LastActivity { get; set; }                  // Date and time of last activity
        public string LastText { get; set; }                        // Last text send or receaved
        public string WebPushId { get; set; }                       // webpush subscriber Id
        public string PictureFile { get; set; }
        public DateTime LastContactTransfered { get; set; }         // Data hora em que o Bot transferiu o ultimo contato para o atendente - para fazer circular uma chamada para cada atendente
    }
}
