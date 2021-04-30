using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections;
using Links;
using DevExpress.LookAndFeel;
using System.Drawing;

namespace Links
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            UserLookAndFeel.Default.SetSkinStyle("Blue");//黑色主题
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new Main()); 
            Hashtable hash = new Hashtable();
            NameValueCollection dic = new NameValueCollection();
            //dic.Add("username","admin");
            //dic.Add("password","admin888");
            //string cookie = string.Empty;
            //if (!hash.ContainsKey("http://www.owl21.com/"))
            //{
            //    cookie = HttpUtils.PostForm("http://www.owl21.com/cms80/login.php?action=logincheck", dic);
            //    hash.Add("http://www.owl21.com/", cookie);
            //}
            //cookie = hash["http://www.owl21.com/"].ToString();
            //string html = HttpUtils.DownloadUrl("http://www.owl21.com/cms80/column/list.php?action=column", cookie);
            //List<Column> list = CrawlerCenter.GetColumns(html);

            //Image a = HttpUtils.GetVercode("http://www.owl21.com/cms80/vercode.php");
            //Bitmap img = new Bitmap(a);
            //ImagDo.imgdo(img);

        }
    }
}
