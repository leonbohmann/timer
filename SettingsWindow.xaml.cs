using System.Windows;

namespace Timer;
/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
	public SettingsWindow(ISettings settings)
	{
		InitializeComponent();
		this.DataContext = new LoginViewModel(settings);
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = true;
	}
}
