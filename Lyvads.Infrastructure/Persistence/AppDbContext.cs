using Lyvads.Domain.Enums;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lyvads.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WaitlistEntry> WaitlistEntries { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<SuperAdmin> SuperAdmins { get; set; }
    public DbSet<Creator> Creators { get; set; }
    public DbSet<RegularUser> RegularUsers { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Withdrawal> Withdrawals { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Deal> Deals { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ExclusiveDeal> ExclusiveDeals { get; set; }
    public DbSet<VerificationRecord> VerificationRecords { get; set; }
    public DbSet<CollaborationRequest> CollaborationRequests { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Custom configurations can be added here
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(e => e.Notifications)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Contents)
            .WithOne(c => c.Creator)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Deals)
            .WithOne(d => d.Creator)
            .HasForeignKey(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Posts)
            .WithOne(p => p.Creator)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Likes)
            .WithOne(l => l.Post)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Wallet>()
            .HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasMany(r => r.Deals)
            .WithOne(d => d.Request)
            .HasForeignKey(d => d.RequestId);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.User)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Deal>()
            .HasOne(d => d.Request)
            .WithMany(r => r.Deals)
            .HasForeignKey(d => d.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Deal)
            .WithMany(d => d.Transactions)
            .HasForeignKey(t => t.DealId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Request)
            .WithMany(r => r.Transactions)
            .HasForeignKey(t => t.RequestId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.FromWallet)
            .WithMany()
            .HasForeignKey(t => t.FromWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ToWallet)
            .WithMany()
            .HasForeignKey(t => t.ToWalletId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ExclusiveDeal>()
            .HasOne(ed => ed.Creator)
            .WithMany(c => c.ExclusiveDeals)
            .HasForeignKey(ed => ed.CreatorId);

        modelBuilder.Entity<ApplicationUser>()
        .HasOne(u => u.Wallet)
        .WithOne(w => w.ApplicationUser)
        .HasForeignKey<Wallet>(w => w.ApplicationUserId);

        modelBuilder.Entity<ApplicationUser>()
        .Property(u => u.WalletId)
        .HasDefaultValue(Guid.NewGuid().ToString());


    }
}
