using System.Collections.Generic;
using System.IO;

namespace flix
{
    public abstract class IContext
    {
        public abstract IEnumerable<FileSystemInfo> SelectedItems
        {
            get;
        }

        public abstract FileSystemInfo FocusedItem
        {
            get;
        }

        public abstract DirectoryInfo CurrentLocation
        {
            get;
        }

        public bool IsDirectory( FileSystemInfo fileSystemInfo )
        {
            return fileSystemInfo is DirectoryInfo;
        }

        public bool IsFile( FileSystemInfo fileSystemInfo )
        {
            return fileSystemInfo is FileInfo;
        }

        public abstract void SwitchToList();
        public abstract void SwitchToAddressBar();
        public abstract void UpDirectory();
        public abstract void OpenWithDefaultProgram( FileSystemInfo fileInfo );
        public abstract void OpenDirectory( FileSystemInfo fileSystemInfo );
        public abstract void OpenFromAddressBar( string location = "" );
    }
}
