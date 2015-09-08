using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Natek.Recorders.Remote.Helpers.Basic;

namespace RRInfoCollector
{
    public partial class frmMain : Form
    {
        private static Dictionary<string, string> recordColumns = new Dictionary<string, string>();

        static frmMain()
        {
            recordColumns["COMPUTERNAME"] = "COMPUTERNAME";
            recordColumns["CUSTOMINT1"] = "CUSTOMINT1";
            recordColumns["CUSTOMINT10"] = "CUSTOMINT10";
            recordColumns["CUSTOMINT2"] = "CUSTOMINT2";
            recordColumns["CUSTOMINT3"] = "CUSTOMINT3";
            recordColumns["CUSTOMINT4"] = "CUSTOMINT4";
            recordColumns["CUSTOMINT5"] = "CUSTOMINT5";
            recordColumns["CUSTOMINT6"] = "CUSTOMINT6";
            recordColumns["CUSTOMINT7"] = "CUSTOMINT7";
            recordColumns["CUSTOMINT8"] = "CUSTOMINT8";
            recordColumns["CUSTOMINT9"] = "CUSTOMINT9";
            recordColumns["CUSTOMSTR1"] = "CUSTOMSTR1";
            recordColumns["CUSTOMSTR10"] = "CUSTOMSTR10";
            recordColumns["CUSTOMSTR2"] = "CUSTOMSTR2";
            recordColumns["CUSTOMSTR3"] = "CUSTOMSTR3";
            recordColumns["CUSTOMSTR4"] = "CUSTOMSTR4";
            recordColumns["CUSTOMSTR5"] = "CUSTOMSTR5";
            recordColumns["CUSTOMSTR6"] = "CUSTOMSTR6";
            recordColumns["CUSTOMSTR7"] = "CUSTOMSTR7";
            recordColumns["CUSTOMSTR8"] = "CUSTOMSTR8";
            recordColumns["CUSTOMSTR9"] = "CUSTOMSTR9";
            recordColumns["DATE_TIME"] = "DATE_TIME";
            recordColumns["DESCRIPTION"] = "DESCRIPTION";
            recordColumns["EVENT_ID"] = "EVENT_ID";
            recordColumns["EVENTCATEGORY"] = "EVENTCATEGORY";
            recordColumns["EVENTTYPE"] = "EVENTTYPE";
            recordColumns["LOG_NAME"] = "LOG_NAME";
            recordColumns["RECORD_NUMBER"] = "RECORD_NUMBER";
            recordColumns["SEVERITY"] = "SEVERITY";
            recordColumns["SIGN"] = "SIGN";
            recordColumns["SIGN_TIME"] = "SIGN_TIME";
            recordColumns["SOURCENAME"] = "SOURCENAME";
            recordColumns["TAXONOMY"] = "TAXONOMY";
            recordColumns["USERSID"] = "USERSID";
        }

        private SystemInfo systemLookup;

        private Dictionary<string, RemoteRecorderInfo<TreeNode>> recorderLookup;
        private TreeNode[] recorderNodes;
        private RemoteRecorderInfo<TreeNode> selectedRecorder = null;
        private Dictionary<string, RecordProperties<TreeNode>> propertyLookup;
        private bool dirty;
        private int fieldCount = 0;
        private List<FieldItem> fields;
        private int definedSelection = 0, newSelection = 0;
        private int dirtyStatus = 0;
        private List<FieldItem> defaultAnalysisQuery;
        private RecordProperties<TreeNode> selectedProperty;
        private TableDef<TreeNode> selectedTable;
        private int propertyDirtyStatus = 0;
        private int newPropertySelection = 0;
        private int definedPropertySelection = 0;
        private int queryFieldCount = 0;

        public frmMain()
        {
            InitializeComponent();
            Init();
        }

        public static readonly Regex RegFilenameNumericStyle = new Regex("([^0-9]+)|([0-9]+)", RegexOptions.Compiled);

        protected virtual int CompareByNameSys(RecordProperties<TreeNode> l, RecordProperties<TreeNode> r)
        {
            return CompareByText(l.SystemName, r.SystemName);
        }

        protected virtual int CompareByNameTable(TableDef<TreeNode> l, TableDef<TreeNode> r)
        {
            if (l == null)
            {
                return r == null ? 0 : 1;
            }
            return r == null ? -1 : l.CompareTo(r);
        }

        protected virtual int CompareByName(FieldItem l, FieldItem r)
        {
            if (l.Selected)
            {
                if (!r.Selected)
                    return -1;
            }
            else if (r.Selected)
                return 1;

            return CompareByText(l.Text, r.Text);
        }

        private int CompareByText(string l, string r)
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
                                       ? int.Parse(mL.Groups[2].Value) - int.Parse(mR.Groups[2].Value)
                                       : string.Compare(mL.Groups[1].Value, mR.Groups[1].Value,
                                                        StringComparison.OrdinalIgnoreCase);
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

        private void Init()
        {
            recorderNodes = new[]
                {
                    new TreeNode("Eşleştirilecek Recorderlar"),
                    new TreeNode("Bilgisi Eksik Recorderlar"),
                    new TreeNode("Tamamlanmış Recorderlar")
                };
            fields = new List<FieldItem>();
            lblRecorder.Text = "Recorder: Henüz Seçilmedi";
            treeRecorders.AllowDrop = true;
            treeRecorders.ItemDrag += new ItemDragEventHandler(treeRecorders_ItemDrag);
            treeRecorders.DragEnter += new DragEventHandler(treeRecorders_DragEnter);
            treeRecorders.DragOver += new DragEventHandler(treeRecorders_DragOver);
            treeRecorders.DragDrop += new DragEventHandler(treeRecorders_DragDrop);

            defaultAnalysisQuery = new List<FieldItem>();

            foreach (var key in recordColumns.Keys)
            {
                defaultAnalysisQuery.Add(new FieldItem { Text = key });
            }
            defaultAnalysisQuery.Sort(CompareByName);
        }

        private void treeRecorders_DragOver(object sender, DragEventArgs e)
        {
            var targetPoint = treeRecorders.PointToClient(new Point(e.X, e.Y));

            treeRecorders.SelectedNode = treeRecorders.GetNodeAt(targetPoint);
        }

        private void treeRecorders_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        private void treeRecorders_DragDrop(object sender, DragEventArgs e)
        {
            var targetPoint = treeRecorders.PointToClient(new Point(e.X, e.Y));

            var targetNode = treeRecorders.GetNodeAt(targetPoint);
            var draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (!draggedNode.Equals(targetNode))
            {
                if (targetNode.Level == 0)
                {
                    if (targetNode.Index == draggedNode.Parent.Index + 1 || targetNode.Index == draggedNode.Parent.Index)
                    {
                        var parent = draggedNode.Parent;
                        draggedNode.Remove();
                        parent.Nodes.Insert(
                            targetNode.Index == parent.Index ? 0 : (parent.Nodes.Count > 0 ? parent.Nodes.Count - 1 : 0),
                            draggedNode);
                    }
                }
                else if (targetNode.Parent == draggedNode.Parent)
                {
                    var parent = draggedNode.Parent;
                    draggedNode.Remove();
                    parent.Nodes.Insert(targetNode.Index, draggedNode);
                }
            }
            treeRecorders.SelectedNode = null;
        }

        private void treeRecorders_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists("info"))
                    Directory.CreateDirectory("info");
            }
            catch
            {
            }

            if (!FillSystemLookup())
            {
                Close();
                return;
            }
            if (!FillRecorderLookup())
            {
                Close();
                return;
            }

            FillTree();

            if (!FillRecordProperties())
            {
                Close();
                return;
            }
            var ds = new RemoteRecorderSystem[systemLookup.SystemLookup.Values.Count];
            systemLookup.SystemLookup.Values.CopyTo(ds, 0);
            Array.Sort(ds);
            lstAssignedFields.Items.Clear();
            cmbSystems.DataSource = ds;
            txtDescription.Text = string.Empty;
            cmbSystems.SelectedIndex = -1;
            treeRecorders.ExpandAll();
            treeRecorders.SelectedNode = null;
        }

        private bool FillRecordProperties()
        {
            try
            {
                var fInfo = new FileInfo(Path.Combine("info", "recorderProperties.json"));
                if (!fInfo.Exists)
                    throw new Exception(string.Format("Recorder sistem-tablo bilgilerinin bulunduğu {0} bulunamadı",
                                                      fInfo.FullName));
                var json =
                    new DataContractJsonSerializer(
                        typeof(Dictionary<string, RecordProperties<TreeNode>>));
                using (var fs = new StreamReader(fInfo.FullName, Encoding.GetEncoding(1254)))
                {
                    var o = json.ReadObject(fs.BaseStream);
                    if (o == null)
                        throw new Exception(string.Format("{0} dosyasında sistem-tablo bilgileri bulunamadı",
                                                          fInfo.FullName));
                    if (!(o is Dictionary<string, RecordProperties<TreeNode>>))
                        throw new Exception(
                            string.Format("{0} dosyasındaki {1} türü beklenen sistem-tablo bilgi sözlük türünde değil",
                                          fInfo.FullName, o.GetType().Name));
                    propertyLookup = (Dictionary<string, RecordProperties<TreeNode>>)o;

                    var ls = new List<RecordProperties<TreeNode>>();
                    foreach (var key in propertyLookup.Keys)
                    {
                        if (systemLookup.SystemLookup.ContainsKey(key))
                        {
                            var prop = propertyLookup[key];
                            ls.Add(prop);
                            if (prop.Table == null)
                                prop.Table = new Dictionary<string, TableDef<TreeNode>>();
                        }
                    }
                    foreach (var key in systemLookup.SystemLookup.Keys)
                    {
                        if (!propertyLookup.ContainsKey(key))
                        {
                            var prop = new RecordProperties<TreeNode>
                                {
                                    SystemName = key,
                                    Table = new Dictionary<string, TableDef<TreeNode>>()
                                };
                            ls.Add(prop);
                            propertyLookup[key] = prop;
                        }
                    }
                    ls.Sort(CompareByNameSys);
                    foreach (var r in ls)
                    {
                        var node = new TreeNode(r.SystemName);
                        r.Data = node;
                        node.Tag = r;
                        if (r.Table == null)
                            r.Table = new Dictionary<string, TableDef<TreeNode>>();
                        else
                        {
                            var tables = new TableDef<TreeNode>[r.Table.Values.Count];
                            r.Table.Values.CopyTo(tables, 0);

                            Array.Sort(tables, CompareByNameTable);
                            foreach (var t in r.Table.Values)
                            {
                                var nodeSub = new TreeNode { Text = t.Description, Tag = t };
                                t.Data = nodeSub;
                                node.Nodes.Add(nodeSub);
                            }
                        }
                        treeProperties.Nodes.Add(node);
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Recorder bilgileri yüklenirken hata oluştu: {0}", e.Message), "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return false;
            }
        }

        private void FillTree()
        {
            var nodes = new[] { new List<TreeNode>(), new List<TreeNode>(), new List<TreeNode>() };
            foreach (var rec in recorderLookup.Values)
            {
                int index = GetIndex(rec);

                if (rec.Fields == null)
                    rec.Fields = new List<string>();
                rec.Data = new TreeNode
                    {
                        Text = rec.Name,
                        Tag = rec
                    };
                nodes[index].Add(rec.Data);
            }
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i].Sort((x, y) =>
                    {
                        var xt = x.Tag as RemoteRecorderInfo<TreeNode>;
                        var yt = y.Tag as RemoteRecorderInfo<TreeNode>;
                        if (xt.Order == yt.Order)
                            return xt.Name.CompareTo(yt.Name);
                        return xt.Order - yt.Order;
                    });
                recorderNodes[i].Nodes.AddRange(nodes[i].ToArray());
                treeRecorders.Nodes.Add(recorderNodes[i]);
            }
        }

        private int GetIndex(RemoteRecorderInfo<TreeNode> rec)
        {
            if (!string.IsNullOrEmpty(rec.SystemName) && rec.Description.Trim().Length > 0 && rec.Fields.Count > 0)
                return 2;
            return !string.IsNullOrEmpty(rec.SystemName) || rec.Description.Trim().Length > 0 || rec.Fields.Count > 0
                       ? 1
                       : 0;
        }

        private bool FillRecorderLookup()
        {
            try
            {
                /*
                var dt = new Dictionary<string, RemoteRecorderInfo<TreeNode>>();
                var lst = new List<string>();
                var id = 0;
                foreach (var fs in Directory.GetFileSystemEntries(@"O:\Projects\Dev\Remote Recorders\Latest DLL"))
                {
                    var fin = new FileInfo(fs);
                    if (fin.Exists && fin.Name.EndsWith("Recorder.dll", true, CultureInfo.InvariantCulture))
                        lst.Add(fin.Name);

                }
                foreach (var fs in Directory.GetFileSystemEntries(@"O:\Projects\Dev\Remote Recorders\Latest DLL\Specific Implementations"))
                {
                    var fin = new FileInfo(fs);
                    if (fin.Exists && fin.Name.EndsWith("Recorder.dll", true, CultureInfo.InvariantCulture))
                        lst.Add(fin.Name);

                }
                lst.Sort();

                for (var idx = 0; idx < lst.Count; idx++)
                {
                    dt[lst[idx]] = new RemoteRecorderInfo<TreeNode>
                        {
                            Id = idx + 1,
                            Order = idx + 1,
                            Name = lst[idx]
                        };
                }
                using (var fs = new StreamWriter(Path.Combine("info", "recorders.json")))
                {
                    var js =
                        new System.Runtime.Serialization.Json.DataContractJsonSerializer(
                            typeof(Dictionary<string, RemoteRecorderInfo<TreeNode>>));
                    js.WriteObject(fs.BaseStream, dt);
                }
                 * */
                var fInfo = new FileInfo(Path.Combine("info", "recorders.json"));
                if (!fInfo.Exists)
                    throw new Exception(string.Format("Recorder bilgilerinin bulunduğu {0} bulunamadı", fInfo.FullName));
                var json =
                    new DataContractJsonSerializer(
                        typeof(Dictionary<string, RemoteRecorderInfo<TreeNode>>));
                using (var fs = new StreamReader(fInfo.FullName, Encoding.GetEncoding(1254)))
                {
                    var o = json.ReadObject(fs.BaseStream);
                    if (o == null)
                        throw new Exception(string.Format("{0} dosyasında recorder bilgileri bulunamadı", fInfo.FullName));
                    if (!(o is Dictionary<string, RemoteRecorderInfo<TreeNode>>))
                        throw new Exception(
                            string.Format("{0} dosyasındaki {1} türü beklenen recorder bilgi sözlük türünde değil",
                                          fInfo.FullName, o.GetType().Name));
                    recorderLookup = (Dictionary<string, RemoteRecorderInfo<TreeNode>>)o;

                    var lookup = new Dictionary<string, bool>();
                    foreach (var shortNotation in systemLookup.ShortNotations)
                        lookup[shortNotation] = true;
                    foreach (var rec in recorderLookup.Values)
                    {
                        if (rec.Fields == null)
                            rec.Fields = new List<string>();
                        for (var i = 0; i < rec.Fields.Count; )
                        {
                            if (lookup.ContainsKey(rec.Fields[i]))
                                ++i;
                            else
                                rec.Fields.RemoveAt(i);
                        }

                        if (rec.SystemName == null)
                            rec.SystemName = string.Empty;
                        if (rec.Description == null)
                            rec.Description = string.Empty;

                        if (!string.IsNullOrEmpty(rec.SystemName) &&
                            !systemLookup.SystemLookup.ContainsKey(rec.SystemName))
                            rec.SystemName = null;
                        rec.Fields.Sort();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Recorder bilgileri yüklenirken hata oluştu: {0}", e.Message), "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return false;
            }
        }

        private bool FillSystemLookup()
        {
            try
            {
                var fInfo = new FileInfo(Path.Combine("info", "systemInfo.json"));
                if (!fInfo.Exists)
                    throw new Exception(string.Format("Sistem bilgilerinin bulunduğu {0} bulunamadı", fInfo.FullName));
                var json = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(SystemInfo));
                using (var fs = new StreamReader(fInfo.FullName, Encoding.GetEncoding(1254)))
                {
                    var o = json.ReadObject(fs.BaseStream);
                    if (o == null)
                        throw new Exception(string.Format("{0} dosyasında sistem bilgileri bulunamadı", fInfo.FullName));
                    if (!(o is SystemInfo))
                        throw new Exception(
                            string.Format("{0} dosyasındaki {1} türü beklenen sistem bilgi sözlük türünde değil",
                                          fInfo.FullName, o.GetType().Name));
                    systemLookup = (SystemInfo)o;
                    if (systemLookup.ShortNotations == null)
                        systemLookup.ShortNotations = new List<string>();
                    systemLookup.ShortNotations.Sort();
                    if (systemLookup.SystemLookup == null)
                        systemLookup.SystemLookup = new Dictionary<string, RemoteRecorderSystem>();
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Sistem bilgileri yüklenirken hata oluştu: {0}", e.Message), "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return false;
            }
        }

        private void treeRecorders_DoubleClick(object sender, EventArgs e)
        {
            if (treeRecorders.SelectedNode != null && treeRecorders.SelectedNode.Level == 1)
            {
                var recorder = treeRecorders.SelectedNode.Tag as RemoteRecorderInfo<TreeNode>;
                if (selectedRecorder != null)
                {
                    if (selectedRecorder.Name.Equals(recorder.Name))
                        return;
                    if (dirty)
                    {
                        switch (
                            MessageBox.Show("Değişiklikleri kaydetmek istiyor musunuz?", "Lütfen Cevaplayınız",
                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button3))
                        {
                            case DialogResult.Yes:
                                SaveSelected();
                                break;
                            case DialogResult.Cancel:
                                treeRecorders.SelectedNode = null;
                                return;
                        }
                    }
                }
                selectedRecorder = recorder;
                ResetSelection();
            }
        }

        private void ResetSelection()
        {
            lstSelectionFree = false;
            try
            {
                if (fields.Count == 0)
                {
                    foreach (var text in systemLookup.ShortNotations)
                        fields.Add(new FieldItem { Text = text });
                }
                else
                {
                    for (var i = 0; i < fields.Count && fields[i].Selected; i++)
                        fields[i].Selected = false;
                }
                fields.Sort();
                fieldCount = 0;
                for (var i = 0; i < selectedRecorder.Fields.Count; )
                {
                    int j = 0;
                    while (j < fields.Count &&
                           string.Compare(fields[j].Text, selectedRecorder.Fields[i], StringComparison.Ordinal) < 0)
                        j++;
                    if (j < fields.Count)
                    {
                        fields[j].Selected = true;
                        ++fieldCount;
                        i++;
                    }
                    else
                        selectedRecorder.Fields.RemoveAt(i);
                }
                fields.Sort();
                newSelection = 0;
                definedSelection = 0;
                dirtyStatus = 0;
                SetDirty(false);
                ResetLst();
                txtDescription.Text = selectedRecorder.Description;
                lblRecorder.Text = "Recorder: ====== " + selectedRecorder.Name + " ======";
                cmbSystems.Text = selectedRecorder.SystemName;
            }
            finally
            {
                lstSelectionFree = true;
            }
        }

        private void ResetLst()
        {
            lstAssignedFields.Items.Clear();
            fieldCount = 0;
            for (var i = 0; i < fields.Count; i++)
            {
                var item = new ListViewItem { Text = fields[i].Text };
                if (fields[i].Selected)
                {
                    item.BackColor = selectedRecorder.Fields.Contains(fields[i].Text) ? Color.Tomato : Color.Firebrick;
                    item.ForeColor = Color.White;
                    ++fieldCount;
                }
                else
                {
                    item.BackColor = Color.White;
                    item.ForeColor = Color.Black;
                }
                lstAssignedFields.Items.Add(item);
            }
            lblFieldCount.Text = fieldCount > 0 ? fieldCount + " Alan Atanmış" : "Henüz Yok";
            newSelection = 0;
            definedSelection = 0;
            lblCtrl.Text = string.Empty;
        }

        private void ResetLstProperty()
        {
            lstQueryFields.Items.Clear();
            queryFieldCount = 0;
            for (var i = 0; i < defaultAnalysisQuery.Count; i++)
            {
                var item = new ListViewItem { Text = defaultAnalysisQuery[i].Text };
                if (defaultAnalysisQuery[i].Selected)
                {
                    item.BackColor = selectedTable != null && selectedTable.DefaultAnalysisQuery.Contains(defaultAnalysisQuery[i].Text) ? Color.Tomato : Color.Firebrick;
                    item.ForeColor = Color.White;
                    ++queryFieldCount;
                }
                else
                {
                    item.BackColor = Color.White;
                    item.ForeColor = Color.Black;
                }
                lstQueryFields.Items.Add(item);
            }
        }

        private void SaveSelected()
        {
            selectedRecorder.Description = txtDescription.Text;
            selectedRecorder.SystemName = cmbSystems.SelectedIndex >= 0 ? cmbSystems.Text : null;
            selectedRecorder.Fields.Clear();
            for (var i = 0; i < fields.Count && fields[i].Selected; i++)
            {
                selectedRecorder.Fields.Add(fields[i].Text);
                lstAssignedFields.Items[i].BackColor = Color.Tomato;
            }
            if (!SaveRecorder())
                return;
            var index = GetIndex(selectedRecorder);
            if (index != selectedRecorder.Data.Parent.Index)
            {
                selectedRecorder.Data.Remove();
                var parent = treeRecorders.Nodes[index];
                if (parent.Nodes.Count == 0)
                    parent.Nodes.Add(selectedRecorder.Data);
                else
                {
                    index = 0;
                    while (index < parent.Nodes.Count && parent.Nodes[index].Text.CompareTo(selectedRecorder.Name) < 0)
                        index++;
                    if (index == parent.Nodes.Count)
                        parent.Nodes.Add(selectedRecorder.Data);
                    else
                        parent.Nodes.Insert(index, selectedRecorder.Data);
                }
            }
            treeRecorders.SelectedNode = null;
            dirtyStatus = 0;
            lblRecorder.Text = "Recorder: ====== " + selectedRecorder.Name + " ======";
            SetDirty(false);
        }

        private bool SaveRecorder()
        {
            try
            {
                var fInfo = new FileInfo(Path.Combine("info", "recorders.json"));
                var tInfo = new FileInfo(fInfo.FullName + ".tmp");
                if (tInfo.Exists)
                    tInfo.Delete();
                using (var fs = new StreamWriter(tInfo.FullName, false, Encoding.GetEncoding(1254)))
                {
                    var json = new DataContractJsonSerializer(typeof(Dictionary<string, RemoteRecorderInfo<TreeNode>>));
                    json.WriteObject(fs.BaseStream, recorderLookup);
                }
                if (fInfo.Exists)
                {
                    try
                    {
                        File.Move(fInfo.FullName, fInfo.FullName + ".orig");
                        tInfo.MoveTo(fInfo.FullName);
                        File.Delete(fInfo.FullName + ".orig");
                    }
                    catch
                    {
                        if (File.Exists(fInfo.FullName + ".orig"))
                            File.Move(fInfo.FullName + ".orig", fInfo.FullName);
                    }
                }
                else
                    tInfo.MoveTo(fInfo.FullName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("İşlemler sırasında hata oluştu: {0}", ex.Message), "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
            }
            return false;
        }

        private bool SaveRecorderProperties()
        {
            try
            {
                var fInfo = new FileInfo(Path.Combine("info", "recorderProperties.json"));
                var tInfo = new FileInfo(fInfo.FullName + ".tmp");
                if (tInfo.Exists)
                    tInfo.Delete();
                using (var fs = new StreamWriter(tInfo.FullName, false, Encoding.GetEncoding(1254)))
                {
                    var json = new DataContractJsonSerializer(typeof(Dictionary<string, RecordProperties<TreeNode>>));
                    json.WriteObject(fs.BaseStream, propertyLookup);
                }
                if (fInfo.Exists)
                {
                    try
                    {
                        File.Move(fInfo.FullName, fInfo.FullName + ".orig");
                        tInfo.MoveTo(fInfo.FullName);
                        File.Delete(fInfo.FullName + ".orig");
                    }
                    catch
                    {
                        if (File.Exists(fInfo.FullName + ".orig"))
                            File.Move(fInfo.FullName + ".orig", fInfo.FullName);
                    }
                }
                else
                    tInfo.MoveTo(fInfo.FullName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("İşlemler sırasında hata oluştu: {0}", ex.Message), "Hata",
                                MessageBoxButtons.OK, MessageBoxIcon.Error,
                                MessageBoxDefaultButton.Button1);
            }
            return false;
        }

        private void SetDirty(bool status)
        {
            dirty = status;
            btnClear.Enabled = btnSave.Enabled = dirty;
        }

        private void ClearSelection()
        {
            selectedRecorder = null;
            cmbSystems.SelectedIndex = -1;
            lstAssignedFields.Items.Clear();
            txtDescription.Text = string.Empty;
            definedSelection = newSelection = 0;
            lblCtrl.Text = string.Empty;
            SetDirty(false);
        }


        private bool lstSelectionFree = true;

        private void lstAssignedFields_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lstSelectionFree)
                lstSelectionFree = false;
            else
                return;
            try
            {
                var field = fields[e.ItemIndex];
                if (e.IsSelected)
                {
                    if (field.Selected)
                    {
                        if (newSelection > 0)
                        {
                            lstAssignedFields.SelectedIndices.Clear();
                            e.Item.Selected = true;
                            newSelection = 0;
                            definedSelection = 0;
                        }
                        ++definedSelection;
                    }
                    else
                    {
                        if (definedSelection > 0)
                        {
                            lstAssignedFields.SelectedIndices.Clear();
                            e.Item.Selected = true;
                            newSelection = 0;
                            definedSelection = 0;
                        }
                        ++newSelection;
                    }
                }
                else if (field.Selected)
                    --definedSelection;
                else
                    --newSelection;
            }
            finally
            {
                if (definedSelection > 0)
                    lblCtrl.Text = "Ctrl+Backspace'e basarak alanları çıkarabilirsiniz";
                else if (newSelection > 0)
                    lblCtrl.Text = "Ctrl+Enter'a basarak alanları ekleyebilirsiniz";
                else
                    lblCtrl.Text = string.Empty;
                lstSelectionFree = true;
            }
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabEditors.SelectedTab == tabRREditor)
                RecorderKeyPressed(e);
            else if (tabEditors.SelectedTab == tabPropertyEditor)
                PropertyKeyPressed(e);
        }

        private void PropertyKeyPressed(KeyEventArgs e)
        {
            if (!e.Control)
                return;
            if (e.KeyCode == Keys.Enter)
            {
                if (newPropertySelection > 0)
                    AddPropertySelections();
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (definedPropertySelection > 0)
                    RemovePropertySelections();
            }
            else if (e.KeyCode == Keys.N)
            {
                if (treeProperties.SelectedNode == null)
                    return;
                CheckPropertySaveStatus((treeProperties.SelectedNode.Level == 0
                    ? treeProperties.SelectedNode.Tag
                    : treeProperties.SelectedNode.Parent.Tag) as RecordProperties<TreeNode>, null);
            }
            else if (btnClearQuery.Enabled)
            {
                if (e.KeyCode == Keys.K)
                    SaveSelectedProperty();
                else if (e.KeyCode == Keys.T)
                    ResetSelectionProperty();
            }
        }

        private void RemovePropertySelections()
        {
            throw new NotImplementedException();
        }

        private void AddPropertySelections()
        {
            throw new NotImplementedException();
        }

        private void RecorderKeyPressed(KeyEventArgs e)
        {
            if (!e.Control)
                return;
            if (e.KeyCode == Keys.Enter)
            {
                if (newSelection > 0)
                    AddSelections();
            }
            else if (e.KeyCode == Keys.Back)
            {
                if (definedSelection > 0)
                    RemoveSelections();
            }
            else if (selectedRecorder != null && dirty)
            {
                if (e.KeyCode == Keys.K)
                    SaveSelected();
                else if (e.KeyCode == Keys.T)
                    ResetSelection();
            }
        }

        private void RemoveSelections()
        {
            lstSelectionFree = false;
            try
            {
                for (var i = 0; i < lstAssignedFields.SelectedIndices.Count; i++)
                {
                    if (fields[lstAssignedFields.SelectedIndices[i]].Selected)
                        fields[lstAssignedFields.SelectedIndices[i]].Selected = false;
                }
                fields.Sort();
                ResetLst();
                CheckChange();
            }
            finally
            {
                lstSelectionFree = true;
            }
        }

        private void AddSelections()
        {
            lstSelectionFree = false;
            try
            {
                for (var i = 0; i < lstAssignedFields.SelectedIndices.Count; i++)
                {
                    if (!fields[lstAssignedFields.SelectedIndices[i]].Selected)
                        fields[lstAssignedFields.SelectedIndices[i]].Selected = true;
                }
                fields.Sort();
                ResetLst();
                CheckChange();
            }
            finally
            {
                lstSelectionFree = true;
            }
        }

        private void CheckChange()
        {
            if (selectedRecorder == null)
                return;
            if (selectedRecorder.Fields.Count == fieldCount)
            {
                int i = 0;
                while (i < fieldCount && fields[i].Text == selectedRecorder.Fields[i])
                    ++i;
                if (i == fieldCount)
                {
                    dirtyStatus ^= (dirtyStatus & 2);
                    if (dirtyStatus == 0)
                        SetDirty(false);
                    return;
                }
            }
            dirtyStatus |= 2;
            if (dirtyStatus == 2)
                SetDirty(true);
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (selectedRecorder == null || !lstSelectionFree)
                return;
            if (StringHelper.NullEmptyEquals(txtDescription.Text, selectedRecorder.Description))
            {
                dirtyStatus ^= (dirtyStatus & 4);
                if (dirtyStatus == 0)
                    SetDirty(false);
            }
            else
            {
                dirtyStatus |= 4;
                if (dirtyStatus == 4)
                    SetDirty(true);
            }
        }

        private void cmbSystems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedRecorder == null || !lstSelectionFree)
                return;
            if (string.IsNullOrEmpty(selectedRecorder.SystemName) && cmbSystems.SelectedIndex < 0
                ||
                !string.IsNullOrEmpty(selectedRecorder.SystemName) && cmbSystems.SelectedIndex >= 0 &&
                selectedRecorder.SystemName == cmbSystems.Text)
            {
                dirtyStatus ^= (dirtyStatus & 1);
                if (dirtyStatus == 0)
                    SetDirty(false);
            }
            else
            {
                dirtyStatus |= 1;
                if (dirtyStatus == 1)
                    SetDirty(true);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ResetSelection();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSelected();
        }

        private void lstAssignedFields_DoubleClick(object sender, EventArgs e)
        {
            if (lstAssignedFields.SelectedIndices.Count == 1)
            {
                if (fields[lstAssignedFields.SelectedIndices[0]].Selected)
                    RemoveSelections();
                else
                    AddSelections();
            }
        }

        private void tabRREditor_Click(object sender, EventArgs e)
        {

        }

        private void treeProperties_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeProperties.SelectedNode == null || treeProperties.SelectedNode.Level != 1)
                return;
            var node = treeProperties.SelectedNode.Tag as TableDef<TreeNode>;
            if (node == selectedTable)
                return;
            CheckPropertySaveStatus(treeProperties.SelectedNode.Parent.Tag as RecordProperties<TreeNode>, node);
        }

        private void CheckPropertySaveStatus(RecordProperties<TreeNode> parent, TableDef<TreeNode> node)
        {
            if (propertyDirtyStatus > 0)
            {
                switch (
                    MessageBox.Show("Değişiklikleri kaydetmek istiyor musunuz?", "Lütfen Cevaplayınız",
                                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button3))
                {
                    case DialogResult.Yes:
                        SaveSelectedProperty();
                        break;
                    case DialogResult.Cancel:
                        treeProperties.SelectedNode = null;
                        return;
                }
            }
            selectedProperty = parent;
            selectedTable = node;
            ResetSelectionProperty();
        }

        private void ResetSelectionProperty()
        {
            lblSystem.Text = selectedProperty.SystemName;
            int i;
            for (i = 0; i < defaultAnalysisQuery.Count && defaultAnalysisQuery[i].Selected; i++)
                defaultAnalysisQuery[i].Selected = false;
            defaultAnalysisQuery.Sort();
            if (selectedTable == null)
            {
                txtTableDesc.Text = string.Empty;
                txtTableName.Text = string.Empty;
            }
            else
            {
                txtTableDesc.Text = selectedTable.Description;
                txtTableName.Text = selectedTable.TableName;
                i = 0;
                for (i = 0; i < selectedTable.DefaultAnalysisQuery.Count; )
                {
                    int j = 0;
                    while (j < defaultAnalysisQuery.Count
                           && selectedTable.DefaultAnalysisQuery[i].CompareTo(defaultAnalysisQuery[j].Text) > 0)
                        j++;
                    if (j < defaultAnalysisQuery.Count)
                    {
                        defaultAnalysisQuery[j].Selected = true;
                        ++i;
                    }
                    else
                        selectedTable.DefaultAnalysisQuery.RemoveAt(i);
                }
            }
            defaultAnalysisQuery.Sort();
            newPropertySelection = 0;
            definedPropertySelection = selectedTable == null ? 0 : selectedTable.DefaultAnalysisQuery.Count;
            propertyDirtyStatus = 0;
            btnSaveQuery.Enabled = false;
            btnClearQuery.Enabled = false;
            ResetLstProperty();
        }

        private void SaveSelectedProperty()
        {
            var i = 0;
            if (selectedTable == null)
            {
                ++i;
                selectedTable = new TableDef<TreeNode>
                    {
                        DefaultAnalysisQuery = new List<string>(),
                        Data = new TreeNode { Text = txtTableName.Text.Trim() }
                    };
                selectedTable.Data.Tag = selectedTable;
            }
            else
            {
                if (!StringHelper.NullEmptyEquals(selectedTable.TableName, txtTableName.Text.Trim()))
                {
                    ++i;
                    selectedProperty.Table.Remove(selectedTable.TableName);
                    selectedTable.Data.Remove();
                }
            }

            selectedTable.Description = txtTableDesc.Text.Trim();
            selectedTable.TableName = txtTableName.Text.Trim();
            selectedProperty.Table[selectedTable.TableName] = selectedTable;
            if (i != 0)
                AdjustNodePlace();
            selectedTable.DefaultAnalysisQuery.Clear();
            for (i = 0; i < defaultAnalysisQuery.Count && defaultAnalysisQuery[i].Selected; i++)
            {
                selectedTable.DefaultAnalysisQuery.Add(defaultAnalysisQuery[i].Text);
            }
            ResetSelectionProperty();
            SaveRecorderProperties();
        }

        private void AdjustNodePlace()
        {
            var i = 0;
            while (i < selectedProperty.Data.Nodes.Count &&
                   String.Compare(selectedTable.TableName, selectedProperty.Data.Nodes[i].Text, StringComparison.Ordinal) <
                   0)
                ++i;
            var node = new TreeNode
            {
                Text = selectedTable.TableName,
                Tag = selectedTable
            };
            if (i < selectedProperty.Data.Nodes.Count)
                selectedProperty.Data.Nodes.Insert(i, node);
            else
                selectedProperty.Data.Nodes.Add(node);
        }

        private void lstQueryFields_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstQueryFields.SelectedItems.Count != 1)
                return;

            defaultAnalysisQuery[lstQueryFields.SelectedIndices[0]].Selected ^= true;
            var alreadyMember = selectedTable != null &&
                                selectedTable.DefaultAnalysisQuery.Contains(
                                    defaultAnalysisQuery[lstQueryFields.SelectedIndices[0]].Text);
            if (defaultAnalysisQuery[lstQueryFields.SelectedIndices[0]].Selected)
            {
                if (alreadyMember)
                    ++definedPropertySelection;
                else
                    ++newPropertySelection;
            }
            else if (alreadyMember)
                --definedPropertySelection;
            else
                --newPropertySelection;
            defaultAnalysisQuery.Sort();
            if (newPropertySelection == 0)
            {
                if (definedPropertySelection == 0 && selectedTable == null
                    || selectedTable != null && selectedTable.DefaultAnalysisQuery.Count == definedPropertySelection)
                    propertyDirtyStatus ^= (propertyDirtyStatus & 4);
                else if (selectedTable != null &&
                         selectedTable.DefaultAnalysisQuery.Count == definedPropertySelection + 1)
                {
                    propertyDirtyStatus |= 4;
                }
            }
            else if (newPropertySelection == 1)
            {
                propertyDirtyStatus |= 4;
            }
            CheckChangeQuery();
            ResetLstProperty();
        }

        private void CheckChangeQuery()
        {
            if (txtTableDesc.Text.Trim().Length == 0 || txtTableName.Text.Length == 0 || propertyDirtyStatus == 0 || (definedPropertySelection + newPropertySelection) == 0)
            {
                btnClearQuery.Enabled = false;
                btnSaveQuery.Enabled = false;
            }
            else
            {
                btnClearQuery.Enabled = true;
                btnSaveQuery.Enabled = true;
            }
        }

        private void txtTableDesc_TextChanged(object sender, EventArgs e)
        {
            if (selectedTable == null && txtTableDesc.Text.Trim().Length == 0
                ||
                selectedTable != null &&
                StringHelper.NullEmptyEquals(selectedTable.Description, txtTableDesc.Text.Trim()))
                propertyDirtyStatus ^= (propertyDirtyStatus & 1);
            else
                propertyDirtyStatus |= 1;
            CheckChangeQuery();
        }

        private void txtTableName_TextChanged(object sender, EventArgs e)
        {
            if (selectedTable == null && txtTableName.Text.Trim().Length == 0
                || selectedTable != null && StringHelper.NullEmptyEquals(selectedTable.TableName, txtTableName.Text.Trim()))
                propertyDirtyStatus ^= (propertyDirtyStatus & 2);
            else
                propertyDirtyStatus |= 2;
            CheckChangeQuery();
        }

        private void btnClearQuery_Click(object sender, EventArgs e)
        {
            ResetSelectionProperty();
        }

        private void btnSaveQuery_Click(object sender, EventArgs e)
        {
            SaveSelectedProperty();
        }
    }
}
