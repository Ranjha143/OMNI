using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI
{
    public class Globals
    {

        public static IMongoDatabase mongoDB { get; set; }
        public static string? AuthToken { get; set; } = null;
        public static string? BaseUrl { get; set; } = null;

        public static string MongoConnectionString { get; set; } = "mongodb://localhost:27017"; // Default connection string
        public static string MongoDatabase { get; set; } = "logo"; // Default database name
        public static string ShopifyApiKey { get; set; } = ""; // Replace with your actual API key
        public static string ShopifyApiSecret { get; set; } = ""; // Replace with your actual API secret
    }

    public class CustomException : Exception
    {
        public int ErrorCode { get; }
        public string ALU { get; }
        public string OrderNo { get; }
        public string StyleSid { get; }
        public CustomException(string message, int errorCode = 0, string orderNo = "", string Alu = "", string styleSid = "") : base(message)
        {
            ErrorCode = errorCode;
            ALU = Alu;
            OrderNo = orderNo;
            StyleSid = styleSid;
        }
    }
}
