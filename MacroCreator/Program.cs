using MacroCreator.Controller;
using MacroCreator.Forms;

namespace MacroCreator;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main(string[] args)
    {
        var cmd = args.Length > 1 ? args[0] : null;
        var filePath = args.LastOrDefault();

        ApplicationConfiguration.Initialize();

        Form? mForm;

        if (cmd == "open" || cmd == "edit" || args.Length == 1)
        {
            mForm = new MainForm(filePath);
        }
        else if (cmd == "run")
        {
            mForm = null;
            if (filePath is not null) await RunMacroFile(filePath);
        }
        else
        {
            mForm = new MainForm();
        }

        if (mForm is not null) Application.Run(mForm);
    }

    static async Task RunMacroFile(string? filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show("文件未找到：\n" + filePath, "MacroCreator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var controller = new MacroController();
            controller.LoadSequence(filePath);
            await controller.StartPlayback();
        }
        catch (Exception ex)
        {
            MessageBox.Show("运行宏文件时出错：\n" + ex.Message, "MacroCreator", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}