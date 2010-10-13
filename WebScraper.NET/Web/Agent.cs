using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebScraper.Data;

namespace WebScraper.Web
{
    public abstract class Agent
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<String, Object> RequestContext { get; set; }

        public Dictionary<String, Object> Outputs { get; set; }

        protected WebAction activeAction;

        protected WebBrowserDocumentCompletedEventHandler completedEventHandler;

        protected AutoResetEvent trigger;

        protected WaitHandle[] waitHandles;

        public Agent()
        {
        }

        public Agent(WebBrowser browser = null)
        {
            this.WebBrowser = browser;
        }


        public virtual void init()
        {
            RequestContext = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            trigger = new AutoResetEvent(false);
            waitHandles = new WaitHandle[] { trigger };

        }

        public virtual void doActions(List<WebAction> actions)
        {
            completedEventHandler = new WebBrowserDocumentCompletedEventHandler(this.pageLoaded);
            WebBrowser.DocumentCompleted += completedEventHandler;
            Queue<WebAction> activeActions = new Queue<WebAction>(actions);
            while (0 < activeActions.Count)
            {
                activeAction = activeActions.Dequeue();
                if (activeAction.canDoAction(this))
                {
                    if (activeAction.shouldWaitAction(this))
                    {
                        trigger.Reset();
                        WaitHandle.WaitAny(waitHandles);
                    }
                    activeAction.doAction(this);
                    if (activeAction.isWaitForEvent())
                    {
                        trigger.Reset();
                        WaitHandle.WaitAny(waitHandles);
                    }
                }
            }
            completedActions();
        }

        public virtual void completedActions()
        {
            WebBrowser.DocumentCompleted -= completedEventHandler;
        }

        public virtual void cleanup()
        {

        }

        public virtual void completedWaitAction()
        {
            trigger.Set();
        }

        public virtual bool validateActiveAction()
        {
            bool ret = false;
            if (null != activeAction && activeAction.isWaitForEvent() && activeAction.validate(this))
            {
                ret = true;
                trigger.Set();
            }
            return ret;
        }

        public virtual void pageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            validateActiveAction();
        }

    }

    public class SimpleAgent : Agent
    {
        public List<WebAction> WebActions { get; set; }

        public SimpleAgent()
            : base()
        {

        }
        public SimpleAgent(WebBrowser browser = null, List<WebAction> actions = null)
            : base(browser: browser)
        {
            this.WebActions = actions;
        }

        public virtual void startAgent()
        {
            doActions(WebActions);
        }

    }

    public class PageDumpAgent : SimpleAgent
    {
    }

}
