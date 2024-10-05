namespace BackendChat.Responses
{
    namespace BackendChat.Responses
    {
        public record SuccessResponse(string Message)
        {
            public string Message { get; init; } = Message;
        }

        public record FailureResponse(string Message)
        {
            public string Message { get; init; } = Message;
        }
    }

}
