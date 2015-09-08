using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Natek.Helpers.IO
{
    public static class FileSystemHelper
    {
        public static readonly Regex RegFilenameNumericStyle = new Regex("([^0-9]+)|([0-9]+)", RegexOptions.Compiled);

        public static int CompareFilesIgnoreCase(string l, string r)
        {
            return CompareFiles(l, r, true);
        }

        public static int CompareFilesNoIgnoreCase(string l, string r)
        {
            return CompareFiles(l, r, false);
        }

        public static int CompareFilesIgnoreCase(FileSystemInfo l, FileSystemInfo r)
        {
            return CompareFiles(l, r, true);
        }

        public static int CompareFilesNoIgnoreCase(FileSystemInfo l, FileSystemInfo r)
        {
            return CompareFiles(l, r, false);
        }

        public static int CompareFiles(FileSystemInfo l, FileSystemInfo r, bool ignoreCase)
        {
            return CompareFiles(l.Name, r.Name, ignoreCase);
        }

        public static int CompareFiles(string l, string r, bool ignoreCase)
        {
            var mL = RegFilenameNumericStyle.Match(l);
            var mR = RegFilenameNumericStyle.Match(r);

            do
            {
                if (mL.Success)
                {
                    if (mR.Success)
                    {
                        var diff = mL.Groups[2].Success && mR.Groups[2].Success
                                   ? Int32.Parse(mL.Groups[2].Value) - Int32.Parse(mR.Groups[2].Value)
                                   : String.Compare(mL.Groups[1].Value, mR.Groups[1].Value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                        if (diff != 0)
                            return diff;
                        mL = mL.NextMatch();
                        mR = mR.NextMatch();
                    }
                    else
                        return 1;
                }
                else if (mR.Success)
                    return -1;
                else
                    return 0;
            } while (true);
        }

        public static bool CreateDirectory(string path, out Exception error)
        {
            try
            {
                var fInfo = new FileInfo(path);
                if (fInfo.Exists)
                {
                    error = new Exception("Cannot create directory, file with the same name exist:" + fInfo.FullName);
                    return false;
                }

                var dInfo = new DirectoryInfo(path);
                if (!dInfo.Exists)
                    dInfo.Create();
                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public static bool CreateDirectoryOf(string file, out Exception error)
        {
            try
            {
                var fInfo = new FileInfo(file);
                if (fInfo.Directory != null && !fInfo.Directory.Exists)
                    fInfo.Directory.Create();
                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e;
            }
            return false;
        }

        public static string FileNameOf(string fullName, string separator)
        {
            if (fullName == null)
                return null;
            if (separator == null)
                separator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

            while (fullName.EndsWith(separator))
                fullName = fullName.Remove(fullName.Length - separator.Length);
            var index = fullName.LastIndexOf(separator, StringComparison.InvariantCulture);
            return index >= 0 ? fullName.Substring(index + separator.Length) : fullName;
        }

        public static bool CreateFileOf(string file, out Exception error)
        {
            try
            {
                var fInfo = new FileInfo(file);
                if (!fInfo.Exists)
                    fInfo.Create();
                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }
    }
}
