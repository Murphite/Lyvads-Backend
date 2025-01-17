using Lyvads.Domain.Enums;
using Lyvads.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Lyvads.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

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
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Withdrawal> Withdrawals { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ExclusiveDeal> ExclusiveDeals { get; set; }
    public DbSet<VerificationRecord> VerificationRecords { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Charge> Charges { get; set; }
    public DbSet<ChargeTransaction> ChargeTransactions { get; set; }
    public DbSet<UserAd> UserAds { get; set; }
    public DbSet<Dispute> Disputes { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<AdminPermission> AdminPermissions { get; set; }
    public DbSet<AdminRole> AdminRoles { get; set; }
    public DbSet<Impression> Impressions { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<RegularUser> RegularUsers { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<Rate> Rates { get; set; }
    public DbSet<CardAuthorization> CardAuthorizations { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       // optionsBuilder.UseSqlServer("Data Source=SQL8011.site4now.net;Initial Catalog=db_ab085c_lyvadsdb;User Id=db_ab085c_lyvadsdb_admin;Password=Lyvads@123");

        modelBuilder.Entity<AdminRole>()
        .Property(r => r.RoleName)
        .HasConversion<string>();

        //modelBuilder.Entity<AdminRole>()
        //    .HasMany(r => r.AdminPermissions)
        //    .WithOne()
        //    .HasForeignKey(p => p.AdminRoleId);

        modelBuilder.Entity<RegularUser>()
        .HasOne(r => r.ApplicationUser)
        .WithOne(u => u.RegularUser)
        .HasForeignKey<RegularUser>(r => r.ApplicationUserId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Creator>()
        .HasOne(r => r.ApplicationUser)
        .WithOne(u => u.Creator)
        .HasForeignKey<Creator>(r => r.ApplicationUserId)
        .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Creator)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.MediaFiles)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RegularUser)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.RegularUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.RegularUser)
            .WithMany(u => u.Requests)
            .HasForeignKey(r => r.RegularUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VerificationRecord>()
            .HasIndex(v => new { v.Email, v.Code })
            .IsUnique();

        modelBuilder.Entity<Impression>()
           .HasOne(i => i.User)
           .WithMany()
           .HasForeignKey(i => i.UserId)
           .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Impression>()
            .HasOne(i => i.Creator)
            .WithMany()
            .HasForeignKey(i => i.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Exclusive Deals Configuration
        modelBuilder.Entity<ExclusiveDeal>()
            .HasOne(ed => ed.Creator)
            .WithMany(c => c.ExclusiveDeals)
            .HasForeignKey(ed => ed.CreatorId);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.ApplicationUser)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Content)
            .WithMany(c => c.Likes)
            .HasForeignKey(l => l.ContentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.ApplicationUser)
            .WithMany()
            .HasForeignKey(f => f.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Creator)
            .WithMany()
            .HasForeignKey(f => f.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(user => user.Creator)
            .WithOne(creator => creator.ApplicationUser)
            .HasForeignKey<Creator>(creator => creator.ApplicationUserId);
        modelBuilder.Entity<Follow>();

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(user => user.RegularUser)
            .WithOne(regularUser => regularUser.ApplicationUser)
            .HasForeignKey<RegularUser>(regularUser => regularUser.ApplicationUserId);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(user => user.Creator)
            .WithOne(creator => creator.ApplicationUser)
            .HasForeignKey<Creator>(creator => creator.ApplicationUserId);

        modelBuilder.Entity<ApplicationUser>()
           .HasOne(user => user.Admin)
           .WithOne(admin => admin.ApplicationUser)
           .HasForeignKey<Admin>(admin => admin.ApplicationUserId);

        modelBuilder.Entity<ApplicationUser>()
           .HasOne(user => user.SuperAdmin)
           .WithOne(superAdmin => superAdmin.ApplicationUser)
           .HasForeignKey<SuperAdmin>(superAdmin => superAdmin.ApplicationUserId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
           .HasIndex(t => t.TrxRef)
           .IsUnique()
           .HasDatabaseName("IX_Transaction_TrxRef");

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Request)
            .WithMany(r => r.Transactions)
            .HasForeignKey(t => t.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Request>()
        .HasOne(r => r.Wallet) // Request has one Wallet
        .WithMany(w => w.Requests) // Wallet has many Requests
        .HasForeignKey(r => r.WalletId);

        modelBuilder.Entity<Wallet>()
       .HasMany(w => w.Requests)
       .WithOne(r => r.Wallet)
       .HasForeignKey(r => r.WalletId)
       .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favorite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
           .HasOne(t => t.ApplicationUser)
           .WithMany() // Assuming ApplicationUser doesn't have a collection of Transactions
           .HasForeignKey(t => t.ApplicationUserId);

    }

}
