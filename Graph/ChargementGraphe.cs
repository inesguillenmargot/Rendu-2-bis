using ClosedXML.Excel;
using OfficeOpenXml;

namespace LivinParisVF;

public class ChargementGraphe
{
    public static Graphe<Station> ChargerGrapheDepuisExcel(string cheminFichier)
    {
        var stations = new Dictionary<int, Station>();
        var lignesStations = new Dictionary<string, List<Station>>(); // Un dictionnaire pour stocker les stations par ligne

        using (var wb = new XLWorkbook(cheminFichier))
        {
            var noeuds = wb.Worksheet("Noeuds");
            var arcs = wb.Worksheet("Arcs");

            // Charger les stations
            foreach (var row in noeuds.RowsUsed().Skip(1)) // skip header
            {
                int id = int.Parse(row.Cell("A").GetValue<string>());
                string nom = row.Cell("C").GetValue<string>();
                double lon = double.Parse(row.Cell("D").GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);
                double lat = double.Parse(row.Cell("E").GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);
                string ligne = row.Cell("B").GetValue<string>();

                var station = new Station(id, nom, ligne, lat, lon);
                stations[id] = station;

                // Ajouter la station à la ligne dans le dictionnaire
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

            // Ajouter les stations dans le graphe
            foreach (var station in stations.Values)
            {
                graphe.AjouterNoeud(station); // Ajouter chaque station au graphe
            }

            foreach (var row in arcs.RowsUsed().Skip(1))
            {
                int fromId = int.Parse(row.Cell("A").GetValue<string>());
                int toId = int.Parse(row.Cell("C").GetValue<string>());
                double temps = double.Parse(row.Cell("D").GetValue<string>(), System.Globalization.CultureInfo.InvariantCulture);
                if (stations.ContainsKey(fromId) && stations.ContainsKey(toId)&& fromId != 0 || toId != 0)
                {
                    var fromStation = stations[fromId];
                    var toStation = stations[toId];
                    graphe.AjouterLien(fromStation, toStation, temps);
                    graphe.AjouterLien(toStation, fromStation, temps);
                }
            }
            return graphe;
        }  
    }
    public static Graphe<string> ChargerGrapheDepuisExcel2(string cheminFichier)
    {
        var stations = new List<string>();
        var arcs = new List<(string, string, double)>(); // (Station A, Station B, poids)

        // Ouvrir le fichier Excel
        using (var package = new ExcelPackage(new FileInfo(cheminFichier)))
        {
            var worksheet = package.Workbook.Worksheets[0]; // On suppose que les données sont dans la première feuille
        
            int rowCount = worksheet.Dimension.Rows;
        
            // Lire les stations (première colonne de la feuille)
            for (int row = 2; row <= rowCount; row++) // Supposons que la première ligne contient les en-têtes
            {
                string stationA = worksheet.Cells[row, 1].Text; // Nom de la station A
                string stationB = worksheet.Cells[row, 2].Text; // Nom de la station B
                double poids = double.Parse(worksheet.Cells[row, 3].Text); // Le poids (temps ou distance entre les stations)
            
                if (!stations.Contains(stationA)) stations.Add(stationA);
                if (!stations.Contains(stationB)) stations.Add(stationB);
            
                arcs.Add((stationA, stationB, poids));
            }
        }

        // Construire le graphe à partir des stations et arcs
        var graphe = new Graphe<string>(stations.Count);
    
        // Ajouter les stations comme nœuds
        foreach (var station in stations)
        {
            graphe.AjouterNoeud(station);
        }

        // Ajouter les arcs entre les stations
        foreach (var arc in arcs)
        {
            graphe.AjouterLien(arc.Item1, arc.Item2, arc.Item3);
            graphe.AjouterLien(arc.Item2, arc.Item1, arc.Item3); // Assumer que les arcs sont bidirectionnels
        }

        return graphe;
    }
}