using Application.Common.Interfaces;

namespace API.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}