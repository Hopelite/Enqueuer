using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence.Models;
using Enqueuer.Persistence.Repositories;
using Enqueuer.Services.Interfaces;
using User = Enqueuer.Persistence.Models.User;

namespace Enqueuer.Services
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private readonly IRepository<User> userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository"><see cref="IRepository{T}"/> with <see cref="User"/> entities.</param>
        public UserService(IRepository<User> userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <inheritdoc/>
        public async Task<User> GetNewOrExistingUserAsync(Telegram.Bot.Types.User telegramUser)
        {
            var user = this.GetUserByUserId(telegramUser.Id);

            if (user is null)
            {
                await this.userRepository.AddAsync(telegramUser);
                return this.GetUserByUserId(telegramUser.Id);
            }

            return user;
        }

        /// <inheritdoc/>
        public User GetUserByUserId(long userId)
        {
            return this.userRepository.GetAll()
                    .FirstOrDefault(user => user.UserId == userId);
        }

        /// <inheritdoc/>
        public IEnumerable<Chat> GetUserChats(long userId)
        {
            var result = this.userRepository.GetAll()
                .FirstOrDefault(user => user.UserId == userId);
            return result?.Chats;
        }
    }
}
