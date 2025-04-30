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
            return Path.Combine(basePath, "Graph", "MetroParis (5).xlsx");
        }
    }


    static void Main()
    {
        string cheminExcel = FichierUtilise.GetCheminExcel();
        var graphe = ChargementGraphe.ChargerGrapheDepuisExcel(cheminExcel);
        var couleurs = graphe.ColorierGrapheWelshPowell();
        var visualiseur = new GrapheVisualizer<Station>(graphe,couleurs); /// si je ne veux pas la coloration, j'enleve le paramètre couleur


        Console.WriteLine("\n--- Coloration du graphe (Welsh-Powell) ---");
        ///var couleurs = graphe.ColorierGrapheWelshPowell();

// Analyse des propriétés du graphe
        int nbCouleurs = couleurs.Values.Distinct().Count();

        if (nbCouleurs == 2)
        {
            Console.WriteLine("Le graphe est biparti, 2 couleurs nécessaires");
        }
        else
        {
            Console.WriteLine($"Le graphe n'est pas biparti ( minimum {nbCouleurs} couleurs nécessaires).");
        }

// Estimation de la planarité (approximative)
        if (nbCouleurs <= 4)
        {
            Console.WriteLine("Le graphe est probablement planaire .");
        }
        else
        {
            Console.WriteLine("Le graphe est probablement non planaire.");
        }

        var stationSource = graphe.GetListeAdjacence().Keys.First();
        ///graphe.ParcoursLargeur(stationSource);
        Console.WriteLine();
        Console.WriteLine(graphe.EstConnexe());
        Console.WriteLine();
        ///graphe.ParcoursProfondeurAvecAffichage(stationSource);

        Console.WriteLine("Entrez le nom de la station de départ :");
        string nomDepart = Console.ReadLine().Trim().ToLower();

        Console.WriteLine("Entrez le nom de la station d’arrivée :");
        string nomArrivee = Console.ReadLine().Trim().ToLower();

        string Normaliser(string texte)
        {
            return texte.ToLower().Trim().Replace("é", "e").Replace("è", "e").Replace("ê", "e").Replace("à", "a").Replace("â", "a");
        }

        var stationDepart = graphe.GetListeAdjacence().Keys.FirstOrDefault(s => Normaliser(s.Nom) == Normaliser(nomDepart));
        var stationArrivee = graphe.GetListeAdjacence().Keys.FirstOrDefault(s => Normaliser(s.Nom) == Normaliser(nomArrivee));
        
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
            //graphe.DijkstraEtAfficheChemin(stationDepart, stationArrivee);
            //graphe.BellmanFordEtAfficheChemin(stationDepart, stationArrivee);
            //graphe.FloydWarshallEtAfficheChemin(stationDepart, stationArrivee);
            graphe.ChoixMeilleurAlgo(stationDepart, stationArrivee);
        }
        visualiseur.DessinerGraphe("graphe_paris.png");
    }
}
