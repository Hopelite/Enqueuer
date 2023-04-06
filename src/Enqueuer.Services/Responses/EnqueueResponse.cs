using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Responses;

public record EnqueueResponse(Queue Queue, int Position);
