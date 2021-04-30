using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Specialized;


namespace Links
{
    public class CrawlerCenter
    {
        /// <summary>
        /// 获取栏目
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Column GetColumns(HtmlDocument doc, int index)
        {

            Column column = new Column();
            string categoryPath = "//table[2]//form[1]/tr[" + index + "]/td[1]";
            string columnPath = "//table[2]//form[1]/tr[" + index + "]/td[2]/a";
            string urlPath = "//table[2]//form[1]/tr[" + index + "]/td[3]/a";
            HtmlNodeCollection categoryList = doc.DocumentNode.SelectNodes(categoryPath);
            HtmlNodeCollection titleList = doc.DocumentNode.SelectNodes(columnPath);
            HtmlNodeCollection urlList = doc.DocumentNode.SelectNodes(urlPath);
            if (categoryList != null && titleList != null && urlList != null)
            {
                column.Id = categoryList[0].InnerText;
                column.Name = titleList[0].InnerText;
                column.Url = urlList[0].Attributes["href"].Value;
            }
            else
                return null;
            return column;
        }
        public static Link GetLinks(HtmlDocument doc, int index)
        {
            Link link = new Link();
            string idPath = "//table[2]//form[1]/tr[" + index + "]/td[1]";
            string urlPath = "//table[2]//form[1]/tr[" + index + "]/td[4]";
            string zxurlPath = "//table[2]//form[1]/tr[" + index + "]/td[7]/a";
            string showPath = "//table[2]//form[1]/tr[" + index + "]/td[8]";
            string statusPath = "//table[2]//form[1]/tr[" + index + "]/td[9]";
            HtmlNodeCollection idList = doc.DocumentNode.SelectNodes(idPath);
            HtmlNodeCollection urlList = doc.DocumentNode.SelectNodes(urlPath);
            HtmlNodeCollection zxurlList = doc.DocumentNode.SelectNodes(zxurlPath);
            HtmlNodeCollection showList = doc.DocumentNode.SelectNodes(showPath);
            HtmlNodeCollection statusList = doc.DocumentNode.SelectNodes(statusPath);
            if (idList != null)
            {
                link.Id = idList == null ? "" : idList[0].InnerText;
                if (zxurlList != null && zxurlList.Count > 1)
                    link.ZxUrl = zxurlList[1].HasAttributes == false ? "" : zxurlList[1].Attributes["href"].Value;
                else if (zxurlList != null)
                    link.ZxUrl = zxurlList[0].HasAttributes == false ? "" : zxurlList[0].Attributes["href"].Value;
                else
                    link.ZxUrl = string.Empty;
                link.Url = urlList == null ? "" : urlList[0].InnerText;
                link.Show = showList == null ? "" : showList[0].InnerText;
                link.Status = statusList == null ? "" : statusList[0].InnerText;
                return link;
            }
            else
                return null;

        }
        public static int[] GetPages(HtmlDocument doc)
        {
            Column column = new Column();
            string pagePath = "//td[@colspan=\"18\"][@align=\"center\"][1]/text()";
            HtmlNodeCollection categoryList = doc.DocumentNode.SelectNodes(pagePath);
            if (categoryList != null)
            {
                HtmlNode[] innerText = categoryList.Where(x => x.InnerText.Contains("共")).ToArray();
                string result = System.Text.RegularExpressions.Regex.Replace(innerText[0].InnerText, @"[^0-9]+", ",");
                string[] args = result.Split(',');
                args = args.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                int[] info = new int[2];
                info[0] = Convert.ToInt16(args[0]);
                info[1] = Convert.ToInt16(args[1]);
                return info;
            }
            else return null;
        }
        public static int[] GetLinkPages(HtmlDocument doc)
        {
            if (doc != null)
            {
                Column column = new Column();
                string pagePath = "//td[@colspan=\"12\"][@align=\"center\"][1]/text()";
                HtmlNodeCollection categoryList = doc.DocumentNode.SelectNodes(pagePath);
                if (categoryList == null)
                {
                    pagePath = "//td[@colspan=\"13\"][@align=\"center\"][1]/text()";
                    categoryList = doc.DocumentNode.SelectNodes(pagePath);
                }
                if (categoryList != null)
                {
                    HtmlNode[] innerText = categoryList.Where(x => x.InnerText.Contains("共")).ToArray();
                    if (innerText.Length > 0)
                    {
                        string result = System.Text.RegularExpressions.Regex.Replace(innerText[0].InnerText, @"[^0-9]+", ",");
                        string[] args = result.Split(',');
                        args = args.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                        int[] info = new int[2];
                        info[0] = Convert.ToInt32(args[0]);
                        info[1] = Convert.ToInt32(args[1]);
                        return info;
                    }
                    else
                        return null;
                }
                else return null;
            }
            else return null;
        }
        public static System.Collections.Specialized.NameValueCollection GetDeleteIds(HtmlDocument doc)
        {
            string pagePath = "//input[@name=\"alllinkid[]\"]";
            HtmlNodeCollection idsList = doc.DocumentNode.SelectNodes(pagePath);
            System.Collections.Specialized.NameValueCollection dic = new System.Collections.Specialized.NameValueCollection();
            if (idsList != null)
            {
                int index = 0;
                foreach (HtmlNode node in idsList)
                {
                    dic.Add("alllinkid[]" + node.Attributes["value"].Value, node.Attributes["value"].Value);
                    index++;
                }
                dic.Add("submit", "删除链接");
            }
            return dic;
        }

        /// <summary>
        /// Merges the specified NameValueCollection arguments into a single NamedValueCollection.
        /// </summary>
        /// <param name="args">An array of NameValueCollection to be merged.</param>
        /// <returns>Merged NameValueCollection</returns>
        /// <remarks>
        /// Returns an empty collection if args passed are null or empty.
        /// </remarks>
        public static NameValueCollection Merge(params NameValueCollection[] args)
        {
            NameValueCollection combined = new NameValueCollection();

            if (args == null || args.Length == 0)
            {
                return combined;
            }

            NameValueCollection current = null;
            for (int i = 0; i < args.Length; i++)
            {

                current = args[i];
                if (i > 0)
                {
                    if (current != null)
                        current.Remove("submit");
                }
                if (current != null && current.Count > 0)
                {
                    combined.Add(current);
                }
            }
            Console.WriteLine("已找到符合项目：" + combined.Count + "个");
            return combined;
        }
        /// <summary>
        /// 去重
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static NameValueCollection Duplicate(NameValueCollection all,NameValueCollection part)
        {
            if (all.Count > part.Count)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    for (int j = 0; j < part.Count; j++)
                    {
                        if (all[i].Equals(part[j]))
                            all.Remove("alllinkid[]" + all[i]);
                    }
                }
            }
            else
                all = new NameValueCollection();
            return all;
        }
    }


}
