using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivinParisVF
{
    public class Arc<T>
    {
        public T Destination { get; set; }
        public double Poids { get; set; }

        public Arc(T destination, double poids)
        {
            Destination = destination;
            Poids = poids;
        }

        public override string ToString()
        {
            return $"{Destination} ({Poids} km)";
        }
    }
    
}
