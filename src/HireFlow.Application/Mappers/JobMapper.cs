using HireFlow.Application.Dtos;
using HireFlow.Domain.Entities;

namespace HireFlow.Application.Mappers;

// Turns a Job entity into the DTO we send to the client (we never send entities).
public static class JobMapper
{
    public static JobResponse ToResponse(this Job job)
    {
        return new JobResponse
        {
            Id = job.Id,
            Title = job.Title,
            Department = job.Department,
            Location = job.Location,
            Description = job.Description,
            RequiredSkills = job.RequiredSkills,
            Positions = job.Positions,
            Status = job.Status,
            CreatedAt = job.CreatedAt
        };
    }
}
