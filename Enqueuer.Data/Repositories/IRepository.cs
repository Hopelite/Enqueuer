﻿using System.Linq;
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
        /// <returns></returns>
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
        public Task AddAsync(T entity);

        /// <summary>
        /// Updates <paramref name="entity"/> in database.
        /// </summary>
        /// <param name="entity">Entity update.</param>
        public Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes <paramref name="entity"/> with specified ID from database.
        /// </summary>
        /// <param name="id">ID of entity to delete.</param>
        public Task DeleteAsync(int id);

        /// <summary>
        /// Deletes <paramref name="entity"/> from database.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        public Task DeleteAsync(T entity);
    }
}
