using Microsoft.EntityFrameworkCore;
using TaskHandler.Application.DTOs;
using TaskHandler.Application.Interfaces;

namespace TaskHandler.Application.Queries.GetTaskItems;

public record GetTaskItemsQuery(Guid? UserId = null) : IQuery<GetTaskItemsResponse>;

public record GetTaskItemsResponse(List<GetTasItemkDTO> TaskItems);

public class GetTaskItemsQueryHandler : IQueryHandler<GetTaskItemsQuery, GetTaskItemsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetTaskItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetTaskItemsResponse> Handle(GetTaskItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TaskItems.AsQueryable();

        if (request.UserId != null)
        {
            query = query.Where(userId => userId.UserId == request.UserId);
        }

        var tasks = await query.Select(t => new GetTasItemkDTO
        {
            UserId = t.UserId,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            TaskType = t.TaskType,
            Priority = t.Priority,
            CompletionDate = t.CompletionDate
        }).ToListAsync(cancellationToken);
        
        return new GetTaskItemsResponse(tasks);
    }
}