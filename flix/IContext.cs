﻿using System.Collections.Generic;
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

        public bool IsDirectory( FileSystemInfo fileSystemInfo )
        {
            return fileSystemInfo is DirectoryInfo;
        }

        public bool IsFile( FileSystemInfo fileSystemInfo )
        {
            return fileSystemInfo is FileInfo;
        }

        public abstract void OpenDirectory( FileSystemInfo fileSystemInfo );
        public abstract void OpenWithDefaultProgram( FileSystemInfo fileInfo );
    }
}
