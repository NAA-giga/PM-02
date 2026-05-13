using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using ПрилТехно.Services;

namespace ПрилТехно.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly ICaptchaService _captchaService;
        private readonly IDialogService _dialogService;

        // Username
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        // Password
        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        // CaptchaInput
        private string _captchaInput = string.Empty;
        public string CaptchaInput
        {
            get => _captchaInput;
            set => SetProperty(ref _captchaInput, value);
        }

        // CaptchaImage
        private ImageSource? _captchaImage;
        public ImageSource? CaptchaImage
        {
            get => _captchaImage;
            set => SetProperty(ref _captchaImage, value);
        }

        // ErrorMessage
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _expectedCaptcha = string.Empty;

        // Событие успешного входа
        public event EventHandler? LoginSucceeded;

        // Команды
        public ICommand GenerateNewCaptchaCommand { get; }
        public ICommand LoginCommand { get; }

        public LoginViewModel(IAuthService authService, ICaptchaService captchaService, IDialogService dialogService)
        {
            _authService = authService;
            _captchaService = captchaService;
            _dialogService = dialogService;

            // Ручное создание команд
            GenerateNewCaptchaCommand = new RelayCommand(GenerateNewCaptcha);
            LoginCommand = new AsyncRelayCommand(LoginAsync);

            GenerateNewCaptcha();
        }

        private void GenerateNewCaptcha()
        {
            var (image, answer) = _captchaService.GenerateCaptcha();
            CaptchaImage = image;
            _expectedCaptcha = answer;
            CaptchaInput = string.Empty;
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Введите логин и пароль";
                return;
            }

            if (string.IsNullOrWhiteSpace(CaptchaInput) || CaptchaInput != _expectedCaptcha)
            {
                ErrorMessage = "Неверный код с картинки";
                GenerateNewCaptcha();
                return;
            }

            var success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "Неверные учётные данные";
                GenerateNewCaptcha();
            }
        }
    }

    // Вспомогательные классы для команд (если их нет в вашем проекте)
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute == null || _canExecute());
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;
            _isExecuting = true;
            RaiseCanExecuteChanged();
            try { await _execute(); }
            finally { _isExecuting = false; RaiseCanExecuteChanged(); }
        }
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}