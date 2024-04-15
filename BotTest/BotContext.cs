using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TextCommandFramework.Models;

namespace TextCommandFramework;

public class BotContext : DbContext
{
    public DbSet<Profile> Profile { get; set; }
    public DbSet<UserList> List { get; set; }
    public DbSet<Weapon> Weapon { get; set; }

    public BotContext(DbContextOptions<BotContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 1, Name = "Fists", Damage = 1, Value = 0 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 2, Name = "Sword", Damage = 5, Value = 2 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 3, Name = "Spear", Damage = 5, Value = 3 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 4, Name = "Axe", Damage = 7, Value = 5 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 5, Name = "GreatSword", Damage = 12, Value = 8 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 6, Name = "Rock", Damage = 200000, Value = 10000 });
        modelBuilder.Entity<Weapon>().HasData(new Weapon { Id = 7, Name = "Dagger", Damage = 3, Value = 4 });
    }

}