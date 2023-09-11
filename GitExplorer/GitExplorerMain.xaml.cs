using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Utility;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string RepoFolder { get; set; } = @"c:\Repos\vscode-fabric";
        public ObservableCollection<string> LstBranches { get; set; } = new();
        public MainWindow()
        {
            this.WindowState = WindowState.Maximized;
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataContext = this;
                var repo = new LibGit2Sharp.Repository(this.RepoFolder);
                LstBranches = new ObservableCollection<string>(repo.Branches.Select(b => b.FriendlyName));
                lvBranches.SelectionChanged += (o, e) =>
                {
                    dbCommits.Children.Clear();
                    var branch = repo.Branches.Where(b => b.FriendlyName == ((string)lvBranches.SelectedItem)).Single();
                    var qCommits = from comm in branch.Commits
                                   select new
                                   {
                                       _commit = comm,
                                       comm.Message,
                                       comm.Author.When,
                                       comm.Id,
                                       comm.Author
                                   };
                    var browCommits = new BrowsePanel(qCommits);
                    dbCommits.Children.Add(browCommits);
                    browCommits.BrowseList.SelectionChanged += (oc, ec) =>
                    {
                        var selected = browCommits.BrowseList.SelectedItems[0];
                        if (selected != null)
                        {
                            dbCommitTree.Children.Clear();
                            var props = TypeDescriptor.GetProperties(selected);
                            var commit = (Commit)(props["_commit"]!.GetValue(selected)!);
                            var commTree = new CommitTreeView(commit);
                            dbCommitTree.Children.Add(commTree);
                            

                        }


                    };
                };
                var res = await ExecCmd.DoCmdAsync("status", this.RepoFolder);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            //var d = new System.Windows.Forms.FolderBrowserDialog
            //{
            //    Description = "",
            //    ShowNewFolderButton = false,
            //};

        }
    }
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
}
