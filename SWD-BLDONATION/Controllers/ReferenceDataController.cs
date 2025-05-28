using Microsoft.AspNetCore.Mvc;
using SWD_BLDONATION.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferenceDataController : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách UserRole dưới dạng Id và Name
        /// </summary>
        [HttpGet("userroles")]
        public IActionResult GetUserRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new
                {
                    Id = (byte)r,
                    Name = r.ToString()
                });

            return Ok(roles);
        }

        /// <summary>
        /// Lấy danh sách UserStatus dưới dạng Id và Name
        /// </summary>
        [HttpGet("userstatuses")]
        public IActionResult GetUserStatuses()
        {
            var statuses = Enum.GetValues(typeof(UserStatus))
                .Cast<UserStatus>()
                .Select(s => new
                {
                    Id = (byte)s,
                    Name = s.ToString()
                });

            return Ok(statuses);
        }
    }
}
