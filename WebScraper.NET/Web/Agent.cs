using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

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

        public double getTimingInSeconds()
        {
            return new TimeSpan(TimingInTicks).TotalSeconds;
        }


    }

    public interface AgentCallback
    {
        void onCompleted(Agent agent);
    }

    public abstract class Agent
    {

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public WebBrowser WebBrowser { get; set; }

        public Dictionary<String, Object> RequestContext { get; set; }

        public Dictionary<String, Object> Outputs { get; set; }

        public Boolean MonitorTimings { get; set; }

        public AgentCallback AgentCallback { get; set; }

        protected Stack<AccessTiming> AccessTimes { get; set; }

        protected WebAction activeAction;

        protected Queue<WebAction> activeActions;

        protected WebBrowserDocumentCompletedEventHandler completedEventHandler;

        protected WebBrowserDocumentCompletedEventHandler completedEventHandlerForTiming;

        public DateTime LastedUpdated { get; private set; }

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
            if (MonitorTimings)
            {
                completedEventHandlerForTiming = new WebBrowserDocumentCompletedEventHandler(this.pageLoadedForMonitoring);
                WebBrowser.DocumentCompleted += completedEventHandlerForTiming;
                AccessTimes = new Stack<AccessTiming>();
            }
        }

        static void scheduleAction(object stateInfo)
        {
            Tuple<WebAction, Agent> actionInfo = (Tuple<WebAction, Agent>)stateInfo;
            actionInfo.Item1.doAction(actionInfo.Item2);
        }
        static void scheduleNextAction(object stateInfo)
        {
            Tuple<Agent> actionInfo = (Tuple<Agent>)stateInfo;
            actionInfo.Item1.doNextAction();
        }

        public void doAction()
        {
            if (activeAction.isWaitForEvent())
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(scheduleAction), Tuple.Create(activeAction, this));
                //Wait
            }
            else
            {
                activeAction.doAction(this);
                ThreadPool.QueueUserWorkItem(new WaitCallback(scheduleNextAction), Tuple.Create(this));
            }
        }

        public virtual void doNextAction()
        {
            if (0 < activeActions.Count)
            {
                activeAction = activeActions.Dequeue();
                if (activeAction.canDoAction(this))
                {
                    if (activeAction.shouldWaitAction(this))
                    {
                        //Wait
                    }
                    else
                    {
                        doAction();
                    }
                }
            } else
            {
                completedActions();
            }

        }
        public virtual void doActions(List<WebAction> actions)
        {
            completedEventHandler = new WebBrowserDocumentCompletedEventHandler(this.pageLoaded);
            WebBrowser.DocumentCompleted += completedEventHandler;
            activeActions = new Queue<WebAction>(actions);
            doNextAction();
        }

        public virtual void completedActions()
        {
            WebBrowser.DocumentCompleted -= completedEventHandler;
            if (null != this.AgentCallback)
            {
                this.AgentCallback.onCompleted(this);
            }
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
            ThreadPool.QueueUserWorkItem(new WaitCallback(scheduleAction), Tuple.Create(activeAction, this));
        }

        public virtual bool validateActiveAction()
        {
            bool ret = false;
            if (null != activeAction && activeAction.isWaitForEvent() && activeAction.validate(this))
            {
                ret = true;
                doNextAction();
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
                LastedUpdated = DateTime.Now;
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
        public List<AccessTiming> getAccessTimings()
        {
            return new List<AccessTiming>(AccessTimes);

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
