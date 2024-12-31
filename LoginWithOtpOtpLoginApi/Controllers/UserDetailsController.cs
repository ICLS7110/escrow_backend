using EscrowApi.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using OtpLoginApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Twilio.TwiML.Messaging;

namespace EscrowApi.Controllers
{
    [ApiController]
    [Route("api/userdetails")]
    [Authorize]
    public class UserDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("save-user-details")]
        public async Task<IActionResult> SaveUserDetails([FromBody] UserDetailsDto userDetailsDto)
        {
            if (userDetailsDto == null)
            {
                return BadRequest("Invalid user data.");
            }

            // Map DTO to Entity
            var userDetails = new UserDetails
            {
                FullName = userDetailsDto.FullName,
                EmailAddress = userDetailsDto.EmailAddress,
                Gender = userDetailsDto.Gender,
                DateOfBirth = userDetailsDto.DateOfBirth,
                BusinessManagerName = userDetailsDto.BusinessManagerName,
                BusinessEmail = userDetailsDto.BusinessEmail,
                VatId = userDetailsDto.VatId,
                ProofOfBusiness = await ConvertFileToByteArray(userDetailsDto.ProofOfBusiness),
                AccountHolderName = userDetailsDto.AccountHolderName,
                IBANNumber = userDetailsDto.IBANNumber,
                BICCode = userDetailsDto.BICCode,
                LoginMethod = userDetailsDto.LoginMethod 
            };

            // Save to Database
            _context.UserDetails.Add(userDetails);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User details saved successfully." });
        }

        [HttpGet("get-user-details")]        
        public async Task<IActionResult> GetUserDetails(string loginMethod, string emailId)
        {
            if (string.IsNullOrEmpty(loginMethod) || string.IsNullOrEmpty(emailId))
            {
                return BadRequest("LoginMethod and EmailId are required.");
            }

            // Find the user based on LoginMethod and EmailId
            var userDetails = await _context.UserDetails
                                            .Where(u => u.LoginMethod == loginMethod && u.EmailAddress == emailId)
                                            .FirstOrDefaultAsync();

            if (userDetails == null)
            {
                return NotFound("User not found.");
            }

            return Ok(userDetails);
        }

        [HttpPost("update-user-details")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UserDetailsDto userDetailsDto)
        {
            if (userDetailsDto == null || string.IsNullOrEmpty(userDetailsDto.EmailAddress))
            {
                return BadRequest("Invalid request. Email address is required.");
            }

            // Find the user by email address
            var existingUser = await _context.UserDetails
                .FirstOrDefaultAsync(u => u.EmailAddress == userDetailsDto.EmailAddress);

            if (existingUser == null)
            {
                return NotFound("User with the provided email address does not exist.");
            }

            // Update fields only if they are provided
            existingUser.FullName = string.IsNullOrEmpty(userDetailsDto.FullName) ? existingUser.FullName : userDetailsDto.FullName;
            existingUser.Gender = string.IsNullOrEmpty(userDetailsDto.Gender) ? existingUser.Gender : userDetailsDto.Gender;
            existingUser.DateOfBirth = userDetailsDto.DateOfBirth ?? existingUser.DateOfBirth;
            existingUser.BusinessManagerName = string.IsNullOrEmpty(userDetailsDto.BusinessManagerName) ? existingUser.BusinessManagerName : userDetailsDto.BusinessManagerName;
            existingUser.BusinessEmail = string.IsNullOrEmpty(userDetailsDto.BusinessEmail) ? existingUser.BusinessEmail : userDetailsDto.BusinessEmail;
            existingUser.VatId = string.IsNullOrEmpty(userDetailsDto.VatId) ? existingUser.VatId : userDetailsDto.VatId;
            existingUser.AccountHolderName = string.IsNullOrEmpty(userDetailsDto.AccountHolderName) ? existingUser.AccountHolderName : userDetailsDto.AccountHolderName;
            existingUser.IBANNumber = string.IsNullOrEmpty(userDetailsDto.IBANNumber) ? existingUser.IBANNumber : userDetailsDto.IBANNumber;
            existingUser.BICCode = string.IsNullOrEmpty(userDetailsDto.BICCode) ? existingUser.BICCode : userDetailsDto.BICCode;
            existingUser.LoginMethod = string.IsNullOrEmpty(userDetailsDto.LoginMethod) ? existingUser.LoginMethod : userDetailsDto.LoginMethod;

            if (userDetailsDto.ProofOfBusiness != null)
            {
                existingUser.ProofOfBusiness = await ConvertFileToByteArray(userDetailsDto.ProofOfBusiness);
            }

            _context.UserDetails.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "User details updated successfully." });
        }


        private async Task<byte[]> ConvertFileToByteArray(IFormFile file)
        {
            if (file == null) return null;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
