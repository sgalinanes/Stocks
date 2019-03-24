using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;

namespace Config
{
    public static class Configuration
    {
        // Connection to database
        public static string sqlDatabaseName = "Stocks";
        public static string sqlConnection = "data source = localhost\\SQLEXPRESS;initial catalog = Stocks; trusted_connection=true;";


        // Table containing Names and tickers for each dataSource
        // This "is" the program's database, if the stock is not here, then no information will be available.
        public static string tableStockTickers = "Tickers";
        // Portfolio table
        public static string tablePortfolio = "Portfolio";

        // Data sources         
        public static string dataSourceGoogleName = "Google Finance";
        public static string dataSourceYahooName = "Yahoo Finance";
        public static string dataSourceOtherName = "Other";

        public static string dataSourceCurrent = dataSourceGoogleName;

        public static string dataSourceGoogleFinance = "http://www.google.com/finance/info?q=";
        public static string dataSourceGoogleFinanceKeyword = "Ticker";
        public static string dataSourceYahooFinance = "";
        public static string dataSourceYahooFinanceKeyword = "TickerYF";
        public static string dataSourceOtherFinance = "";
        public static string dataSourceOtherKeyword = "";

        // Easier to access for whomever needs!
        public static Pair<string, string> dataSourceGoogle = new Pair<string, string>(dataSourceGoogleFinance, dataSourceGoogleFinanceKeyword);
        public static Pair<string, string> dataSourceYahoo = new Pair<string, string>(dataSourceYahooFinance, dataSourceYahooFinanceKeyword);
        public static Pair<string, string> dataSourceOther = new Pair<string, string>(dataSourceOtherFinance, dataSourceOtherKeyword);

        // Finding method to database
        public static string findMethodExact = "exact";
        public static string findMethodSimilar = "similar";


        // Timer interval to update current stocks in user's portfolio
        public static int timerUpdateInterval = (1 * 60 * 1000);// in ms


    }
}
