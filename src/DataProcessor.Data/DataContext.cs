using DataProcessor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataProcessor.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<ProcessedReport> ProcessedReports => Set<ProcessedReport>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SettlementUpload> SettlementUploads => Set<SettlementUpload>();
    public DbSet<SettlementOrder> SettlementOrders => Set<SettlementOrder>();
    public DbSet<SettlementOrderItem> SettlementOrderItems => Set<SettlementOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Cnpj)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Cnpj)
            .IsRequired()
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(160);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<SettlementUpload>()
            .Property(u => u.AccountType)
            .HasMaxLength(80);

        modelBuilder.Entity<SettlementUpload>()
            .Property(u => u.UserCnpj)
            .HasMaxLength(20);

        modelBuilder.Entity<SettlementOrder>()
            .HasOne(o => o.SettlementUpload)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.SettlementUploadId);

        modelBuilder.Entity<SettlementOrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.SettlementOrderId);
    }
}
