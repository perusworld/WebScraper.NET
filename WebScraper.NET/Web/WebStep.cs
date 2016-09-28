using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using Spring.Expressions;

namespace WebScraper.Web
{
    public interface WebStep
    {
        string getName();
        void execute(Agent agent);
        bool validate(Agent agent);
    }

    public abstract class AbstractWebStep : WebStep
    {
        public string Name { get; set; }
        public AbstractWebStep()
        {

        }
        public AbstractWebStep(string name = null)
        {
            this.Name = name;
        }

        public string getName()
        {
            return Name;
        }

        public void execute(Agent agent)
        {
            MethodInvoker delegateCall = delegate
            {
                internalExecute(agent);
            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
        }
        public abstract void internalExecute(Agent agent);
        public abstract bool validate(Agent agent);

    }

    public class UrlWebStep : AbstractWebStep
    {
        public string UrlOrContextKey { get; set; }
        public WebValidator Validator { get; set; }

        public UrlWebStep()
            : base()
        {

        }
        public UrlWebStep(String name = null, String urlOrContextKey = null, WebValidator validator = null)
            : base(name)
        {
            this.UrlOrContextKey = urlOrContextKey;
            this.Validator = validator;
        }

        protected string resolveURL(Agent agent)
        {
            string ret = null;
            if (agent.RequestContext.ContainsKey(UrlOrContextKey))
            {
                ret = agent.RequestContext[UrlOrContextKey].ToString();
            } else
            {
                ret = UrlOrContextKey;
            }
            return ret;
        }

        public override void internalExecute(Agent agent)
        {
            agent.WebBrowser.Navigate(resolveURL(agent));
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
    }
    public class FormWebStep : AbstractWebStep
    {
        public HtmlElementLocator ElementLocator { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public WebValidator Validator { get; set; }
        public WebCallback PreElementLocatorCallback { get; set; }

        public FormWebStep()
            : base()
        {

        }
        public FormWebStep(String name = null, HtmlElementLocator locator = null, Dictionary<string, string> parameters = null, string method = "submit", WebValidator validator = null, WebCallback preElementLocatorCallback = null)
            : base(name)
        {
            this.ElementLocator = locator;
            this.Method = method;
            this.Parameters = parameters;
            this.Validator = validator;
            this.PreElementLocatorCallback = preElementLocatorCallback;
        }
        public override void internalExecute(Agent agent)
        {
            if (null != Parameters)
            {
                foreach (string key in Parameters.Keys)
                {
                    HtmlElement element = agent.WebBrowser.Document.GetElementById(key);
                    if (null != element)
                    {
                        string value = Parameters[key];
                        Object valueObj = agent.RequestContext.ContainsKey(value) ? agent.RequestContext[value] : null;
                        Object exprObj = null;
                        try
                        {
                            exprObj = ExpressionEvaluator.GetValue(agent, value);
                            if (null != exprObj)
                            {
                                value = exprObj.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            //NOOP
                        }
                        if (null == exprObj)
                        {
                            if (null != valueObj)
                            {
                                value = valueObj.ToString();
                            }
                        }
                        else
                        {
                            value = exprObj.ToString();
                        }
                        element.SetAttribute("value", value);
                    }
                }
            }
            if (null != PreElementLocatorCallback)
            {
                PreElementLocatorCallback.callback(agent);
            }
            if (null != ElementLocator)
            {
                HtmlElement element = ElementLocator.locate(agent);
                if (null != element)
                {
                    element.InvokeMember(Method);
                }
            }
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
    }
    public class ClickWebStep : AbstractWebStep
    {
        public HtmlElementLocator ElementLocator { get; set; }
        public string Method { get; set; }
        public WebValidator Validator { get; set; }

        public ClickWebStep()
            : base()
        {

        }
        public ClickWebStep(String name = null, HtmlElementLocator locator = null, string method = "click", WebValidator validator = null)
            : base(name)
        {
            this.ElementLocator = locator;
            this.Method = method;
            this.Validator = validator;
        }
        public override void internalExecute(Agent agent)
        {
            if (null != ElementLocator)
            {
                HtmlElement element = ElementLocator.locate(agent);
                if (null != element)
                {
                    element.InvokeMember(Method);
                }
            }
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
    }
    public class CookieClearWebStep : AbstractWebStep
    {
        public WebValidator Validator { get; set; }

        public CookieClearWebStep()
            : base()
        {

        }
        public CookieClearWebStep(String name = null)
            : base(name)
        {
        }
        public override void internalExecute(Agent agent)
        {
            agent.WebBrowser.Document.Cookie = null;
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
    }
    public class MonitorWebStep : AbstractWebStep
    {
        public int SleepTime { get; set; }
        public int MaxCount { get; set; }
        public WebValidator Validator { get; set; }
        public MonitorWebStep()
            : base()
        {

        }
        public MonitorWebStep(String name = null, int sleepTime = 500, int maxCount = 120, WebValidator validator = null)
            : base(name)
        {
            this.SleepTime = sleepTime;
            this.MaxCount = maxCount;
            this.Validator = validator;
        }
        public override void internalExecute(Agent agent)
        {
            int count = 0;
            bool done = false;
            MethodInvoker delegateCall = delegate
            {
                if (Validator.validate(agent))
                {
                    done = true;
                    Console.WriteLine("Thread Completed");
                }
                else
                {
                    Console.WriteLine("Thread Started");
                }
            };
            while (!done && count < MaxCount)
            {
                Thread.Sleep(SleepTime);
                if (agent.WebBrowser.InvokeRequired)
                {
                    agent.WebBrowser.Invoke(delegateCall);
                }
                else
                {
                    delegateCall();
                }
                count++;
            }
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
    }
    public class TimedProxyWebStep : AbstractWebStep
    {
        public System.Threading.Timer Timer { get; set; }
        public WebStep Step { get; set; }
        public WebValidator Validator { get; set; }
        public TimedProxyWebStep(WebStep webStep = null, WebValidator validator = null)
            : base()
        {
            Step = webStep;
            this.Validator = validator;
        }
        public override void internalExecute(Agent agent)
        {
            bool ret = false;
            ret = Validator.validate(agent);
            if (!ret)
            {
                if (null == Timer)
                {
                    TimerCallback callback = timerCallback;
                    Timer = new System.Threading.Timer(callback, agent, 500, 500);
                }
            }
        }
        public override bool validate(Agent agent)
        {
            return null == Validator ? true : Validator.validate(agent);
        }
        public void timerCallback(Object argument)
        {
            if (Validator.validate((Agent)argument))
            {
                Timer.Dispose();
                Timer = null;
                Step.execute((Agent)argument);
            }
        }
    }

}
