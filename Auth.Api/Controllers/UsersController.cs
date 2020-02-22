using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Auth.Api.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }
        
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody]Login login)
        {
            var user = _userService.Authenticate(login.UserName, login.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet("getusers")]
        public ActionResult<List<User>> Get() =>
            _userService.Get();        

        [HttpPost("createuser")]
        public ActionResult<User> Create(User user)
        {
            _userService.Create(user);

            return Ok(user);
        }      
    }
}
