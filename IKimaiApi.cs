using Refit;

using System.Net.Http;

namespace Timer;
public interface IKimaiApi
{
	[Headers("Authorization: Bearer")]
	[Get("/users/me")]
	Task<KimaiUser> GetMe();

	[Headers("Authorization: Bearer")]
	[Post("/timesheets")]
	Task<ApiResponse<KimaiSheetEntry>> CreateEntry([Body] Dictionary<string, object> entry);

	[Headers("Authorization: Bearer")]
	[Get("/timesheets/{id}/stop")]
	Task StopEntry(int id);

	[Headers("Authorization: Bearer")]
	[Get("/timesheets/{id}")]
	Task<ApiResponse<KimaiSheetEntry>> GetEntry(int id);

	[Headers("Authorization: Bearer")]
	[Patch("/timesheets/{id}")]
	Task<HttpResponseMessage> UpdateEntryDescription(int id, [Body] Dictionary<string, object> entry);

	[Headers("Authorization: Bearer")]
	[Get("/projects")]
	Task<ApiResponse<IEnumerable<KimaiProject>>> GetProjects();
	[Headers("Authorization: Bearer")]
	[Get("/activities")]
	Task<ApiResponse<IEnumerable<KimaiActivity>>> GetActivities();
}
