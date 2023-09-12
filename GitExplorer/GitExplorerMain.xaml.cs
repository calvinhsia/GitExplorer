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
    public partial class GitexplorerMainWindow : Window
    {
        public string RepoFolder { get; set; } = @"c:\Repos\vscode-fabric";

        private Repository? _repository; // disposable

        public ObservableCollection<string> LstBranches { get; set; } = new();
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static GitexplorerMainWindow Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GitexplorerMainWindow()
        {
            Instance = this;
            this.WindowState = WindowState.Maximized;
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataContext = this;
                this._repository = new Repository(this.RepoFolder);
                LstBranches = new ObservableCollection<string>(this._repository.Branches.Select(b => b.FriendlyName));
                lvBranches.SelectionChanged += (o, e) =>
                {
                    dpCommits.Children.Clear();
                    var branch = this._repository.Branches.Where(b => b.FriendlyName == ((string)lvBranches.SelectedItem)).Single();
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
                    dpCommits.Children.Add(browCommits);
                    browCommits.BrowseList.SelectionChanged += (oc, ec) =>
                    {
                        var selected = browCommits.BrowseList.SelectedItems[0];
                        if (selected != null)
                        {
                            dpCommitTree.Children.Clear();
                            var props = TypeDescriptor.GetProperties(selected);
                            var commit = (Commit)(props["_commit"]!.GetValue(selected)!);
                            var commTree = new CommitTreeView(commit);
                            dpCommitTree.Children.Add(commTree);
                            commTree.SelectedItemChanged += (ot, et) =>
                            {
                                var itm = commTree.SelectedItem as TreeItem;
                                if (itm?._gitObject is Blob blob)
                                {
                                    var path = itm._TreeEntry?.Path;
                                    var histList = this._repository.Commits.QueryBy(path).ToList();
                                    dpFileCommits.Children.Clear();
                                    var qhist = from logEntry in histList
                                                select new
                                                {
                                                    _logEntry = logEntry,
                                                    Comm = logEntry.Commit.MessageShort

                                                };
                                    var brhist = new BrowsePanel(qhist);
                                    dpFileCommits.Children.Add(brhist);
                                    brhist.BrowseList.SelectionChanged += (oh, eh) =>
                                    {
                                        var selHist = brhist.BrowseList.SelectedItem;
                                        var logentry = (LogEntry)(TypeDescriptor.GetProperties(selHist)["_logEntry"]!.GetValue(selHist)!);
                                        var ndx = histList.FindIndex(h => h.Commit.Id == logentry.Commit.Id);
                                        if (ndx < histList.Count - 1)
                                        {
                                            var tr1 = histList[ndx].Commit.Tree;
                                            var tr2 = histList[ndx + 1].Commit.Tree;
                                            var diff = this._repository.Diff.Compare<TreeChanges>(tr1, tr2, new[] { path });
                                            dpFileDiff.Children.Clear();
                                            var lv = new ListView();
                                            dpFileDiff.Children.Add(lv);
                                            foreach (var change in diff.Modified)
                                            {
                                                var bNew = this._repository.Lookup(change.Oid) as Blob;
                                                var bOld = this._repository.Lookup(change.OldOid) as Blob;
                                                var dbob = this._repository.Diff.Compare(bOld, bNew);
                                                var patch = dbob.Patch;
                                                lv.Items.Add(patch);
                                            }

                                        }

                                    };


                                }
                                else if (itm?._gitObject is Tree tree)
                                {
                                    var differ = this._repository.Diff;
                                    //                                    var diff = differ.Compare()

                                }

                            };


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
            this._repository?.Dispose();
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
