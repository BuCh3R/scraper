using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace consoleScrape
{

    public class scrapedata
    {

        public string? MatchID { get; set; }
        public Danskespil Danskespil { get; set; }
        public Betfair Betfair { get; set; }
        public scrapedata()
        {
            Danskespil = new Danskespil();
            Betfair = new Betfair();

        }

    }
    public class Danskespil
    {
        public string? Odds1 { get; set; }
        public string? Oddsx { get; set; }
        public string? Odds2 { get; set; }

    }
    public class Betfair
    {
        public string? Odds1 { get; set; }
        public string? Oddsx { get; set; }
        public string? Odds2 { get; set; }
    }

}
