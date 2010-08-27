using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WebScraper.Web
{
    public abstract class CookieLocator : ElementLocator<String>
    {
        public string Name { get; set; }
        public string ContextKey { get; set; }
        public CookieLocator()
        {

        }
        public CookieLocator(string name = null, string contextKey = null)
        {
            this.Name = name;
            this.ContextKey = contextKey;
        }
        public string getName()
        {
            return Name;
        }
        public abstract String locate(Agent agent);
    }

    public class CookieElementLocator : CookieLocator
    {
        public Regex NameRegex { get; set; }

        public CookieElementLocator()
            : base()
        {

        }
        public CookieElementLocator(string name = null, Regex nameRegex = null, String contextKey = null)
            : base(name, contextKey)
        {
            this.NameRegex = nameRegex;
        }

        public override String locate(Agent agent)
        {
            String ret = null;
            if (null != agent)
            {
                String cookieValue = agent.WebBrowser.Document.Cookie;
                if (null != cookieValue && null != NameRegex)
                {
                    if (NameRegex.IsMatch(cookieValue))
                    {
                        ret = NameRegex.Match(cookieValue).Value;
                    }
                }
            }
            if (null != ContextKey)
            {
                agent.RequestContext.Add(ContextKey, ret);
            }
            return ret;
        }
    }

}
