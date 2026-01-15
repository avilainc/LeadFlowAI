using Microsoft.EntityFrameworkCore;
using LeadFlowAI.Domain.Entities;
using LeadFlowAI.Domain.Interfaces;
using LeadFlowAI.Infrastructure.Persistence;

namespace LeadFlowAI.Infrastructure.Repositories;

public class LeadEventRepository : ILeadEventRepository
{
    private readonly ApplicationDbContext _context;

    public LeadEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LeadEvent leadEvent, CancellationToken cancellationToken = default)
    {
        await _context.LeadEvents.AddAsync(leadEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LeadEvent>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.LeadEvents
            .Where(e => e.LeadId == leadId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
