namespace Cognitx.McpServer.Models
{
    public sealed record TodoCandidatesResponse(
        List<Todo> Candidates
        )
    {
        public int Count => Candidates.Count;
    }
}