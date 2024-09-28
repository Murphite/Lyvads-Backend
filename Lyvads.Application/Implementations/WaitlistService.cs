
using Lyvads.Domain.Interfaces;
using Lyvads.Application.Dtos;
using Lyvads.Infrastructure.Services;
using Lyvads.Application.Interfaces;
using Lyvads.Domain.Repositories;
using Lyvads.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Lyvads.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Lyvads.Application.Dtos.AuthDtos;
using Microsoft.AspNetCore.Identity;

namespace Lyvads.Application.Implementations;

public class WaitlistService : IWaitlistService
{
    private readonly ILogger<WaitlistService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository _repository;
    private readonly IEmailService _emailService;

    public WaitlistService(IRepository repository, IEmailService emailService, IUnitOfWork unitOfWork, ILogger<WaitlistService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> AddToWaitlist(string email)
    {
        if (string.IsNullOrEmpty(email))
            return new Error[] { new("Waitlist.Error", "Email is required") };

        // Ensure email is unique
        var emailExist = await _repository.FindByCondition<WaitlistEntry>(w => w.Email == email);
        if (emailExist != null)
            return new Error[] { new("Waitlist.Error", "Email already exists") };

        var entry = new WaitlistEntry
        {
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.Add(entry);
        await _unitOfWork.SaveChangesAsync();

        // Send confirmation email
        var emailSubject = "Welcome to the Waitlist!";
        var emailBody = "<p>Thank you for joining our waitlist. We will notify you when Lyvads goes live.</p>";
        var emailResult = await _emailService.SendEmailAsync(email, emailSubject, emailBody);
        if (!emailResult)
            return new Error[] { new("Waitlist.Error", "User added to waitlist but failed to send confirmation email") };

        return Result.Success("User added to waitlist successfully");
    }



    public async Task<Result> NotifyWaitlistUsers()
    {
        _logger.LogInformation("********* Inside the Notify Waitlist Method  ********");
        var waitlistEntries = await _repository.GetAll<WaitlistEntry>().ToListAsync();

        foreach (var entry in waitlistEntries)
        {
            var emailBody = "Our app is now live! Thank you for waiting.";
            var emailResult = await _emailService.SendEmailAsync(entry.Email ?? string.Empty, "App Launch Notification", emailBody);

            if (!emailResult)
                return new Error[] { new("Notification.Error", $"Failed to send email to {entry.Email}") };
        }

        return Result.Success();
    }
}
