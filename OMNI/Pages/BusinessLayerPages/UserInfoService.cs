using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;


namespace OMNI.Pages.BusinessLayerPages
{
    public class UserInfoService
    {
       
        private readonly IMongoCollection<UserInfo> logCollection;
        private readonly MongoClient mongoDbClient = new(Globals.MongoConnectionString);
        public UserInfoService() {
            var mongoDB = mongoDbClient.GetDatabase(Globals.MongoDatabase);
            logCollection = mongoDB.GetCollection<UserInfo>("users");
        }

        public async Task<UserInfo> GetUser(string userName) {

            try
            {
                var uaerInfo = await logCollection.Find(u=>u.username == userName).FirstOrDefaultAsync();

                return uaerInfo;

            }
            catch (Exception)
            {
                return new UserInfo();
            }
        }
    }



}
