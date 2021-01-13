# A .Net based Web Scraper using the WebBrowser control

[![nuget](https://img.shields.io/nuget/v/WebScraper.NET)](https://www.nuget.org/packages/WebScraper.NET) [![Build status](https://ci.appveyor.com/api/projects/status/imhrkqa17reo84kw?svg=true)](https://ci.appveyor.com/project/perusworld/webscraper-net)


## Usage

### Install
Install the library using nuget package manager (search for WebScraper.NET) or using Package Manager Console
```bash
Install-Package WebScraper.NET -Version 3.0.0
```
Install the library using .NET CLI
```bash
dotnet add package WebScraper.NET --version 3.0.0
```

### Sample Code

Create a form with a WebBrowser control

* Import Namespace
```c#
using WebScraper.Web;
```

Implement the *AgentCallback* interface

* Build the actions that you want to do, for example
```c#
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
```

* Initialize and call the agent
```c#
SimpleAgent bingSearchAgent = new SimpleAgent(webBrowser, actions);
bingSearchAgent.AgentCallback = this;
bingSearchAgent.init();
bingSearchAgent.startAgent();
```

* once the actions are done, the onCompleted AgentCallback will be called
```c#
public void onCompleted(Agent agent)
{
	//completed actions
}
```
