using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace OMNI
{
    public class updatePassowrd
    {
        public string? Id { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

    }
    public class UserInfo
    {
        [BsonId] // Marks this as the MongoDB document ID
        [BsonRepresentation(BsonType.ObjectId)] // Converts it from string to ObjectId automatically
        public string? Id { get; set; }

        [JsonProperty("full_name")]
        public string full_name { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("password")]
        public string password { get; set; }

        [JsonProperty("assigned_store")]
        public string assigned_store { get; set; }

        [JsonProperty("assigned_store_name")]
        public string? assigned_store_name { get; set; }

        [JsonProperty("expiry")]
        public DateTime expiry { get; set; }

        [JsonProperty("role")]
        public string role { get; set; }

        [JsonProperty("auth_key")]
        public string auth_key { get; set; }

        [JsonProperty("Active")]
        public bool Active { get; set; } = true;

        [JsonProperty("IsSuccess")]
        public bool IsSuccess { get; set; } = false;

        [JsonProperty("exception_message")]
        public string? exception_message { get; set; }

        [JsonProperty("exception")]
        public Exception? exception { get; set; }
    }


}
