using System.Linq;
using System.Threading.Tasks;
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
        public async Task<User> GetNewOrExistingUser(Telegram.Bot.Types.User telegramUser)
        {
            var user = this.userRepository.GetAll()
                .FirstOrDefault(user => user.UserId == telegramUser.Id);

            if (user is null)
            {
                await this.userRepository.AddAsync(telegramUser);
                return telegramUser;
            }

            return user;
        }
    }
}
