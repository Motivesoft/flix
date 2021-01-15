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

        private readonly List<string> directoryHistory = new List<string>();

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

                foreach ( var file in Directory.GetFiles( location ) )
                {
                    var fInfo = new FileInfo( file );
                    var length = Math.Ceiling( (double) fInfo.Length / 1024 );
                    var fileType = FileTypes.GetFileTypeDescription( fInfo.FullName );

                    if ( !imagesSmall.Images.ContainsKey( fInfo.FullName ) )
                    {
                        imagesSmall.Images.Add( fInfo.FullName, FileIcons.GetSmallIcon( fInfo.FullName ) );
                    }

                    var lfItem = new ListViewItem();
                    lfItem.Tag = fInfo;
                    lfItem.Text = fInfo.Name;
                    lfItem.ImageKey = fInfo.FullName;

                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{fInfo.LastWriteTime:g}" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, $"{length:N0} KB" ) );
                    lfItem.SubItems.Add( new ListViewItem.ListViewSubItem( lfItem, fileType ) );

                    listBrowser.Items.Add( lfItem );
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
    }
}
