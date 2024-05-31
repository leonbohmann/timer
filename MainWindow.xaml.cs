using System.Windows;

namespace Timer;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private async void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var dc = (MainViewModel)DataContext;
		await dc.Fetch();
	}
}