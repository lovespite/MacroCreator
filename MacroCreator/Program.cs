using MacroCreator.Forms;

namespace MacroCreator;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        var cmd = args.Length > 1 ? args[0] : null;
        var filePath = args.LastOrDefault();

        ApplicationConfiguration.Initialize();

        Form mForm;

        if (cmd == "open" || args.Length == 1)
            mForm = new MainForm(filePath);
        else
            mForm = new MainForm();

        Application.Run(mForm);
    }
}