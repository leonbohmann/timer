using System.Windows;

namespace Timer;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnExit(ExitEventArgs e)
	{
		TimerSettings.Save();
		base.OnExit(e);
	}
}

