namespace Timer;
internal class LoginViewModel
{
	private readonly ISettings _settings;

	public string Server { get => _settings.Server; set => _settings.Server = value; }
	public string Username { get => _settings.Username; set => _settings.Username = value; }
	public string Token { get => _settings.Token; set => _settings.Token = value; }

	public LoginViewModel(ISettings settings)
	{
		_settings = settings;
	}
}
