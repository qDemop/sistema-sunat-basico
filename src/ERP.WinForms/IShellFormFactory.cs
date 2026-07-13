using ERP.WinForms.Forms;

namespace ERP.WinForms;

public interface IShellFormFactory
{
    LoginForm CreateLoginForm();
    MainForm CreateMainForm();
}
