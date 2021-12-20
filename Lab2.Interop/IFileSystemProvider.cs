using System;
using System.Collections.Generic;
using System.Text;

namespace Lab2.Interop
{
    public interface IFileSystemProvider
    {
        IFileSystemInterop CreateNewFileSystem(string fsName, ushort descriptorsCount);

        IFileSystemInterop MountExistingFileSystem(string fsName);
    }
}
