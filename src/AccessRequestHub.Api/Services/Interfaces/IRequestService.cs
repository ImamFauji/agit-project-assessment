using AccessRequestHub.Api.DTOs;

namespace AccessRequestHub.Api.Services.Interfaces;
public interface IRequestService
{
    Task<(AccessRequestResponse Response, bool WasCreated)> CreateRequestAsync(
        string requesterEmail,
        CreateRequestDto dto);
    Task<IEnumerable<AccessRequestResponse>> ListRequestsAsync(string actorEmail);
    Task<AccessRequestDetailResponse?> GetRequestDetailAsync(string actorEmail, Guid requestId);
    Task<AccessRequestResponse> ApproveRequestAsync(
        string actorEmail,
        Guid requestId,
        ApproveRequestDto dto);
    Task<AccessRequestResponse> RejectRequestAsync(
        string actorEmail,
        Guid requestId,
        RejectRequestDto dto);
}
