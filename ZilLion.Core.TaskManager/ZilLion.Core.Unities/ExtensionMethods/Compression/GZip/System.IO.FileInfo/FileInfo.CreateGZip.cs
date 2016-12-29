





using System.IO;
using System.IO.Compression;

public static partial class Extensions
{
    /// <summary>
    ///     A FileInfo extension method that creates a zip file.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    public static void CreateGZip(this FileInfo @this)
    {
        using (FileStream originalFileStream = @this.OpenRead())
        {
            using (FileStream compressedFileStream = File.Create(@this.FullName + ".gz"))
            {
                using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressionStream);
                }
            }
        }
    }

    /// <summary>
    ///     A FileInfo extension method that creates a zip file.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="destination">Destination for the zip.</param>
    public static void CreateGZip(this FileInfo @this, string destination)
    {
        using (FileStream originalFileStream = @this.OpenRead())
        {
            using (FileStream compressedFileStream = File.Create(destination))
            {
                using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressionStream);
                }
            }
        }
    }

    /// <summary>
    ///     A FileInfo extension method that creates a zip file.
    /// </summary>
    /// <param name="this">The @this to act on.</param>
    /// <param name="destination">Destination for the zip.</param>
    public static void CreateGZip(this FileInfo @this, FileInfo destination)
    {
        using (FileStream originalFileStream = @this.OpenRead())
        {
            using (FileStream compressedFileStream = File.Create(destination.FullName))
            {
                using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                {
                    originalFileStream.CopyTo(compressionStream);
                }
            }
        }
    }
}