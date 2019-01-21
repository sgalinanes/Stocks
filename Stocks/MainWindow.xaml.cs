using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data.SqlClient;
using System.Data;

using System.Windows.Forms;

using Config;

namespace Stocks
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Timer updateTimer;
        public Timer UpdateTimer
        {
            get { return this.updateTimer; }
            set { this.updateTimer = value; }
        }

        private DataTable currentData;
        public DataTable CurrentData
        {
            get { return this.currentData; }
            set { this.currentData = value; }
        }


        public MainWindow()
        {
                    /* DOCSTRING
         * 
         * MainWindow() -> Constructor
         * parameters: 
         *      - None
         * returns:
         *      - void
         * exceptions:
         *      - None
         * objective/operatory:
         *      -   Initialize MainWindow 
         */

            InitializeComponent();

            Entities.Data database = new Entities.Data();

            database.Update();
            this.CurrentData = database.GetPortfolio();
            dgPortfolio.DataContext = CurrentData.DefaultView;

        }

        public void InitTimer()
        {
                    /* DOCSTRING
         * 
         * InitTimer: MainWindow -> void
         * parameters: 
         *      - None
         * returns:
         *      - void
         * exceptions:
         *      - None
         * objective/operatory:
         *      -   The objective of the function is initialize a timer that will work
         *          with a pre-estabished interval, raising an event each time the timer
         *          is called.
         *          
         *          The operatory consists of creaating a Windows.Forms.Timer and store
         *          it in a Class Property, the interval is then established, and the
         *          event for said property is called when every interval is reached.
         */

            System.Windows.MessageBox.Show("Timer calld");
            this.UpdateTimer = new Timer();
            this.UpdateTimer.Interval = Config.Configuration.timerUpdateInterval;
            this.UpdateTimer.Tick += new EventHandler(this.updateTimer_Tick);
            this.UpdateTimer.Start();

        }



        private void updateTimer_Tick(object sender, EventArgs e)
        {
                    /* DOCSTRING
        * 
        * updateTimer_Tick: private MainWindow -> void
        * parameters: 
        *      - sender and e.
        * returns:
        *      - void
        * exceptions:
        *      - To be handled by the exception handler.
        * objective/operatory:
        *      -    The objective of the function that is called every time a timer 'ticks'
        *           with a pre-established interval, is to update the database with
        *           real-time data from the stock market. After that, it also must
        *           update the program UI with said data.
        *          
        *          The operatory consists of creating an instance of a Data class and calling
        *          the Update method to update its values. After this we create a stockWindow
        *          that will be not null if a wpfShowStock is active and call an update method
        *          to update its values. The same is done for the current mainWindow WPF.
        */

            Entities.Data database = new Entities.Data();

            try
            {
                // Update database.
                database.Update();

                var stockWindow = System.Windows.Application.Current.Windows.OfType<wpfShowStock>().SingleOrDefault(w => w.IsActive);

                // this.UpdateData(database);

                // Sanity check
                if(stockWindow != null)
                {
                    // Update data from stock window in real time :p
                    stockWindow.UpdateUI(database);
                }

                // Also update current wpf
                this.UpdateUI(sender, e);


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnFindStock_Click(object sender, RoutedEventArgs e)
        {
                    /* DOCSTRING
        * 
        * btnFindStock_Click: private MainWindow -> void (Raised by Event Click)
        * parameters: 
        *      - sender, e
        * returns:
        *      - void
        * exceptions:
        *      - Exceptions are handled by the exception handler.
        * objective/operatory:
        *      -    The objective of the function is to find a desired stock in the
        *           market, as long as the information to retreive this data is
        *           available in the program's database. The function must also
        *           manage what WPF should be created to display the found data
        *           depending on the information found.
        *          
        *           The operatory consists in creating a Data instance and calling
        *           the Find method. If any information was found it wil be in a
        *           property called ListData in the database instance.
        *           It then adds into a list of stocks: the current data retreived
        *           and manages this information to show different WPF depending
        *           on how many stocks were found.
        *           
        *           //TODO:
        *               In the property CurrentData the current Portfolio is stored,
        *               thus when calling find we should check if the data is in the 
        *               portfolio before looking it up in the website because the data
        *               in the portfolio is already to be used in the CurrentData
        *               property (a DataTable) whilst the website information must be
        *               retreived in real-time.
        */


            // Code to find a stock (ANY) in the market.
            string stockStr = txbStock.Text.ToString();
            System.Windows.MessageBox.Show(stockStr);

            List<Entities.Stock> listStocks = new List<Entities.Stock>();
            Entities.Data database = new Entities.Data();

            // Try to find stock in web.
            try
            {

                // Stock data will be stored in a property in the database instance created.
                database.Find(stockStr, Config.Configuration.findMethodSimilar, true);

                if(database.ListData.Any())
                {
                    // There may be multiple "similar" stocks (e.g PAMP ADR, PAMP BCBA or substring of n strings 
                    // e.g PAMP can be PAMPA ENERGIA or PETROLERA PAMPA).
                    for (int i = 0; i < database.ListData.Count; i++)
                    {
                        listStocks.Add(new Entities.Stock());
                        listStocks[i].AddData(database.ListData[i], Configuration.dataSourceCurrent);
                    }

                    if(listStocks.Count == 1)
                    {
                        // One stock found.
                        wpfShowStock wpfStock = new wpfShowStock(listStocks[0], this.CurrentData);
                        wpfStock.CurrentData = this.CurrentData;
                        wpfStock.Show();

                    } else
                    {


                        // Multiple stocks found
                        System.Windows.MessageBox.Show("Please be more precise.", "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                        // TODO: Intermediate WPF to choose one of the found stocks and then open a wpfShowStock.
                    }


                } else
                {
                    // Either stock was found in the web (thus listData count is 0) or there is no data to show.
                    if(database.StockFoundInPortfolio.Rows.Count > 0)
                    {
                        // Load stock found as dataTable in a stock instance.
                        Entities.Stock stock = new Entities.Stock();
                        stock.AddData(database.StockFoundInPortfolio);

                        // Show stock
                        wpfShowStock wpfStock = new wpfShowStock(stock, this.CurrentData);
                        wpfStock.CurrentData = this.CurrentData;
                        wpfStock.Show();
                        
                    } else
                    {
                        System.Windows.MessageBox.Show("No information was found", "Information", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    }
                }

            }
            
            catch (Exception Ex)
            {

                System.Windows.MessageBox.Show(Ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Update any data that needs updating in main wpf.
            this.dataGridFill(sender, e);
        }

        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            this.InitTimer();
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dataGridFill(object sender, EventArgs e)
        {

            // Add data from database at initialization and after every update happens.
            Entities.Data database = new Entities.Data();
            try
            {
                // Get portfolio in a dataTable.
                this.CurrentData = database.GetPortfolio();

                // Fill datagrid 
                dgPortfolio.DataContext = this.CurrentData.DefaultView;

            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }





        }
    }
}
