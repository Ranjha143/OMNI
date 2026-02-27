using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginManager;

namespace Shopify
{
    public static class GraphAPI
    {
        public static async Task<IGraphQLResponse?> QueryAsync(string query)
        {
            try
            {
                if (GlobalVariables.ShopifyConfig != null && GlobalVariables.ShopifyConfig.GraphUrl !=null)
                {
                    var serializer = new NewtonsoftJsonSerializer();
                    var graphQLClient = new GraphQLHttpClient(GlobalVariables.ShopifyConfig.GraphUrl, serializer);
                    graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", GlobalVariables.ShopifyConfig.ApiAccessToken);

                    return await graphQLClient.SendQueryAsync<JObject>(query);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<IGraphQLResponse?> QueryAsync(GraphQLRequest query)
        {
            try
            {
                if (GlobalVariables.ShopifyConfig != null && GlobalVariables.ShopifyConfig.GraphUrl != null)
                {
                    var serializer = new NewtonsoftJsonSerializer();
                    var graphQLClient = new GraphQLHttpClient(GlobalVariables.ShopifyConfig.GraphUrl, serializer);
                    graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", GlobalVariables.ShopifyConfig.ApiAccessToken);

                    return await graphQLClient.SendQueryAsync<JObject>(query);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<IGraphQLResponse?> SendMutation(GraphQLRequest request)
        {
            try
            {
                if (GlobalVariables.ShopifyConfig != null && GlobalVariables.ShopifyConfig.GraphUrl != null)
                {
                    var serializer = new NewtonsoftJsonSerializer();
                    var graphQLClient = new GraphQLHttpClient(GlobalVariables.ShopifyConfig.GraphUrl, serializer);
                    graphQLClient.HttpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", GlobalVariables.ShopifyConfig.ApiAccessToken);

                    return await graphQLClient.SendMutationAsync<JObject>(request);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
