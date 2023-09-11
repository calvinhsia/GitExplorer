using LibGit2Sharp;
using System.Windows.Controls;

namespace GitExplorer
{
    internal class CommitTreeView : TreeView
    {
        private Commit commit;

        public CommitTreeView(Commit commit)
        {
            this.commit = commit;
            var childItems = new TreeItem(commit);
            this.Items.Add(childItems);
        }
    }

    internal class TreeItem : TreeViewItem
    {
        public TreeItem()
        {
            this.IsExpanded = true;
        }

        public TreeItem(Commit commit)
        {
            this.Header = $"{commit.MessageShort} {commit.Id}";
            this.AddTree(commit.Tree);
            this.IsExpanded = true;

        }
        public void AddTree(Tree tree)
        {
            foreach (var childTreeEntry in tree)
            {
                switch(childTreeEntry.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        var treechild = (childTreeEntry.Target as Tree)!;
                        var newitem = new TreeItem();
                        newitem.Header = $"{childTreeEntry.Name} ${childTreeEntry.Mode}";
                        newitem.AddTree(treechild);
                        this.Items.Add(newitem);
                        break;
                    case TreeEntryTargetType.Blob:
                        var blob = (childTreeEntry.Target as Blob)!;
                        var newblobitem = new TreeItem();
                        newblobitem.Header = $"{childTreeEntry.Name} ${childTreeEntry.Mode}";
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