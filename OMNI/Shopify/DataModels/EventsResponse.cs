namespace OMNI.Shopify.DataModels
{
    public class EventsResponse
    {
        public EventsConnection Events { get; set; }
    }

    public class EventsConnection
    {
        public List<EventEdge> Edges { get; set; }
        public PageInfo PageInfo { get; set; }
    }

    public class EventEdge
    {
        public string Cursor { get; set; }
        public EventNode Node { get; set; }
    }

    public class PageInfo
    {
        public bool HasNextPage { get; set; }
        public string EndCursor { get; set; }
    }

    public class EventNode
    {
        public string Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string __typename { get; set; }

        // BasicEvent
        public string Action { get; set; }
        public string SubjectId { get; set; }
        public string SubjectType { get; set; }

        // CommentEvent
        public string RawMessage { get; set; }
    }

}
