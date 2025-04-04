using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivinParisVF
{
    public class Station
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public List<string> Lignes { get; } // Liste des lignes auxquelles cette station appartient
        public List<Lien<Station>> Voisins { get; set; }
        public Station(int id, string nom, string ligne, double lat, double lon)
        {
            Id = id;
            Nom = nom;
            Lignes = new List<string> { ligne };
            Latitude = lat;
            Longitude = lon;
            Voisins = new List<Lien<Station>>();
        }

        public override string ToString() => $"{Nom} ({string.Join(", ", Lignes)})";

        public void AjouterLigne(string ligne)
        {
            if (!Lignes.Contains(ligne))
            {
                Lignes.Add(ligne);
            }
        }

        public void AjouterVoisin(Lien<Station> voisin)
        {
            Voisins.Add(voisin);
        }
        public double DistanceVers(Station autre)
        {
            double R = 6371e3; // Rayon de la Terre en mètres

            double φ1 = Latitude * Math.PI / 180;
            double φ2 = autre.Latitude * Math.PI / 180;
            double Δφ = (autre.Latitude - Latitude) * Math.PI / 180;
            double Δλ = (autre.Longitude - Longitude) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                       Math.Cos(φ1) * Math.Cos(φ2) *
                       Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;

            return d; // en mètres
        }

        public override bool Equals(object obj)
        {
            return obj is Station station && this.Nom == station.Nom;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
