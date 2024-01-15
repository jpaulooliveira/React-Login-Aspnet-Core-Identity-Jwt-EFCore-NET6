using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoginRegisterManageUsers.Models.Identity
{
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}
