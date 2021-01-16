using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        private readonly ListViewColumnSorter listViewColumnSorter = new ListViewColumnSorter();

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

        private void textLocation_PreviewKeyDown( object sender, PreviewKeyDownEventArgs e )
        {
            if ( e.KeyCode == Keys.D && e.Alt )
            {
                textLocation.SelectAll();
            }
        }

        private void textLocation_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return )
            {
                OpenDirectoryView( textLocation.Text );
                e.Handled = true;
            }
            else if ( e.KeyCode == Keys.Down )
            {
                listBrowser.Focus();
                e.Handled = true;
            }
        }

        private void listBrowser_PreviewKeyDown( object sender, PreviewKeyDownEventArgs e )
        {
            if ( e.KeyCode == Keys.D && e.Alt )
            {
                textLocation.Focus();
                textLocation.SelectAll();
            }
        }

        private void listBrowser_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return )
            {
                foreach ( var selectedItem in listBrowser.SelectedItems )
                {
                    var item = ( selectedItem as ListViewItem ).Tag;
                    if ( item is DirectoryInfo )
                    {
                        OpenDirectoryView( ( item as DirectoryInfo ).FullName );
                    }
                }
                e.Handled = true;
            }
            else if ( e.KeyCode == Keys.Back )
            {
                // Previous folder from history
                if ( directoryHistory.Count > 1 )
                {
                    // Take the current location off the stack
                    var current = directoryHistory.Pop();
                    var previous = directoryHistory.Pop();

                    // If we are going back up the hierarchy, try and retain the current folder as the selected/focused one
                    OpenDirectoryView( previous.FullName, String.Equals( previous.FullName, current.Parent.FullName ) ? current.Name : "" );
                }
            }
            else if ( e.KeyCode == Keys.Right )
            {
                // Go into focused child
                var focusedItem = listBrowser.FocusedItem;
                if ( focusedItem != null )
                {
                    if ( focusedItem.Tag is DirectoryInfo )
                    {
                        OpenDirectoryView( (focusedItem.Tag as DirectoryInfo).FullName );
                    }
                }
            }
            else if ( e.KeyCode == Keys.Left )
            {
                // Go upto parent
                var currentLocation = listBrowser.Tag;
                if ( currentLocation is DirectoryInfo )
                {
                    var currentDirectory = currentLocation as DirectoryInfo;
                    var parent = Directory.GetParent( currentDirectory.FullName );
                    if ( parent != null )
                    {
                        OpenDirectoryView( parent.FullName, currentDirectory.Name );
                    }
                }
            }
        }

        private void listBrowser_MouseDoubleClick( object sender, MouseEventArgs e )
        {
            var item = listBrowser.GetItemAt( e.Location.X, e.Location.Y );
            if ( item != null )
            {
                if ( item.Tag is DirectoryInfo )
                {
                    OpenDirectoryView( ( item.Tag as DirectoryInfo ).FullName );
                }
            }
        }
    }
}
