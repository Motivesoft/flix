﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace flix
{
    public class ListViewColumnSorter : IComparer
    {
        private readonly Dictionary<Type, int> SortableTypes = new Dictionary<Type, int>();

        /// <summary>
        /// Specifies the column to be sorted
        /// </summary>
        private int ColumnToSort;

        /// <summary>
        /// Specifies the order in which to sort (i.e. 'Ascending').
        /// </summary>
        private SortOrder OrderOfSort;

        /// <summary>
        /// Case insensitive comparer object
        /// </summary>
        private CaseInsensitiveComparer ObjectCompare;

        /// <summary>
        /// Class constructor. Initializes various elements
        /// </summary>
        public ListViewColumnSorter()
        {
            // Initialize the column to '0'
            ColumnToSort = 0;

            // Initialize the sort order to 'none'
            OrderOfSort = SortOrder.None;

            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();

            SortableTypes[ typeof( DirectoryInfo ) ] = 1;
            SortableTypes[ typeof( FileInfo ) ] = 2;
        }

        public int Compare( object x, object y )
        {
            int compareResult = 0;

            ListViewItem xItem = x as ListViewItem;
            ListViewItem yItem = y as ListViewItem;

            // Failsafe
            if ( xItem == null || yItem == null )
            {
                return compareResult;
            }

            // Group things together - sort by type if different
            var xTypeValue = SortableTypes.GetValueOrDefault( xItem.Tag.GetType(), 0 );
            var yTypeValue = SortableTypes.GetValueOrDefault( yItem.Tag.GetType(), 0 );
            if ( xTypeValue != yTypeValue )
            {
                compareResult = xTypeValue.CompareTo( yTypeValue );
            }
            else
            {
                switch ( ColumnToSort )
                {
                    case 0:
                        compareResult = ObjectCompare.Compare( xItem.Text, yItem.Text );
                        break;

                    case 1: // Date
                    {
                        var xDate = ( xItem.Tag is FileSystemInfo ) ? ( xItem.Tag as FileSystemInfo ).LastWriteTime : DateTime.MinValue;
                        var yDate = ( yItem.Tag is FileSystemInfo ) ? ( yItem.Tag as FileSystemInfo ).LastWriteTime : DateTime.MinValue;

                        compareResult = xDate.CompareTo( yDate );
                        break;
                    }

                    case 2: // Length
                    {
                        var xLength = ( xItem.Tag is FileInfo ) ? ( xItem.Tag as FileInfo ).Length : -1;
                        var yLength = ( yItem.Tag is FileInfo ) ? ( yItem.Tag as FileInfo ).Length : -1;

                        compareResult = xLength.CompareTo( yLength );
                        break;
                    }

                    case 3: // Type
                        compareResult = ObjectCompare.Compare( xItem.SubItems[ 3 ].Text, yItem.SubItems[ 3 ].Text );
                        break;
                }
            }

            // Ascending or descending
            if ( OrderOfSort == SortOrder.Descending )
            {
                compareResult = -compareResult;
            }

            return compareResult;
        }

        /// <summary>
        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
        /// </summary>
        public int SortColumn
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        /// <summary>
        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
        /// </summary>
        public SortOrder Order
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }


    }
}
