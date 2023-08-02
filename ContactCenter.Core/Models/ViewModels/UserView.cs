using ContactCenter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Models.view
{
    public class UserView
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string NickName { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedAt { get; set; }
        public string Action { get; set; }
        public NotificationLevel Notification { get; set; }
    }
}
