using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LoginRegisterManageUsers.Models.Identity;
using LoginRegisterManageUsers.Components;
using LoginRegisterManageUsers.Models;
using LoginRegisterManageUsers.Data.Data;
using Microsoft.EntityFrameworkCore;
using LoginRegisterManageUsers.Services;
using LoginRegisterManageUsers.Services.Interfaces;
using LoginRegisterManageUsers.API.Components;
using Microsoft.Extensions.Logging;
using LoginRegisterManageUsers.Models.DTO;
using LoginRegisterManageUsers.Helpers;
using static LoginRegisterManageUsers.Models.ContactPropertyDTO;
using LoginRegisterManageUsers.Services.Base;
using Microsoft.AspNetCore.Http;

namespace LoginRegisterManageUsers.Services.CoreServices
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        #region Constructor        
        private IUserService _userService;
        protected RoleManager<ApplicationRole> _roleManager;
        
        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration,
            IMapper mapper,
            IUserService us,
            ApplicationContext context,
            ILogger<AuthenticationService> logger,
            IHttpContextAccessor hca) : base(userManager, context, mapper, hca, logger, configuration)
        {
            _roleManager = roleManager;
            _userService = us;
        }
        #endregion

        #region Login
        public async Task<ServiceResponse<UserDTO>> Login(Login model)
        {
            var r = new ServiceResponse<UserDTO>();

            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var dto = await GetUserInfo(user);
                    //await userManager.AddClaimAsync(user, new Claim("YourCustomClaimType", "true"));

                    return r.Ok(dto);
                }
                return r.BadRequest("Either Username or Password is Incorrect");
            }
            catch (Exception ex)
            {
                return r.BadRequest(ex.Message);
            }
        }
        #endregion

        #region Validations
        public async Task<ServiceResponse<UserDTO>> ConfirmEmail(ConfirmInput model)
        {
            var r = new ServiceResponse<UserDTO>();
            try
            {
                var user = await userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                    return r.NotFound("user does not exist");

                // if there are spaces, then replace with a + because JS is being dumb
                model.EmailToken = model.EmailToken.Replace(" ", "+");

                var result = await userManager.ConfirmEmailAsync(user, model.EmailToken);

                if (result.Succeeded)
                {
                    var dto = await GetUserInfo(user);
                    return r.Ok(dto);
                }

                return r.BadRequest("There was a problem with the token.");
            }
            catch (Exception ex)
            {
                return r.BadRequest(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDTO>> ConfirmPassword(ConfirmInput model)
        {
            var r = new ServiceResponse<UserDTO>();
            try
            {
                var user = await userManager.FindByIdAsync(model.UserId.ToString());
                if (user == null)
                    return r.NotFound("user does not exist");

                // if there are spaces, then replace with a + because JS is being dumb
                model.PasswordToken = model.PasswordToken.Replace(" ", "+");

                // update the password
                var result = await userManager.ResetPasswordAsync(user, model.PasswordToken, model.Password);
                if (!result.Succeeded)
                    return r.BadRequest(result.Errors);

                // if we have an email token, try to validate that too
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);

                var dto = await GetUserInfo(user);
                return r.Ok(dto);
            }
            catch (Exception ex)
            {
                return r.BadRequest($"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse> ResetPassword(string email)
        {
            var r = new GenericResponse();

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
                return r.NotFound("user not found with that email");

            return r.Ok();
        }
        #endregion

        #region Helpers
        private async Task<UserDTO> GetUserInfo(ApplicationUser user)
        {
        try
        {
            // get the Contact related to this user/login
            user.Contact = await db.Contact.FirstAsync(o => o.Id == user.ContactId);

            // get user roles
            user.UserRoles = db.UserRoles
                                .Include(o => o.Role)
                                .Where(o => o.UserId == user.Id).ToList();

            // send down user info including the new JWT token
            var dto = mapper.Map<UserDTO>(user);

                var x = 10;

            dto.Token = GetToken(user);

            // update user's lastlogin date
            user.LastLoginOn = DateTime.UtcNow;
            user.IsDeleted = false; // in case they deleted and are now coming back
            await userManager.UpdateAsync(user);


            return dto;
        }
        catch (Exception ex)
        {
            log.LogError(ex, ex.Message);
            return null;
        }
    }

        private string GetToken(ApplicationUser user)
        {
            var authClaims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Get the key from AppSettings
            var appSettingsSection = config.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.UTF8.GetBytes(appSettings.Secret);

            // Pad the key to reach 256 bits (32 bytes)
            var paddedKey = new byte[32];
            Array.Copy(key, paddedKey, Math.Min(key.Length, paddedKey.Length));

            var authSigningKey = new SymmetricSecurityKey(paddedKey);

            // Generate JWT token
            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
