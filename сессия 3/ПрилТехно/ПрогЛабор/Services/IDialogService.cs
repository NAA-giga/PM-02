namespace ПрогЛабор.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Информация");
        bool ShowConfirmation(string message, string title = "Подтверждение");
    }
}