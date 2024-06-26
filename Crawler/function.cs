using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;


namespace Crawler
{
    internal static class function
    {
        //========== (יצירת העץ (שלב א + HTML קליטת ==============================================================
        public static async Task<string> Load(string url)//מכתובת אתר HTML קריאת 
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }

        public static string[] htmlArr(string html)//המרת הטקסט למערך מסודר
        {
            var cleanHtml = new Regex("\t").Replace(html, "");
            var cleanHtml1 =new Regex("\r").Replace(cleanHtml, "");
            var cleanHtml2 =new Regex("\n").Replace(cleanHtml1, "");

            // "<(.*?)>" קבלת כל הסטרינגים
            //הכנסה של כל השורות לטקסט בלי השורות הריקות
            var htmlLines = new Regex("<(.*?)>").Split(cleanHtml2).Where(s => s.Length>0 && !string.IsNullOrWhiteSpace(s));
            return htmlLines.ToArray();
        }

        public static HtmlElement newHTML(string html)//פונקציה שמקבלת תגית כמחרוזת והופכת אותה לאלמנט
        {
            HtmlElement h = new HtmlElement();
            int len = html.IndexOf(' ');
            if (len<0)
                len = html.Length;
            h.name = html.Substring(0, len);//חיפוש המילה הראשונה עד הרווח היא שם התגית
            var attributes = new Regex("([^\\s]*?)=\"(.*?)\"").Matches(html);
            List<string> at = new List<string>();
            foreach (var item in attributes)
            {
                string it = item.ToString();
                if (it.StartsWith("id=") || it == "id =(.*?)")
                {
                    int len1 = it.Length-1-it.IndexOf('"')-1;//id אורך ה
                    h.id = it.Substring(it.IndexOf("\"")+1, len1);
                }
                else if (it.StartsWith("class=") || it =="class=(.*?)")
                {
                    int k = it.IndexOf('"')+1;
                    string class1 = it.Substring(k, it.LastIndexOf('"')-k);
                    string[] classes = class1.Split(' ');
                    List<string> list = new List<string>();
                    foreach (var ss in classes)
                    {
                        list.Add(ss);
                    }
                    h.Classes=list;
                }
                else
                    at.Add(it);
            }
            h.Attributes = at;
            return h;
        }

        public static HtmlElement htmlTree(string[] arr)//יצירת העץ
        {
            //הגדרת התגיות
            HtmlHelper hh = HtmlHelper.helper;
            List<string> notCloseTag = hh.notCloseTags;
            List<string> tag = hh.tags;

            //הפונקציה הינה בהנחה והקלט תקין
            HtmlElement root = null, temp = null;
            for (int i = 0; i<arr.Length; i++)
            {
                int len = arr[i].IndexOf(' ');
                if (len == -1)
                    len = arr[i].Length;
                if (notCloseTag.Contains(arr[i].Substring(0, len)))//אם מדובר בתגית שאין לה פתיחה וסגירה
                {
                    HtmlElement h = newHTML(arr[i]);
                    //חיבור בן ואב
                    h.father = temp;
                    temp.children.Add(h);
                }
                else if (tag.Contains(arr[i].Substring(0, len)))//אם מדובר בתגית פותחת
                {
                    HtmlElement h = newHTML(arr[i]);
                    //חיבור בן ואב
                    if (temp!=null)
                        temp.children.Add(h);
                    else//אם מדובר בראשון
                        root= h;
                    h.father = temp;
                    temp = h;
                }
                else if (arr[i].StartsWith("!") || arr[i].StartsWith("//") || arr[i].StartsWith("/*"));//מדובר בתגית הערה

                else if (arr[i].StartsWith("/"))//אם מדובר בתגית סוגרת
                {
                    if (temp.name==arr[i].Substring(1))
                        temp=temp.father;//שמור את אב התגית הנוכחית
                    else
                    {
                        while (temp.name==arr[i].Substring(1))
                            temp=temp.father;
                    }
                    if (temp==null)
                        return root;

                }
                else //innerHTML מדובר ב
                {
                    temp.innerHtml += arr[i];
                }
            }

            return root;
        }


        //=========== (בנית הסלקטור (שלב ב =======================================================================
        public static Selector newSelect(string s)//פונקציה שממירה מחרוזת סלקטור לאוביקט
        {
            Selector sel = new Selector();
            string[] strings = s.Split(new char[] { '.', '#' });
            int i = 1;
            if(!s.StartsWith(".") && !s.StartsWith("#"))//אם זה אמור להיות שם תגית
            {
                HtmlHelper hh = HtmlHelper.helper;
                if (hh.tags.Contains(strings[0]))
                {
                    sel.name = strings[0];
                }
                else
                {
                    return null;
                }
                s = s.Substring(strings[0].Length);//תחתוך את החלק הנוכחי בסלקטור המחרוזתי
            }

            for (; i<strings.Length; i++)
            {
                if(s.StartsWith("."))//class מדובר ב
                    sel.Classes.Add(strings[i]);
                else//id מדובר ב 
                    sel.id = strings[i];
                if(i!=strings.Length-1)
                s=s.Substring(strings[i].Length+1);
            }
            return sel;
        }

        public static Selector selectorTree(string s)//יצירת עץ סלקטורים
        {
            string[] sels = s.Split(' ');
            Selector father = newSelect(sels[0]),temp=father,temp2;
            for(int i=1;i<sels.Length;i++)
            {
                temp2=newSelect(sels[i]);
                temp2.father = temp;
                temp.child=temp2;
                temp = temp2;
            }
            return father;
        }

    }
}
