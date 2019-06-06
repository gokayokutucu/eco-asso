using Microsoft.EntityFrameworkCore;
using EcoAsso.Models;
using System.Collections.Generic;

namespace EcoAsso.Context {
    public class YeryuzuContext : DbContext {
	readonly string _directory;
	public YeryuzuContext(string directory)
	{
	    _directory = directory;
	}
	public DbSet<Person> People { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_directory}yeryuzu.db");
        }
    }
}
