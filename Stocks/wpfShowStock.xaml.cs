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
using System.Windows.Shapes;

using System.Data.SqlClient;
using System.Data;

namespace Stocks
{
    /// <summary>
    /// Interaction logic for wpfFoundStocks.xaml
    /// </summary>
    public partial class wpfShowStock : Window
    {

        private string defaultLblAlreadyAddedContent = "TBA";
        public string DefaultLblAlreadyAddedContent
        {
            get { return this.defaultLblAlreadyAddedContent; }
        }

        private DataTable currentData;
        public DataTable CurrentData
        {
            get { return this.currentData; }
            set { this.currentData = value; }
        }

        private Entities.Stock currentDisplayedStock = new Entities.Stock();
        public Entities.Stock CurrentDisplayedStock
        {
            get { return this.currentDisplayedStock; }
            set { this.currentDisplayedStock = value; }
        }



        public wpfShowStock()
        {
            InitializeComponent();
        }
        public wpfShowStock(Entities.Stock stock, DataTable currentData)
        {
            InitializeComponent();

            // Update current portfolio data
            this.CurrentData = currentData;

            // Update displayed stock
            this.UpdateCurrentDisplayedStock(stock);



        }


        public void UpdateUI(Entities.Data database)
        {
            List<string> listData = database.ListData;
            Entities.Stock stock = new Entities.Stock();
            int i = 0;

            // Go through names in portfolio.. (this goes in ORDER!!)
            foreach (var name in database.ListNames)
            {
                // If name is the same as the current name displayed, it needs updating.
                if (this.lblName.Content.ToString().ToUpper() == name.ToUpper())
                {
                    // We can enter the listData because names and data is correlated
                    // pos[n] in listNames = pos[n] in listData
                    stock.AddData(database.ListData[i], Config.Configuration.dataSourceCurrent);
                    this.UpdateCurrentDisplayedStock(stock);
                }

                // Update counter.
                i++;
            }
        }
        private void UpdateCurrentDisplayedStock(Entities.Stock stock)
        {
            // Pass data to attribute:
            this.CurrentDisplayedStock.Name = stock.Name;
            this.CurrentDisplayedStock.Var_day = stock.Var_day;
            this.CurrentDisplayedStock.Var_dayPorcent = stock.Var_dayPorcent;
            this.CurrentDisplayedStock.Opening = stock.Opening;
            this.CurrentDisplayedStock.Max = stock.Max;
            this.CurrentDisplayedStock.Min = stock.Min;
            this.CurrentDisplayedStock.Last = stock.Last;
            this.CurrentDisplayedStock.Time = stock.Time;



            this.UpdateWPF();
        }
        private void UpdateWPF()
        {
            // TODO: Last and Opening are not correctly displayed
            // TODO: Some fashion bro... pls

            lblName.Content = this.CurrentDisplayedStock.Name;
            lblVariation.Content = this.CurrentDisplayedStock.Var_day.ToString();
            lblVariationPorcent.Content = this.CurrentDisplayedStock.Var_dayPorcent.ToString();
            lblOpening.Content = "Opening: " + this.CurrentDisplayedStock.Opening.ToString();
            lblLast.Content = "Last: " + this.CurrentDisplayedStock.Last.ToString();

            // Max? Min? More?? From where?

            this.btnAddInitialize(this.CurrentDisplayedStock);
        }


        private void btnAddInitialize(Entities.Stock stock)
        {
            try
            {
                foreach (DataRow row in this.CurrentData.Rows)
                {
                    if (row["Name"].ToString() == stock.Name)
                    {
                        // This means that the name is already in portfolio.
                        btnAdd.IsEnabled = false;
                        lblAlreadyAdded.Content = "Already added in Portfolio.";
                        break;

                    }
                }


                // If it has the default value then don't have content
                if(lblAlreadyAdded.Content.ToString() == this.DefaultLblAlreadyAddedContent) { lblAlreadyAdded.Content = String.Empty; }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "error");
            }





        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Entities.Data database = new Entities.Data();

            try
            {
                // Add to database
                database.Add(this.CurrentDisplayedStock);

                // Update current WPF current data (in other windows the update will come automatically
                // once the interval auto-update comes in.
                this.CurrentData = database.GetPortfolio();

                // Button disable.
                this.btnAddInitialize(this.CurrentDisplayedStock);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
