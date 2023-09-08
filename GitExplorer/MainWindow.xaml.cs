using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
/*
https://www.youtube.com/watch?v=RxHJdapz2p0

git hash-object <filename> -w
git cat-file blob <hash>
git log --oneline
git ls-tree <hash>
git branch
git --no-page log --oneline


 */
namespace GitExplorer
{
    class ExecCmd
    {
        public static async Task<string> DoCmdAsync(string args, string workdir)
        {
            var procStart = new ProcessStartInfo("git", args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true, 
                WorkingDirectory = workdir
            };

            using var proc = new Process();
            proc.EnableRaisingEvents = true;
            var sbOutput = new StringBuilder();
            var sbError = new StringBuilder();
            proc.OutputDataReceived += (o, e) =>
            {
                sbOutput.AppendLine(e.Data);

            };
            proc.ErrorDataReceived += (o, e) =>
            {
                if (e.Data?.Length > 0)
                {
                    sbError.AppendLine(e.Data);
                }
            };
            proc.StartInfo = procStart;
            var started = proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            await proc.WaitForExitAsync();
            if (sbError.Length > 0)
            {
                throw new Exception(sbError.ToString());
            }
            return sbOutput.ToString();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //var d = new System.Windows.Forms.FolderBrowserDialog
                //{
                //    Description = "",
                //    ShowNewFolderButton = false,
                //};
                var dir = @"c:\Repos\vscode-fabric";
                var repo = new LibGit2Sharp.Repository(dir);
                var br = repo.Branches;
                var res = await ExecCmd.DoCmdAsync("status", dir);
                var xs = 33;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
