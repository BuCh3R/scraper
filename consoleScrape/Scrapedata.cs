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

    public class Scrapedata
    {
        public string? MatchID { get; set; }
        public SpilUdbyder SpilUdbyder { get; set; } = new SpilUdbyder();

    }

    public class SpilUdbyder
    {
        public string? UdbyderID { get; set; }
        public string? Odds1 { get; set; }
        public string? Oddsx { get; set; }
        public string? Odds2 { get; set; }
    }


}
