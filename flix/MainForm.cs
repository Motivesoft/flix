using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace flix
{
    public partial class MainForm : Form
    {
        private readonly string defaultLocation = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );

        private readonly Stack<DirectoryInfo> directoryHistory = new Stack<DirectoryInfo>();

        private readonly Dictionary<DirectoryInfo,int> mruHistory = new Dictionary<DirectoryInfo, int>();

        private readonly ListViewColumnSorter listViewColumnSorter = new ListViewColumnSorter();

        private readonly IContext Context;

        private readonly Commands Commands;

        public MainForm( string[] args )
        {
            InitializeComponent();

            // Additional forms initialization
            listBrowser.ListViewItemSorter = listViewColumnSorter;

            // Startup
            var location = defaultLocation;
            var selection = "";
            if ( args.Length > 0 )
            {
                var suppliedLocation = args[ 0 ];
                if ( Directory.Exists( suppliedLocation ) )
                {
                    location = suppliedLocation;
                }
                else if ( File.Exists( suppliedLocation ) )
                {
                    var fileInfo = new FileInfo( suppliedLocation );
                    location = fileInfo.DirectoryName;
                    selection = fileInfo.Name;
                }
            }

            // Run
            OpenDirectoryView( location, selection );

            Context = new FlixContext( this );
            Commands = new Commands();
        }

        private void OpenDirectoryView( string location, string selection = "" )
        {
            if ( !Directory.Exists( location ) )
            {
                MessageBox.Show( $"{location} is not a directory", "Error" );
                return;
            }

            textLocation.Text = location;

            try
            {
                // Set this from the provided value or configure it later
                var selectionItem = selection;

                listBrowser.BeginUpdate();
                listBrowser.Items.Clear();

                // Record where we are
                var locationInfo = new DirectoryInfo( location );
                listBrowser.Tag = locationInfo;

                foreach ( var directory in Directory.GetDirectories( location ) )
                {
                    var dInfo = new DirectoryInfo( directory );

                    // Apply filters
                    if ( ( dInfo.Attributes & FileAttributes.Hidden ) == FileAttributes.Hidden )
                    {
                        continue;
                    }

                    if ( ( dInfo.Attributes & FileAttributes.System ) == FileAttributes.System )
                    {
                        continue;
                    }

                    if ( !imagesSmall.Images.ContainsKey( dInfo.FullName ) )
                    {
                        imagesSmall.Images.Add( dInfo.FullName, FileIcons.GetSmallIcon( dInfo.FullName ) );
                    }

                    var fileType = FileTypes.GetFileTypeDescription( dInfo.FullName );
                    var lfItem = new ListViewItem {
                        Tag = dInfo,
                        Text = dInfo.Name,
                        ImageKey = dInfo.FullName,
                    };

                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{dInfo.LastWriteTime:g}" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, fileType ) );

                    listBrowser.Items.Add( lfItem );

                    // Make a default selection if there isn't a provided one
                    if ( string.IsNullOrEmpty( selectionItem ) )
                    {
                        selectionItem = dInfo.Name;
                    }

                    if ( dInfo.Name.Equals( selectionItem ) )
                    {
                        lfItem.Selected = true;
                        lfItem.Focused = true;
                    }
                }

                foreach ( var file in Directory.GetFiles( location ) )
                {
                    var fInfo = new FileInfo( file );

                    // Apply filters
                    if ( ( fInfo.Attributes & FileAttributes.Hidden ) == FileAttributes.Hidden )
                    {
                        continue;
                    }

                    if ( ( fInfo.Attributes & FileAttributes.System ) == FileAttributes.System )
                    {
                        continue;
                    }

                    if ( !imagesSmall.Images.ContainsKey( fInfo.FullName ) )
                    {
                        imagesSmall.Images.Add( fInfo.FullName, FileIcons.GetSmallIcon( fInfo.FullName ) );
                    }

                    var length = Math.Ceiling( (double) fInfo.Length / 1024 );
                    var fileType = FileTypes.GetFileTypeDescription( fInfo.FullName );

                    var lfItem = new ListViewItem
                    {
                        Tag = fInfo,
                        Text = fInfo.Name,
                        ImageKey = fInfo.FullName,
                    };

                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{fInfo.LastWriteTime:g}" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{length:N0} KB" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, fileType ) );

                    listBrowser.Items.Add( lfItem );

                    // Make a default selection if there isn't a provided one
                    if ( string.IsNullOrEmpty( selectionItem ) )
                    {
                        selectionItem = fInfo.Name;
                    }

                    if ( fInfo.Name.Equals( selectionItem ) )
                    {
                        lfItem.Selected = true;
                        lfItem.Focused = true;
                    }
                }

                // Push the location onto the history stack, but not if we're just refreshing
                if ( directoryHistory.Count == 0 || !directoryHistory.Peek().Equals( locationInfo ) )
                {
                    directoryHistory.Push( locationInfo );

                    if ( mruHistory.ContainsKey( locationInfo ) )
                    {
                        mruHistory[ locationInfo ]++;
                    }
                    else
                    {
                        mruHistory[ locationInfo ] = 0;
                    }
                }
            }
            finally
            {
                listBrowser.EndUpdate();
            }
        }

        private void listBrowser_ColumnClick( object sender, ColumnClickEventArgs e )
        {
            if ( e.Column == listViewColumnSorter.SortColumn )
            {
                // Reverse the current sort direction for this column.
                if ( listViewColumnSorter.Order == SortOrder.Ascending )
                {
                    listViewColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    listViewColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                listViewColumnSorter.SortColumn = e.Column;
                listViewColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            listBrowser.Sort();
        }

        private void textLocation_KeyDown( object sender, KeyEventArgs e )
        {
            e.Handled = Utilities.ProcessKeyStoke( e, (k) => Commands.Process( k, Control.AddressBar, Context ) );
        }

        private void listBrowser_KeyDown( object sender, KeyEventArgs e )
        {
            e.Handled = Utilities.ProcessKeyStoke( e, ( k ) => Commands.Process( k, Control.List, Context ) );
        }

        private void listBrowser_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            // If on an item, do something. Otherwise...?
            var item = listBrowser.GetItemAt( e.Location.X, e.Location.Y );
            if ( item != null && item.Tag is FileSystemInfo )
            {
                // No configuration here, merely do a standard Open here
                Commands.Run( BuiltInCommands.Open, Context );
            }
        }

        class FlixContext : IContext
        {
            private readonly MainForm Form;

            public FlixContext( MainForm form )
            {
                Form = form;
            }

            public override IEnumerable<FileSystemInfo> SelectedItems
            {
                get
                {
                    var selection = new List<FileSystemInfo>();
                    foreach ( var item in Form.listBrowser.SelectedItems )
                    {
                        if ( item is ListViewItem )
                        {
                            selection.Add( ( item as ListViewItem ).Tag as FileSystemInfo );
                        }
                    }
                    return selection;
                }
            }

            public override FileSystemInfo FocusedItem
            {
                get
                {
                    var item = Form.listBrowser.FocusedItem;
                    return item == null ? null : ( item as ListViewItem ).Tag as FileSystemInfo;
                }
            }

            public override DirectoryInfo CurrentLocation => Form.listBrowser.Tag as DirectoryInfo;

            public override void SwitchToList()
            {
                if ( !Form.listBrowser.Focused )
                {
                    Form.listBrowser.Focus();
                }
            }

            public override void SwitchToAddressBar()
            {
                if ( !Form.textLocation.Focused )
                {
                    Form.textLocation.Focus();
                }

                Form.textLocation.SelectAll();
            }

            public override void OpenWithDefaultProgram( FileSystemInfo fileInfo )
            {
                new Task( () =>
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo( fileInfo.FullName )
                    {
                        UseShellExecute = true
                    };
                    p.Start();
                } ).Start();
            }

            public override void PreviousDirectory()
            {
                // Previous folder from history
                if ( Form.directoryHistory.Count > 1 )
                {
                    // Take the current location off the stack
                    var current = Form.directoryHistory.Pop();
                    var previous = Form.directoryHistory.Pop();

                    // If we are going back up the hierarchy, try and retain the current folder as the selected/focused one
                    Form.OpenDirectoryView( previous.FullName, String.Equals( previous.FullName, current.Parent.FullName ) ? current.Name : "" );
                }
            }

            public override void UpDirectory()
            {
                // Go upto parent
                var currentDirectory = CurrentLocation;
                var parent = Directory.GetParent( currentDirectory.FullName );
                if ( parent != null )
                {
                    Form.OpenDirectoryView( parent.FullName, currentDirectory.Name );
                }
            }

            public override void OpenDirectory( FileSystemInfo fileSystemInfo )
            {
                if ( IsDirectory( fileSystemInfo ) )
                {
                    Form.OpenDirectoryView( fileSystemInfo.FullName );
                }
                else if( fileSystemInfo is FileInfo )
                {
                    var fileInfo = fileSystemInfo as FileInfo;
                    Form.OpenDirectoryView(  fileInfo.DirectoryName, fileInfo.Name );
                }
            }

            public override void OpenFromAddressBar( string location = "" )
            {
                if ( !string.IsNullOrEmpty( location ) )
                {
                    Form.textLocation.Text = location;
                }

                Form.OpenDirectoryView( Form.textLocation.Text );
            }
        }
    }
}
