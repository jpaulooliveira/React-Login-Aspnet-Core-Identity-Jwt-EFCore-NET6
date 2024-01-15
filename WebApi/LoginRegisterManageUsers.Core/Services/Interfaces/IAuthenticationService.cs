using LoginRegisterManageUsers.Models;
using LoginRegisterManageUsers.Models.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using static LoginRegisterManageUsers.Models.ContactPropertyDTO;

namespace LoginRegisterManageUsers.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ServiceResponse<UserDTO>> Login(Login model);
        Task<ServiceResponse<UserDTO>> ConfirmEmail(ConfirmInput model);
        Task<ServiceResponse<UserDTO>> ConfirmPassword(ConfirmInput model);
        Task<GenericResponse> ResetPassword(string email);
    }
}
