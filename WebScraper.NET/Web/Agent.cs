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

        public List<WebAction> WebActions { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<String, Object> RequestContext { get; set; }

        public Dictionary<String, Object> Outputs { get; set; }

        protected Queue<WebAction> activeActions;

        protected WebAction activeAction;


        protected AutoResetEvent trigger;

        protected WaitHandle[] waitHandles;

        public Agent()
        {
        }

        public virtual void init()
        {
            RequestContext = new Dictionary<string, object>();
            Outputs = new Dictionary<string, object>();
            activeActions = new Queue<WebAction>(WebActions);
            WebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.pageLoaded);
            trigger = new AutoResetEvent(false);

            waitHandles = new WaitHandle[] { trigger };

        }

        public virtual void startAgent()
        {
            doActions();
        }

        public virtual void doActions()
        {
            while (0 < activeActions.Count)
            {
                activeAction = activeActions.Dequeue();
                if (activeAction.canDoAction(this))
                {
                    activeAction.doAction(this);
                    if (activeAction.isWaitForEvent())
                    {
                        trigger.Reset();
                        WaitHandle.WaitAny(waitHandles);
                    }
                }
            }
        }

        public virtual void pageLoaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (null != activeAction && activeAction.isWaitForEvent() && activeAction.validate(this))
            {
                trigger.Set();
            }
        }

    }


    public class PageDumpAgent : Agent
    {
    }

}
