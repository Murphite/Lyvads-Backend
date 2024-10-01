using Lyvads.Domain.Enums;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Lyvads.Infrastructure.Repositories;

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
    public DbSet<Collaboration> Collaborations { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Charge> Charges { get; set; }
    public DbSet<ChargeTransaction> ChargeTransactions { get; set; }
    public DbSet<UserAd> UserAds { get; set; }
    public DbSet<Dispute> Disputes { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser Configuration
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(e => e.Notifications)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.ApplicationUser)
            .HasForeignKey<Wallet>(w => w.ApplicationUserId);

        // Creator Configuration
        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Contents)
            .WithOne(c => c.Creator)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Deals)
            .WithOne(d => d.Creator)
            .HasForeignKey(d => d.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Creator>()
            .HasMany(c => c.Posts)
            .WithOne(p => p.Creator)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Post Configuration
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

        // Request and Deal Configuration
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

        modelBuilder.Entity<Creator>()
         .HasMany(c => c.Collaborations)
         .WithOne(collab => collab.Creator)
         .HasForeignKey(collab => collab.CreatorId);

        // Ignore the conflicting property 'Request.User' 
        modelBuilder.Entity<Request>()
            .Ignore(r => r.User);

        // Configure the relationship between 'Request' and 'RegularUser'
        modelBuilder.Entity<Request>()
            .HasOne(r => r.RegularUser)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VerificationRecord>()
            .HasIndex(v => new { v.Email, v.Code })
            .IsUnique();


        // Wallet and Transaction Configuration
        // Transaction has a FromWallet (origin) and a ToWallet (destination)
        //modelBuilder.Entity<Transaction>()
        //    .HasOne(t => t.FromWallet)
        //    .WithMany(w => w.FromTransactions)
        //    .HasForeignKey(t => t.FromWalletId)
        //    .OnDelete(DeleteBehavior.Restrict);

        //modelBuilder.Entity<Transaction>()
        //    .HasOne(t => t.ToWallet)
        //    .WithMany(w => w.ToTransactions) 
        //    .HasForeignKey(t => t.ToWalletId)
        //    .OnDelete(DeleteBehavior.Restrict);

        // Ensure Wallet entity has distinct navigation properties for each relationship
        //modelBuilder.Entity<Wallet>()
        //    .HasMany(w => w.FromTransactions)
        //    .WithOne(t => t.FromWallet)
        //    .HasForeignKey(t => t.FromWalletId);

        //modelBuilder.Entity<Wallet>()
        //    .HasMany(w => w.ToTransactions)
        //    .WithOne(t => t.ToWallet)
        //    .HasForeignKey(t => t.ToWalletId);


        // Exclusive Deals Configuration
        modelBuilder.Entity<ExclusiveDeal>()
            .HasOne(ed => ed.Creator)
            .WithMany(c => c.ExclusiveDeals)
            .HasForeignKey(ed => ed.CreatorId);

    }

}
