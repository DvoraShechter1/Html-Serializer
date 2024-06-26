using Newtonsoft.Json;

namespace Crawler
{
    internal class HtmlElement
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> Attributes { get; set; }
        public List<string> Classes { get; set; }
        public string innerHtml { get; set; }
        public HtmlElement father { get; set; }
        public List<HtmlElement> children { get; set; }
        public HtmlElement()
        {
            children = new List<HtmlElement>();
            innerHtml = "";
        }
        public override string ToString()//הדפסה ללא הילדים
        {
            string r = "name="+name+". ";
            if (!string.IsNullOrWhiteSpace(id))
                r +="id="+id+". ";
            if (Classes != null && Classes.Count()>0)
            {
                r += "classes: ";
                foreach (string s in Classes)
                {
                    r += s+", ";
                }
                r+= ". ";
            }
            if (Attributes!=null && Attributes.Count()>0)
            {
                r+="Attributes: ";
                foreach (string s in Attributes)
                {
                    r += s+", ";
                }
                r+=". ";
            }
            if (innerHtml!="")
            {
                r+="inner html="+innerHtml+". ";
            }
            return r;
        }
        public string ToString(bool a)
        {
            string r = "name="+name+". ";
            if (!string.IsNullOrWhiteSpace(id))
                r +="id="+id+". ";
            if (Classes != null && Classes.Count()>0)
            {
                r += "classes: ";
                foreach (string s in Classes)
                {
                    r += s+", ";
                }
                r+= ". ";
            }
            if (Attributes!=null && Attributes.Count()>0)
            {
                r+="Attributes: ";
                foreach (string s in Attributes)
                {
                    r += s+", ";
                }
                r+=". ";
            }
            if (innerHtml!="")
            {
                r+="inner html="+innerHtml+". ";
            }
            if (children.Count()>0)
            {
                r+="children: ";
                foreach (var child in children)
                {
                    r+=child.ToString(true);
                }
            }
            return "\n"+r;
        }//הדפסה עם ילדים


        //====================התאמות==============================================================
        public IEnumerable<HtmlElement> Descendants()//החזרת הצאצאים
        {
            Queue<HtmlElement> q = new Queue<HtmlElement>();
            q.Enqueue(this);
            while (q.Count > 0)
            {
                HtmlElement el = q.Dequeue();
                foreach (HtmlElement h in el.children)
                {
                    q.Enqueue(h);
                }
                yield return el;
            }
        }

        public List<HtmlElement> Ancestors()//החזרת אבות קדמונים
        {
            List<HtmlElement> l= new List<HtmlElement>();
            HtmlElement temp = this;
            while (temp != null)
            {
                l.Add(temp);
                temp=temp.father;
            }
            return l;
        }

        public bool fitOne(Selector s)//התאמה של סלקטור לאלמנט נוכחי
        {
            if (s.Classes.Count()==0)
                goto ok1;
            if (this.Classes == null && s.Classes.Count()>0)
                return false;
            if(this.Classes.Count()<s.Classes.Count())
                return false;
            else if(this.Classes != null && Classes.Count()>0)
                foreach (string cl in s.Classes)
                {
                    if(!this.Classes.Contains(cl))
                        return false;
                }
            ok1:
            return (s.name==this.name || s.name==null) && (s.id==this.id || s.id==null);
        }

        public List<HtmlElement> fit(Selector s)
        {
            ///ע"מ לא ליצור בכל פעם רשימה של כלל הצאצאים מה שיגרור סיבוכיות רבה:
            ///ניצור רשימה של כלל האלמנטים בעץ ונבדוק מי מהם מתאים לסלקטור הפנימי ביותר
            ///אותו נמצא ע"י גישה לצאצא האחרון של עץ הסלקטורים 
            ///ברגע שיש בידינו רשימת אלמנטים מתאימים נעבור על האבות שלהם בהתאמה לעץ הסלקטורים
            List<HtmlElement> fit = new List<HtmlElement>();
            List<HtmlElement> allFit = new List<HtmlElement>();
            List<HtmlElement> Descendants = this.Descendants().ToList();//קליטת כל הצאצאים
            //קבלת הסלקטור האחרון
            Selector last=s,temp=s;
            while (temp!=null)
            {
                last = temp;
                temp = temp.child;
            }
            foreach (HtmlElement el in Descendants)
                if(el.fitOne(last))
                    fit.Add(el);
            foreach (HtmlElement el in fit)
            {
                Selector fs = last.father;
                HtmlElement fH = el.father;
                while (fs != null && fH!=null)
                {
                    if (fH.fitOne(fs))
                    {
                        fs=fs.father;
                        fH=fH.father;
                    }
                    else
                        fH=fH.father;
                }
                if (fs == null)
                    allFit.Add(el);
            }
            return allFit;
        }//התאמה כללית של אוביקט לסלקטור
    }


    internal class HtmlHelper
    {
        private static readonly HtmlHelper _helper = new HtmlHelper();
        public static HtmlHelper helper => _helper;
        public List<string> tags { get; set; }
        public List<string> notCloseTags { get; set; }
        private HtmlHelper()
        {
            tags= JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("HtmlTags.json"));
            notCloseTags = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("HtmlVoidTags.json"));
        }
    }

}
