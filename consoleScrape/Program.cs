// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text;
using consoleScrape;
using System.Text.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection.Metadata;

IWebDriver driver = new ChromeDriver();

List<scrapedata> _data = new List<scrapedata>();

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

    //var csv = new StringBuilder();

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

        }
        else
        {
            createMatchID = Regex.Replace(wrap4.Text, @"\s+", "");

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

            //Console.WriteLine("1: " + homeTeam.Text + " - " + "2: " + awayTeam.Text + "\r\n" + odds.Text + "\r\n");
            //csv.AppendLine("1: " + homeTeam + " - " + "2: " + awayTeam + "\r\n" + odds.Text + "\r\n" + myObj.dk + myObj.bf);

            var danskespilObj = new Danskespil();
            danskespilObj.Odds1 = homeOdds;
            danskespilObj.Oddsx = drawOdds;
            danskespilObj.Odds2 = awayOdds;

            _data.Add(new scrapedata()
            {

                MatchID = createMatchID,
                Danskespil = danskespilObj

            });
            
        }

        //Console.Write(createMatchID);

        //string json = JsonSerializer.Serialize(_data);
        //File.WriteAllText("..\\..\\..\\test.json", json);
        
    }
    a();
    //File.WriteAllText("..\\..\\..\\danskespil.csv", csv.ToString());
    Console.WriteLine("Closing browser in 5 seconds...");
    Thread.Sleep(5 * 1000);
    driver.Quit();
}
//danskeSpil(writeToJson, "https://danskespil.dk/oddset/sport/12/fodbold/matches");
//danskeSpil(appendToJson, "https://danskespil.dk/oddset/dagenskampe");

void writeToJson()
{
    string json = JsonSerializer.Serialize(_data);
    //json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
    File.WriteAllText("..\\..\\..\\test.json", json);
}

void appendToJson()
{

    string json = JsonSerializer.Serialize(_data);
    //json = json.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
    File.AppendAllText("..\\..\\..\\test.json", json);
}

//string json = File.ReadAllText("..\\..\\..\\test.json");
//dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
//jsonObj[0]["Betfair"]["Odds1"] = ":D";
//string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
//File.WriteAllText("..\\..\\..\\test.json", output);



// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA
// NEED TO FIND A WAY TO CHECK IF CURRENT JSONFILE HAVE FIELD CONTAINING MATCHID -> INSERT NEW DATA FROM BETFAIR -- ELSE -- ADD NEW OBJECT AND INSERT DATA







void betFair(string url)
{
    //IWebDriver driver = new ChromeDriver();
    driver.Navigate().GoToUrl(url);

    Thread.Sleep(2000);
    var cookieButton = driver.FindElement(By.Id("onetrust-accept-btn-handler"));
    cookieButton.Click();

    var csv = new StringBuilder();

    IList<IWebElement> elements = driver.FindElements(By.ClassName("event-information"));
    int id = 0;
    string createMatchID;


    foreach (IWebElement e in elements)
    {
        var odds1 = e.FindElement(By.ClassName("avb-col-markets"));
        var odds2 = odds1.FindElement(By.ClassName("market-3-runners"));
        var odds3 = odds2.FindElement(By.ClassName("runner-list"));
        var odds4 = odds3.FindElement(By.ClassName("runner-list-selections"));

        var teams1 = e.FindElement(By.ClassName("avb-col-runners"));
        var teams2 = teams1.FindElement(By.ClassName("event-name-info"));
        var teams3 = teams2.FindElement(By.ClassName("teams-container"));

        createMatchID = Regex.Replace(teams3.Text, @"\s+", "");

        //Console.WriteLine("ID: " + id + "\r\n" + teams3.Text + "\r\n" + odds4.Text + "\r\n");
        //csv.AppendLine("ID: " + id + "\r\n" + teams3.Text + "\r\n" + odds4.Text + "\r\n");
        id++;


        string json = File.ReadAllText("..\\..\\..\\test.json");
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);




    }
    //File.WriteAllText("..\\..\\..\\betfair.csv", csv.ToString());
    Console.WriteLine("Done!");
    Thread.Sleep(10 * 1000);
    driver.Quit();
}
//betFair("https://www.betfair.com/sport/football");


Console.Read();