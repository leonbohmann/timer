using System.Windows;

namespace Timer;
/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
	private readonly TimerSettings _settings;

	public SettingsWindow(TimerSettings settings)
	{
		InitializeComponent();
		_settings = settings;
		this.DataContext = new LoginViewModel(settings);
	}

	private async void Button_Click(object sender, RoutedEventArgs e)
	{
		KimaiApi.CreateApi(_settings);

		if (await KimaiApi.CheckAuthorized() == false)
		{
			MessageBox.Show("Verbindung war nicht möglich!");
			return;
		}

		DialogResult = true;
	}
}
