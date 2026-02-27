using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace OMNI.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
      
      
        private readonly IMongoCollection<UserInfo> userCollection;
        private readonly MongoClient mongoDbClient = new(Globals.MongoConnectionString);


        public AuthorizationController() {
            var mongoDB = mongoDbClient.GetDatabase(Globals.MongoDatabase);
            userCollection = mongoDB.GetCollection<UserInfo>("users");
        }

        [HttpPost]
        [Route("api/v1/login")]
        public async Task<IActionResult> Login(UserInfo loginInfo)
        {
            try
            {
                var userInfo = await userCollection.Find(u => u.username == loginInfo.username).FirstOrDefaultAsync();

                if (userInfo != null && userInfo.password == loginInfo.password)
                {
                    userInfo.IsSuccess = true;
                    return Ok(new { userInfo });
                }
                else
                {
                    userInfo = new UserInfo
                    {
                        IsSuccess = false,
                        exception_message = "Credentials not verified"
                    };
                    return StatusCode((int)HttpStatusCode.Forbidden, new { userInfo });
                }
            }
            catch (Exception ex)
            {
                var user = new UserInfo
                {
                    IsSuccess = false,
                    exception = ex,
                    exception_message = ex.Message
                };

                return StatusCode((int)HttpStatusCode.BadRequest, new { user });
            }
        }


        // user_list

        [HttpGet]
        [Route("api/v1/user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var userInfo = await userCollection.Find(u=>u.Id == id).FirstOrDefaultAsync();

                if (userInfo != null)
                {
                    userInfo.IsSuccess = true;
                    return StatusCode((int)HttpStatusCode.OK, userInfo);
                }
                else
                {
                    userInfo = new UserInfo { exception_message="user not fornd"};
                    return StatusCode((int)HttpStatusCode.OK, userInfo);
                }

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }

        }

        [HttpPut]
        [Route("api/v1/user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserInfo userInfo)
        {
            try
            {
                
                var updateOrderFilter = "{username:'" + userInfo.username + "'}";
                var orderUpdate = Builders<UserInfo>.Update.Set("password", userInfo.password);
                await userCollection.UpdateOneAsync(updateOrderFilter, orderUpdate);


                return StatusCode((int)HttpStatusCode.OK, new { });

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }

        }

        
        [HttpPost]
        [Route("api/v1/user")]
        public async Task<IActionResult> SaveUser([FromBody] UserInfo userInfo)
        {
            try
            {
                var storeSidQuery = $@"select to_char(sid) as sid, store_name from rps.store where sid = {userInfo.assigned_store} ";
                List<JObject> storeSIdObj = RetailPro2_X.BL.ADO.ReadAsync<JObject>(storeSidQuery);

                userInfo.assigned_store_name = storeSIdObj[0]["STORE_NAME"]?.ToString();

                await userCollection.InsertOneAsync(userInfo);

                return StatusCode((int)HttpStatusCode.OK, new { success = true });

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }

        }


        [HttpPost]
        [Route("api/v1/user_old_pass")]
        public async Task<IActionResult> VarifyUserOldPassword([FromBody] UserInfo userInfo)
        {
            try
            {
                var user = await userCollection.Find(u => u.username == userInfo.username).FirstOrDefaultAsync();

                if (user.password == userInfo.password)
                {
                    return StatusCode((int)HttpStatusCode.OK, true);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.OK, false);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }

        }


        [HttpPut]
        [Route("api/v1/user/update_pass")]
        public async Task<IActionResult> UpdatePassword([FromBody] updatePassowrd userInfo)
        {
            try
            {

                var filter = @$"{{_id:ObjectId('{userInfo.Id}')}}";

                var update = Builders<UserInfo>.Update.Set("password", userInfo.password);
                var result = await userCollection.UpdateOneAsync(filter, update);

                return StatusCode((int)HttpStatusCode.OK, new { success = true });

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }

        }




        [HttpGet]
        [Route("api/v1/user/list")]
        public async Task<IActionResult> GetUserList()
        {
            try
            {
                var userInfoList = await userCollection.Find(_ => true).ToListAsync();

                return StatusCode((int)HttpStatusCode.OK, Newtonsoft.Json.JsonConvert.SerializeObject(userInfoList));

            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, new { ex });
            }
        }

    }

    public class UserListResponce
    {
        [JsonProperty("data")]
        public List<UserInfo> data { get; set; }
    }
}
