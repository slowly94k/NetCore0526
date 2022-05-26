﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Data.Classes
{
    public class User
    {
        //어노테이션 [key]값만 입력
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        //멤버변수 하나 추가 12.
        public string AccessFailedCount { get; set; }
        public bool IsMembershipWithdrawn { get; set; }
        public System.DateTime JoinedUtcDate { get; set; }
    
        public virtual ICollection<UserRolesByUser> UserRolesByUsers { get; set; }
    }
}
