using GalaSoft.MvvmLight.CommandWpf;

using PropertyChanged;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Timer;
public class MainViewModel : INotifyPropertyChanged
{
	public TimerSettings Settings { get; private set; } = new();

	private KimaiSheetEntry CurrentEntry { get; set; }

	public string Description { get => CurrentEntry.Description; set => CurrentEntry.Description = value; }
	public ICommand StartCommand { get; }
	public ICommand StopCommand { get; }

	public bool IsStarted => Settings?.CurrentTimesheetEntryId.HasValue ?? false;
	public bool IsNotStarted => !IsStarted;
	public bool IsReady { get; set; } = false;

	[OnChangedMethod(nameof(OnSelectedProjectAndActivityChanged))]
	public KimaiActivity? SelectedActivity { get; set; }


	[OnChangedMethod(nameof(OnSelectedProjectAndActivityChanged))]
	public KimaiProject? SelectedProject { get; set; }

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
		StartCommand = new RelayCommand(Start);
		StopCommand = new RelayCommand(Stop);

		CurrentEntry = new();
	}

	/// <summary>
	/// This will run until the connection is established.
	/// If the process is cancelled (e.g. by closing the settings window), the application will be shut down.
	/// </summary>
	private async Task<bool> MakeSureConnected()
	{
		if (await InitializeSettingsAndCreateApi() is false)
		{
			Application.Current.Shutdown();
			return false;
		}
		return true;
	}

	/// <summary>
	/// Loads the settings and creates the <see cref="KimaiApi.Api"/>.
	/// </summary>
	private async Task<bool?> InitializeSettingsAndCreateApi()
	{
		// first, loads the settings. With these settings, try to create the api. If that fails

		Settings = TimerSettings.Load();

		// when creating the api fails, we need to show the settings window
		if (KimaiApi.CreateApi(Settings) is null || await KimaiApi.CheckAuthorized() == false)
		{
			// settingswindow will create the api before allowing it to quit
			var window = new SettingsWindow(Settings);
			var code = window.ShowDialog();
			TimerSettings.Save();
			return code;
		}
		else
			return true;
	}


	/// <summary>
	/// Starts a new entry.
	/// </summary>
	private async void Start()
	{
		try
		{
			// the keys of the dictionary must not be changed. The api expects them.
			var entry = new Dictionary<string, object>()
			{
				{ "begin", DateTime.Now },
				{ "end", DateTime.Now },
				{ "project", SelectedProject?.ID ?? 2 },
				{ "activity", SelectedActivity?.ID ?? 1 },
				{ "description", Description },
				{ "fixedRate", 0 },
				{ "hourlyRate", 0 },
				{ "user", Settings.UserId },
				{ "tags", "" }
			};

			var createdEntry = await KimaiApi.Api.CreateEntry(entry);
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

	/// <summary>
	/// Stops the last known entry.
	/// </summary>
	private async void Stop()
	{
		try
		{
			if (Settings.CurrentTimesheetEntryId is int id)
			{
				// updates the last known entry with the description and the current time
				var response = await KimaiApi.Api.UpdateEntryDescription(id, new()
				{
					{ "description", Description },
					{ "end", DateTime.Now }
				});

				if (response.StatusCode != System.Net.HttpStatusCode.OK)
					MessageBox.Show($"Beim Stoppen ist etwas schief gelaufen: {response.ReasonPhrase}.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

				// stop the entry (even if there were errors, since we can't regain control over the last entry
				Settings.CurrentTimesheetEntryId = null;
				CurrentEntry = new();
				OnIsStartedChanged();
			}
			else
				MessageBox.Show("No active entry to update description.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error updating description: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}

	/// <summary>
	/// Notify that a transaction has started/ended.
	/// </summary>
	private void OnIsStartedChanged()
	{
		OnPropertyChanged(nameof(IsStarted));
		OnPropertyChanged(nameof(IsNotStarted));
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Fetches current data from the server.
	/// Initializes base information about the current entry as well as available projects and activities.
	/// </summary>
	/// <returns></returns>
	internal async Task Fetch()
	{
		await MakeSureConnected();

		var projects_result = await KimaiApi.Api.GetProjects();
		var activities_result = await KimaiApi.Api.GetActivities();
		var me_result = await KimaiApi.Api.GetMe();

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
			var entry_response = await KimaiApi.Api.GetEntry(id);
			if (entry_response.IsSuccessStatusCode && entry_response.Content is KimaiSheetEntry entry)
			{
				Description = entry.Description;
			}
			else
				MessageBox.Show($"Beim Laden des letzten Eintrags traten Fehler auf: {entry_response.ReasonPhrase}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			OnIsStartedChanged();
		}

		IsReady = true;
	}
}