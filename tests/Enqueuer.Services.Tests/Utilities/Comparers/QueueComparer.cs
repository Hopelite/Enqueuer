// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using Enqueuer.Persistence.Models;

// namespace Enqueuer.Services.Tests.Utilities.Comparers
// {
//     internal class QueueComparer : IEqualityComparer<Queue>
//     {
//         /// <inheritdoc/>
//         public bool Equals(Queue x, Queue y)
//         {
//             return (x, y) switch
//             {
//                 (_, null) => false,
//                 (null, _) => false,
//                 _ => object.Equals(x.Name, y.Name)
//                 && x.ChatId == y.ChatId
//                 && x?.CreatorId == y?.CreatorId
//             };
//         }

//         /// <inheritdoc/>
//         public int GetHashCode([DisallowNull] Queue obj)
//         {
//             return obj.GetHashCode();
//         }
//     }
// }
