namespace ERP.WinForms.Presenters;

public interface ILoginFormView
{
    string Username { get; }
    string Password { get; }

    void ShowError(string message);
    void ClearError();
    void SetBusy(bool busy);
}
