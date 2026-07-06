using MediatR;

namespace ERP.WinForms;

public sealed class MainForm : Form
{
    private readonly IMediator _mediator;

    public MainForm(IMediator mediator)
    {
        _mediator = mediator;
        Text = "ERP - Sistema SUNAT Basico";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1024;
        Height = 768;
    }
}
