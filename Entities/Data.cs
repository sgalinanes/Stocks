using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using static Config.Configuration;

using System.Windows.Forms;

using System.Data.SqlClient;
using System.Data;

namespace Entities
{
    public class Data : Exception_Handler
    {
        // Attributes

        //--- Connect method --///

        // The current sqlConnection connection instance.
        private SqlConnection sqlConnection;
        public SqlConnection SqlConnection
        {
            get { return this.sqlConnection; }
            set { this.sqlConnection = value; }
        }

        // The connectionString to the database.
        private string connectionString = Config.Configuration.sqlConnection;
        public string ConnectionString
        {
            get { return this.connectionString; }
        }

        //--- Find method --///

        // DataTable to retrieve a stock that was found in the database
        // That is, it was not needed to find it in the web because we already have it in the portfolio.
        private DataTable stockFoundInPortfolio = new DataTable();
        public DataTable StockFoundInPortfolio
        {
            get { return this.stockFoundInPortfolio; }
            set { this.stockFoundInPortfolio = value; }
        }

        // A string list with data from every table 
        // where a match was found for the parameter passed
        private List<string> listData = new List<string>();
        public List<string> ListData
        {
            get { return this.listData; }
            set { this.listData = value; }
        }

        // Current data source in use to retreive data from the web.
        private string currentDataSource = Config.Configuration.dataSourceCurrent;
        public string CurrentDataSource
        {
            get { return this.currentDataSource; }
        }

        private Dictionary<string, Config.Pair<string, string>> tickerSource = new Dictionary<string, Config.Pair<string, string>>()
        {
            { "Google Finance", Configuration.dataSourceGoogle },
            { "Yahoo Finance", Configuration.dataSourceYahoo },
            { "Other Source", Configuration.dataSourceOther }
        };

        // Dictionary, where key is the financial API Source.
        // The pair consists of the current URL in use and the keyword that is used in the database to address the ticker content
        // E.g: "key = Google Finance", "value = Name = http://google.com/finance/....", "Keyword = 'TickerGF' ";
        public Dictionary<string, Config.Pair<string, string>> TickerSource
        {
            get { return this.tickerSource; }
        }

        // Name of stock/ticker table in database.
        private string tableStockTickers = Config.Configuration.tableStockTickers;
        public string TableStockTickers
        {
            get { return this.tableStockTickers; }
        }

        //--- Update method --///

        // List of names to be used by the update method.
        // Here names of stocks to be updated will be stored temporarily.
        private List<string> listNames = new List<string>();
        public List<string> ListNames
        {
            get { return this.listNames; }
            set { this.listNames = value; }
        }

        // Name of portfolio table in database
        private string tablePortfolio = Config.Configuration.tablePortfolio;
        public string TablePortfolio
        {
            get { return this.tablePortfolio; }
        }

        // Methods

        public void Find(string stockStr, string findMethod, bool lookInDatabase)
        {
            /* DOCSTRING
             * 
             * Find: Entities.data -> void
             * parameters: 
             *      - string stockStr:  Represents a string to that represents a name
             *                          which represents itself a stock in the market.
             *                          (any market that is supported by this program, 
             *                          that is, that is in the Tickers table in the
             *                          database).
             *      - string findMethod:    Represents a string that tells the callee
             *                              how to match the stockStr parameter against
             *                              the database.
             *                              There are currently two types:
             *                                  - exact: Match must provide the exact name to be found.
             *                                  - similar: Match must be alike the exact name to be found.
             * returns:
             *      - void
             * exceptions:
             *      - Exceptions are handled by calling the Exception Handler methods.
             * objective/operatory:
             *      -   The objective of the function is to try and match the string that
             *          is passed as a parameter, with a current stock in the market.
             *          If found, then the data of that stock will be retreived by the function
             *          and passed to a property (ListData) in the current instance of the
             *          class.
             *          If not found, the above mentioned property will remain empty.
             */ 
            try
            {

                // Check if exists in database (current portfolio) //

                ////
                // First let's see if stock is in currentData (already in portfolio)
                if (lookInDatabase)
                {
                    DataTable currentData = this.GetPortfolio();

                    foreach (DataColumn column in currentData.Columns)
                    {
                        
                        if (column.ColumnName.ToUpper() == "Name".ToUpper())
                        {

                            foreach (DataRow row in currentData.Rows)
                            {
                                if (stockStr.ToUpper() == (row[column.ColumnName]).ToString().ToUpper())
                                {

                                    // Update variable in database instance.
                                    this.StockFoundInPortfolio = currentData.Clone();
                                    this.StockFoundInPortfolio.Clear();
                                    this.StockFoundInPortfolio.Rows.Add(row.ItemArray);

                                    return;

                                }

                            }
                        }
                    }

                }

                // Check if exists in web //

                ////

                // Connect to db & Open connection.
                this.Connect();
                this.SqlConnection.Open();

                // Create sqlCommander and add the current connection to its Connection property.
                var comm = new SqlCommand(); comm.Connection = this.SqlConnection;

                // Query to be passed to the database.
                // This query selects the ticker for the name passed as a parameter.
                string query = String.Empty;

                if (findMethod == Config.Configuration.findMethodExact)
                {
                    query = "SELECT " + (this.TickerSource[this.CurrentDataSource]).Keyword;
                    query += " FROM " + this.TableStockTickers + " WHERE Name = @Stockstr;";

                    // Do not allow SQL Injections
                    comm.Parameters.AddWithValue("@stockStr", stockStr.ToUpper());
                } else if(findMethod == Config.Configuration.findMethodSimilar)
                {
                    query = "SELECT " + (this.TickerSource[this.CurrentDataSource]).Keyword;
                    query += " FROM " + this.TableStockTickers + " WHERE Name LIKE @Stockstr;";

                    // Do not allow SQL Injections
                    comm.Parameters.AddWithValue("@stockStr", "%" + stockStr.ToUpper() + "%");

                } else
                {
                    // There is an error, log it.
                }

 

                // Add query to commander.
                comm.CommandText = query;

                // Buffer to temporarily store tickers in a list of strings.
                List<string> buffer = new List<string>();

                // Reader to read data from database.
                SqlDataReader reader = comm.ExecuteReader();

                while(reader.Read())
                {
                    // It will always retreive a dataSet with only 1 column (the tickers).
                    buffer.Add(reader.GetString(0));
                }


                // For each ticker in buffer, call API to retreive data
                System.Net.WebClient webClient = new System.Net.WebClient();
                for(int i = 0; i < buffer.Count; i++)
                {
                    // Retreive data to a list.
                    // Where source is website and buffer is ticker to find.
                    // For Google Finance, we will retreieve a JSON table.
                    this.listData.Add(webClient.DownloadString(this.TickerSource[this.CurrentDataSource].Data + buffer[i]));

                }

            }
            catch(InvalidOperationException invOpEx)
            {
                // this.LogException(invOpEx);
                //this.displayException(invOpEx, Config.Configuration.exceptions.invalidoperation);
                throw;
            }
            catch(SqlException sqlEx)
            {
                // this.displayException(sqlEx, "sql exception");
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.SqlConnection.Close();
            }
        }

        private void Connect()
        {
            /* DOCSTRING
             * 
             * Connect: Entities.Data -> void
             * parameters: 
             *      - none.
             * returns:
             *      - void
             * exceptions:
             *      - Exceptions are handled by calling the Exception Handler methods.
             * objective/operatory:
             *      -   The objective of the function is to connect to a database.
             *          The connection string is a current property of the class instance.
             *          The SqlConnection connection will be stored in a property (SqlConnection), 
             *          so it can be later used for other methods in the current class.
             */

            var conn = new SqlConnection();

            try
            {
                conn.ConnectionString = this.ConnectionString;
            }
            catch (ArgumentException argEx)
            {
                // Argument exception
                throw argEx;
            }
            catch (Exception ex)
            {
                // Any other exception
                throw ex;
            }

            this.SqlConnection = conn;
        }
        
        public void Update()
        {
           /* DOCSTRING
            * 
            * Update: Entities.Data -> void
            * parameters: 
            *      - None.
            * returns:
            *      - void
            * exceptions: //TODO
            *      - Exceptions are handled by calling the Exception Handler methods.
            * objective/operatory:
            *      -    The objective of the function is to update the current state of the
            *           database to match real-time data from the stock markets. 
            *           It also is supposed to (for that instance of the database, a new one
            *           is created each time the timer is called) retreive the data for all
            *           of the current stocks in the portfolio, and update it (keeping the
            *           data for use in its properties), so it can be used for the program.
            *          
            *           The operatory consists of firstly getting all the current stocks
            *           in the user's portfolio by adding it to a list of strings (listNames).
            *           After that, for each of those names, the Find method will be called to 
            *           retreive the current data from the web, thus the list of data (listData)
            *           will be filled as a result of this call.
            *           The Update query is then performed to update the current database.
            *           
            */


            try
            {
                List<Stock> listStock = new List<Stock>();

                // Connect & Open db.
                this.Connect();
                this.sqlConnection.Open();

                // Query.
                string selQuery = "SELECT Name FROM " + this.TablePortfolio + ";";

                // Make sqlcmd.
                SqlCommand comm = new SqlCommand(selQuery, this.sqlConnection);
                SqlDataReader reader = comm.ExecuteReader();
                try
                {
                    // Read data
                    while (reader.Read())
                    {
                        // Add names to list to be found in internet.
                        this.listNames.Add(reader.GetString(0));
                    }


                }
                catch (Exception)
                {

                    throw;

                } finally
                {
                    reader.Close();
                }


                for (int i = 0; i < this.listNames.Count; i++)
                {
                    // Call find, find will check in the website and return the values in JSON in listData property.
                    // The findMethod must be exact, if we do not have 1 JSON in listData the one-to-one correlation
                    // between listNames and listData is lost.
                    this.Find(this.listNames[i], Config.Configuration.findMethodExact, false);

                    // Add the data to a stock, decode the info...
                    listStock.Add(new Entities.Stock());
                    (listStock[i]).AddData(this.listData[i], this.CurrentDataSource);

                    // UPDATE Query for that stock and execute query.
                    string updQuery = "UPDATE " + this.TablePortfolio + " SET Var_Day = @Var_day, Last = @Last, Opening = @Opening WHERE Name = @Name;";

                    comm.Parameters.AddWithValue("@Var_day", listStock[i].Var_day);
                    comm.Parameters.AddWithValue("@Last", listStock[i].Last);
                    comm.Parameters.AddWithValue("@Opening", listStock[i].Opening);
                    comm.Parameters.AddWithValue("@Name", listStock[i].Name);

                    comm.CommandText = updQuery;
                    comm.ExecuteNonQuery();

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                throw;
            } finally
            {
                this.SqlConnection.Close();
            }


        }

        public void Add(Entities.Stock stock)
        {

            this.Connect();
            this.SqlConnection.Open();

            SqlCommand comm = new SqlCommand(); comm.Connection = this.SqlConnection;

            string query = "INSERT INTO " + this.TablePortfolio + " (Name, Var_day, Last, Opening, Os2) VALUES (@Name, @Var_day, @Last, @Opening, @Os2);";
            comm.Parameters.AddWithValue("@Name", stock.Name);
            comm.Parameters.AddWithValue("@Var_day", stock.Var_day);
            comm.Parameters.AddWithValue("@Last", stock.Last);
            comm.Parameters.AddWithValue("@Opening", stock.Opening);
            comm.Parameters.AddWithValue("@Os2", String.Empty);

            comm.CommandText = query;

            comm.ExecuteNonQuery();

        }

        public DataTable GetPortfolio()
        {
            try
            {
                // Connect & Open connection.
                this.Connect(); 
                this.SqlConnection.Open();

                string query = "SELECT * FROM " + this.TablePortfolio;

                // Sql adapter
                SqlDataAdapter adap = new SqlDataAdapter(query, this.SqlConnection);
                DataTable dt = new DataTable();

                // Fill datatable.
                adap.Fill(dt);
                adap.Dispose();

                return dt;

            }
            catch (Exception)
            {

                throw;
            } finally
            {
                this.SqlConnection.Close();

            }

        }

        public string GetNamefromTicker(string ticker)
        {
            /* DOCSTRING
             * 
             * Connect: Entities.Data -> string
             * parameters: 
             *      - string: ticker
             *          - The parameter refers to the ticker to be matched in the database.
             * returns:
             *      - string:
             *          -   The returned string is the name that corresponds to the ticker
             *              that was previously passed as a parameter.
             * exceptions: //TODO
             *      - Exceptions are handled by calling the Exception Handler methods.
             * objective/operatory:
             *      -   The objective of the function is to retreive the name string from
             *          the database where in its same row, the ticker passed as a parameter
             *          can be found.
             *          
             *          The operatory consists of retreving the string from the table that
             *          lies in the database and contains the information.
             */

            string ret;
            try
            {
                // Temporary string
                string temp = String.Empty;

                // Connect & Open connection
                this.Connect();
                this.SqlConnection.Open();

                // Call commander and connect
                SqlCommand comm = new SqlCommand();
                comm.Connection = this.SqlConnection;

                // Query
                string query = "SELECT Name FROM " + this.tableStockTickers + " WHERE TickerGF = '" + ticker + "';";
                comm.CommandText = query;

                // Read data from query.
                SqlDataReader reader = comm.ExecuteReader();

                while(reader.Read())
                {
                    // Save in temporary string
                    temp = reader.GetString(0);
                }

                // Always close.
                reader.Close();

                // Pass temporary string value to return the string.
                ret = temp;

            }
            catch (Exception)
            {

                throw;
            } finally
            {
                this.SqlConnection.Close();
            }


            return ret;
        }

        // TODO
        public string GetTickerfromName(string name)
        {
            string ret = String.Empty;


            return ret;
        }
      

    }
}
