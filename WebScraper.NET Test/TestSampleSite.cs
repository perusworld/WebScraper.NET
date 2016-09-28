using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WebScraper.Web;

namespace WebScraper.NET_Test
{

    public partial class TestSampleSite : Form, AgentCallback
    {

        public TestSampleSite()
        {
            InitializeComponent();
        }

        public void onCompleted(Agent agent)
        {
            List<AccessTiming> timings = agent.getAccessTimings();
            timings.Reverse();
            listLogs.Items.Clear();
            foreach (AccessTiming timing in timings)
            {
                listLogs.Items.Add(String.Format("{0} , {1} Seconds", timing.URI, timing.getTimingInSeconds()));
            }
            MessageBox.Show("Check logs tab for timings", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listLogs.Items.Clear();
            List<WebAction> actions = new List<WebAction>();
            //goto home url - https://www.bing.com/
            actions.Add(new SimpleWebAction(new UrlWebStep("open search", "https://www.bing.com/"),
                new LocatorCheckValidator(new SimpleHtmlElementLocator("q search box",
                new AttributeHtmlElementMatcher("q search box", "name", "q"))), waitForEvent: true));
            //submit a search
            actions.Add(new SimpleWebAction(new FormWebStep("submit search", new IdElementLocator("locate form to submit", "sb_form"), new Dictionary<String, String>
            {
                {"sb_form_q", "WebScraper.NET github"}
            }
            ), waitForEvent: true));
            //load results
            actions.Add(new ExtractWebAction<String>(new StringHtmlElementDataExtractor("href"), "firstNavLink",
                new TagElementLocator("match results", "ol", false, "firstResultLink",
                new SimpleChildHtmlElementLocator("find first link",
                filter: new SimpleChildHtmlElementLocator("get first a", new TagHtmlElementMatcher("match first a", "a"))),
                new IdHtmlElementMatcher("match results ol", "b_results"))));
            //goto first result
            actions.Add(new SimpleWebAction(new UrlWebStep("open search", "firstNavLink"), new TitleWebValidator("GitHub - perusworld/WebScraper.NET: A .Net based Web Scraper using the WebBrowser control"), waitForEvent: true));
            SimpleAgent bingSearchAgent = new SimpleAgent(webBrowser, actions);
            bingSearchAgent.MonitorTimings = true;
            bingSearchAgent.AgentCallback = this;
            bingSearchAgent.init();
            bingSearchAgent.startAgent();
        }
    }
}
