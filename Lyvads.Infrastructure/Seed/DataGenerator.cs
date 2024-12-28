

using Bogus;
using Bogus.DataSets;
using Lyvads.Domain.Constants;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Enums;
using Lyvads.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Lyvads.Infrastructure.Seed;

public class DataGenerator
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public DataGenerator(UserManager<ApplicationUser> userManager, AppDbContext context, IConfiguration config)
    {
        _userManager = userManager;
        _context = context;
        _config = config;
        Randomizer.Seed = new Random(123);
    }

    public async Task Run()
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                //await GenerateDisputes(10);
                await GenerateCreators(20);
                await GenerateRegularUsers(20);
                await GenerateAdmins(10);
                await GeneratePosts(20);
                await GenerateComments(20);
                //await GenerateContent(20);
                await GenerateLikes(10);
                await GenerateRates(10);
                await GenerateExclusiveDeals(10);
                await GenerateWallets(10);
                await GenerateFavorites(10);
                await GenerateFollows(10);
                await GenerateImpressions(20);
                await GenerateMedias(10);
                await GeneratePromotions(10);           
                await GenerateRequests(10);                
               // await GenerateDisputes(10);
                await GenerateTransactions(20);                
                await GenerateCharges(10);
                await GenerateChargeTransactions(10);
                //await GenerateTransfers(10);
                await GenerateUserAds(10);    
                await GenerateSuperAdmins(10);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error in DataGenerator: {ex.Message}");
                Console.WriteLine($"Error during seeding: {ex.Message}");
                throw;                
            }
        }
    }

    private async Task GeneratePosts(int count = 10)
    {
        var posts = new List<Post>();
        var postFaker = new Faker<Post>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(p => p.Caption, f => f.Lorem.Sentence(5))
            .RuleFor(p => p.Location, f => f.Address.City())
            .RuleFor(p => p.IsDeleted, f => f.Random.Bool())
            .RuleFor(p => p.Visibility, f => f.PickRandom<PostVisibility>())
            .RuleFor(p => p.CreatedAt, f => f.Date.PastOffset(1))
            .RuleFor(p => p.UpdatedAt, f => f.Date.RecentOffset(30))
            .RuleFor(p => p.PostStatus, f => f.PickRandom<PostStatus>());

        // Fetch existing Creator IDs to assign to posts
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        if (!creatorIds.Any())
        {
            throw new InvalidOperationException("No creators found in the database.");
        }

        // Generate posts and assign a valid CreatorId
        for (var i = 0; i < count; i++)
        {
            var post = postFaker.Generate();
            var randomIndex = new Faker().Random.Int(0, creatorIds.Count - 1); // Generate a random index using a new Faker instance
            post.CreatorId = creatorIds[randomIndex]; // Assign random existing CreatorId using the generated index
            posts.Add(post);
        }

        // Add the generated posts to the database
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateComments(int count = 10)
    {
        // Ensure there are posts available to assign to comments
        var posts = await _context.Posts.ToListAsync();

        if (posts.Count == 0)
        {
            throw new Exception("No posts found in the database to associate with comments.");
        }

        var comments = new List<Comment>();
        var commentFaker = new Faker<Comment>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString()) // Generate a truly unique GUID for each comment
            .RuleFor(c => c.PostId, f => f.PickRandom(posts).Id) // Pick a random PostId from the existing posts
            .RuleFor(c => c.Content, f => f.Lorem.Paragraph())
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(1))
            .RuleFor(c => c.UpdatedAt, f => f.Date.RecentOffset(30))
            .RuleFor(c => c.CommentBy, f => f.Name.FullName())
            .RuleFor(c => c.IsDeleted, f => f.Random.Bool());

        // Generate comments using Faker
        for (var i = 0; i < count; i++)
        {
            var comment = commentFaker.Generate();
            comments.Add(comment);
        }

        // Add the generated comments to the database
        await _context.Comments.AddRangeAsync(comments);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateContent(int count = 10)
    {
        // Ensure there are users, creators, and requests available to associate with content
        var users = await _context.Users.ToListAsync();  // Get all users
        var creators = await _context.Creators.ToListAsync();  // Get all creators
        var requests = await _context.Requests.ToListAsync();  // Get all requests

        if (users.Count == 0 || creators.Count == 0 || requests.Count == 0)
        {
            throw new Exception("No users, creators, or requests found in the database to associate with content.");
        }

        var contentList = new List<Content>();
        var contentFaker = new Faker<Content>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString()) // Generate a truly unique GUID for each content
            .RuleFor(c => c.Url, f => f.Internet.Url()) // Generate a random URL for the content
            .RuleFor(c => c.HasWatermark, f => f.Random.Bool()) // Randomly determine if content has a watermark
            .RuleFor(c => c.UserId, f => f.PickRandom(users).Id) // Pick a random UserId from the existing users
            .RuleFor(c => c.Creator, f => f.PickRandom(creators)) // Pick a random Creator from the existing creators
            .RuleFor(c => c.RequestId, f => f.PickRandom(requests).Id) // Pick a random RequestId from the existing requests
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset(1)) // Random date in the past for CreatedAt
            .RuleFor(c => c.Likes, f => new List<Like>()); // Initialize an empty list of Likes (you can add likes later if necessary)

        // Generate content using Faker
        for (var i = 0; i < count; i++)
        {
            var content = contentFaker.Generate();
            contentList.Add(content);
        }

        // Add the generated content to the database
        await _context.Contents.AddRangeAsync(contentList);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateLikes(int count = 10)
    {
        // Retrieve some existing users, posts, comments, and contents
        var users = await _context.Users.ToListAsync();  // Get all users
        var posts = await _context.Posts.ToListAsync();  // Get all posts
        var comments = await _context.Comments.ToListAsync();  // Get all comments
        //var contents = await _context.Contents.ToListAsync();  // Get all contents

        if (users.Count == 0 || posts.Count == 0 || comments.Count == 0 )
        {
            throw new Exception("No users, posts, comments, or contents found in the database to associate with likes.");
        }

        var likes = new List<Like>();
        var likeFaker = new Faker<Like>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(l => l.UserId, f => f.PickRandom(users).Id) // Pick a random UserId from the existing users
            .RuleFor(l => l.CommentId, f => f.PickRandom(comments).Id) // Pick a random CommentId from the existing comments
            .RuleFor(l => l.PostId, f => f.PickRandom(posts).Id) // Pick a random PostId from the existing posts
            //.RuleFor(l => l.ContentId, f => f.PickRandom(contents).Id) // Pick a random ContentId from the existing contents
            .RuleFor(l => l.LikedBy, f => f.Name.FullName())
            .RuleFor(l => l.CreatedAt, f => f.Date.PastOffset(1));

        // Generate likes using Faker
        for (var i = 0; i < count; i++)
        {
            var like = likeFaker.Generate();
            likes.Add(like);
        }

        // Add the generated likes to the database
        await _context.Likes.AddRangeAsync(likes);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateCharges(int count = 10)
    {
        var charges = new List<Charge>();
        var chargeFaker = new Faker<Charge>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(c => c.ChargeName, f => f.Commerce.ProductName())
            .RuleFor(c => c.Percentage, f => f.Finance.Amount(0, 100))
            .RuleFor(c => c.MinAmount, f => f.Finance.Amount(1, 50))
            .RuleFor(c => c.MaxAmount, f => f.Finance.Amount(51, 100))
            .RuleFor(c => c.Status, f => f.PickRandom<ChargeStatus>())
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(c => c.UpdatedAt, f => f.Date.RecentOffset());

        // Generate charges using Faker
        for (var i = 0; i < count; i++)
        {
            var charge = chargeFaker.Generate();
            charges.Add(charge);
        }

        // Add the generated charges to the database
        await _context.Charges.AddRangeAsync(charges);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateChargeTransactions(int count = 10)
    {
        // Retrieve existing ApplicationUser IDs and Transaction IDs
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
        var transactionIds = await _context.Transactions.Select(t => t.Id).ToListAsync();

        if (!userIds.Any())
        {
            throw new InvalidOperationException("No users found in the database to associate with charge transactions.");
        }

        if (!transactionIds.Any())
        {
            throw new InvalidOperationException("No transactions found in the database to associate with charge transactions.");
        }

        var chargeTransactions = new List<ChargeTransaction>();
        var chargeTransactionFaker = new Faker<ChargeTransaction>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(c => c.ChargeName, f => f.Commerce.ProductName())
            .RuleFor(c => c.Amount, f => f.Finance.Amount(10, 100))
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.Status, f => f.PickRandom<CTransStatus>())
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(c => c.UpdatedAt, f => f.Date.RecentOffset())
            .RuleFor(c => c.ApplicationUserId, f => f.PickRandom(userIds)) // Pick valid ApplicationUserId
            .RuleFor(c => c.TransactionId, f => f.PickRandom(transactionIds)); // Pick valid TransactionId

        // Generate charge transactions using Faker
        for (var i = 0; i < count; i++)
        {
            var chargeTransaction = chargeTransactionFaker.Generate();
            chargeTransactions.Add(chargeTransaction);
        }

        // Add the generated charge transactions to the database
        await _context.ChargeTransactions.AddRangeAsync(chargeTransactions);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateDisputes(int count = 10)
    {
        var disputes = new List<Dispute>();

        // Retrieve valid RegularUserIds from RegularUsers table
        var validRegularUserIds = await _context.RegularUsers
            .Select(r => r.Id) // Use RegularUser's Id for the foreign key constraint
            .ToListAsync();

        // Retrieve valid CreatorIds from Creators table
        var creatorIds = await _context.Creators
            .Select(c => c.Id) // Use Creator's Id for the foreign key constraint
            .ToListAsync();

        // Retrieve the ApplicationUserIds from AspNetUsers table
        var validApplicationUserIds = await _context.Users
            .Where(u => validRegularUserIds.Contains(u.Id)) // Only get users linked to regular users
            .Select(u => u.Id)
            .ToListAsync();

        // Ensure there are valid RegularUsers, Creators, and ApplicationUsers
        if (!validRegularUserIds.Any() || !creatorIds.Any() || !validApplicationUserIds.Any())
        {
            throw new InvalidOperationException("No RegularUsers, Creators, or ApplicationUsers exist in the database to associate disputes with.");
        }

        var disputeFaker = new Faker<Dispute>()
            .RuleFor(c => c.Id, (f, d) => Guid.NewGuid().ToString())
            .RuleFor(d => d.RequestId, (f, d) => Guid.NewGuid().ToString()) // Replace with valid RequestId logic if required
            .RuleFor(d => d.RegularUserId, f => f.PickRandom(validRegularUserIds)) // Assign valid RegularUserId
            .RuleFor(d => d.CreatorId, f => f.PickRandom(creatorIds)) // Assign valid CreatorId
            .RuleFor(d => d.ApplicationUserId, f => f.PickRandom(validApplicationUserIds)) // Pick a valid ApplicationUserId from AspNetUsers
            .RuleFor(d => d.Amount, (f, d) => f.Finance.Amount(50, 500))
            .RuleFor(d => d.DisputeMessage, (f, d) => f.Lorem.Paragraph())
            .RuleFor(d => d.Reason, (f, d) => f.PickRandom<DisputeReasons>())
            .RuleFor(d => d.Status, (f, d) => f.PickRandom<DisputeStatus>())
            .RuleFor(d => d.CreatedAt, (f, d) => f.Date.PastOffset())
            .RuleFor(d => d.UpdatedAt, (f, d) => f.Date.RecentOffset());

        // Generate disputes using Faker
        for (var i = 0; i < count; i++)
        {
            var dispute = disputeFaker.Generate();
            disputes.Add(dispute);
        }

        // Add the generated disputes to the database
        await _context.Disputes.AddRangeAsync(disputes);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateFavorites(int count = 10)
    {
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        var favoriteFaker = new Faker<Favorite>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(f => f.UserId, f => f.PickRandom(userIds))
            .RuleFor(f => f.CreatorId, f => f.PickRandom(creatorIds))
            .RuleFor(f => f.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(f => f.UpdatedAt, f => f.Date.RecentOffset());

        var favorites = favoriteFaker.Generate(count);

        await _context.Favorites.AddRangeAsync(favorites);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateFollows(int count = 10)
    {
        var follows = new List<Follow>();

        // Retrieve valid user IDs from the AspNetUsers table
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();

        // Retrieve valid Creator IDs from the Creators table
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        if (userIds.Count < 2 || creatorIds.Count == 0)
        {
            throw new InvalidOperationException("Not enough users or creators in the database to generate follows.");
        }

        // Use a random number generator instead of DbFunctions or database-bound logic
        var random = new Random();

        for (var i = 0; i < count; i++)
        {
            var userId = userIds[random.Next(userIds.Count)];
            var creatorId = creatorIds[random.Next(creatorIds.Count)];

            // Ensure the same user doesn't follow themselves
            while (userId == creatorId)
            {
                creatorId = creatorIds[random.Next(creatorIds.Count)];
            }

            follows.Add(new Follow
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = userId,
                CreatorId = creatorId,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-random.Next(1, 30)),
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        // Add the generated follows to the database
        await _context.Follows.AddRangeAsync(follows);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateImpressions(int count = 10)
    {
        var impressions = new List<Impression>();

        // Retrieve valid User IDs from the AspNetUsers table
        var userIds = await _context.Users.Select(u => u.Id).ToListAsync();

        // Retrieve valid Creator IDs from the Creators table
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        if (userIds.Count == 0 || creatorIds.Count == 0)
        {
            throw new InvalidOperationException("Not enough users or creators in the database to generate impressions.");
        }

        var impressionFaker = new Faker<Impression>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(i => i.UserId, f => f.Random.ListItem(userIds)) // Use valid User IDs
            .RuleFor(i => i.CreatorId, f => f.Random.ListItem(creatorIds)) // Use valid Creator IDs
            .RuleFor(i => i.ContentId, f => Guid.NewGuid().ToString()) // Generate random Content IDs
            .RuleFor(i => i.ContentType, f => f.PickRandom<ContentType>()) // Pick a random ContentType
            .RuleFor(i => i.ViewedAt, f => f.Date.RecentOffset())
            .RuleFor(i => i.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(i => i.UpdatedAt, f => f.Date.RecentOffset());

        // Generate impressions using Faker
        for (var i = 0; i < count; i++)
        {
            var impression = impressionFaker.Generate();
            impressions.Add(impression);
        }

        // Add the generated impressions to the database
        await _context.Impressions.AddRangeAsync(impressions);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateMedias(int count = 10)
    {
        var cloudinaryBaseUrl = "https://res.cloudinary.com/dvrghpls1/image/upload/";

        var medias = new List<Media>();

        // Retrieve valid Post IDs from the Posts table
        var postIds = await _context.Posts.Select(p => p.Id).ToListAsync();

        if (postIds.Count == 0)
        {
            throw new InvalidOperationException("No posts exist in the database to associate media with.");
        }

        var mediaFaker = new Faker<Media>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(m => m.PostId, f => f.Random.ListItem(postIds)) // Use valid Post IDs
            .RuleFor(m => m.Url, f => $"{cloudinaryBaseUrl}{f.Random.AlphaNumeric(10)}.jpg") // Cloudinary URL
            .RuleFor(m => m.FileType, f => f.System.FileExt()) // Simulate file type
            .RuleFor(m => m.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(m => m.UpdatedAt, f => f.Date.RecentOffset());

        // Generate media using Faker
        for (var i = 0; i < count; i++)
        {
            var media = mediaFaker.Generate();
            medias.Add(media);
        }

        // Add the generated media to the database
        await _context.Media.AddRangeAsync(medias);
        await _context.SaveChangesAsync();
    }

    private async Task GeneratePromotions(int count = 10)
    {
        var cloudinaryBaseUrl = "https://res.cloudinary.com/dvrghpls1/image/upload/";

        var promotions = new List<Promotion>();
        var promotionFaker = new Faker<Promotion>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.ShortDescription, f => f.Lorem.Sentence())
            .RuleFor(p => p.Price, f => f.Finance.Amount(10, 100))
            .RuleFor(p => p.MediaUrl, f => $"{cloudinaryBaseUrl}{f.Random.AlphaNumeric(10)}.jpg")
            .RuleFor(p => p.IsHidden, f => f.Random.Bool())
            .RuleFor(p => p.CreatedAt, f => f.Date.Past())
            .RuleFor(p => p.UpdatedAt, f => f.Date.Recent());

        // Generate promotions using Faker
        for (var i = 0; i < count; i++)
        {
            var promotion = promotionFaker.Generate();
            promotions.Add(promotion);
        }

        // Add the generated promotions to the database
        await _context.Promotions.AddRangeAsync(promotions);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateRequests(int count = 10)
    {
        var requests = new List<Request>();

        // Retrieve lists of user IDs for RegularUser and Creator from the database
        var regularUserIds = await _context.RegularUsers.Select(u => u.Id).ToListAsync();
        var creatorIds = await _context.Creators.Select(u => u.Id).ToListAsync();

        if (regularUserIds.Count == 0 || creatorIds.Count == 0)
        {
            throw new InvalidOperationException("No users available to associate with requests.");
        }

        var f = new Faker();

        var requestFaker = new Faker<Request>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(r => r.Script, f => f.Lorem.Paragraph())
            .RuleFor(r => r.RequestAmount, f => f.Finance.Amount(50, 500))
            .RuleFor(r => r.TotalAmount, f => f.Finance.Amount(500, 1000))
            .RuleFor(r => r.FastTrackFee, f => f.Finance.Amount(10, 50))
            .RuleFor(r => r.PaymentMethod, f => f.PickRandom<AppPaymentMethod>())
            .RuleFor(r => r.RequestType, f => f.Random.Word())
            .RuleFor(r => r.Status, f => f.PickRandom<RequestStatus>())
            .RuleFor(r => r.TransactionStatus, f => f.Random.Bool())
            .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(r => r.UpdatedAt, f => f.Date.RecentOffset())
            .RuleFor(r => r.VideoUrl, f => $"https://res.cloudinary.com/dvrghpls1/video/upload/v{f.Date.Recent().Year}/{f.Random.AlphaNumeric(10)}.mp4");

        // Generate requests using Faker
        for (var i = 0; i < count; i++)
        {
            var request = requestFaker.Generate();

            // Ensure every request has both RegularUserId and CreatorId
            request.RegularUserId = f.PickRandom(regularUserIds);
            request.CreatorId = f.PickRandom(creatorIds);

            requests.Add(request);
        }

        // Add the generated requests to the database
        await _context.Requests.AddRangeAsync(requests);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateUserAds(int count = 10)
    {
        var userAds = new List<UserAd>();
        var users = await _context.Users.ToListAsync(); // Retrieve all users from the database

        var userAdFaker = new Faker<UserAd>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Description, f => f.Lorem.Sentence())
            .RuleFor(u => u.Amount, f => f.Finance.Amount(10, 1000))
            .RuleFor(u => u.Status, f => f.PickRandom<UserAdStatus>())
            .RuleFor(u => u.ApplicationUserId, f => f.PickRandom(users).Id) // Assign an existing user's Id
            .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(u => u.UpdatedAt, f => f.Date.RecentOffset());

        // Generate user ads using Faker
        for (var i = 0; i < count; i++)
        {
            var userAd = userAdFaker.Generate();
            userAds.Add(userAd);
        }

        // Add the generated user ads to the database
        await _context.UserAds.AddRangeAsync(userAds);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateTransfers(int count = 10)
    {
        var transfers = new List<Transfer>();
        var transferFaker = new Faker<Transfer>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(t => t.UserId, f => f.Random.Guid().ToString())
            .RuleFor(t => t.Amount, f => f.Finance.Amount(10, 500))
            .RuleFor(t => t.TransferReference, f => f.Finance.TransactionType())
            .RuleFor(t => t.Status, f => f.PickRandom(new[] { "Pending", "Completed", "Failed" }))
            .RuleFor(t => t.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(t => t.UpdatedAt, f => f.Date.RecentOffset());

        // Generate transfers using Faker
        for (var i = 0; i < count; i++)
        {
            var transfer = transferFaker.Generate();
            transfers.Add(transfer);
        }

        // Add the generated transfers to the database
        await _context.Transfers.AddRangeAsync(transfers);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateWallets(int count = 10)
    {
        // Retrieve valid ApplicationUser IDs from the database
        var applicationUserIds = await _context.Users.Select(u => u.Id).ToListAsync();

        if (!applicationUserIds.Any())
        {
            throw new InvalidOperationException("No application users found in the database to associate with wallets.");
        }

        var wallets = new List<Wallet>();
        var walletFaker = new Faker<Wallet>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(w => w.Balance, f => f.Finance.Amount(100, 10000))
            .RuleFor(w => w.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(w => w.UpdatedAt, f => f.Date.RecentOffset())
            .RuleFor(w => w.ApplicationUserId, f => f.PickRandom(applicationUserIds)); // Assign valid ApplicationUserId

        // Generate wallets using Faker
        for (var i = 0; i < count; i++)
        {
            var wallet = walletFaker.Generate();
            wallets.Add(wallet);
        }

        // Add the generated wallets to the database
        await _context.Wallets.AddRangeAsync(wallets);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateWithdrawals(int count = 10)
    {
        var withdrawals = new List<Withdrawal>();
        var withdrawalFaker = new Faker<Withdrawal>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(w => w.UserId, f => f.Random.Guid().ToString())
            .RuleFor(w => w.Amount, f => f.Finance.Amount(50, 1000))
            .RuleFor(w => w.TransferReference, f => f.Finance.TransactionType())
            .RuleFor(w => w.Status, f => f.PickRandom<TransferStatus>())
            .RuleFor(w => w.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(w => w.UpdatedAt, f => f.Date.RecentOffset());

        // Generate withdrawals using Faker
        for (var i = 0; i < count; i++)
        {
            var withdrawal = withdrawalFaker.Generate();
            withdrawals.Add(withdrawal);
        }

        // Add the generated withdrawals to the database
        await _context.Withdrawals.AddRangeAsync(withdrawals);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateTransactions(int count = 10)
    {
        var transactions = new List<Transaction>();
        var transactionFaker = new Faker<Transaction>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(t => t.Name, f => f.Commerce.ProductName())
            .RuleFor(t => t.Amount, f => (int)f.Finance.Amount(10, 500))
            .RuleFor(t => t.TrxRef, f => $"{f.Finance.TransactionType()}_{Guid.NewGuid()}")
            .RuleFor(t => t.Email, f => f.Internet.Email())
            .RuleFor(t => t.Status, f => f.Random.Bool())
            .RuleFor(t => t.Type, f => f.PickRandom<TransactionType>())
            .RuleFor(t => t.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(t => t.UpdatedAt, f => f.Date.RecentOffset());

        // Generate transactions using Faker
        for (var i = 0; i < count; i++)
        {
            var transaction = transactionFaker.Generate();
            transactions.Add(transaction);
        }

        // Add the generated transactions to the database
        await _context.Transactions.AddRangeAsync(transactions);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateExclusiveDeals(int count = 10)
    {
        // Retrieve existing Creator IDs
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        if (!creatorIds.Any())
        {
            throw new InvalidOperationException("No creators found in the database to associate with exclusive deals.");
        }

        var exclusiveDeals = new List<ExclusiveDeal>();
        var exclusiveDealFaker = new Faker<ExclusiveDeal>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(e => e.Industry, f => f.PickRandom(new[] { "Entertainment", "Tech", "Healthcare", "Education", "Fashion", "Finance" }))
            .RuleFor(e => e.BrandName, f => f.Company.CompanyName())
            .RuleFor(e => e.CreatorId, f => f.PickRandom(creatorIds)); // Assign a valid CreatorId

        // Generate exclusive deals using Faker
        for (var i = 0; i < count; i++)
        {
            var exclusiveDeal = exclusiveDealFaker.Generate();
            exclusiveDeals.Add(exclusiveDeal);
        }

        // Add the generated exclusive deals to the database
        await _context.ExclusiveDeals.AddRangeAsync(exclusiveDeals);
        await _context.SaveChangesAsync();
    }


    private async Task GenerateRates(int count = 10)
    {
        // Retrieve existing Creator IDs
        var creatorIds = await _context.Creators.Select(c => c.Id).ToListAsync();

        if (!creatorIds.Any())
        {
            throw new InvalidOperationException("No creators found in the database to associate with rates.");
        }

        var rates = new List<Rate>();
        var rateFaker = new Faker<Rate>()
            .RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(r => r.Type, f => f.Lorem.Word())
            .RuleFor(r => r.Price, f => f.Finance.Amount(10, 500))
            .RuleFor(r => r.CreatorId, f => f.PickRandom(creatorIds)) // Pick valid CreatorId
            .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(r => r.UpdatedAt, f => f.Date.RecentOffset());

        // Generate rates using Faker
        for (var i = 0; i < count; i++)
        {
            var rate = rateFaker.Generate();
            rates.Add(rate);
        }

        // Add the generated rates to the database
        await _context.Rates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();
    }

    public static async Task<RegularUser> GenerateRegularUserAsync(UserManager<ApplicationUser> userManager)
    {
        var appUser = await GenerateApplicationUserAsync(userManager, RolesConstant.RegularUser);

        return new Faker<RegularUser>()
            .RuleFor(r => r.ApplicationUserId, appUser.Id)
            //.RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(r => r.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(r => r.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    public static async Task<Creator> GenerateCreatorAsync(UserManager<ApplicationUser> userManager)
    {
        var appUser = await GenerateApplicationUserAsync(userManager, RolesConstant.Creator);
        var creator = new Faker<Creator>()
            .RuleFor(c => c.ApplicationUserId, appUser.Id)
            //.RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(c => c.Price, f => f.Finance.Amount(50, 500))
            .RuleFor(c => c.Instagram, f => f.Internet.UserName())
            .RuleFor(c => c.Facebook, f => f.Internet.UserName())
            .RuleFor(c => c.XTwitter, f => f.Internet.UserName())
            .RuleFor(c => c.Tiktok, f => f.Internet.UserName())
            .RuleFor(c => c.HasExclusiveDeal, f => f.Random.Bool())
            .RuleFor(c => c.AdvertAmount, f => f.Finance.Amount(100, 1000))
            .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(c => c.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();

        return creator;
    }

    public static async Task<Admin> GenerateAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var appUser = await GenerateApplicationUserAsync(userManager, RolesConstant.Admin);
        return new Faker<Admin>()
            .RuleFor(a => a.ApplicationUserId, appUser.Id)
            //.RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(a => a.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(a => a.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    public static async Task<SuperAdmin> GenerateSuperAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var appUser = await GenerateApplicationUserAsync(userManager, RolesConstant.SuperAdmin);
        return new Faker<SuperAdmin>()
            .RuleFor(sa => sa.ApplicationUserId, appUser.Id)
           //.RuleFor(c => c.Id, f => Guid.NewGuid().ToString())
            .RuleFor(sa => sa.CreatedAt, f => f.Date.PastOffset())
            .RuleFor(sa => sa.UpdatedAt, f => f.Date.RecentOffset())
            .Generate();
    }

    public static async Task<ApplicationUser> GenerateApplicationUserAsync(UserManager<ApplicationUser> userManager, string role)
    {
        ApplicationUser user;

        // Generate a user with Faker
        do
        {
            user = new Faker<ApplicationUser>()
                //.RuleFor(c => c.Id, f => Guid.NewGuid().ToString()) // Always new Guid
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.AppUserName, f => f.Internet.UserName()) // Nickname as the AppUserName
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.UserName, (f, u) => u.Email) // ASP.NET Identity uses UserName as the login identifier
                .RuleFor(u => u.ImageUrl, f => $"https://res.cloudinary.com/dvrghpls1/image/upload/v{f.Date.Recent().Year}/{f.Random.AlphaNumeric(10)}.jpg")
                .RuleFor(u => u.Occupation, f => f.Name.JobTitle())
                .RuleFor(u => u.Bio, f => f.Lorem.Paragraph())
                .RuleFor(u => u.Location, f => f.Address.City())
                .RuleFor(u => u.IsVerified, f => f.Random.Bool())
                .RuleFor(u => u.StripeAccountId, f => f.Random.Guid().ToString())
                .RuleFor(u => u.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(u => u.UpdatedAt, f => f.Date.RecentOffset())
                .Generate();
        }
        while (await userManager.Users.AnyAsync(u => u.Id == user.Id || u.Email == user.Email));

        // Set default password
        var password = "Password@123";

        // Create the user in the database
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Assign the user to the specified role
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            throw new Exception($"Failed to assign role '{role}' to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }

        return user;
    }


    private async Task GenerateRegularUsers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var regularUser = await GenerateRegularUserAsync(_userManager);
            _context.RegularUsers.Add(regularUser);
        }

        await _context.SaveChangesAsync();
    }

    private async Task GenerateAdmins(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var admin = await GenerateAdminAsync(_userManager);
            _context.Admins.Add(admin);
        }

        await _context.SaveChangesAsync();
    }

    private async Task GenerateCreators(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var creator = await GenerateCreatorAsync(_userManager);
            _context.Creators.Add(creator);
        }

        await _context.SaveChangesAsync();
    }

    private async Task GenerateSuperAdmins(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var superAdmin = await GenerateSuperAdminAsync(_userManager);
            _context.SuperAdmins.Add(superAdmin);
        }

        await _context.SaveChangesAsync();
    }



}
