using System;
using System.Windows.Forms;

namespace WebScraper.Web
{
    public abstract class HtmlElementLocator : ElementLocator<HtmlElement>
    {
        public string Name { get; set; }
        public string ContextKey { get; set; }
        public HtmlElementLocator()
        {

        }
        public HtmlElementLocator(string name = null, string contextKey = null)
        {
            this.Name = name;
            this.ContextKey = contextKey;
        }
        public string getName()
        {
            return Name;
        }
        public HtmlElement locate(Agent agent)
        {
            HtmlElement ret = null;
            MethodInvoker delegateCall = delegate
            {
                ret = internalLocate(agent);
                if (null != ContextKey)
                {
                    agent.RequestContext.Add(ContextKey, ret);
                }

            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
            return ret;
        }
        public abstract HtmlElement internalLocate(Agent agent);
    }

    public class SimpleHtmlElementLocator : HtmlElementLocator
    {
        public ElementMatcher<HtmlElement> Matcher { get; set; }

        public SimpleHtmlElementLocator()
            : base()
        {

        }
        public SimpleHtmlElementLocator(string name = null, ElementMatcher<HtmlElement> matcher = null
            )
            : base(name)
        {
            this.Matcher = matcher;
        }

        public override HtmlElement internalLocate(Agent agent)
        {
            HtmlElement ret = null;
            if (null != agent)
            {
                foreach (HtmlElement element in agent.WebBrowser.Document.All)
                {
                    if (Matcher.match(element))
                    {
                        ret = element;
                        break;
                    }
                }
            }
            return ret;
        }
    }


    public class IdElementLocator : HtmlElementLocator
    {
        public string Id { get; set; }
        public ElementMatcher<HtmlElement> Matcher { get; set; }

        public IdElementLocator()
            : base()
        {

        }

        public IdElementLocator(string name = null, string id = null, String contextKey = null, ElementMatcher<HtmlElement> matcher = null)
            : base(name, contextKey)
        {
            this.Id = id;
            this.Matcher = matcher;
        }

        public override HtmlElement internalLocate(Agent agent)
        {
            HtmlElement ret = null;
            if (null != agent && null != agent.WebBrowser.Document)
            {
                ret = agent.WebBrowser.Document.GetElementById(Id);
            }
            if (null != ret && null != Matcher)
            {
                if (!Matcher.match(ret))
                {
                    ret = null;
                }
            }
            return ret;
        }
    }

    public class TagElementLocator : HtmlElementLocator
    {
        public string Tag { get; set; }
        public bool Recursive { get; set; }
        public ElementMatcher<HtmlElement> Matcher { get; set; }
        public ChildHtmlElementLocator ChildLocator { get; set; }
        public TagElementLocator()
            : base()
        {

        }

        public TagElementLocator(string name = null, string tag = null, bool recursive = false, String contextKey = null, ChildHtmlElementLocator childLocator = null, ElementMatcher<HtmlElement> matcher = null)
            : base(name, contextKey)
        {
            this.Tag = tag;
            this.Recursive = recursive;
            this.ChildLocator = childLocator;
            this.Matcher = matcher;
        }

        private HtmlElement locateInternal(HtmlDocument document)
        {
            HtmlElement ret = null;
            if (null != document)
            {
                HtmlElementCollection elements = document.GetElementsByTagName(Tag);
                if (null != elements)
                {
                    foreach (HtmlElement element in elements)
                    {
                        if (null == Matcher || Matcher.match(element))
                        {
                            if (null == ChildLocator)
                            {
                                ret = element;
                            }
                            else
                            {
                                ret = ChildLocator.locate(element);
                            }
                            if (null != ret)
                            {
                                break;
                            }
                        }
                    }
                }
                if (null == ret && Recursive)
                {
                    if (null != document.Window.Frames && 0 < document.Window.Frames.Count)
                    {
                        foreach (HtmlWindow window in document.Window.Frames)
                        {
                            ret = locateInternal(window.Document);
                            if (null != ret)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        public override HtmlElement internalLocate(Agent agent)
        {
            HtmlElement ret = null;
            if (null != agent)
            {
                ret = locateInternal(agent.WebBrowser.Document);
            }
            return ret;
        }
    }

}
