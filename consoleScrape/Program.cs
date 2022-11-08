// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using consoleScrape;
using System.Text.Json;
using System.Text.RegularExpressions;
//using Newtonsoft.Json.Linq;
using System;
using System.Reflection.Metadata;
//using Newtonsoft.Json;
using System.Xml.Linq;
using System.Linq;
using System.IO;

IWebDriver driver = new ChromeDriver();
List<Scrapedata> _data = new List<Scrapedata>();

//method saves new string inbetween 2 string values in a string
static string getBetween(string strSource, string strStart, string strEnd)
{
    if (strSource.Contains(strStart) && strSource.Contains(strEnd))
    {
        int Start, End;
        Start = strSource.IndexOf(strStart, 0) + strStart.Length;
        End = strSource.IndexOf(strEnd, Start);
        return strSource.Substring(Start, End - Start);
    }

    return "";
}

IWebElement wrap1;
IWebElement wrap2;
IWebElement wrap3;
IWebElement wrap4;


bool isElementPresent(By by)
{
    try
    {
        wrap4.FindElement(by);
        return true;
    }
    catch (NoSuchElementException e)
    {
        return false;
        
    }
}


void danskeSpil(Action a, string url)
{

    driver.Navigate().GoToUrl(url);

    Thread.Sleep(2* 1000);
    var cookieButton = driver.FindElement(By.Id("ensSaveAll"));
    cookieButton.Click();

    // targets event div and creates main loop
    IList<IWebElement> elements = driver.FindElements(By.ClassName("event-list__item-link"));
    foreach (IWebElement e in elements)
    {

        //targeting teams/player data div
        wrap1 = e.FindElement(By.ClassName("event-list__item__content"));
        wrap2 = wrap1.FindElement(By.ClassName("event-card"));
        wrap3 = wrap2.FindElement(By.ClassName("event-card__body"));
        wrap4 = wrap3.FindElement(By.ClassName("event-card__body__name"));

        //scrapedata placeholder strings
        string homeTeam;
        string awayTeam;
        string createMatchID;
        string homeOdds;
        string drawOdds;
        string awayOdds;

        // checks if regular match else = custom match
        if (isElementPresent(By.ClassName("event-card__body__name__home")))
        {
            homeTeam = wrap4.FindElement(By.ClassName("event-card__body__name__home")).Text;
            awayTeam = wrap4.FindElement(By.ClassName("event-card__body__name__away")).Text;
            string tempID = homeTeam + awayTeam;
            createMatchID = Regex.Replace(tempID, @"\s+", "");
            createMatchID = createMatchID.Replace(@"ø", "o").Replace(@"æ", "ae").Replace(@"å", "aa");

        }
        else
        {
            createMatchID = Regex.Replace(wrap4.Text, @"\s+", "");
            createMatchID = createMatchID.Replace(@"ø", "o").Replace(@"æ", "ae").Replace(@"å", "aa");

        }

        //targeting odds data div
        var outer1 = wrap1.FindElement(By.ClassName("event-list__item__event-market"));
        var outer2 = outer1.FindElement(By.ClassName("market__body"));
        var outer3 = outer2.FindElement(By.ClassName("market__body__outcomes"));
        var odds = outer3.FindElement(By.ClassName("market__body__outcome-row"));
        string source = odds.Text + "endoftext";
        string oldHomeOdds;

        if (odds.Text.Contains("X"))
        {
            // string manipulation to fit json format
            // -------------- BUG WHEN ODDS 1 AND X IS THE SAME VALUE OR X VALUE > 9,99 ---------------
            string homedata = getBetween(source, "1", "X");
            oldHomeOdds = homedata.Remove(0, 2);
            homeOdds = oldHomeOdds.Remove(oldHomeOdds.Length - 2, 2);

            string drawdata = getBetween(source, "X", "endoftext");
            string str = drawdata.Substring(1, 5);
            drawOdds = str.Remove(0, 1);

            string awaydata = getBetween(source, drawOdds, "endoftext");
            awayOdds = awaydata.Remove(0, 5);

        }
        else
        {
            // needs to target non 3 outcome matches (switch statement might be better when more outcomes shows)
            string tmpSrc = odds.Text;
            string addSplit = Regex.Replace(tmpSrc, @"\r\n", "SPLIT");
            string homedata = getBetween(addSplit, "SPLIT", "SPLIT");

            string lastVal = addSplit.Substring(addSplit.LastIndexOf("T") + 1);
            homeOdds = homedata;
            drawOdds = "";
            awayOdds = lastVal;

        }

        //filters out to only load match winner odds
        if (odds.Text.Contains("+") || odds.Text.Contains("Over") && odds.Text.Contains("Under"))
        {

        }
        else
        {
            var danskespilObj = new SpilUdbyder
            {
                UdbyderID = "Danskespil",
                Odds1 = homeOdds,
                Oddsx = drawOdds,
                Odds2 = awayOdds
            };

            _data.Add(new Scrapedata()
            {

                MatchID = createMatchID,
                SpilUdbyder = danskespilObj

            });
        }
    }
    a();
    Console.WriteLine("Done! Redirecting browser in 5 seconds...");
    Thread.Sleep(5 * 1000);
    
}
danskeSpil(writeToJson, "https://danskespil.dk/oddset/sport/12/fodbold/matches?preselectedFilters=70");

void writeToJson()
{
    string json = JsonSerializer.Serialize(_data);
    File.WriteAllText("..\\..\\..\\test.json", json);
}


void betFair(string url)
{
    //IWebDriver driver = new ChromeDriver();
    driver.Navigate().GoToUrl(url);

    Thread.Sleep(2000);
    var cookieButton = driver.FindElement(By.Id("onetrust-accept-btn-handler"));
    cookieButton.Click();

    IList<IWebElement> elements = driver.FindElements(By.ClassName("event-information"));

    string createMatchID;
    var json = File.ReadAllText("..\\..\\..\\test.json");
    var scrapeObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Scrapedata>>(json);
    foreach (IWebElement e in elements)
    {
        var odds1 = e.FindElement(By.ClassName("avb-col-markets"));
        var odds2 = odds1.FindElement(By.ClassName("market-3-runners"));
        var odds3 = odds2.FindElement(By.ClassName("runner-list"));
        var odds4 = odds3.FindElement(By.ClassName("runner-list-selections"));
        var homeOdds = odds4.FindElement(By.ClassName("sel-0"));
        var drawOdds = odds4.FindElement(By.ClassName("sel-1"));
        var awayOdds = odds4.FindElement(By.ClassName("sel-2"));
        var homeOdds1 = homeOdds.FindElement(By.ClassName("ui-runner-price")).Text;
        var drawOdds1 = drawOdds.FindElement(By.ClassName("ui-runner-price")).Text;
        var awayOdds1 = awayOdds.FindElement(By.ClassName("ui-runner-price")).Text;
        homeOdds1 = homeOdds1.Replace(".", ",");
        drawOdds1 = drawOdds1.Replace(".", ",");
        awayOdds1 = awayOdds1.Replace(".", ",");

        var teams1 = e.FindElement(By.ClassName("avb-col-runners"));
        var teams2 = teams1.FindElement(By.ClassName("event-name-info"));
        var teams3 = teams2.FindElement(By.ClassName("teams-container"));

        createMatchID = Regex.Replace(teams3.Text, @"\s+", "");
        createMatchID = createMatchID.Replace(@"ø", "o").Replace(@"æ", "ae").Replace(@"å", "aa");

        var bfairOjb = new SpilUdbyder
        {
            UdbyderID = "Betfair",
            Odds1 = homeOdds1,
            Oddsx = drawOdds1,
            Odds2 = awayOdds1
        };
        scrapeObj.Add(new Scrapedata
        {
            MatchID = createMatchID,
            SpilUdbyder = bfairOjb
        });
        string newJson = JsonSerializer.Serialize(scrapeObj);
        File.WriteAllText("..\\..\\..\\test.json", newJson);

    }
    
    Console.WriteLine("Done!");
    Thread.Sleep(5 * 1000);
    driver.Quit();
}
betFair("https://www.betfair.com/sport/football?selectedTabType=COUNTRY_CODE_FOOTBALL");


void sameMatch()
{
    var json = File.ReadAllText("..\\..\\..\\test.json");
    var scrapeObj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Scrapedata>>(json);
    var matchList = scrapeObj.Select(x => x).ToList();
    var query = matchList.GroupBy(x => x.MatchID, x => x.MatchID, (matchID, count) => new
    {
        Key = matchID,
        Count = count.Count()
    });

    foreach(var result in query)
    {
        Console.WriteLine("\nMatchID: " + result.Key);
        Console.WriteLine("Number of matches with same MatchID: " + result.Count);
        if (result.Count > 1)
        {
            var match = scrapeObj.Where(x => x.MatchID == result.Key);
            var bigOdds1 = match.Select(x => decimal.Parse(x.SpilUdbyder.Odds1)).Max();
            var bigOddsx = match.Select(x => decimal.Parse(x.SpilUdbyder.Oddsx)).Max();
            var bigOdds2 = match.Select(x => decimal.Parse(x.SpilUdbyder.Odds2)).Max();
            var bestBookieOdds1 = match.Select(x => x.SpilUdbyder).Where(x => decimal.Parse(x.Odds1) == bigOdds1);
            var bookielist1 = bestBookieOdds1.Select(x => x.UdbyderID).ToList();
            var bestBookieOddsx = match.Select(x => x.SpilUdbyder).Where(x => decimal.Parse(x.Oddsx) == bigOddsx);
            var bookielistx = bestBookieOddsx.Select(x => x.UdbyderID).ToList();
            var bestBookieOdds2 = match.Select(x => x.SpilUdbyder).Where(x => decimal.Parse(x.Odds2) == bigOdds2);
            var bookielist2 = bestBookieOdds2.Select(x => x.UdbyderID).ToList();
            string bestBookie1 = "";
            string bestBookiex = "";
            string bestBookie2 = "";

            for (int i = 0; i < bookielist1.Count; i++)
            {
                bestBookie1 = bookielist1[i];
            }
            for (int i = 0; i < bookielistx.Count; i++)
            {
                bestBookiex = bookielistx[i];
            }
            for (int i = 0; i < bookielist2.Count; i++)
            {
                bestBookie2 = bookielist2[i];
            }
            Console.WriteLine("////////////////////////");
            Console.WriteLine(bestBookie1 + " Has the Highest Odds 1; " + bigOdds1);
            Console.WriteLine(bestBookiex + " Has the Highest Odds X; " + bigOddsx);
            Console.WriteLine(bestBookie2 + " Has the Highest Odds 2; " + bigOdds2);
            Console.WriteLine("////////////////////////");
            Console.WriteLine("--------------------------------------------------------");
        }
    }
}
sameMatch();
Console.Read();