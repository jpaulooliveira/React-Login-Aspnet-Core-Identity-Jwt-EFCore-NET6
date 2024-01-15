using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LoginRegisterManageUsers.Models.Identity;

namespace LoginRegisterManageUsers.Data.Models.BaseModels
{
    public class BaseTrackedByModel : BaseTrackedModel
    {        
        [Required]
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
