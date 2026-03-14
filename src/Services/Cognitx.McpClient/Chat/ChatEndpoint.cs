namespace Cognitx.McpClient.Chat
{
    public static class ChatEndpoint
    {
        private static async Task<IResult> HandleAsync(
            ChatRequest request,
            IAgentService agentService,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await agentService.ProcessAsync(request, cancellationToken);

                return Results.Ok(response);
            }
            catch (OperationCanceledException)
            {
                return Results.StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception)
            {
                return Results.Problem(
                    detail: "An error occurred while processing your request.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoint)
        {
            var group = endpoint.MapGroup("/api")
                .WithTags("Chat");

            group.MapPost("/chat", HandleAsync)
                .WithName("Chat")
                .WithDescription("Process a user message and return the agent's response")
                .Produces<ChatResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status408RequestTimeout)
                .Produces(StatusCodes.Status500InternalServerError);

            return endpoint;
        }
    }
}