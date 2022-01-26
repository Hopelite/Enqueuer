using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Enqueuer.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class.
        /// </summary>
        /// <param name="context"></param>
        public Repository(EnqueuerContext context)
        {
            this.Context = context;
            this.Entities = this.Context.Set<T>();
        }

        /// <summary>
        /// <see cref="DbContext"/> repository operates with.
        /// </summary>
        protected DbContext Context { get; set; }

        /// <summary>
        /// Entities repository operates with.
        /// </summary>
        protected DbSet<T> Entities { get; set; }

        /// <inheritdoc/>
        public virtual T Get(int id)
        {
            return Entities.Find(id);
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> GetAll()
        {
            return this.Entities;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="entity"/> is null.</exception>
        public virtual async Task AddAsync(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await this.Entities.AddAsync(entity);
            await this.Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="entity"/> is null.</exception>
        public virtual async Task UpdateAsync(T entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            this.Entities.Update(entity);
            await this.Context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(int id)
        {
            var entity = await this.Entities.FindAsync(id);
            await this.DeleteAsync(entity);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(T entity)
        {
            this.Entities.Remove(entity);
            await this.Context.SaveChangesAsync();
        }
    }
}
