namespace LivinParisVF;
using System;
using ClosedXML.Excel;

class Program
{

    
    public static string NormaliserNom(string nom)
    {
        return nom.ToLower().Trim(); // Convertit en minuscules et enlève les espaces avant/après
    }

    
    static void Main()
    {
        string cheminExcel = @"C:\Users\guill\RiderProjects\LivinParis_V3\Graph\bin\Debug\net8.0\MetroParis (4).xlsx";
        var graphe = ChargementGraphe.ChargerGrapheDepuisExcel(cheminExcel);
        var visualiseur = new GrapheVisualizer<Station>(graphe);
        visualiseur.DessinerGraphe("graphe_paris.png");

        var stationSource = graphe.GetListeAdjacence().Keys.First();
        graphe.ParcoursLargeur(stationSource);
        Console.WriteLine(graphe.EstConnexe());

        // 1. Tester l'algorithme de Dijkstra
        Console.WriteLine("\n--- Algorithme de Dijkstra ---");
        var dijkstraDistances = graphe.Dijkstra(stationSource);
        Console.WriteLine("Distances depuis la station source (Dijkstra) :");
        foreach (var item in dijkstraDistances)
        {
            Console.WriteLine($"Station: {item.Key}, Distance: {item.Value}");
        }

        // 2. Tester l'algorithme de Bellman-Ford
        Console.WriteLine("\n--- Algorithme de Bellman-Ford ---");
        var bellmanFordDistances = graphe.BellmanFord(stationSource);
        if (bellmanFordDistances != null)
        {
            Console.WriteLine("Distances depuis la station source (Bellman-Ford) :");
            foreach (var item in bellmanFordDistances)
            {
                Console.WriteLine($"Station: {item.Key}, Distance: {item.Value}");
            }
        }
    }
}
