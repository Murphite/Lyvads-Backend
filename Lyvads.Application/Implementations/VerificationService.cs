using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Domain.Responses;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lyvads.Application.Implementations;

public class VerificationService : IVerificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(AppDbContext context,
        ILogger<VerificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveVerificationCode(string email, string code)
    {
        var record = new VerificationRecord
        {
            Email = email,
            Code = code,
            IsVerified = false,
            VerifiedAt = DateTime.UtcNow
        };

        _context.VerificationRecords.Add(record);
        await _context.SaveChangesAsync();
    }

    public async Task<string> GetEmailByVerificationCode(string code)
    {
        var record = await _context.VerificationRecords
            .Where(r => r.Code == code && !r.IsVerified)
            .FirstOrDefaultAsync();

        return record?.Email ?? string.Empty;
    }

    public async Task<bool> ValidateVerificationCode(string email, string code)
    {
        var record = await _context.VerificationRecords
            .Where(r => r.Email == email && r.Code == code && !r.IsVerified)
            .FirstOrDefaultAsync();

        return record != null;
    }

    // Ensure this method correctly updates the database
    public async Task MarkEmailAsVerified(string email)
    {
        var record = await _context.VerificationRecords
        .Where(r => r.Email == email)
        .OrderByDescending(r => r.VerifiedAt)
        .FirstOrDefaultAsync();

        if (record == null)
        {
            _logger.LogWarning($"No record found for email {email}");
            return;
        }

        _logger.LogInformation($"Record before update: IsVerified={record.IsVerified}, VerifiedAt={record.VerifiedAt}");

        record.IsVerified = true;
        record.VerifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Record after update: IsVerified={record.IsVerified}, VerifiedAt={record.VerifiedAt}");

    }


    public async Task<bool> IsEmailVerified(string email)
    {
        var record = await _context.VerificationRecords
            .Where(r => r.Email == email && r.IsVerified)
            .FirstOrDefaultAsync();

        return record != null;
    }

    public async Task<string> GetVerifiedEmail(string email)
    {
        ServerResponse<string> result = new();
        _logger.LogInformation($"Searching for verified email: {email}");

        var emailRecord = await _context.VerificationRecords
            .Where(r => r.Email == email && r.IsVerified)
            .OrderByDescending(r => r.VerifiedAt)
            .FirstOrDefaultAsync();

        if (emailRecord == null)
        {
            _logger.LogWarning($"No verified record found for email: {email}");
            result.IsSuccessful = false;
            result.ErrorResponse = new ErrorResponse()
            {
                ResponseCode = "06",
                ResponseMessage = "No verification found for email.",
                ResponseDescription = "No verified record found for email.",
            };
        }

        _logger.LogInformation($"Verified email found: {emailRecord.Email}");
        //result.Data = emailRecord;
        result.IsSuccessful = true;
        result.ResponseMessage = "Verification found for email.";

        return result.Data;
    }

}
