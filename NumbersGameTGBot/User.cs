using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class NumberGameTGBotContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public string DbPath { get; }

    public NumberGameTGBotContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "NumbersGameTGBot.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

public class User
{
    [Required]
    public long Id { get; set; }
    [Required]
    public uint Streak {  get; set; }
    [Required]
    public DateTime LastMessageDate {  get; set; }
}
