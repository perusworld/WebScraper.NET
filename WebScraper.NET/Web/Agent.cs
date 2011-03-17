using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebScraper.Data;

namespace WebScraper.Web
{

    public class AccessTiming
    {

        public Uri URI { get; set; }

        [DefaultValue(0)]
        public long TimingInTicks { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime CurrentStartTime { get; private set; }

        public AccessTiming(Uri uri, long TimingInTicks = 0)
        {
            this.URI = uri;
            this.TimingInTicks = TimingInTicks;
            this.StartTime = DateTime.UtcNow;
            this.CurrentStartTime = this.StartTime;
        }

        public void markTiming()
        {
            DateTime nowTime = DateTime.UtcNow;
            TimingInTicks += nowTime.Ticks - this.CurrentStartTime.Ticks;
        }

        public void startTiming()
        {
            this.CurrentStartTime = DateTime.UtcNow;
        }

        public void addTiming(AccessTiming accessTiming)
        {
            this.TimingInTicks += accessTiming.TimingInTicks;
        }

        public int getTimingInSeconds()
        {
            return new TimeSpan(TimingInTicks).Seconds;
        }


    }

    public abstract class Agent
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<String, Object> RequestContext { get; set; }

        public Dictionary<String, Object> Outputs { get; set; }

        public Boolean MonitorTimings { get; set; }

        protected Stack<AccessTiming> AccessTimes { get; set; }

        protected WebAction activeAction;

        protected WebBrowserDocumentCompletedEventHandler completedEventHandler;

        protected WebBrowserDocumentCompletedEventHandler completedEventHandlerForTiming;

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
            if (MonitorTimings)
            {
                completedEventHandlerForTiming = new WebBrowserDocumentCompletedEventHandler(this.pageLoadedForMonitoring);
                WebBrowser.DocumentCompleted += completedEventHandlerForTiming;
                AccessTimes = new Stack<AccessTiming>();
            }
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
            if (null != completedEventHandlerForTiming)
            {
                updateAccessTimings(WebBrowser.Url, true);
                WebBrowser.DocumentCompleted -= completedEventHandlerForTiming;
            }
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

        public virtual void pageLoadedForMonitoring(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (MonitorTimings)
            {
                updateAccessTimings(e.Url);
            }
        }

        protected void updateAccessTimings(Uri url, Boolean updateOldOnly = false)
        {
            if (null != AccessTimes)
            {
                AccessTiming lastEntry = 0 == AccessTimes.Count ? null : AccessTimes.Peek();
                if (null != lastEntry)
                {
                    lastEntry.markTiming();
                }
                if (!updateOldOnly)
                {
                    AccessTimes.Push(new AccessTiming(url));
                }
            }
        }

        public List<AccessTiming> getDomainAccessTimings()
        {
            List<AccessTiming> ret = new List<AccessTiming>();
            Dictionary<String, AccessTiming> timingMap = new Dictionary<String, AccessTiming>();
            if (null != AccessTimes)
            {
                foreach (AccessTiming accessTime in AccessTimes)
                {
                    AccessTiming timing = timingMap.ContainsKey(accessTime.URI.Host) ? timingMap[accessTime.URI.Host] : null;
                    if (null == timing)
                    {
                        timing = new AccessTiming(accessTime.URI, accessTime.TimingInTicks);
                        timingMap[accessTime.URI.Host] = timing;
                        ret.Add(timing);
                    }
                    else
                    {
                        timing.addTiming(accessTime);
                    }
                }
                ret.Reverse();
            }
            return ret;
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
