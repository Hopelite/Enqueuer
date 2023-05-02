namespace Enqueuer.Service.API.Services.Types;

public class PutActionResponse
{
    public PutAction Action { get; set; }

    public static implicit operator PutAction(PutActionResponse response) => response.Action;

    public static implicit operator PutActionResponse(PutAction action) => new() { Action = action };
}

public class PutActionResponse<T> : PutActionResponse
{
    public T Resource { get; set; } = default!;
}
