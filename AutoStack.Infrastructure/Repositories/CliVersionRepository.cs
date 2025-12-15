using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Infrastructure.Repositories;

public class CliVersionRepository : ICliVersionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CliVersionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CliVersion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CliVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.Id == id, cancellationToken);
    }
}
