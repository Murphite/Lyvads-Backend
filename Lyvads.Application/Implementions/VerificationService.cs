using Lyvads.Application.Interfaces;
using Lyvads.Domain.Entities;
using Lyvads.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Lyvads.Application.Implementions
{
    public class VerificationService : IVerificationService
    {
        private readonly AppDbContext _context;

        public VerificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveVerificationCode(string email, string code)
        {
            var record = new VerificationRecord
            {
                Email = email,
                Code = code,
                IsVerified = false
            };

            _context.VerificationRecords.Add(record);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GetEmailByVerificationCode(string code)
        {
            var record = await _context.VerificationRecords
                .Where(r => r.Code == code && !r.IsVerified)
                .SingleOrDefaultAsync();

            return record?.Email ?? string.Empty;
        }

        public async Task<bool> ValidateVerificationCode(string email, string code)
        {
            var record = await _context.VerificationRecords
                .Where(r => r.Email == email && r.Code == code && !r.IsVerified)
                .SingleOrDefaultAsync();

            return record != null;
        }

        public async Task MarkEmailAsVerified(string email)
        {
            var records = await _context.VerificationRecords
                .Where(r => r.Email == email && !r.IsVerified)
                .ToListAsync();

            foreach (var record in records)
            {
                record.IsVerified = true;
                record.VerifiedAt = DateTimeOffset.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailVerified(string email)
        {
            var record = await _context.VerificationRecords
                .Where(r => r.Email == email && r.IsVerified)
                .SingleOrDefaultAsync();

            return record != null;
        }

        public async Task<string> GetVerifiedEmail(string email)
        {
            var record = await _context.VerificationRecords
                .Where(r => r.Email == email && r.IsVerified)
                .OrderByDescending(r => r.VerifiedAt)
                .FirstOrDefaultAsync();

            return record?.Email ?? string.Empty;
        }
    }
}
