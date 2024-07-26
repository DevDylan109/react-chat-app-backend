using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using react_chat_app_backend.Models;

//using react_chat_app_backend.Entities;

namespace react_chat_app_backend.Context;

public partial class AppDbContext : DbContext
{
    public DbSet<MessageData> Messages { get; set; }
    
    public string DbPath { get; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
