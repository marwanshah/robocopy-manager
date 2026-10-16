using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;

internal sealed class InstallerForm : Form
{
    private readonly string defaultDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "RoboCopyManager");
    private readonly Panel header = new Panel();
    private readonly Panel body = new Panel();
    private readonly Panel footer = new Panel();
    private readonly Label heading = new Label();
    private readonly Label detail = new Label();
    private readonly TextBox directoryBox = new TextBox();
    private readonly Button browseButton = new Button();
    private readonly Button backButton = new Button();
    private readonly Button nextButton = new Button();
    private readonly Button cancelButton = new Button();
    private readonly ProgressBar progress = new ProgressBar();
    private readonly CheckBox launchCheck = new CheckBox();
    private int page;
    private string installedAppPath;

    private InstallerForm()
    {
        Text = "RoboCopy Manager Setup";
        using (Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("InstallerIcon.ico"))
        {
            if (iconStream != null) Icon = new Icon(iconStream);
        }
        ClientSize = new Size(600, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        BuildLayout();
        ShowPage(0);
    }

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new InstallerForm());
    }

    private void BuildLayout()
    {
        header.Dock = DockStyle.Top;
        header.Height = 86;
        header.BackColor = Color.FromArgb(245, 247, 250);
        heading.Location = new Point(24, 17);
        heading.Size = new Size(540, 30);
        heading.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        detail.Location = new Point(26, 51);
        detail.Size = new Size(540, 23);
        detail.ForeColor = Color.DimGray;
        header.Controls.Add(heading);
        header.Controls.Add(detail);

        body.Dock = DockStyle.Fill;
        body.Padding = new Padding(28, 22, 28, 16);
        directoryBox.Location = new Point(30, 108);
        directoryBox.Size = new Size(400, 25);
        browseButton.Text = "Browse...";
        browseButton.Location = new Point(440, 106);
        browseButton.Size = new Size(90, 28);
        browseButton.Click += BrowseClicked;
        progress.Location = new Point(30, 112);
        progress.Size = new Size(500, 22);
        progress.Style = ProgressBarStyle.Marquee;
        launchCheck.Text = "Launch RoboCopy Manager when setup closes";
        launchCheck.Location = new Point(30, 115);
        launchCheck.AutoSize = true;
        body.Controls.Add(directoryBox);
        body.Controls.Add(browseButton);
        body.Controls.Add(progress);
        body.Controls.Add(launchCheck);

        footer.Dock = DockStyle.Bottom;
        footer.Height = 58;
        footer.Padding = new Padding(10);
        footer.BorderStyle = BorderStyle.FixedSingle;
        backButton.Text = "< Back";
        backButton.Size = new Size(88, 30);
        backButton.Location = new Point(305, 13);
        backButton.Click += delegate { ShowPage(0); };
        nextButton.Text = "Next >";
        nextButton.Size = new Size(88, 30);
        nextButton.Location = new Point(399, 13);
        nextButton.Click += NextClicked;
        cancelButton.Text = "Cancel";
        cancelButton.Size = new Size(88, 30);
        cancelButton.Location = new Point(493, 13);
        cancelButton.Click += delegate { Close(); };
        footer.Controls.Add(backButton);
        footer.Controls.Add(nextButton);
        footer.Controls.Add(cancelButton);
        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private void ShowPage(int value)
    {
        page = value;
        ControlBox = value != 2;
        directoryBox.Visible = value == 1;
        browseButton.Visible = value == 1;
        progress.Visible = value == 2;
        launchCheck.Visible = value == 3;
        backButton.Visible = value == 1;
        backButton.Enabled = value == 1;
        cancelButton.Enabled = value < 2;
        if (value == 0)
        {
            heading.Text = "Welcome to RoboCopy Manager Setup";
            detail.Text = "This wizard will install RoboCopy Manager on your computer.";
            body.Controls.Clear();
            body.Controls.Add(new Label { Text = "The required .NET runtime is included. You do not need to download or install it separately.", Location = new Point(30, 32), Size = new Size(500, 55) });
            nextButton.Text = "Next >";
            nextButton.Enabled = true;
            cancelButton.Text = "Cancel";
        }
        else if (value == 1)
        {
            heading.Text = "Choose Install Location";
            detail.Text = "Select where RoboCopy Manager should be installed.";
            body.Controls.Clear();
            body.Controls.Add(new Label { Text = "Setup will install the app for all users. Windows may ask for administrator approval.", Location = new Point(30, 38), Size = new Size(520, 40) });
            body.Controls.Add(directoryBox);
            body.Controls.Add(browseButton);
            directoryBox.Text = defaultDirectory;
            nextButton.Text = "Install";
            nextButton.Enabled = true;
            cancelButton.Text = "Cancel";
        }
        else if (value == 2)
        {
            heading.Text = "Installing RoboCopy Manager";
            detail.Text = "Please wait while setup copies the application and creates shortcuts.";
            body.Controls.Clear();
            body.Controls.Add(progress);
            nextButton.Text = "Installing...";
            nextButton.Enabled = false;
            cancelButton.Enabled = false;
        }
        else
        {
            heading.Text = "Setup Complete";
            detail.Text = "RoboCopy Manager has been installed successfully.";
            body.Controls.Clear();
            body.Controls.Add(new Label { Text = "Use the Start Menu shortcut to open the application.", Location = new Point(30, 42), Size = new Size(500, 28) });
            body.Controls.Add(launchCheck);
            launchCheck.Checked = true;
            nextButton.Text = "Finish";
            nextButton.Enabled = true;
            cancelButton.Visible = false;
        }
    }

    private void BrowseClicked(object sender, EventArgs e)
    {
        using (FolderBrowserDialog picker = new FolderBrowserDialog())
        {
            picker.Description = "Choose the folder where RoboCopy Manager will be installed.";
            picker.SelectedPath = directoryBox.Text;
            if (picker.ShowDialog(this) == DialogResult.OK) directoryBox.Text = picker.SelectedPath;
        }
    }

    private void NextClicked(object sender, EventArgs e)
    {
        if (page == 0) { ShowPage(1); return; }
        if (page == 1)
        {
            string folder = directoryBox.Text.Trim();
            if (folder.Length == 0) { MessageBox.Show(this, "Choose an install folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            installedAppPath = Path.Combine(folder, "RoboCopyManager.exe");
            ShowPage(2);
            ThreadPool.QueueUserWorkItem(delegate { InstallFiles(folder); });
            return;
        }
        if (page == 3)
        {
            if (launchCheck.Checked) { try { Process.Start(installedAppPath); } catch { } }
            Close();
        }
    }

    private void InstallFiles(string folder)
    {
        Exception failure = null;
        try
        {
            Directory.CreateDirectory(folder);
            using (Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoboCopyManager.exe"))
            {
                if (payload == null) throw new InvalidOperationException("The application files are missing from the setup package.");
                using (FileStream output = new FileStream(installedAppPath, FileMode.Create, FileAccess.Write)) payload.CopyTo(output);
            }
            File.WriteAllText(Path.Combine(folder, "Uninstall-RoboCopyManager.ps1"),
                "$identity = [Security.Principal.WindowsIdentity]::GetCurrent()\r\n" +
                "$principal = New-Object Security.Principal.WindowsPrincipal($identity)\r\n" +
                "if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {\r\n" +
                "  Start-Process powershell.exe -Verb RunAs -ArgumentList @('-NoProfile','-ExecutionPolicy','Bypass','-File',\"`\"$PSCommandPath`\"\")\r\n" +
                "  exit\r\n" +
                "}\r\n" +
                "$installFolder = $PSScriptRoot\r\n" +
                "$startMenu = Join-Path $env:ProgramData 'Microsoft\\Windows\\Start Menu\\Programs\\RoboCopy Manager'\r\n" +
                "Remove-Item -LiteralPath (Join-Path $startMenu 'RoboCopy Manager.lnk') -Force -ErrorAction SilentlyContinue\r\n" +
                "Remove-Item -LiteralPath (Join-Path $startMenu 'Uninstall RoboCopy Manager.lnk') -Force -ErrorAction SilentlyContinue\r\n" +
                "Remove-Item -LiteralPath 'HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\RoboCopyManager' -Recurse -Force -ErrorAction SilentlyContinue\r\n" +
                "Start-Sleep -Seconds 2\r\n" +
                "Remove-Item -LiteralPath $installFolder -Recurse -Force\r\n",
                System.Text.Encoding.UTF8);
            File.WriteAllText(Path.Combine(folder, "Uninstall-RoboCopyManager.cmd"),
                "@echo off\r\npowershell.exe -NoProfile -ExecutionPolicy Bypass -File \"%~dp0Uninstall-RoboCopyManager.ps1\"\r\n",
                System.Text.Encoding.ASCII);
        }
        catch (Exception ex) { failure = ex; }
        BeginInvoke((MethodInvoker)delegate
        {
            if (failure != null)
            {
                MessageBox.Show(this, "Setup could not finish.\\n\\n" + failure.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowPage(1);
                cancelButton.Enabled = true;
                return;
            }
            try
            {
                string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "RoboCopy Manager");
                Directory.CreateDirectory(startMenu);
                CreateShortcut(Path.Combine(startMenu, "RoboCopy Manager.lnk"), installedAppPath);
                string uninstallPath = Path.Combine(directoryBox.Text, "Uninstall-RoboCopyManager.cmd");
                CreateShortcut(Path.Combine(startMenu, "Uninstall RoboCopy Manager.lnk"), uninstallPath);
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\RoboCopyManager"))
                {
                    key.SetValue("DisplayName", "RoboCopy Manager");
                    key.SetValue("DisplayVersion", "1.0.0");
                    key.SetValue("Publisher", "RoboCopy Manager");
                    key.SetValue("InstallLocation", directoryBox.Text);
                    key.SetValue("DisplayIcon", installedAppPath);
                    key.SetValue("UninstallString", "\"" + uninstallPath + "\"");
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    key.SetValue("EstimatedSize", (int)(new FileInfo(installedAppPath).Length / 1024), RegistryValueKind.DWord);
                }
                ShowPage(3);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The application was copied, but setup could not create a Start Menu shortcut.\\n\\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ShowPage(3);
            }
        });
    }

    private static void CreateShortcut(string shortcutPath, string targetPath)
    {
        Type shellType = Type.GetTypeFromProgID("WScript.Shell");
        object shell = Activator.CreateInstance(shellType);
        object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
        Type shortcutType = shortcut.GetType();
        shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath });
        shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { Path.GetDirectoryName(targetPath) });
        shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { targetPath + ",0" });
        shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
        Marshal.FinalReleaseComObject(shortcut);
        Marshal.FinalReleaseComObject(shell);
    }
}
