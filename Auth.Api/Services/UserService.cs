#region snippet_UserServiceClass
using Auth.Api.Infrastructure.Helpers;
using Auth.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly AppSettings _appSettings;

        #region UserService Constructor
        public UserService(IUserstoreDatabaseSettings settings, IOptions<AppSettings> appSettings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<User>(settings.UsersCollectionName);

            _appSettings = appSettings.Value;
        }       

        #endregion

        public List<User> Get() =>
            _users.Find(user => true).ToList();

        public User Authenticate(string username, string password)
        {
             var user = _users.Find<User>(usr => usr.UserName == username && usr.Password == password).FirstOrDefault();
            
            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            // remove password before returning
            user.Password = null;

            return user;
        }       

        public User Create(User user)
        {
            _users.InsertOne(user);
            return user;
        }       
    }
}
#endregion
