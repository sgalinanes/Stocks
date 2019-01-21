using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Data.SqlClient;
using System.Data;

namespace Entities
{
    public class Stock
    {

        // Attributes / Fields
        private int UID;
        private string name;
        private double var_min, var_day, last, max, min, opening;
        private DateTime date, time;
        private double var_dayPorcent;

        // Properties
        public int _UID
        {
            get { return this.UID; }
            set { this.UID = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public double Var_min
        {
            get { return this.var_min; }
            set { this.min = value; }
        }
        public double Var_day
        {
            get { return this.var_day; }
            set { this.var_day = value; }
        }
        public double Last
        {
            get { return this.last; }
            set { this.last = value; }
        }
        public double Max
        {
            get { return this.max; }
            set { this.max = value; }
        }
        public double Min
        {
            get { return this.min; }
            set { this.min = value; }
        }
        public double Opening
        {
            get { return this.opening; }
            set { this.opening = value; }
        }
        public DateTime Date
        {
            get { return this.date; }
            set { this.date = value; }
        }
        public DateTime Time
        {
            get { return this.time; }
            set { this.time = value; }
        }

        public double Var_dayPorcent
        {
            get { return this.var_dayPorcent;  }
            set { this.var_dayPorcent = value; }
        }


        public void AddData(DataTable dt)
        { 
            // Load data into stock attributes.
            // TODO: Sanity checks
            this.Name = dt.Rows[0]["Name"].ToString();
            this.Var_day = Double.Parse(dt.Rows[0]["Var_day"].ToString());
            this.Last = Double.Parse(dt.Rows[0]["Last"].ToString());
            // this.Var_dayPorcent = Double.Parse(dt.Rows[0]["Var_dayPorcent"].ToString());
            this.Opening = Double.Parse(dt.Rows[0]["Opening"].ToString());

            /* this.Max = Double.Parse(dt.Rows[0]["Max"].ToString());
                  this.Min = Double.Parse(dt.Rows[0]["Min"].ToString());
                  this.Opening = Double.Parse(dt.Rows[0]["Opening"].ToString());
                  this.Date = DateTime.Parse(dt.Rows[0]["Date"].ToString());
                  this.Time = DateTime.Parse(dt.Rows[0]["Time"].ToString());*/


        }

        public void AddData(string buffer, string sourceType)
        {
            if(sourceType == Config.Configuration.dataSourceGoogleName)
            {


                JSONFormat jsonHandler = new JSONFormat();

                jsonHandler.Add(buffer);
                Dictionary<string, string> dictData = jsonHandler.DumpDataAsDictionary(sourceType);

                /* dictData:
                 * t : value_title (this plus e is the stock name)
                 * e : value_name (idem with t)
                 * l_fix : value_last
                 * ltt : value_time
                 * lt_dts : value_date (until character T is reached)
                 * c_fix : value_varday
                 * cp_fix : value_vardaypercent.
                 * pcls_fix : value_opening
                 */

                foreach(var keyValue in dictData)
                {
                    //MessageBox.Show("Key: " + keyValue.Key + ", Value: " + keyValue.Value);

                    // ParseValues(...) ?
                }

                Entities.Data database = new Entities.Data();

                // Name is the ticker's name.
                string ticker = dictData["e"] + ":" + dictData["t"];
                string name = database.GetNamefromTicker(ticker);

                // Fill stock.
                this.Name = name;
                this.Last = Double.Parse(dictData["lfix"], System.Globalization.NumberStyles.AllowDecimalPoint);
                this.Var_day = Double.Parse(dictData["cfix"]);
                this.Var_dayPorcent = Double.Parse(dictData["cpfix"]);
                this.Opening = Double.Parse(dictData["pclsfix"], System.Globalization.NumberStyles.AllowDecimalPoint);


    
            }
        }

        // Constructor
        public Stock()
        {

        }
    }
}
