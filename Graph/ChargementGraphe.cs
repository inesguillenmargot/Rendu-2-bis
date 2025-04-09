namespace LivinParisVF;

using System;
using System.Collections.Generic;
using System.Globalization;
using ClosedXML.Excel;

public static class ChargementGraphe
{
    public static Graphe<Station> ChargerGrapheDepuisExcel(string cheminFichier)
    {
        var stations = new Dictionary<int, Station>();
        var lignesStations = new Dictionary<string, List<Station>>(); // Pour les stations par ligne

        using (var wb = new XLWorkbook(cheminFichier))
        {
            var noeuds = wb.Worksheet("Noeuds");
            var arcs = wb.Worksheet("Arcs");

            // Lecture des noeuds (stations)
            foreach (var row in noeuds.RowsUsed().Skip(1)) // Ignorer l'en-tête
            {
                int id = int.Parse(row.Cell("A").GetValue<string>());
                string ligne = row.Cell("B").GetValue<string>();
                string nom = row.Cell("C").GetValue<string>();
                double lon = double.Parse(row.Cell("D").GetValue<string>(), CultureInfo.InvariantCulture);
                double lat = double.Parse(row.Cell("E").GetValue<string>(), CultureInfo.InvariantCulture);

                var station = new Station(id, nom, ligne, lat, lon);
                stations[id] = station;

                if (!lignesStations.ContainsKey(ligne))
                {
                    lignesStations[ligne] = new List<Station>();
                }
                lignesStations[ligne].Add(station);
            }

            // Initialiser le graphe
            var graphe = new Graphe<Station>(stations.Count);

            // Ajouter les stations au graphe
            foreach (var station in stations.Values)
            {
                graphe.AjouterNoeud(station);
            }

            // Lecture des arcs (liaisons)
            foreach (var row in arcs.RowsUsed().Skip(1)) // Ignorer l'en-tête
            {
                int fromId = int.Parse(row.Cell("A").GetValue<string>());
                int toId = int.Parse(row.Cell("C").GetValue<string>());
                int temps = int.Parse(row.Cell("E").GetValue<string>(), CultureInfo.InvariantCulture);

                if (stations.ContainsKey(fromId) && stations.ContainsKey(toId) && (fromId != 0 && toId != 0))
                {
                    var fromStation = stations[fromId];
                    var toStation = stations[toId];

                    // Graphe non-orienté
                    graphe.AjouterLien(fromStation, toStation, temps);
                    graphe.AjouterLien(toStation, fromStation, temps);
                }
            }

            return graphe;
        }
    }
}