// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using Enqueuer.Persistence.Models;

// namespace Enqueuer.Services.Tests.Utilities.Comparers
// {
//     /// <inheritdoc/>
//     public class ChatComparer : IEqualityComparer<Chat>
//     {
//         /// <inheritdoc/>
//         public bool Equals(Chat x, Chat y)
//         {
//             return (x, y) switch
//             {
//                 (_, null) => false,
//                 (null, _) => false,
//                 _ => x.ChatId == y.ChatId
//             };
//         }

//         /// <inheritdoc/>
//         public int GetHashCode([DisallowNull] Chat obj)
//         {
//             return obj.GetHashCode();
//         }
//     }
// }
