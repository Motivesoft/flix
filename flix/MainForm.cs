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

        private readonly Queue<string> directoryHistory = new Queue<string>();

        private readonly ListViewColumnSorter listViewColumnSorter = new ListViewColumnSorter();

        public MainForm( string[] args )
        {
            InitializeComponent();

            // Additional forms initialization
            listBrowser.ListViewItemSorter = listViewColumnSorter;

            // Startup
            var location = defaultLocation;
            if ( args.Length > 0 )
            {
                var suppliedLocation = args[ 0 ];
                if ( Directory.Exists( suppliedLocation ) )
                {
                    location = suppliedLocation;
                }
                else if ( File.Exists( suppliedLocation ) )
                {
                    location = Directory.GetParent( suppliedLocation ).FullName;
                }
            }

            // Run
            OpenDirectoryView( location );
        }

        private void OpenDirectoryView( string location )
        {
            textLocation.Text = location;

            try
            {
                listBrowser.BeginUpdate();
                listBrowser.Items.Clear();
                listBrowser.Tag = location;

                foreach ( var directory in Directory.GetDirectories( location ) )
                {
                    var dInfo = new DirectoryInfo( directory );
                    var fileType = FileTypes.GetFileTypeDescription( dInfo.FullName );

                    if ( !imagesSmall.Images.ContainsKey( dInfo.FullName ) )
                    {
                        imagesSmall.Images.Add( dInfo.FullName, FileIcons.GetSmallIcon( dInfo.FullName ) );
                    }

                    var lfItem = new ListViewItem {
                        Tag = dInfo,
                        Text = dInfo.Name,
                        ImageKey = dInfo.FullName,
                    };

                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{dInfo.LastWriteTime:g}" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, fileType ) );

                    listBrowser.Items.Add( lfItem );
                }

                foreach ( var file in Directory.GetFiles( location ) )
                {
                    var fInfo = new FileInfo( file );
                    var length = Math.Ceiling( (double) fInfo.Length / 1024 );
                    var fileType = FileTypes.GetFileTypeDescription( fInfo.FullName );

                    if ( !imagesSmall.Images.ContainsKey( fInfo.FullName ) )
                    {
                        imagesSmall.Images.Add( fInfo.FullName, FileIcons.GetSmallIcon( fInfo.FullName ) );
                    }

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
                }

                var locationPath = Path.GetFullPath( location );
                if ( directoryHistory.Count > 0 )
                {
                    if ( locationPath.Equals( Path.GetFullPath( directoryHistory.Peek() ) ) )
                    {
                        directoryHistory.Dequeue();
                    }
                }

                directoryHistory.Enqueue( locationPath );
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
                // Remove the directory we're running from
                if ( directoryHistory.Count > 0 )
                {
                    OpenDirectoryView( directoryHistory.Dequeue() );
                }
            }
        }
    }
}
