using GraphQL;
using GraphQL.Client.Abstractions;
using Newtonsoft.Json;
using OMNI.Shopify.DataModels;
using Shopify;

namespace OMNI.Shopify.BusinessLogic
{
    public class ShopifyEvents
    {
        static string OrderEventsQuery = @"
                query OrderEvents(
                  $first: Int!
                  $after: String
                  $eventsQuery: String!
                ) {
                  events(
                    first: $first
                    after: $after
                    query: $eventsQuery
                    sortKey: CREATED_AT
                    reverse: false
                  ) {
                    edges {
                      node {
                        id
                        message
                        createdAt
                        __typename
                        ... on BasicEvent {
                          action
                          subjectId
                          subjectType
                        }
                        ... on CommentEvent {
                          rawMessage
                        }
                      }
                    }
                    pageInfo {
                      hasNextPage
                      endCursor
                    }
                  }
                }";

        public static async Task<List<EventNode>> GetOrderEventsSequentially(DateTimeOffset fromDate, int pageSize = 50)
        {
            var allEvents = new List<EventNode>();
            string afterCursor = null;
            string eventsQuery = $"subject_type:'ORDER' AND created_at:>={fromDate:yyyy-MM-ddTHH:mm:ssZ}";

            do
            {
                var request = new GraphQLRequest
                {
                    Query = OrderEventsQuery,
                    Variables = new
                    {
                        first = pageSize,
                        after = afterCursor,
                        eventsQuery
                    }
                };

                var response = await GraphAPI.QueryAsync(request);
                var events = JsonConvert.DeserializeObject<EventsResponse>(JsonConvert.SerializeObject(response.Data))?.Events ?? new();

                foreach (var edge in events.Edges)
                {
                    var node = edge.Node;
                    allEvents.Add(node);
                }

                afterCursor = events.PageInfo.HasNextPage ? events?.PageInfo?.EndCursor?? null : null;

            } while (!string.IsNullOrEmpty(afterCursor));

            return allEvents.Count > 0 ? allEvents.OrderByDescending(e => e.CreatedAt).ToList() : new List<EventNode>();
        }
    }
}


// Filter ONLY this order
//if (node.__typename == "BasicEvent")  // &&  node.SubjectId == orderGid
//{
//    allEvents.Add(node);
//}
//else if (node.__typename == "CommentEvent")
//{
//    // Already ORDER-scoped by query
//    allEvents.Add(node);
//}