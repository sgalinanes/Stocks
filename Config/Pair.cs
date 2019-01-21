using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Config
{
    public class Pair<TFirst, TSecond>
    {

        private TFirst first;
        private TSecond second;

        public TFirst Data
        {
            get { return this.first; }
            set { this.first = value; }
        }
        public TSecond Keyword
        {
            get { return this.second; }
            set { this.second = value; }
        }

        public Pair(TFirst first, TSecond second)
        {
            this.Data = first;
            this.Keyword = second;
        }

    }
}
