using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication1.Properties;

namespace WindowsFormsApplication1
{
    public partial class RecorderProjectsCsprojUpdater : Form
    {
        List<string> projectFileList = new List<string>();
        private bool isRelease;
        private bool ReleasePath;
        private bool DebugPath;

        public RecorderProjectsCsprojUpdater()
        {
            InitializeComponent();
        }

        public void GetProjectFiles(string sDir, string extension)
        {
            try
            {
                foreach (string directories in Directory.GetDirectories(sDir))
                {
                    foreach (string files in Directory.GetFiles(directories, extension))
                    {
                        projectFileList.Add(files);
                    }
                    GetProjectFiles(directories, extension);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }//GetProjectFiles

        public static string After(string value, string a, int type)
        {
            //type = 0 first
            //type = 1 last
            int posA = 0;

            if (type == 1)
            {
                posA = value.IndexOf(a, StringComparison.Ordinal);
            }
            else if (type == 0)
            {
                posA = value.LastIndexOf(a, StringComparison.Ordinal);
            }

            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }

        public static string Before(string value, string a, int type)
        {

            int posA = 0;

            if (type == 1)
            {
                posA = value.LastIndexOf(a, StringComparison.Ordinal);
            }

            if (type == 0)
            {
                posA = value.IndexOf(a, StringComparison.Ordinal);
            }

            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        } // Before

        public static string Between(string value, string a, string b)
        {
            int posA = value.IndexOf(a, StringComparison.Ordinal);
            int posB = value.LastIndexOf(b, StringComparison.Ordinal);

            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        } // Between

        private void LstListBoxMouseDoubleClick(object sender, MouseEventArgs e)
        {
            string asd = LstListBox.SelectedItem.ToString();
            Process.Start(asd.Split('.')[1].Trim());
        }

        private void BtnOpenFolderClick(object sender, EventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog { SelectedPath = txtFilePath.Text };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void BtnStartClick(object sender, EventArgs e)
        {
            StartReplacer();
            //OldCsProjReplace();
        }

        public void OldCsProjReplace()
        {
            try
            {
                GetProjectFiles(txtFilePath.Text, "*.csproj");
                projectFileList.Sort();
                int i = 0;
                foreach (var v in projectFileList)
                {
                    //File.Delete(v);
                    //if (File.Exists(v + ".old"))
                    //{
                    //    File.Move(v + ".old", v);
                    //}
                    LstListBox.Items.Add(i + ". " + v);
                    ++i;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MessageBox.Show(Resources.Form1_StartReplacer_Operation_Completed_);

        }
        bool delete = false;

        public void StartReplacer()
        {
            string currentProject = "";
            try
            {
                GetProjectFiles(txtFilePath.Text, txtFileExtension.Text);
                int i = 1;
                projectFileList.Sort();
                foreach (var v in projectFileList)
                {
                    LstListBox.Items.Add(i + ". " + v);

                    if (File.Exists(v + ".tmp"))
                    {
                        File.Delete(v + ".tmp");
                    }

                    if (File.Exists(v + ".old"))
                    {
                        File.Delete(v + ".old");
                    }

                    if (File.Exists(v + ".old_old"))
                    {
                        File.Delete(v + ".old_old");
                    }

                    var reader = new StreamReader(v, Encoding.UTF8);
                    var writer = new StreamWriter(v + ".tmp", true, Encoding.ASCII);

                    string line;
                    currentProject = v;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //if (line.Trim() == "<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \">")
                        //{
                        //    ReleasePath = true;
                        //}

                        //if (line.Trim() == "<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">")
                        //{
                        //    DebugPath = true;
                        //}

                        //if (ReleasePath)
                        //{
                        //    //if (!string.IsNullOrEmpty(line) && line.Contains("DebugType"))
                        //    //{
                        //    //    const string newReplacePdbText = "none";
                        //    //    string oldReplacePdbText = Between(line, "<DebugType>", "</DebugType>");
                        //    //    line = line.Replace(oldReplacePdbText, newReplacePdbText);
                        //    //}

                        //    if (!string.IsNullOrEmpty(line) && line.Contains("OutputPath"))
                        //    {
                        //        //const string newReplaceText = @"..\..\..\..\Latest DLL";
                        //        //const string newReplaceText = @"..\..\..\..\..\Latest DLL\Specific Implementations";
                        //        const string newReplaceText = @"bin\Release\";
                        //        string oldReplaceText = Between(line, "<OutputPath>", "</OutputPath>");
                        //        line = line.Replace(oldReplaceText, newReplaceText);
                        //        ReleasePath = false;
                        //    }
                        //}

                        //if (DebugPath)
                        //{
                        //    if (!string.IsNullOrEmpty(line) && line.Contains("OutputPath"))
                        //    {
                        //        //const string newRelaceText = @"..\..\..\Libraries\Base\";
                        //        //const string newRelaceText = @"..\..\..\..\Libraries\Obsolote\";
                        //        //const string newRelaceText = @"..\..\..\..\Libraries\Specific Implementations\";
                        //        const string newRelaceText = @"bin\Debug\";
                        //        string oldReplaceText = Between(line, "<OutputPath>", "</OutputPath>");
                        //        line = line.Replace(oldReplaceText, newRelaceText);
                        //        DebugPath = false;
                        //    }
                        //}

                        //if (!string.IsNullOrEmpty(line) && line.Contains("HintPath"))
                        //{
                        //    const string newRelaceText = @"..\..\..\..\Libraries\Base";
                        //    string oldReplaceText = Between(line, "<HintPath>", "</HintPath>");
                        //    string oldPath = Before(oldReplaceText, "\\", 1);
                        //    line = line.Replace(oldPath, newRelaceText);
                        //}

                        if (line.Contains("<PostBuildEvent>"))
                        {
                            delete = true;
                        }

                        if (line.Contains("</PostBuildEvent>"))
                        {
                            line =
                                "<PostBuildEvent>pushd ..\\..\\..\\..\\..\\Libraries\\Base\\\r\n" +
                                "set COPY_DIR=%25CD%25\r\n" +
                                "popd\r\n" +
                                "copy \"$(TargetPath)\" \"%25COPY_DIR%25\"\r\n" +
                                "if \"$(ConfigurationName)\" NEQ \"Release\" goto NotRelease\r\n" +
                                "pushd ..\\..\\..\\..\\..\\..\\\"Latest DLL\\\"\r\n" +
                                "set COPY_DIR=%25CD%25\r\n" +
                                "copy \"$(TargetPath)\" \"%25COPY_DIR%25\"\r\n" +
                                "svn add --force \"$(TargetFileName)\"\r\n" +
                                "svn commit -m \"svn Auto Commit Operation $(TargetFileName)\"\r\n" +
                                ":NotRelease\r\n" +
                                "</PostBuildEvent>";
                            delete = false;
                        }

                        if (delete)
                        {
                            line = "";
                        }

                        if (!delete)
                        {
                            writer.WriteLine(line);
                            writer.Flush();
                        }
                    }
                    writer.Close();
                    reader.Close();

                    if (File.Exists(v))
                    {
                        if (File.Exists(v + ".old"))
                        {
                            File.Move(v + ".old", v + ".old_old");
                        }
                        File.Move(v, v + ".old");
                    }

                    if (File.Exists(v + ".tmp"))
                    {
                        File.Move(v + ".tmp", v);
                    }
                    i++;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + currentProject);
            }

            MessageBox.Show(Resources.Form1_StartReplacer_Operation_Completed_);
        } // StartReplacer

        private void Form1Load(object sender, EventArgs e)
        {

        }

        private void BtnCompileClick(object sender, EventArgs e)
        {
            // Libraries\Base altındaki *Recorder.dll'ler silinip yeniden Compile oldu.

            GetProjectFiles(txtFilePath.Text, txtFileExtension.Text.Trim());
            LstListBox.Items.Clear();
            int i = 0;
            projectFileList.Sort();
            foreach (var v in projectFileList)
            {
                LstListBox.Items.Add(i + ". " + v);
                i++;
            }
            int j = 1;
            foreach (var v in projectFileList)
            {
                CompileProjects(v);
                label3.Text = string.Format("Total {0} Project, Current {1}, {2} ", projectFileList.Count,
                                            v.Split('\\')[v.Split('\\').Length - 1],
                                            j.ToString(CultureInfo.InvariantCulture));
                ++j;
            }
            CompileProjects(txtFileExtension.Text);
            MessageBox.Show(Resources.Form1_StartReplacer_Operation_Completed_);
        }

        public void CompileProjects(string slnPath)
        {
            if (slnPath != null)
            {
                try
                {
                    const string exeName = @"C:\Windows\Microsoft.NET\Framework64\v3.5\MSBuild.exe";
                    //slnPath = @"M:\RecordersTrunk\Base\ParserRecorders\CiscoDhcpBindV_1_0_0Recorder\CiscoDhcpBindV_1_0_0Recorder.sln";
                    string arguments;
                    if (isRelease)
                        arguments = "/property:Configuration=Release " + " " + "\"" + slnPath + "\"";
                    else
                        arguments = "/property:Configuration=Debug " + " " + "\"" + slnPath + "\"";

                    int exitCode;
                    string errorMessage;
                    var start = new ProcessStartInfo
                    {
                        Arguments = arguments,
                        FileName = exeName,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = false
                    };

                    using (var proc = Process.Start(start))
                    {
                        errorMessage = proc.StandardError.ReadToEnd();
                        proc.WaitForExit();
                        exitCode = proc.ExitCode;
                    }

                    if (exitCode != 0)
                    {
                        WriteCompileLog(slnPath, errorMessage, exitCode);
                    }

                    //if (exitCode == 1)
                    //MessageBox.Show(arguments + Resources.Form1_CompileProjects____ + exitCode);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
            else
            {
                MessageBox.Show(Resources.Form1_CompileProjects_Argument_is_null);
            }
        }

        public void WriteCompileLog(string projectName, string errorMessage, int exitCode)
        {
            try
            {
                var streamWriter = new StreamWriter(@"C:\ErrorCompileLog\NatekCompilerErrorLog.txt", true, Encoding.UTF8);
                streamWriter.WriteLine("An error occured on {0}, project, errormesage is {1}, exitcodeis {2}, datetime is {3} ", projectName, errorMessage, exitCode, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void chkRelease_CheckedChanged(object senderk, EventArgs e)
        {
            if (chkRelease.Checked)
            {
                isRelease = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindKeyword("LastRecordDate");
        }

        /*
         //ContainsDeleteFunction(v);
                                //ContainsAfterMethod(v);
                                ContainsBeforeMethod(v);
         */

        public void FindKeyword(string keyword)
        {
            GetProjectFiles(txtFilePath.Text, txtFileExtension.Text);
            projectFileList.Sort();

            foreach (var v in projectFileList)
            {
                using (var reader = new StreamReader(v))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (line.Trim().Contains(keyword))
                            {
                                LstListBox.Items.Add(v);
                                break;
                            }
                        }
                    }
                }
            }
            MessageBox.Show("OP.Comp.");
        }

        private static void ContainsAfterMethod(string s)
        {
            using (var streamWriter = new StreamWriter("ContainsAfter.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine("Base - "+s);
            }
        } // ContainsAfterMethod

        private static void ContainsBeforeMethod(string s)
        {
            using (var streamWriter = new StreamWriter("ContainsBefore.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine("Specific Implementations - " + s);
            }
        } // ContainsAfterMethod

        public void ContainsDeleteFunction(string projectName)
        {
            try
            {
                var streamWriter = new StreamWriter(@"C:\ErrorCompileLog\ContainsDeleteFunction.txt", true, Encoding.UTF8);
                streamWriter.WriteLine(projectName);
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FindKeyword("File.Delete");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FindKeyword("After(");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FindKeyword("Before(");
        }
    }
}
