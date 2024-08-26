using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Models;

//using react_chat_app_backend.Entities;

namespace react_chat_app_backend.Context;

public partial class AppDbContext : DbContext
{
    public DbSet<ChatMessage> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserFriendShip> UserFriendShips { get; set; }
    
    public string DbPath { get; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFriendShip>()
            .HasKey(ur => new { ur.UserId, ur.RelatedUserId });

        modelBuilder.Entity<UserFriendShip>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserFriendShips)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserFriendShip>()
            .HasOne(ur => ur.RelatedUser)
            .WithMany()
            .HasForeignKey(ur => ur.RelatedUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
