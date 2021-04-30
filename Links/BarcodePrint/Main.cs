using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections;
using System.Collections.Specialized;
using Links;
using System.Threading;
using HtmlAgilityPack;
using System.IO;
using System.Net;

namespace Links
{
    public partial class Main : DevExpress.XtraEditors.XtraForm
    {
        Hashtable hash = new Hashtable();
        Hashtable hashFaile = new Hashtable();
        Hashtable DeleteArticleSuccess = new Hashtable();
        Hashtable DeleteColumnSuccess = new Hashtable();
        Hashtable DeleteWebNameSuccess = new Hashtable();
        Hashtable ht = new Hashtable();
        DataTable _dt = new DataTable();
        DataTable _Link = new DataTable();
        DataTable _Delete = new DataTable();
        DataTable dt_column = new DataTable();
        int trys = 1;
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            _dt.Columns.Add("xh", typeof(int));
            _dt.Columns.Add("wz", typeof(string));
            _dt.Columns.Add("yhm", typeof(string));
            _dt.Columns.Add("mm", typeof(string));
            _dt.Columns.Add("lj", typeof(string));
            _dt.Columns.Add("zt", typeof(string));
            _dt.Columns.Add("cookie", typeof(string));


            _Link.Columns.Add("id", typeof(string));
            _Link.Columns.Add("name", typeof(string));
            _Link.Columns.Add("url", typeof(string));
            _Link.Columns.Add("mburl", typeof(string));
            _Link.Columns.Add("status", typeof(string));

            _Delete.Columns.Add("url", typeof(string));
            _Delete.Columns.Add("mburl", typeof(string));
            _Delete.Columns.Add("status", typeof(string));

            dt_column.Columns.Add("id", typeof(string));
            dt_column.Columns.Add("name", typeof(string));
            dt_column.Columns.Add("address", typeof(string));

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox4.SelectedIndex = 0;
        }

        private void Login()
        {
            if (gridView1.DataSource == null)
                return;
            _dt = ((DataView)gridView1.DataSource).ToTable();
            DevExpress.Data.CurrencyDataController.DisableThreadingProblemsDetection = true;
            AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
            if (_dt != null && _dt.Rows.Count > 0)
            {
                ThreadPool.SetMaxThreads(100, 100);
                foreach (DataRow dr in _dt.Rows)
                {
                    ThreadPool.QueueUserWorkItem(muilti, dr);
                    //muilti(dr);
                }
            }
            RegisteredWaitHandle registeredWaitHandle = null;
            registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
            {
                int workerThreads = 0;
                int maxWordThreads = 0;
                int compleThreads = 0;
                ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                //Console.WriteLine("Check 可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                if (workerThreads == maxWordThreads)
                {
                    mainAutoResetEvent.Set();
                    registeredWaitHandle.Unregister(null);
                }
            }), null, 5000, false);
            mainAutoResetEvent.WaitOne();
            foreach (DataRow dr in _dt.Rows)
            {
                if (hash.Contains(dr["wz"]))
                {
                    dr["zt"] = "登录成功";
                    dr["cookie"] = hash[dr["wz"]];
                }
                else
                {
                    //hashFaile.Add(dr["wz"],string.Empty);
                    dr["zt"] = "登录失败";
                    dr["cookie"] = string.Empty;
                }
            }

            gridControl1.DataSource = _dt;
            trys++;

            if (trys < 4 && _dt.Rows.Count > hash.Count)
            {
                Console.WriteLine("登录成功网站" + hash.Count + "个！进行第" + trys + "轮登录！");
                this.Login();
            }
            else
                MessageBox.Show("登录成功网站" + hash.Count + "个！请执行后续操作");

        }


        private void muilti(object o)
        {
            DataRow dr = _dt.NewRow();
            dr = (DataRow)o;
            NameValueCollection dic = new NameValueCollection();
            dic.Add("username", dr["yhm"].ToString());
            dic.Add("password", dr["mm"].ToString());
            string cookie = string.Empty;

            if (!hash.ContainsKey(dr["wz"]))
            {
                cookie = HttpUtils.PostForm(dr["wz"].ToString() + dr["lj"].ToString() + "/login.php?action=logincheck", dic);
                if (cookie != null && !string.IsNullOrEmpty(cookie))
                {
                    hash.Add(dr["wz"], cookie);
                    ht.Add(dr["wz"], dr["lj"].ToString());
                    //dr["zt"] = "登录成功";
                    Console.WriteLine(dr["wz"] + "登录成功！累计成功" + hash.Count + "个");
                    //dr["cookie"] = cookie;
                }
                else
                {
                    //dr["zt"] = "登录失败";
                    Console.WriteLine(dr["wz"] + "登录失败！");
                    //dr["cookie"] = string.Empty;
                }
            }

        }
        private void 获取栏目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView1.DataSource != null)
            {
                DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                if (gridView2.DataSource != null)
                {
                    dt_column.Clear();
                    gridControl2.DataSource = null;
                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["zt"] != null && dr["zt"].ToString() == "登录成功")
                        {
                            string cookie = hash[dr["wz"]].ToString();
                            label1.Text = dr["wz"].ToString();
                            Application.DoEvents();
                            string html = HttpUtils.DownloadUrl(dr["wz"].ToString() + dr["lj"].ToString() + "/column/list.php?action=column&pnum=1000", cookie);
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(html);
                            //总页数
                            int[] info = CrawlerCenter.GetPages(doc);
                            int pages = info[0];
                            int total = info[1];
                            progressBarControl1.Properties.Maximum = total;
                            progressBarControl1.Properties.Minimum = 0;
                            progressBarControl1.Properties.ShowTitle = true;
                            progressBarControl1.Position = 0;
                            for (int i = 1; i <= pages; i++)
                            {
                                for (int j = 1; j <= 1000; j++)
                                {
                                    Column col = new Column();
                                    if (i == 1)
                                        col = CrawlerCenter.GetColumns(doc, j);
                                    else
                                    {
                                        if (j == 1)
                                        {
                                            html = HttpUtils.DownloadUrl(dr["wz"].ToString() + dr["lj"].ToString() + "/column/list.php?action=column&page=" + i + "&pnum=1000", cookie);
                                            doc = new HtmlAgilityPack.HtmlDocument();
                                            doc.LoadHtml(html);
                                        }
                                        col = CrawlerCenter.GetColumns(doc, j);
                                    }
                                    if (col == null || string.IsNullOrEmpty(col.Id))
                                        break;
                                    progressBarControl1.Position += 1;
                                    DataRow dr_col = dt_column.NewRow();
                                    dr_col["id"] = col.Id.Trim();
                                    dr_col["name"] = col.Name;
                                    dr_col["address"] = col.Url;
                                    dt_column.Rows.Add(dr_col);
                                    //加入列表
                                    gridControl2.DataSource = dt_column;
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                }
            }
            else
                MessageBox.Show("请先登录网站！");
        }

        private void 获取友链ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView1.DataSource != null)
            {
                DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                DataTable dt_column = new DataTable();
                dt_column.Columns.Add("id", typeof(string));
                dt_column.Columns.Add("url", typeof(string));
                dt_column.Columns.Add("zxurl", typeof(string));
                dt_column.Columns.Add("show", typeof(string));
                dt_column.Columns.Add("status", typeof(string));
                DataRow dr_col = dt_column.NewRow();
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (dr["zt"] != null && dr["zt"].ToString() == "登录成功")
                        {
                            string cookie = hash[dr["wz"]].ToString();
                            label1.Text = dr["wz"].ToString();
                            Application.DoEvents();
                            string html = HttpUtils.DownloadUrl(dr["wz"].ToString() + dr["lj"].ToString() + "/link/list.php?action=link&pnum=1000", cookie);
                            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(html);
                            //总页数
                            int[] info = CrawlerCenter.GetLinkPages(doc);
                            int pages = info[0];
                            int total = info[1];
                            progressBarControl1.Properties.Maximum = total;
                            progressBarControl1.Properties.Minimum = 0;
                            progressBarControl1.Properties.ShowTitle = true;
                            progressBarControl1.Position = 0;
                            for (int i = 1; i <= pages; i++)
                            {
                                Link col = new Link();
                                for (int j = 1; j <= 1000; j++)
                                {
                                    col = new Link();
                                    if (i == 1)
                                        col = CrawlerCenter.GetLinks(doc, j);
                                    else
                                    {
                                        if (j == 1)
                                        {
                                            html = HttpUtils.DownloadUrl(dr["wz"].ToString() + dr["lj"].ToString() + "/link/list.php?action=link&page=" + i + "&pnum=1000", cookie);
                                            doc = new HtmlAgilityPack.HtmlDocument();
                                            doc.LoadHtml(html);
                                        }
                                        col = CrawlerCenter.GetLinks(doc, j);
                                    }
                                    if (col != null && !string.IsNullOrEmpty(col.Id))
                                    {
                                        progressBarControl1.Position += 1;
                                        dr_col = dt_column.NewRow();
                                        dr_col["id"] = col.Id.Trim();
                                        dr_col["url"] = col.Url;
                                        dr_col["zxurl"] = col.ZxUrl;
                                        dr_col["show"] = col.Show;
                                        dr_col["status"] = col.Status;
                                        dt_column.Rows.Add(dr_col);
                                        //加入列表
                                        gridControl3.DataSource = dt_column;
                                        Application.DoEvents();
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            else
                MessageBox.Show("请先登录网站！");
        }

        private void 加载用户配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _dt.Clear();
            OpenFileDialog dia = new OpenFileDialog();
            dia.InitialDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            dia.Filter = "文本文件|*.txt";
            if (dia.ShowDialog() == DialogResult.OK)
            {
                string line;
                int count = 1;
                string fName = dia.FileName;
                System.IO.StreamReader file = new System.IO.StreamReader(fName, Encoding.Default);
                while ((line = file.ReadLine()) != null)
                {
                    string[] args = line.Split(new string[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
                    DataRow dr = _dt.NewRow();
                    dr["xh"] = count;
                    dr["wz"] = args[0];
                    dr["yhm"] = args[2];
                    dr["mm"] = args[3];
                    dr["lj"] = args[4];
                    dr["zt"] = "";
                    dr["cookie"] = "";
                    _dt.Rows.Add(dr);
                    count++;
                }
                gridControl1.DataSource = _dt;
                file.Close();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (gridView2.DataSource != null)
            {
                DataTable dt = ((DataView)gridView2.DataSource).ToTable();
                if (dt != null)
                {
                    string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "栏目列表.txt";
                    FileStream fs = new FileStream(path, FileMode.Append);
                    StreamWriter wr = null;
                    wr = new StreamWriter(fs);
                    foreach (DataRow dr in dt.Rows)
                    {
                        wr.WriteLine(dr["id"] + "###" + dr["name"] + "###" + dr["address"]);
                    }
                    wr.Close();
                    MessageBox.Show("导出成功！");
                }
            }
            else
                MessageBox.Show("无数据可供导出！");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _Link.Clear();
            OpenFileDialog dia = new OpenFileDialog();
            dia.InitialDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            dia.Filter = "文本文件|*.txt";
            if (dia.ShowDialog() == DialogResult.OK)
            {
                string line;
                int count = 1;
                string fName = dia.FileName;
                System.IO.StreamReader file = new System.IO.StreamReader(fName, Encoding.Default);
                while ((line = file.ReadLine()) != null)
                {
                    string[] args = line.Split(new string[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
                    if (args != null && args.Length > 0)
                    {
                        DataRow dr = _Link.NewRow();
                        dr["id"] = args[0];
                        dr["name"] = args[1];
                        dr["url"] = args[2];
                        dr["mburl"] = args[3];
                        dr["status"] = "";
                        _Link.Rows.Add(dr);
                        count++;
                    }
                }
                gridControl4.DataSource = _Link;
                file.Close();
            }

        }
        /// <summary>
        /// 添加友链
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (gridView4.DataSource != null)
            {
                DataTable dt = ((DataView)gridView4.DataSource).ToTable();
                if (dt != null && dt.Rows.Count > 0)
                {
                    progressBarControl1.Properties.Maximum = dt.Rows.Count;
                    progressBarControl1.Properties.Minimum = 0;
                    progressBarControl1.Properties.ShowTitle = true;
                    progressBarControl1.Position = 0;
                    Application.DoEvents();
                    foreach (DataRow dr in dt.Rows)
                    {
                        NameValueCollection dic = new NameValueCollection();
                        dic.Add("linkwebname", dr["name"].ToString());
                        dic.Add("linkweburl", dr["url"].ToString());
                        dic.Add("linkcolumnid", dr["id"].ToString());
                        dic.Add("linktype", comboBox2.Text.Substring(0, 1));
                        dic.Add("linkcate", comboBox3.Text.Substring(0, 1));
                        dic.Add("linkpos", comboBox1.Text.Substring(0, 1));
                        dic.Add("linkshow", comboBox4.Text.Substring(0, 1));
                        dic.Add("linkwebkey", "1");
                        CookieContainer cc = new CookieContainer();
                        if (hash.ContainsKey(dr["mburl"]))
                        {
                            Cookie c = new Cookie("PHPSESSID", hash[dr["mburl"]].ToString().Split('=')[1]);
                            cc.Add(new Uri(dr["mburl"].ToString()), c);
                            bool flag = HttpUtils.LinkPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/add_save.php", dic, cc);
                            progressBarControl1.Position += 1;
                            if (flag)
                                dr["status"] = "成功";
                            else
                                dr["status"] = "失败";
                            gridControl4.DataSource = dt;
                            Application.DoEvents();
                        }
                        else
                        {
                            MessageBox.Show("请先登录网站！");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("无数据可添加！");
                }
            }
        }
        /// <summary>
        /// 导出友链
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (gridView3.DataSource != null)
            {
                DataTable dt = ((DataView)gridView3.DataSource).ToTable();
                if (dt != null)
                {
                    string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "友链列表.txt";
                    FileStream fs = new FileStream(path, FileMode.Append);
                    StreamWriter wr = null;
                    wr = new StreamWriter(fs);
                    foreach (DataRow dr in dt.Rows)
                    {
                        wr.WriteLine(dr["id"] + "###" + dr["url"] + "###" + dr["zxurl"]);
                    }
                    wr.Close();
                    MessageBox.Show("导出成功！");
                }
            }
            else
                MessageBox.Show("无数据可供导出！");
        }
        private void 开始登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login();
        }
        /// <summary>
        /// 导入域名删除配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            _Delete.Clear();
            OpenFileDialog dia = new OpenFileDialog();
            dia.InitialDirectory = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            dia.Filter = "文本文件|*.txt";
            if (dia.ShowDialog() == DialogResult.OK)
            {
                string line;
                int count = 1;
                string fName = dia.FileName;
                System.IO.StreamReader file = new System.IO.StreamReader(fName, Encoding.Default);
                while ((line = file.ReadLine()) != null)
                {
                    string[] args = line.Split(new string[] { "###" }, StringSplitOptions.RemoveEmptyEntries);
                    if (args != null && args.Length > 0)
                    {
                        DataRow dr = _Delete.NewRow();
                        dr["url"] = args[0];
                        dr["mburl"] = args[1];
                        dr["status"] = string.Empty;
                        _Delete.Rows.Add(dr);
                        count++;
                    }
                }
                gridControl5.DataSource = _Delete;
                file.Close();
            }
        }
        //开始删除
        private void simpleButton2_Click_bak(object sender, EventArgs e)
        {
            if (gridView5.DataSource != null)
            {
                DataTable dt = ((DataView)gridView5.DataSource).ToTable();
                if (dt != null && dt.Rows.Count > 0)
                {

                    foreach (DataRow dr in dt.Rows)
                    {
                        NameValueCollection dic = new NameValueCollection();
                        CookieContainer cc = new CookieContainer();
                        dic.Add("weburl", dr["url"].ToString());
                        dic.Add("search_submit", " 搜 索 ");
                        if (hash.ContainsKey(dr["mburl"]))
                        {

                            Cookie c = new Cookie("PHPSESSID", hash[dr["mburl"]].ToString().Split('=')[1]);
                            cc.Add(new Uri(dr["mburl"].ToString()), c);
                            HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?pnum=100", dic, cc);
                            //总页数
                            int[] info = CrawlerCenter.GetLinkPages(doc);
                            int pages = info == null ? 0 : info[0];
                            int total = info == null ? 0 : info[1];
                            NameValueCollection deleteRows = new NameValueCollection();
                            for (int i = 1; i <= pages; i++)
                            {
                                if (i == 1)
                                {
                                    deleteRows = CrawlerCenter.GetDeleteIds(doc);
                                    bool flag = HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", deleteRows, cc);
                                    if (flag)
                                        DeleteArticleSuccess.Add(dr["mburl"], true);
                                }
                                else
                                {
                                    NameValueCollection ids = HttpUtils.ListPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                                    HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", ids, cc);
                                }
                            }


                            //NameValueCollection ids = HttpUtils.ListPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?pnum=100", dic, cc);
                            //bool flag = HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", ids, cc);
                            //progressBarControl1.Position += 1;
                            //if (flag)
                            //    dr["status"] = "成功";
                            //else
                            //    dr["status"] = "失败";
                            //gridControl5.DataSource = dt;
                            //Application.DoEvents();
                        }
                        else
                        {
                            MessageBox.Show("请先登录网站！");
                        }
                    }
                }
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (gridView5.DataSource != null)
            {
                DataTable dt = ((DataView)gridView5.DataSource).ToTable();
                AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                foreach (DataRow dr in dt.Rows)
                {
                    ThreadPool.QueueUserWorkItem(muiltiDeleteArticle2, dr);
                }
                RegisteredWaitHandle registeredWaitHandle = null;
                registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                {
                    int workerThreads = 0;
                    int maxWordThreads = 0;
                    int compleThreads = 0;
                    ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                    ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                    //Console.WriteLine("线程检测===========可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                    //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                    if (workerThreads == maxWordThreads)
                    {
                        mainAutoResetEvent.Set();
                        registeredWaitHandle.Unregister(null);
                    }
                }), null, 1000, false);
                mainAutoResetEvent.WaitOne();
                Console.WriteLine("删除执行完毕！");
                MessageBox.Show("删除执行完毕！");
            }
            else
            {
                MessageBox.Show("请先登录网站！");
            }
        }

        private void 一键删除无效友链ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView1.DataSource != null)
            {
                DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                foreach (DataRow dr in dt.Rows)
                {
                    ThreadPool.QueueUserWorkItem(muiltiDeleteColumn, dr);
                }
                RegisteredWaitHandle registeredWaitHandle = null;
                registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                {
                    int workerThreads = 0;
                    int maxWordThreads = 0;
                    int compleThreads = 0;
                    ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                    ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                    //Console.WriteLine("线程检测===========可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                    //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                    if (workerThreads == maxWordThreads)
                    {
                        mainAutoResetEvent.Set();
                        registeredWaitHandle.Unregister(null);
                    }
                }), null, 1000, false);
                mainAutoResetEvent.WaitOne();
                Console.WriteLine("删除执行完毕！有效删除网站个数：" + DeleteColumnSuccess.Count);
                MessageBox.Show("删除执行完毕！有效删除网站个数：" + DeleteColumnSuccess.Count);
            }
            else
            {
                MessageBox.Show("请先登录网站！");
            }
        }

        private void 一键删除无效文章ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView1.DataSource != null)
            {
                DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                foreach (DataRow dr in dt.Rows)
                {
                    ThreadPool.QueueUserWorkItem(muiltiDeleteArticle, dr);
                }
                RegisteredWaitHandle registeredWaitHandle = null;
                registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                {
                    int workerThreads = 0;
                    int maxWordThreads = 0;
                    int compleThreads = 0;
                    ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                    ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                    //Console.WriteLine("线程检测===========可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                    //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                    if (workerThreads == maxWordThreads)
                    {
                        mainAutoResetEvent.Set();
                        registeredWaitHandle.Unregister(null);
                    }
                }), null, 1000, false);
                mainAutoResetEvent.WaitOne();
                Console.WriteLine("删除执行完毕！有效删除网站个数：" + DeleteArticleSuccess.Count);
                MessageBox.Show("删除执行完毕！有效删除网站个数：" + DeleteArticleSuccess.Count);
            }
            else
            {
                MessageBox.Show("请先登录网站！");
            }
        }

        private void muiltiDeleteArticle2(object o)
        {
            DataRow dr = _Delete.NewRow();
            dr = (DataRow)o;
            NameValueCollection dic = new NameValueCollection();
            CookieContainer cc = new CookieContainer();
            dic.Add("weburl", dr["url"].ToString());
            dic.Add("search_submit", " 搜 索 ");
            if (hash.ContainsKey(dr["mburl"]))
            {

                Cookie c = new Cookie("PHPSESSID", hash[dr["mburl"]].ToString().Split('=')[1]);
                cc.Add(new Uri(dr["mburl"].ToString()), c);
                HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?pnum=100", dic, cc);
                //总页数
                int[] info = CrawlerCenter.GetLinkPages(doc);
                int pages = info == null ? 0 : info[0];
                int total = info == null ? 0 : info[1];
                NameValueCollection deleteRows = new NameValueCollection();
                for (int i = 1; i <= pages; i++)
                {
                    if (i == 1)
                    {
                        deleteRows = CrawlerCenter.GetDeleteIds(doc);
                        bool flag = HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", deleteRows, cc);
                    }
                    else
                    {
                        NameValueCollection ids = HttpUtils.ListPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                        HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", ids, cc);
                    }
                }


                //NameValueCollection ids = HttpUtils.ListPost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/list.php?pnum=100", dic, cc);
                //bool flag = HttpUtils.deletePost(dr["mburl"].ToString() + ht[dr["mburl"]] + "/link/set.php", ids, cc);
                //progressBarControl1.Position += 1;
                //if (flag)
                //    dr["status"] = "成功";
                //else
                //    dr["status"] = "失败";
                //gridControl5.DataSource = dt;
                //Application.DoEvents();
            }
            else
            {
                Console.WriteLine("请先登录网站！");
            }
        }

        private void muiltiDeleteArticle(object o)
        {
            DataRow dr = _dt.NewRow();
            dr = (DataRow)o;
            NameValueCollection dic = new NameValueCollection();
            CookieContainer cc = new CookieContainer();
            dic.Add("artid", "0");
            dic.Add("pos", "4");
            if (hash.ContainsKey(dr["wz"]))
            {
                Console.WriteLine(dr["wz"] + "开始执行。。。");
                Cookie c = new Cookie("PHPSESSID", hash[dr["wz"]].ToString().Split('=')[1]);
                cc.Add(new Uri(dr["wz"].ToString()), c);
                HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?pnum=100", dic, cc);
                //总页数
                int[] info = CrawlerCenter.GetLinkPages(doc);
                int pages = info == null ? 0 : info[0];
                int total = info == null ? 0 : info[1];
                NameValueCollection deleteRows = new NameValueCollection();
                for (int i = 1; i <= pages; i++)
                {
                    if (i == 1)
                    {
                        deleteRows = CrawlerCenter.GetDeleteIds(doc);
                        bool flag = HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", deleteRows, cc);
                        if (flag)
                            DeleteArticleSuccess.Add(dr["wz"], true);
                    }
                    else
                    {
                        NameValueCollection ids = HttpUtils.ListPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                        HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", ids, cc);
                    }
                }
                Console.WriteLine(dr["wz"] + "执行完毕。。。");
            }
        }

        private void muiltiDeleteColumn(object o)
        {

            DataRow dr = _dt.NewRow();
            dr = (DataRow)o;

            NameValueCollection dic = new NameValueCollection();
            CookieContainer cc = new CookieContainer();
            dic.Add("colid", "0");
            dic.Add("pos", "2");
            if (hash.ContainsKey(dr["wz"]))
            {
                Console.WriteLine(dr["wz"] + "开始执行。。。");
                Cookie c = new Cookie("PHPSESSID", hash[dr["wz"]].ToString().Split('=')[1]);
                cc.Add(new Uri(dr["wz"].ToString()), c);
                HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?pnum=100", dic, cc);
                //总页数
                int[] info = CrawlerCenter.GetLinkPages(doc);
                int pages = info == null ? 0 : info[0];
                int total = info == null ? 0 : info[1];
                NameValueCollection deleteRows = new NameValueCollection();
                for (int i = 1; i <= pages; i++)
                {
                    if (i == 1)
                    {
                        deleteRows = CrawlerCenter.GetDeleteIds(doc);
                        bool flag = HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", deleteRows, cc);
                        if (flag)
                            DeleteColumnSuccess.Add(dr["wz"], true);
                    }
                    else
                    {
                        NameValueCollection ids = HttpUtils.ListPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                        HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", ids, cc);
                    }
                }
                Console.WriteLine(dr["wz"] + "执行完毕。。。");
            }
        }
        /// <summary>
        /// 小工具-删除http
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "确定要开始执行删除吗？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (gridView1.DataSource != null)
                {
                    DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                    AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                    foreach (DataRow dr in dt.Rows)
                    {
                        ThreadPool.QueueUserWorkItem(muiltiDeleteWebName, dr);
                    }
                    RegisteredWaitHandle registeredWaitHandle = null;
                    registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                    {
                        int workerThreads = 0;
                        int maxWordThreads = 0;
                        int compleThreads = 0;
                        ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                        ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                        //Console.WriteLine("线程检测===========可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                        //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                        if (workerThreads == maxWordThreads)
                        {
                            mainAutoResetEvent.Set();
                            registeredWaitHandle.Unregister(null);
                        }
                    }), null, 1000, false);
                    mainAutoResetEvent.WaitOne();
                    MessageBox.Show("程序执行完毕!");
                }
                else
                {
                    MessageBox.Show("请先登录网站！");
                }
            }
        }

        private void muiltiDeleteWebName(object o)
        {
            DataRow dr = _Delete.NewRow();
            dr = (DataRow)o;
            NameValueCollection dic = new NameValueCollection();
            CookieContainer cc = new CookieContainer();
            dic.Add("webname", "http");
            dic.Add("search_submit", " 搜 索 ");
            if (hash.ContainsKey(dr["wz"]))
            {
                Cookie c = new Cookie("PHPSESSID", hash[dr["wz"]].ToString().Split('=')[1]);
                cc.Add(new Uri(dr["wz"].ToString()), c);
                HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?pnum=100", dic, cc);
                //总页数
                int[] info = CrawlerCenter.GetLinkPages(doc);
                int pages = info == null ? 0 : info[0];
                int total = info == null ? 0 : info[1];
                NameValueCollection deleteRows = new NameValueCollection();
                for (int i = 1; i <= pages; i++)
                {
                    if (i == 1)
                    {
                        deleteRows = CrawlerCenter.GetDeleteIds(doc);
                        bool flag = HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", deleteRows, cc);
                    }
                    else
                    {
                        NameValueCollection ids = HttpUtils.ListPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                        HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", ids, cc);
                    }
                }
            }
            else
            {
                Console.WriteLine("请先登录网站！");
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "此功能耗时较长，确定要开始执行删除吗？", "询问", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ThreadPool.SetMinThreads(5, 5);  //设置最小线程数为5个
                ThreadPool.SetMaxThreads(20, 20);  //设置最大线程数为20个，这两个方法要配合使用才能控制线程数量
                if (gridView1.DataSource != null)
                {
                    DataTable dt = ((DataView)gridView1.DataSource).ToTable();
                    AutoResetEvent mainAutoResetEvent = new AutoResetEvent(false);
                    foreach (DataRow dr in dt.Rows)
                    {
                        ThreadPool.QueueUserWorkItem(muiltiDeleteNoHttp, dr);
                    }
                    RegisteredWaitHandle registeredWaitHandle = null;
                    registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(new AutoResetEvent(false), new WaitOrTimerCallback(delegate(object obj, bool timeout)
                    {
                        int workerThreads = 0;
                        int maxWordThreads = 0;
                        int compleThreads = 0;
                        ThreadPool.GetAvailableThreads(out workerThreads, out compleThreads);
                        ThreadPool.GetMaxThreads(out maxWordThreads, out compleThreads);
                        //Console.WriteLine("线程检测===========可用线程{0},最大线程{1}", workerThreads, maxWordThreads);
                        //当可用的线数与池程池最大的线程相等时表示线程池中所有的线程已经完成
                        if (workerThreads == maxWordThreads)
                        {
                            mainAutoResetEvent.Set();
                            registeredWaitHandle.Unregister(null);
                        }
                    }), null, 1000, false);
                    mainAutoResetEvent.WaitOne();
                    MessageBox.Show("程序执行完毕!");
                }
                else
                {
                    MessageBox.Show("请先登录网站！");
                }
            }
        }
        /// <summary>
        /// 删除不带http的链接
        /// </summary>
        /// <param name="o"></param>
        private void muiltiDeleteNoHttp(object o)
        {
            try
            {
                DataRow dr = _Delete.NewRow();
                dr = (DataRow)o;
                NameValueCollection dic = new NameValueCollection();
                NameValueCollection dic_all = new NameValueCollection();
                CookieContainer cc = new CookieContainer();
                dic.Add("weburl", "http");
                dic.Add("search_submit", " 搜 索 ");
                dic_all.Add("search_submit", " 搜 索 ");
                if (hash.ContainsKey(dr["wz"]))
                {

                    Cookie c = new Cookie("PHPSESSID", hash[dr["wz"]].ToString().Split('=')[1]);
                    cc.Add(new Uri(dr["wz"].ToString()), c);
                    Console.WriteLine(dr["wz"] + " 开始执行。。。。。。。。。。");
                    //查询所有的IDS
                    HtmlAgilityPack.HtmlDocument doc_all = HttpUtils.HtmlPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?pnum=100", dic_all, cc);
                    //查询带http的IDS
                    HtmlAgilityPack.HtmlDocument doc = HttpUtils.HtmlPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?pnum=100", dic, cc);

                    #region + 获取所有链接的IDS
                    int[] info_all = CrawlerCenter.GetLinkPages(doc_all);
                    int pages_all = info_all == null ? 0 : info_all[0];
                    int total_all = info_all == null ? 0 : info_all[1];
                    NameValueCollection deleteRows_all_1 = new NameValueCollection();
                    NameValueCollection deleteRows_all_e = new NameValueCollection();
                    NameValueCollection[] paras = new NameValueCollection[pages_all];
                    for (int i = 1; i <= pages_all; i++)
                    {
                        if (i == 1)
                        {
                            deleteRows_all_1 = CrawlerCenter.GetDeleteIds(doc_all);
                            if (deleteRows_all_1 != null)
                                paras[0] = deleteRows_all_1;
                        }
                        else
                        {
                            deleteRows_all_e = HttpUtils.ListPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic_all, cc);
                            if (deleteRows_all_e != null)
                                paras[i - 1] = deleteRows_all_e;
                        }
                    }
                    NameValueCollection all_ids = CrawlerCenter.Merge(paras);
                    #endregion
                    #region + 获取带http的链接的IDS
                    //总页数
                    int[] info = CrawlerCenter.GetLinkPages(doc);
                    int pages = info == null ? 0 : info[0];
                    int total = info == null ? 0 : info[1];
                    NameValueCollection deleteRows_1 = new NameValueCollection();
                    NameValueCollection deleteRows_e = new NameValueCollection();
                    NameValueCollection[] paras_ = new NameValueCollection[pages];
                    for (int i = 1; i <= pages; i++)
                    {
                        if (i == 1)
                        {
                            deleteRows_1 = CrawlerCenter.GetDeleteIds(doc);
                            if (deleteRows_1 != null)
                                paras_[0] = deleteRows_1;
                        }
                        else
                        {
                            deleteRows_e = HttpUtils.ListPost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/list.php?action=link&pnum=100&page=" + i, dic, cc);
                            if (deleteRows_e != null)
                                paras_[i - 1] = deleteRows_e;
                        }
                    }
                    NameValueCollection ids = CrawlerCenter.Merge(paras_);
                    NameValueCollection finalIds = CrawlerCenter.Duplicate(all_ids, ids);
                    Console.WriteLine(dr["wz"] + "找到未含有http链接个数：" + finalIds.Count + "个");
                    if (finalIds.Count > 0)
                        HttpUtils.deletePost(dr["wz"].ToString() + ht[dr["wz"]] + "/link/set.php", finalIds, cc);
                    Console.WriteLine(dr["wz"] + "执行完毕。");
                    #endregion
                }
                else
                {
                    Console.WriteLine("请先登录网站！");
                }
            }
            catch (Exception e1) { Console.WriteLine("发生异常：" + e1.Message); }
        }

    }
}