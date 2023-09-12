using LibGit2Sharp;
using System.Windows.Controls;

namespace GitExplorer
{
    internal class CommitTreeView : TreeView
    {
        private readonly Commit commit;

        public CommitTreeView(Commit commit)
        {
            this.commit = commit;
            var childItems = new TreeItem(commit, treeEntry: null);
            this.Items.Add(childItems);
        }
    }

    internal class TreeItem : TreeViewItem
    {
        public GitObject _gitObject;
        public TreeEntry? _TreeEntry;
        public TreeItem(GitObject gitObject, TreeEntry? treeEntry)
        {
            this._TreeEntry = treeEntry;
            this._gitObject = gitObject;
            this.IsExpanded = true;
            if (gitObject is Commit commit)
            {
                this.Header = $"{commit.MessageShort} {commit.Id}";
                this.AddTreeItems(commit.Tree);
            }
            _TreeEntry = treeEntry;
        }
        public void AddTreeItems(Tree tree)
        {
            foreach (var childTreeEntry in tree)
            {
                switch (childTreeEntry.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        var treechild = (childTreeEntry.Target as Tree)!;
                        var newitem = new TreeItem(childTreeEntry.Target!, childTreeEntry)
                        {
                            Header = $"{childTreeEntry.Name}"
                        };
                        newitem.AddTreeItems(treechild);
                        this.Items.Add(newitem);
                        break;
                    case TreeEntryTargetType.Blob:
                        var blob = (childTreeEntry.Target as Blob)!;
                        var newblobitem = new TreeItem(blob, childTreeEntry)
                        {
                            Header = $"{childTreeEntry.Name}"
                        };
                        this.Items.Add(newblobitem);
                        var txt = blob.GetContentText();
                        newblobitem.ToolTip = txt;
                        break;
                    case TreeEntryTargetType.GitLink:
                        break;

                }

            }

        }

    }
}