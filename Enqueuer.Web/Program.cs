using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Enqueuer.Web
{
    /// <summary>
    /// Represents application starting point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application starting point.
        /// </summary>
        /// <param name="args">Input console parameters.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates application host builder.
        /// </summary>
        /// <param name="args">Input console parameters.</param>
        /// <returns>Configured <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}