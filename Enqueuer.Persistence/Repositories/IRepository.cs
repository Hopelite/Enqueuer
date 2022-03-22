using System.Linq;
using System.Threading.Tasks;

namespace Enqueuer.Persistence.Repositories
{
    /// <summary>
    /// Contains basic repository operations.
    /// </summary>
    /// <typeparam name="T">Type of entities in repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity with specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        /// <returns><typeparamref name="T"/> with specified <paramref name="id"/> if exists; false otherwise.</returns>
        public T Get(int id);

        /// <summary>
        /// Gets all entities in repository.
        /// </summary>
        /// <returns>Query to all entities in repository.</returns>
        public IQueryable<T> GetAll();

        /// <summary>
        /// Adds <paramref name="entity"/> to database.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task AddAsync(T entity);

        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        /// <param name="entity">Entity update.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes entity with specified <paramref name="id"/> from database.
        /// </summary>
        /// <param name="id">ID of entity to delete.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task DeleteAsync(int id);

        /// <summary>
        /// Deletes <paramref name="entity"/> from database.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <returns>Awaitable <see cref="Task"/>.</returns>
        public Task DeleteAsync(T entity);
    }
}
