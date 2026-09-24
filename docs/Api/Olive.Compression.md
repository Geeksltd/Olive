**Olive.Compression**

**Overview:**
The `Olive.Compression` provides methods for compressing and decompressing files and directories using different compression formats, such as ZIP, GZIP, and TAR.

**Methods:**

1. **Compress(IEnumerable<FileInfo>, CompressionFormat)**
   - Compresses a list of files into a specified compression format.
   - Throws an exception if duplicate filenames exist.
   - Returns the compressed data as a byte array.

2. **Compress(DirectoryInfo, CompressionFormat)**
   - Compresses an entire directory into a specified compression format.
   - Returns the compressed data as a byte array.

3. **Compress(DirectoryInfo, FileInfo, CompressionFormat, bool)**
   - Creates a compressed file from a directory and saves it to a specified destination.
   - Supports ZIP, GZIP, and TAR formats.
   - Throws an exception if the destination file already exists unless overwrite is set to `true`.

4. **Decompress(FileInfo, DirectoryInfo, bool, bool)**
   - Decompresses a compressed file into a specified directory.
   - Supports extracting full paths and overwriting existing files.

---

**SevenZip Documentation**

**Overview:**
The `SevenZip` class provides an interface to compress and decompress files using the 7-Zip utility.

**Properties:**

- `SEVEN_ZIP_EXE_FILE_PATH`: Specifies the path of the 7-Zip executable.

**Methods:**

1. **Compress(string, string[])**
   - Compresses specified folders into a 7-Zip archive.

2. **Compress(string, int?, string[])**
   - Supports splitting archives into parts of a specified size (in KB).

3. **Compress(string, int?, CompressionMode, string, string[], string[])**
   - Compresses folders into a 7-Zip archive with custom parameters and optional exclusion filters.

4. **Compress(IEnumerable<FileInfo>, CompressionMode, string)**
   - Compresses a list of files into a temporary 7-Zip file and returns the resulting file.

5. **CompressToBytes(IEnumerable<FileInfo>, CompressionMode, string)**
   - Compresses a list of files into a 7-Zip archive and returns the compressed data as a byte array.

6. **Compress(FileInfo, IEnumerable<FileInfo>, CompressionMode, string)**
   - Compresses specified files into a 7-Zip archive and saves it to a specified destination.

