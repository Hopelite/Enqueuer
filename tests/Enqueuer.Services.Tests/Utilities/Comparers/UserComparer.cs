using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Tests.Utilities.Comparers
{
    /// <inheritdoc/>
    public class UserComparer : IEqualityComparer<User>
    {
        /// <inheritdoc/>
        public bool Equals(User x, User y)
        {
            return (x, y) switch
            {
                (_, null) => false,
                (null, _) => false,
                _ => x.UserId == y.UserId
                && x.FirstName.Equals(y.FirstName)
                && x.LastName.Equals(y.LastName)
            };
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] User obj)
        {
            return obj.GetHashCode();
        }
    }
}
