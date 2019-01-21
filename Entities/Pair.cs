using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pair<TFirst, TSecond>
    {

        private TFirst first;
        private TSecond second;

        public TFirst First
        {
            get { return this.first; }
            set { this.first = value; }
        }       
        public TSecond Second
        {
            get { return this.second; }
            set { this.second = value; }
        }

        public Pair(TFirst first, TSecond second)
        {
            this.First = first;
            this.Second = second;
        }

    }
}
