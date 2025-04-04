using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace LivinParisVF
{
    public class Graphe<T>
    {
        private Dictionary<T, List<Lien<T>>> listeAdjacence;
        private Dictionary<T, int> indexNoeuds;
        private int currentIndex;

        public Graphe(int nbNoeuds)
        {
            listeAdjacence = new Dictionary<T, List<Lien<T>>>();
            indexNoeuds = new Dictionary<T, int>();
            currentIndex = 0;
        }
        // Méthode pour importer les données depuis un fichier Excel
        public void ImporterDonneesDepuisExcel(string cheminExcel)
        {
            var workbook = new XLWorkbook(cheminExcel);
            var worksheet = workbook.Worksheet(1);

            // Charger les données du graphe depuis le fichier Excel
            var graphData = worksheet.RowsUsed().Skip(1)
                .Select(row =>
                {
                    string station = row.Cell(1).Value.ToString().Trim(); // Supprimer les espaces inutiles
                    string lien = row.Cell(2).Value.ToString().Trim();
                    double poids = 0;

                    try
                    {
                        poids = Convert.ToDouble(row.Cell(3).Value);
                    }
                    catch (InvalidCastException)
                    {
                        Console.WriteLine($"Avertissement : Impossible de convertir la cellule en nombre pour la station {station}.");
                    }

                    return new
                    {
                        Station = station,
                        Lien = lien,
                        Poids = poids
                    };
                }).ToList();

            // Afficher toutes les stations et leurs liens pour vérifier que "Concorde" est bien dans la liste
            Console.WriteLine("Stations importées :");
            foreach (var data in graphData)
            {
                Console.WriteLine($"Station: {data.Station}, Lien: {data.Lien}, Poids: {data.Poids}");
            }

            // Construire la liste d'adjacence pour le graphe
            foreach (var data in graphData)
            {
                AjouterNoeud((T)Convert.ChangeType(data.Station, typeof(T)));  // Ajouter chaque station en tant que noeud

                if (!listeAdjacence.ContainsKey((T)Convert.ChangeType(data.Station, typeof(T))))
                {
                    listeAdjacence[(T)Convert.ChangeType(data.Station, typeof(T))] = new List<Lien<T>>();
                }

                listeAdjacence[(T)Convert.ChangeType(data.Station, typeof(T))].Add(new Lien<T>((T)Convert.ChangeType(data.Lien, typeof(T)), data.Poids));
            }

            // Afficher le contenu du dictionnaire listeAdjacence pour s'assurer que "Concorde" est dedans
            Console.WriteLine("\nContenu du dictionnaire listeAdjacence :");
            foreach (var station in listeAdjacence)
            {
                Console.WriteLine($"Station: {station.Key}, Nombre de voisins: {station.Value.Count}");
            }
        }
        public void AjouterNoeud(T noeud)
        {
            if (!listeAdjacence.ContainsKey(noeud))
            {
                listeAdjacence[noeud] = new List<Lien<T>>();
                indexNoeuds[noeud] = currentIndex++;
            }
        }

        public void AjouterLien(T depart, T destination, double poids)
        {
            if (listeAdjacence.ContainsKey(depart))
            {
                listeAdjacence[depart].Add(new Lien<T>(destination, poids));
            }
            else
            {
                listeAdjacence[depart] = new List<Lien<T>> { new Lien<T>(destination, poids) };
            }
        }

        public void ParcoursLargeur(T depart)
        {
            var file = new Queue<T>();
            var visite = new HashSet<T>(); 
            file.Enqueue(depart);
            visite.Add(depart);
            int compteur = 0;

            while (file.Count > 0)
            {
                var noeud = file.Dequeue();
                Console.WriteLine(noeud);
                compteur++;

                foreach (var voisin in listeAdjacence[noeud])
                {
                    if (!visite.Contains(voisin.Destination))
                    {
                        visite.Add(voisin.Destination);
                        file.Enqueue(voisin.Destination);
                    }
                }
            }
            Console.WriteLine($"Nombre de nœuds parcourus : {compteur}");
        }

        public bool EstConnexe()
        {
            if (listeAdjacence.Count == 0) return false;

            var stationDeDepart = listeAdjacence.Keys.First();
            var file = new Queue<T>();
            var visite = new HashSet<T>();
            file.Enqueue(stationDeDepart);
            visite.Add(stationDeDepart);

            while (file.Count > 0)
            {
                var noeud = file.Dequeue();

                foreach (var voisin in listeAdjacence[noeud])
                {
                    if (!visite.Contains(voisin.Destination))
                    {
                        visite.Add(voisin.Destination);
                        file.Enqueue(voisin.Destination);
                    }
                }
            }

            return visite.Count ==221;
        }

        public Dictionary<T, List<Lien<T>>> GetListeAdjacence()
        {
            return listeAdjacence;
        }

        public void VerifierStationsIsolées()
        {
            foreach (var noeud in listeAdjacence.Keys)
            {
                if (listeAdjacence[noeud].Count == 0)
                {
                    Console.WriteLine($"La station {noeud} est isolée et n'a pas de voisins.");
                }
            }
        }

        public Dictionary<T, double> Dijkstra(T source)
        {
            var distances = new Dictionary<T, double>();  // Stocke la distance de la source à chaque nœud
            var parents = new Dictionary<T, T>();         // Pour reconstruire le chemin
            var priorityQueue = new SortedDictionary<double, T>(); // File de priorité

            // Initialisation
            foreach (var noeud in listeAdjacence.Keys)
            {
                distances[noeud] = double.MaxValue; // Distance infinie
                parents[noeud] = default(T);        // Pas de parent initialement
            }
            distances[source] = 0;  // La distance de la source à elle-même est 0
            priorityQueue[0] = source;

            while (priorityQueue.Count > 0)
            {
                var currentNode = priorityQueue.First().Value;
                priorityQueue.Remove(priorityQueue.First().Key);

                foreach (var voisin in listeAdjacence[currentNode])
                {
                    double newDist = distances[currentNode] + voisin.Poids;
                    if (newDist < distances[voisin.Destination])
                    {
                        distances[voisin.Destination] = newDist;
                        parents[voisin.Destination] = currentNode;

                        // Ajouter le voisin à la file de priorité avec la nouvelle distance
                        if (!priorityQueue.ContainsValue(voisin.Destination))
                        {
                            priorityQueue[newDist] = voisin.Destination;
                        }
                    }
                }
            }

            return distances;
        }

        public Dictionary<T, double> BellmanFord(T source)
        {
            var distances = new Dictionary<T, double>();
            var parents = new Dictionary<T, T>();

            // Initialisation
            foreach (var noeud in listeAdjacence.Keys)
            {
                distances[noeud] = double.MaxValue;
                parents[noeud] = default(T);
            }
            distances[source] = 0;

            // Relaxation des arêtes V - 1 fois
            for (int i = 1; i < listeAdjacence.Keys.Count; i++)
            {
                foreach (var noeud in listeAdjacence.Keys)
                {
                    foreach (var voisin in listeAdjacence[noeud])
                    {
                        if (distances[noeud] + voisin.Poids < distances[voisin.Destination])
                        {
                            distances[voisin.Destination] = distances[noeud] + voisin.Poids;
                            parents[voisin.Destination] = noeud;
                        }
                    }
                }
            }

            // Détection de cycle négatif
            foreach (var noeud in listeAdjacence.Keys)
            {
                foreach (var voisin in listeAdjacence[noeud])
                {
                    if (distances[noeud] + voisin.Poids < distances[voisin.Destination])
                    {
                        Console.WriteLine("Le graphe contient un cycle négatif.");
                        return null;
                    }
                }
            }

            return distances;
        }

        public Dictionary<T, Dictionary<T, double>> FloydWarshall()
        {
            var distances = new Dictionary<T, Dictionary<T, double>>();
            var parents = new Dictionary<T, Dictionary<T, T>>();

            // Initialisation
            foreach (var noeud in listeAdjacence.Keys)
            {
                distances[noeud] = new Dictionary<T, double>();
                parents[noeud] = new Dictionary<T, T>();

                foreach (var voisin in listeAdjacence[noeud])
                {
                    distances[noeud][voisin.Destination] = voisin.Poids;
                    parents[noeud][voisin.Destination] = noeud;
                }

                // La distance d'un nœud à lui-même est 0
                distances[noeud][noeud] = 0;
            }

            // Appliquer l'algorithme de Floyd-Warshall
            foreach (var k in listeAdjacence.Keys)
            {
                foreach (var i in listeAdjacence.Keys)
                {
                    foreach (var j in listeAdjacence.Keys)
                    {
                        if (distances[i][k] + distances[k][j] < distances[i][j])
                        {
                            distances[i][j] = distances[i][k] + distances[k][j];
                            parents[i][j] = parents[k][j];
                        }
                    }
                }
            }

            return distances;
        }
    }

    
}


