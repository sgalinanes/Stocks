using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entities
{
    public class JSONFormat
    {

        private string str;
        public string Str
        {
            get { return this.str; }
            set { this.str = value; }
        }


        public Dictionary<string, string> DumpDataAsDictionary(string sourceType)
        {

            Dictionary<string, string> retDict = new Dictionary<string, string>();
            // Data is in str!

            // No data
            if(this.Str == String.Empty)
            {
                return retDict;
            }

            try
            {
                /* 
                 * // [ 
                 * { "id": "393322345722481" , -- Id number
                 * "t" : "IAR" , -- Title
                 * "e" : "BCBA" , -- Emitter??
                 * e+t = GF => Get Name where GF = e+t => Get market name :)
                 * "l" : "21,675.13" , -- Last
                 * "l_fix" : "21675.13" , -- Last with no ,
                 * "l_cur" : "$21,675.13" , -- Last as money (excel)
                 * "s": "0" , -- 
                 * "ltt":"4:42PM GMT-3" , -- Local time
                 * "lt" : "Jun 5, 4:42PM GMT-3" , -- Local time 
                 * "lt_dts" : "2017-06-05T16:42:22Z" , -- Local time
                 * "c" : "-150.00" , -- Change
                 * "c_fix" : "-150.00" , -- Change with no ,
                 * "cp" : "-0.69" , -- Change percentage
                 * "cp_fix" : "-0.69" , -- Change percentage with no ,
                 * "ccol" : "chr" , -- ??
                 * "pcls_fix" : "21825.13" 
                 * } ] 
                 * 
                 */

                // Split everything
                List<string> list = (this.Str.Split('"', ':', ',')).ToList();

                // Clean empty spaces and unwanted symbols.
                list = cleanList(list, sourceType);

                if (sourceType == Config.Configuration.dataSourceGoogleName)
                {
                    // Wanted symbols for google finance:
                    List<string> expectedKeys = new List<string>()
                    {
                        "t", "e", "lfix", "ltt", "ltdts", "cfix", "cpfix", "pclsfix"
                    // title, emitterm last, time, date, change(vard), change(%), opening
                    };

                    for (int i = 0; i < list.Count; i++)
                    {
                        // For clarification..
                        var str = list[i];

                        for (int j = 0; j < expectedKeys.Count; j++)
                        {

                            // Each 'str' is a string from the list.
                            if (str == expectedKeys[j])
                            {
                                // This means we reached a 'key', we must fetch the value.
                                // If the key is NOT "ltt" or "lt_dts" (these need special fetching), then the value is
                                // the next string in the list.
                                if (str != "ltt" && str != "ltdts")
                                {
                                    // Not ltt nor lt_dts
                                    // Add key (current str) and value (next iteration)
                                    retDict.Add(list[i], list[i + 1]);

                                    // Loop to pass value (it is not needed)
                                    i++;

                                    // Break, if we found the key then thats it.
                                    break;

                                }
                                else
                                {
                                    // TODO
                                }
                            }

                        }
                    }
                }
            }

            catch (Exception Ex)
            {

                MessageBox.Show(Ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return retDict;

        }

        public void Add(string str)
        {
            this.Str = str;
        }

        private List<string> cleanList(List<string> list, string dataSource)
        {

            List<string> retList = new List<string>();

            if (dataSource == Config.Configuration.dataSourceCurrent)
            {

                try
                {
                    // Loop through list
                    for (int i = 0; i < list.Count; i++)
                    {
                        // Clean input from non control chars.
                        string temp = new string(list[i].Where(c => char.IsLetter(c) || char.IsDigit(c) || c == '.' || c == '-').ToArray());


                        // Dismiss empty and non interesting symbols.
                        if (temp == null || temp == String.Empty || temp.Contains('/') || temp.Contains('[') || temp.Contains('{') || temp.Contains(']') || temp.Contains('}'))
                        {
                            continue;
                        }

                        retList.Add(temp);

                    }


                }
                catch (Exception ex)
                {

                    throw;
                }

            }

            return retList;

        }
    }
}
