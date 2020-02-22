using Auth.Api;
using Auth.Api.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Auth.Api.Tests
{
    public class UserControllerTests : IClassFixture<TestFixture<Startup>>
    {       
        private readonly HttpClient _httpClient;

        public UserControllerTests(TestFixture<Startup> fixture)
        {
            _httpClient = fixture.Client;
        }

        [Fact]
        public async Task ValidateUserLogin_PassValidUserCredentials_ReturnsToken()
        {
            // Arrange
            User authResponse = null;
            var authRequest = new Login
            {
                UserName = "admin",
                Password = "admin"
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(authRequest), Encoding.UTF8, "application/json");
            var request = "/api/v1/users/login";

            // Act
            var response = await _httpClient.PostAsync(request, httpContent);

            // Assert
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                authResponse = JsonConvert.DeserializeObject<User>(responseContent);
            }

            Assert.True(authResponse != null && authResponse.UserName == "admin");
        }

        [Fact]
        public async Task ValidateInvalidUserLogin_InvalidUserCredentials()
        {
            // Arrange
            User authResponse = null;
            var authRequest = new Login
            {
                UserName = "admin",
                Password = "admin123"
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(authRequest), Encoding.UTF8, "application/json");
            var request = "/api/v1/users/login";

            // Act
            var response = await _httpClient.PostAsync(request, httpContent);

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest && authResponse == null);
        }
                          
        [Fact]
        public async Task CreateNewUser_PassValidUserInfo_ReturnsUserWithIdAndToken()
        {
            // Arrange
            User userInfo = null;
            string json = @"{'FirstName': 'Test', 'LastName': 'user','UserName': 'Test','Password': 'Test','Email': 'Test@vms.com', 'Role': 'Admin'}";
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var request = "/api/v1/users/createuser";

            // Act
            var response = await _httpClient.PostAsync(request, httpContent);

            // Assert
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                userInfo = JsonConvert.DeserializeObject<User>(responseContent);
            }

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK && userInfo.Id != null);
        }

        [Fact]
        public async Task GetAllUser_WithoutParameter_ReturnsListUsers()
        {
            // Arrange
            List<User> userInfo = null;           
            var request = "/api/v1/users/getusers";

            // Act
            var response = await _httpClient.GetAsync(request);

            // Assert
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                userInfo = JsonConvert.DeserializeObject<List<User>>(responseContent);
            }

            Assert.True(userInfo != null && userInfo.Count > 0);
        }
    }
}
