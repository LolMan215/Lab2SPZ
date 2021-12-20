using Lab2.Domain.Enums;
using Lab2.Domain.Models;
using Lab2.Interop.Extensions;
using Lab2.Interop.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab2.Interop
{
    namespace TinyFs.Interop
    {
        public class FileSystemProvider : IFileSystemProvider
        {
            public IFileSystemInterop CreateNewFileSystem(string fsName, ushort descriptorsCount)
            {
                if (descriptorsCount >= FileSystemSettings.NullDescriptor)
                {
                    throw new Exception("Maximum descriptor count exceeded");
                }

                var file = File.Open(fsName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                file.Flush();

                file.WriteObject(descriptorsCount, 0);
                var bitArray = new BitArray(
                    FileSystemSettings.BlocksCount,
                    FileSystemSettings.BitmaskFreeBit);
                bitArray[0] = !FileSystemSettings.BitmaskFreeBit;
                var buffer = new byte[OpHelper.DivWithRoundUp(FileSystemSettings.BlocksCount, 8)];
                bitArray.CopyTo(buffer, 0);
                file.WriteBytes(buffer, FileSystemSettings.BitMapOffset);

                byte zero = 0;

                byte[] descriptorsSpace =
                    Enumerable.Repeat(zero, SizeHelper.GetStructureSize<FileDescriptor>() * descriptorsCount).ToArray();
                file.WriteBytes(descriptorsSpace, FileSystemSettings.DescriptorsOffset);

                var root = new FileDescriptor
                {
                    Id = 0,
                    FileDescriptorType = FileDescriptorType.Directory,
                    FileSize = 0,
                    References = 1,
                    Blocks = new ushort[]
                    {
                    0, 0, 0, 0,
                    },
                    MapIndex = 0
                };

                file.WriteObject(root, FileSystemSettings.DescriptorsOffset);

                return new FileSystem(file);
            }

            public IFileSystemInterop MountExistingFileSystem(string fsName)
            {
                var file = File.Open(fsName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                return new FileSystem(file);
            }
        }
    }
}
