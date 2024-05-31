namespace Timer;
internal class LoginViewModel
{
	private readonly TimerSettings _settings;

	public string Server { get => _settings.Server; set => _settings.Server = value; }
	public string Token { get => _settings.Token; set => _settings.Token = value; }

	public LoginViewModel(TimerSettings settings)
	{
		_settings = settings;
	}
}
