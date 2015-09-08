using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace RRInfoCollector
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //CreateRemoteRecordersJson();
            //CreateSystemInfoJson();
            if (!RestoreOrig())
                return;
            Application.EnableVisualStyles();
            Application.Run(new frmMain());
        }

        private static bool RestoreOrig()
        {
            do
            {
                try
                {
                    var fInfo = new FileInfo(Path.Combine("info", "recorders.json"));
                    var oInfo = new FileInfo(fInfo.FullName + ".orig");
                    if (oInfo.Exists)
                    {
                        if (fInfo.Exists)
                            oInfo.Delete();
                        else
                            oInfo.MoveTo(fInfo.FullName);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    if (MessageBox.Show(
                        string.Format(
                            "Orjinal dosya kontrolü sırasında '{0}' hatası oluştu. Tekrar denemek istiyor musunuz?",
                            e.Message), "Lütfen Yanıtlayınız", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1) == DialogResult.No)
                        return false;
                }
            } while (true);
        }

        private static void CreateRemoteRecordersJson()
        {
            using (
                var conn = new SqlConnection("Data Source=.;User Id=sa; Password=@aa11aa!;Initial Catalog=SMGSC_CONF"))
            {
                conn.Open();
                var recorders = new Dictionary<string, RemoteRecorderInfo<TreeNode>>();
                using (var cmd = new SqlCommand(@"SELECT RECORDERNAME FROM REMOTE_RECORDER_LIST ORDER BY RECORDERNAME", conn))
                {
                    using (var rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            var r = new RemoteRecorderInfo<TreeNode> { Name = rs.GetString(0) };
                            recorders[r.Name] = r;
                        }
                    }
                }
                SerializeTo(@"info\recorders.json", recorders);
            }
        }

        private static void SerializeTo<T>(string file, T data)
        {
            var f = new FileInfo(file);
            var mode = f.Exists ? FileMode.Truncate : FileMode.CreateNew;
            if (mode == FileMode.CreateNew && f.Directory != null && !f.Directory.Exists)
            {
                f.Directory.Create();
                f.Refresh();
            }
            var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
            using (
                var fs = new FileStream(f.FullName, mode))
            {
                json.WriteObject(fs, data);
            }

        }

        private static void CreateSystemInfoJson()
        {
            using (
                var conn = new SqlConnection("Data Source=.;User Id=sa; Password=@aa11aa!;Initial Catalog=SMGSC_CONF"))
            {
                conn.Open();
                var sysInfo = new SystemInfo
                    {
                        ShortNotations = new List<string>(),
                        SystemLookup = new Dictionary<string, RemoteRecorderSystem>()
                    };

                using (var cmd = new SqlCommand(@"SELECT ID,NAME FROM REMOTE_RECORDER_SYSTEMS", conn))
                {
                    using (var rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            var sys = new RemoteRecorderSystem { Id = rs.GetInt32(0), Name = rs.GetString(1) };
                            sysInfo.SystemLookup[sys.Name] = sys;
                        }
                    }
                }
                using (var cmd = new SqlCommand(@"SELECT DISTINCT SHORT_NOTATION
FROM REMOTE_RECORDER_TYPE_PARAMETERS", conn))
                {
                    using (var rs = cmd.ExecuteReader())
                    {
                        while (rs.Read())
                        {
                            sysInfo.ShortNotations.Add(rs.GetString(0));
                        }
                    }
                }

                SerializeTo(@"info\systemInfo.json", sysInfo);
            }
        }
    }
}
