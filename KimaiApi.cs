using Newtonsoft.Json.Serialization;

using Refit;

namespace Timer;
public sealed class KimaiApi
{
	public static RefitSettings Settings { get; set; } = new();
	public static IKimaiApi Api { get; set; } = null!;

	public static string Token { get; set; } = "";

	static KimaiApi()
	{
		Settings = new RefitSettings()
		{
			AuthorizationHeaderValueGetter = (r, c) => Task.FromResult(Token),
			ContentSerializer = new NewtonsoftJsonContentSerializer(new()
			{
				//PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
				DateFormatString = "yyyy-MM-ddTHH:mm:ss",
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			})
		};
	}


	/// <summary>
	/// Create the api using the settings.
	/// </summary>
	/// <returns>
	/// The created API, if the creation (!) was successful. Otherwise, null.
	/// If the credentials are incorrect, the API will be created but the requests will fail.
	/// </returns>
	public static IKimaiApi? CreateApi(TimerSettings settings)
	{
		var serveradress = $"{settings.Server}/api";
		if (settings.Server == "") return null;
		Token = settings.Token;
		Api = RestService.For<IKimaiApi>(serveradress, Settings);

		return Api;
	}

	/// <summary>
	/// Checks, if the <see cref="Api"/> is authorized.
	/// </summary>
	/// <returns><see langword="false"/>, if the <see cref="Api"/> is <see langword="null"/> or a request to projects fails.</returns>
	public static async Task<bool> CheckAuthorized()
	{
		if (Api is null) return false;

		try
		{
			var response = await Api.GetProjects();
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				return false;
			return true;
		}
		catch (Exception)
		{
			return false;
		}

	}

}
