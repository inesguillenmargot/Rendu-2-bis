namespace LivinParisVF;
using System;
using ClosedXML.Excel;

class Program
{
    /// <summary>
    /// Méthode permettant de récupérer le fichier excel, et toutes les composantes de celui-ci
    /// </summary>
    public static class FichierUtilise
    {
        public static string GetCheminExcel()
        {
            string basePath = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
            return Path.Combine(basePath, "Graph", "MetroParis (4).xlsx");
        }
    }


    static void Main()
    {
        string cheminExcel = FichierUtilise.GetCheminExcel();
        var graphe = ChargementGraphe.ChargerGrapheDepuisExcel(cheminExcel);
        var visualiseur = new GrapheVisualizer<Station>(graphe);
        


        var stationSource = graphe.GetListeAdjacence().Keys.First();
        graphe.ParcoursLargeur(stationSource);
        Console.WriteLine();
        Console.WriteLine(graphe.EstConnexe());
        Console.WriteLine();
        graphe.ParcoursProfondeurAvecAffichage(stationSource);

        Console.WriteLine("Entrez le nom de la station de départ :");
        string nomDepart = Console.ReadLine().Trim().ToLower();

        Console.WriteLine("Entrez le nom de la station d’arrivée :");
        string nomArrivee = Console.ReadLine().Trim().ToLower();

        var stationDepart = graphe.GetListeAdjacence().Keys
            .FirstOrDefault(s => s.Nom.ToLower().Contains(nomDepart));

        var stationArrivee = graphe.GetListeAdjacence().Keys
            .FirstOrDefault(s => s.Nom.ToLower().Contains(nomArrivee));
        if (stationDepart == null)
        {
            Console.WriteLine("La station de départ est introuvable.");
        }
        else if (stationArrivee == null)
        {
            Console.WriteLine("La station d’arrivée est introuvable.");
        }
        else
        {
            graphe.DijkstraEtAfficheChemin(stationDepart, stationArrivee);
            ///graphe.BellmanFordEtAfficheChemin(stationDepart, stationArrivee);
            ///graphe.FloydWarshallEtAfficheChemin();
            visualiseur.DessinerGraphe("graphe_paris.png");
        }
    }
}
