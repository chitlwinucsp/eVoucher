using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using eVoucher.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace eVoucher.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private readonly IDistributedCache distributedCache;

        public AccountController(JwtSettings jwtSettings, IDistributedCache distributedCache)
        {
            this.jwtSettings = jwtSettings;
            this.distributedCache = distributedCache;
        }

        private List<Users> logins=new List<Users>()
        {
            new Users()
            {
                Id = Guid.NewGuid(),
                EmailId = "admin1@gmail.com",
                UserName ="Admin",
                Password="Admin",
            },
            new Users()
            {
                Id = Guid.NewGuid(),
                EmailId = "admin2@gmail.com",
                UserName ="Admin2",
                Password="Admin2",
            }
        };

        /// <summary>
        /// Generate an Access token
        /// </summary>
        /// <param name="userLogins"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult GetToken(UserLogins userLogins)
        {
            try
            {
                var Token = new UserTokens();
                var Valid = logins.Any(x=>x.UserName.Equals(userLogins.UserName,StringComparison.OrdinalIgnoreCase));
                if (Valid)
                {
                    var user = logins.FirstOrDefault(x => x.UserName.Equals(userLogins.UserName, StringComparison.OrdinalIgnoreCase));
                    Token = JwtHelpers.JwtHelpers.GenTokenkey(new UserTokens()
                    {
                        EmailId = user.EmailId,
                        GuidId = Guid.NewGuid(),
                        UserName = user.UserName,
                        Id = user.Id,

                    }, jwtSettings);
                }
                else
                {
                    return BadRequest($"wrong password");
                }
                return Ok(Token);
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// Get List of UserAccounts   
        /// </summary>
        /// <returns>List Of UserAccounts</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<Users>> GetList()
        {
            var cacheKey = "getUserLists";

            List<Users> userLists;
            string serializedUserLists;

            var encodedMovies = await distributedCache.GetAsync(cacheKey);

            if (encodedMovies != null)
            {
                serializedUserLists = Encoding.UTF8.GetString(encodedMovies);
                userLists = JsonConvert.DeserializeObject<List<Users>>(serializedUserLists);
            }
            else
            {
                userLists = logins;
                serializedUserLists = JsonConvert.SerializeObject(userLists);
                encodedMovies = Encoding.UTF8.GetBytes(serializedUserLists);
                var options = new DistributedCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                .SetAbsoluteExpiration(DateTime.Now.AddHours(6));
                await distributedCache.SetAsync(cacheKey, encodedMovies, options);
            }
            return userLists;
        }
    }
}
