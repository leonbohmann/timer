using GalaSoft.MvvmLight.CommandWpf;

using Newtonsoft.Json.Serialization;

using PropertyChanged;

using Refit;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Timer;
public class MainViewModel : INotifyPropertyChanged
{
	public TimerSettings Settings { get; }

	private readonly IKimaiApi _kimaiApi;
	private KimaiSheetEntry CurrentEntry { get; set; }

	public string Description { get => CurrentEntry.Description; set => CurrentEntry.Description = value; }
	public ICommand StartCommand { get; }
	public ICommand StopCommand { get; }

	public bool IsStarted => Settings.CurrentTimesheetEntryId.HasValue;
	public bool IsNotStarted => !IsStarted;

	[OnChangedMethod(nameof(OnSelectedProjectAndActivityChanged))]
	public KimaiActivity SelectedActivity { get; set; }


	[OnChangedMethod(nameof(OnSelectedProjectAndActivityChanged))]
	public KimaiProject SelectedProject { get; set; }

	public IEnumerable<KimaiProject>? Projects { get; set; }
	public IEnumerable<KimaiActivity>? Activites { get; private set; }

	private void OnSelectedProjectAndActivityChanged()
	{
		if (SelectedActivity is not null)
			Settings.ActivityId = SelectedActivity.ID;
		if (SelectedProject is not null)
			Settings.ProjectId = SelectedProject?.ID;
	}

	public MainViewModel()
	{
		Settings = TimerSettings.Load();
		if (Settings.Server == "") TryAuthorize();

		var serveradress = $"{Settings.Server}/api";
		// Initialize Refit API interface
		var settings = new RefitSettings()
		{
			AuthorizationHeaderValueGetter = (r, c) => Task.FromResult(Settings.Token),
			ContentSerializer = new NewtonsoftJsonContentSerializer(new()
			{
				//PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
				DateFormatString = "yyyy-MM-ddTHH:mm:ss",
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			})
		};
		//var httpClient = new HttpClient(new HttpLoggingHandler()) { BaseAddress = new Uri(serveradress) };
		//httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Settings.Token}");
		//_kimaiApi = RestService.For<IKimaiApi>(httpClient, settings);
		_kimaiApi = RestService.For<IKimaiApi>(serveradress, settings);

		StartCommand = new RelayCommand(Start);
		StopCommand = new RelayCommand(Stop);

		CurrentEntry = new();
	}


	public bool? TryAuthorize()
	{
		var window = new SettingsWindow(Settings);
		var code = window.ShowDialog();
		TimerSettings.Save();
		return code;
	}



	private async void Start()
	{
		try
		{
			var entry = new Dictionary<string, object>()
			{
				{ "begin", DateTime.UtcNow },
				{ "end", DateTime.UtcNow },
				{ "project", SelectedProject?.ID ?? 2 },
				{ "activity", SelectedActivity?.ID ?? 1 },
				{ "description", Description },
				{ "fixedRate", 0 },
				{ "hourlyRate", 0 },
				{ "user", Settings.UserId },
				{ "tags", "" }
			};

			var createdEntry = await _kimaiApi.CreateEntry(entry);
			//Trace.TraceWarning(await createdEntry.Content.ReadAsStringAsync());
			if (createdEntry.StatusCode == System.Net.HttpStatusCode.OK)
			{
				Settings.CurrentTimesheetEntryId = createdEntry.Content!.Id;
				CurrentEntry = createdEntry.Content;
				OnIsStartedChanged();
			}
			else
			{
				MessageBox.Show($"Error starting activity: {createdEntry.Error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error starting activity: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private async void Stop()
	{
		try
		{
			if (Settings.CurrentTimesheetEntryId is int id)
			{
				var response = await _kimaiApi.UpdateEntryDescription(id, new()
				{
					{ "description", Description },
					{ "end", DateTime.UtcNow }
				});
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					Settings.CurrentTimesheetEntryId = null;
					CurrentEntry = new();
					OnIsStartedChanged();
				}
				else
				{
					MessageBox.Show($"Beim Stoppen ist etwas schief gelaufen: {response.ReasonPhrase}.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
					Settings.CurrentTimesheetEntryId = null;
					CurrentEntry = new();
				}
			}
			else
				MessageBox.Show("No active entry to update description.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error updating description: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	private void OnIsStartedChanged()
	{
		OnPropertyChanged(nameof(IsStarted));
		OnPropertyChanged(nameof(IsNotStarted));
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal async Task Fetch()
	{
		var projects_result = await _kimaiApi.GetProjects();

		if (!projects_result.IsSuccessStatusCode)
		{
			if (TryAuthorize() is bool result && result)
			{
				await Fetch();
			}
		}
		var activities_result = await _kimaiApi.GetActivities();

		var me_result = await _kimaiApi.GetMe();

		Settings.UserId = me_result.Id;

		Projects = projects_result.Content!;
		Activites = activities_result.Content!;

		if (Settings.ProjectId is int pid)
			SelectedProject = Projects.FirstOrDefault(p => p.ID == pid)!;
		SelectedProject ??= Projects.FirstOrDefault()!;

		if (Settings.ActivityId is int aid)
			SelectedActivity = Activites.FirstOrDefault(a => a.ID == aid)!;
		SelectedActivity ??= Activites.FirstOrDefault()!;

		// now, fetch the current entry
		if (Settings.CurrentTimesheetEntryId is int id)
		{
			var entry_response = await _kimaiApi.GetEntry(id);
			if (entry_response.IsSuccessStatusCode && entry_response.Content is KimaiSheetEntry entry)
			{
				Description = entry.Description;
				OnIsStartedChanged();
			}
			else
				MessageBox.Show($"Beim Laden des letzten Eintrags traten Fehler auf: {entry_response.ReasonPhrase}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);


		}
	}
}