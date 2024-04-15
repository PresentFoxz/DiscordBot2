using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace TextCommandFramework
{
    public class BloggingContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        public BotContext CreateDbContext(string[] args)
        {
            Debug.WriteLine("Using design time context builder")
                ;
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            path = System.IO.Path.Join(path, "bot.db");

            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseSqlite($"Data Source={path}");

            return new BotContext(optionsBuilder.Options);
        }
    }
}
