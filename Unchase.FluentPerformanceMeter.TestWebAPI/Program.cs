using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Unchase.FluentPerformanceMeter.TestWebAPI
{
    /// <summary>
    /// Startup program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create WebHost.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns><see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
