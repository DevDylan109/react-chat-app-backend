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
    
    // public AppDbContext()
    // {
    //     var folder = Environment.SpecialFolder.LocalApplicationData;
    //     var path = Environment.GetFolderPath(folder);
    //     DbPath = Path.Join(path, "blogging.db");
    // }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlite($"Data Source=Application.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        // modelBuilder.Entity<MessageData>().HasData(
        //     new MessageData { ReceiverId = "test1", SenderId = "test2", Text = "Hello World" });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
