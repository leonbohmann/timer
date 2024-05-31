using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Timer;

[JsonSerializable(typeof(TimerSettings))]
public class TimerSettings : ISettings
{
	private static TimerSettings _current;

	public string Server { get; set; } = "";
	public string Username { get; set; } = "";
	public string Token { get; set; } = "";

	public int? CurrentTimesheetEntryId { get; set; } = null;
	public int UserId { get; set; } = 0;

	public int? ActivityId { get; set; }
	public int? ProjectId { get; set; }

	static readonly string name = "settings.json";

	public async Task<bool> CheckAuthorized(IKimaiApi api)
	{
		var response = await api.GetProjects();
		if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
			return false;

		return true;
	}

	public static TimerSettings Load()
	{
		_current ??= LoadInternal();
		return _current;
	}
	public static void Save()
	{
		_current.SaveInternal();
	}

	private static TimerSettings LoadInternal()
	{
		// load from "settings.json"
		if (!File.Exists(name))
			return new TimerSettings();

		using var reader = new StreamReader(name);
		var content = reader.ReadToEnd();
		if (JsonSerializer.Deserialize<TimerSettings>(content) is TimerSettings settings)
			return settings;

		return new();
	}

	public void SaveInternal()
	{
		using var writer = new StreamWriter(name);
		writer.Write(JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true }));
	}
}
