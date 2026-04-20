using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("Mod 文件复制器")]
[assembly: AssemblyProduct("Mod 文件复制器")]
[assembly: AssemblyCopyright("Copyright (c) 2026 uyujkk")]
[assembly: AssemblyVersion("2.2.0.0")]
[assembly: AssemblyFileVersion("2.2.0.0")]
[assembly: AssemblyInformationalVersion("2.2")]

internal static class WinUILauncher
{
    [STAThread]
    private static void Main()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string runtimeDirectory = Path.Combine(baseDirectory, "WinUI3");
        string targetExe = Path.Combine(runtimeDirectory, "ModFolderCopier.WinUI.exe");

        if (!File.Exists(targetExe))
        {
            MessageBox.Show(
                "未找到 WinUI 3 运行文件：\n" + targetExe,
                "启动失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = targetExe,
                WorkingDirectory = runtimeDirectory,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "启动 WinUI 3 版本时出错：\n" + ex.Message,
                "启动失败",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
