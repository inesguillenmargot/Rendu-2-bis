using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace LivinParisVF
{
   public class Graphe<T>
    {
        private List<T> dernierChemin = new List<T>();
        private Dictionary<T, List<Lien<T>>> listeAdjacence;
        private Dictionary<T, int> indexNoeuds;
        private int currentIndex;

        public Graphe(int nbNoeuds)
        {
            listeAdjacence = new Dictionary<T, List<Lien<T>>>();
            indexNoeuds = new Dictionary<T, int>();
            currentIndex = 0;
        }

        public List<T> GetDernierChemin()
        {
            return dernierChemin;
        }
        public void AjouterNoeud(T noeud)
        {
            if (!listeAdjacence.ContainsKey(noeud))
            {
                listeAdjacence[noeud] = new List<Lien<T>>();
                indexNoeuds[noeud] = currentIndex++;
            }
        }

        public void AjouterLien(T depart, T destination, int poids)
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
            Console.WriteLine("Parcours en Largeur");
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

            return visite.Count == listeAdjacence.Count;
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

        public void ParcoursProfondeurAvecAffichage(T sommetDepart)
        {
            Console.WriteLine("Parcours en Profondeur");
            var visite = new HashSet<T>();
            int compteur = 0;

            DFS_Affichage(sommetDepart, visite, ref compteur);

            Console.WriteLine($"\nNombre total de sommets visités : {compteur}");
        }

        private void DFS_Affichage(T sommet, HashSet<T> visite, ref int compteur)
        {
            visite.Add(sommet);
            Console.WriteLine($"Sommet visité : {sommet}");
            compteur++;

            foreach (var voisin in listeAdjacence[sommet])
            {
                if (!visite.Contains(voisin.Destination))
                {
                    DFS_Affichage(voisin.Destination, visite, ref compteur);
                }
            }
        }

        public void DijkstraEtAfficheChemin(T depart, T arrivee)
        {
            Console.WriteLine("DIJKSTRA");
            var distances = new Dictionary<T, double>();
            var precedents = new Dictionary<T, T>();
            var filePriorite = new PriorityQueue<T, double>();
            var visites = new HashSet<T>();

            // Initialisation des distances à +∞
            foreach (var noeud in listeAdjacence.Keys)
            {
                distances[noeud] = double.PositiveInfinity;
            }

            distances[depart] = 0;
            filePriorite.Enqueue(depart, 0);

            // Algorithme principal
            while (filePriorite.Count > 0)
            {
                var courant = filePriorite.Dequeue();

                if (!visites.Add(courant)) continue;

                if (courant.Equals(arrivee)) break;

                foreach (var voisin in listeAdjacence[courant])
                {
                    var voisinNoeud = voisin.Destination;
                    var poids = voisin.Poids;

                    // Vérifie si la station a été bien initialisée dans distances
                    if (!distances.ContainsKey(voisinNoeud))
                    {
                        distances[voisinNoeud] = double.PositiveInfinity;
                    }

                    double nouvelleDistance = distances[courant] + poids;

                    if (nouvelleDistance < distances[voisinNoeud])
                    {
                        distances[voisinNoeud] = nouvelleDistance;
                        precedents[voisinNoeud] = courant;
                        filePriorite.Enqueue(voisinNoeud, nouvelleDistance);
                    }
                }
            }

            // Si aucun chemin trouvé
            if (!precedents.ContainsKey(arrivee) && !arrivee.Equals(depart))
            {
                Console.WriteLine("Aucun chemin trouvé entre les deux stations.");
                return;
            }

            // Reconstruction du chemin
            var chemin = new List<T>();
            var actuel = arrivee;
            while (!actuel.Equals(depart))
            {
                chemin.Insert(0, actuel);
                actuel = precedents[actuel];
            }
            chemin.Insert(0, depart);

            // Affichage du chemin
            Console.WriteLine("\nChemin le plus court :");
            foreach (var station in chemin)
                Console.WriteLine($"  {station}");

            dernierChemin = chemin;
            // Calcul du temps réel en suivant les arcs du graphe
            int tempsTotal = 0;
            for (int i = 0; i < chemin.Count - 1; i++)
            {
                var from = chemin[i];
                var to = chemin[i + 1];

                // Trouver le lien entre from et to
                var lien = listeAdjacence[from].FirstOrDefault(l => l.Destination.Equals(to));

                if (lien != null)
                {
                    
                    tempsTotal += lien.Poids;
                }
                else
                {
                    Console.WriteLine($" Lien manquant entre {from} et {to} (temps ignoré)");
                }
            }
            Console.WriteLine($"\nTemps total estimé (vérifié) : {tempsTotal} minutes");


        }
        public void BellmanFordEtAfficheChemin(T depart, T arrivee)
        {
            Console.WriteLine("BELLMANFORD");
            var distances = new Dictionary<T, double>();
            var precedents = new Dictionary<T, T>();

            // Initialisation des distances
            foreach (var noeud in listeAdjacence.Keys)
            {
                distances[noeud] = double.PositiveInfinity;
            }
            distances[depart] = 0;

            var noeuds = listeAdjacence.Keys.ToList();

            // Étapes de relaxation
            for (int i = 0; i < noeuds.Count - 1; i++)
            {
                foreach (var u in listeAdjacence.Keys)
                {
                    foreach (var lien in listeAdjacence[u])
                    {
                        var v = lien.Destination;
                        var poids = lien.Poids;

                        if (distances[u] + poids < distances[v])
                        {
                            distances[v] = distances[u] + poids;
                            precedents[v] = u;
                        }
                    }
                }
            }

            // Détection de cycle négatif
            foreach (var u in listeAdjacence.Keys)
            {
                foreach (var lien in listeAdjacence[u])
                {
                    var v = lien.Destination;
                    if (distances[u] + lien.Poids < distances[v])
                    {
                        Console.WriteLine("Le graphe contient un cycle de poids négatif !");
                        return;
                    }
                }
            }

            // Si aucun chemin trouvé
            if (!precedents.ContainsKey(arrivee) && !arrivee.Equals(depart))
            {
                Console.WriteLine(" Aucun chemin trouvé entre les deux stations.");
                return;
            }

            // Reconstruction du chemin
            var chemin = new List<T>();
            var actuel = arrivee;

            while (!actuel.Equals(depart))
            {
                chemin.Insert(0, actuel);
                actuel = precedents[actuel];
            }
            chemin.Insert(0, depart);

            dernierChemin = chemin;
            // Affichage du chemin
            Console.WriteLine("\n Chemin le plus court (Bellman-Ford) :");
            foreach (var station in chemin)
            {
                Console.WriteLine($" {station}");
            }

            // Calcul du temps total réel avec vérification des arcs
            double tempsTotal = 0;
            for (int i = 0; i < chemin.Count - 1; i++)
            {
                var from = chemin[i];
                var to = chemin[i + 1];

                var lien = listeAdjacence[from].FirstOrDefault(l => l.Destination.Equals(to));

                if (lien != null)
                {
                    tempsTotal += lien.Poids;
                }
                else
                {
                    Console.WriteLine($" Lien manquant entre {from} et {to} (temps ignoré)");
                }
            }

            Console.WriteLine($"\n Temps total estimé : {tempsTotal} minutes");
        }
        
        public void FloydWarshallEtAfficheChemin()
        {
            var noeuds = listeAdjacence.Keys.ToList();
            int n = noeuds.Count;

            // Dictionnaires pour indexer les sommets
            var indexNoeud = new Dictionary<T, int>();
            var inverseIndex = new Dictionary<int, T>();

            for (int i = 0; i < n; i++)
            {
                indexNoeud[noeuds[i]] = i;
                inverseIndex[i] = noeuds[i];
            }

            // Matrice des distances
            double[,] distances = new double[n, n];
            T?[,] precedents = new T[n, n];

            // Initialisation
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = 0;
                    }
                    else
                    {
                        distances[i, j] = double.PositiveInfinity;
                    }
                    precedents[i, j] = default;
                }

            // Remplir avec les poids connus
            foreach (var u in listeAdjacence.Keys)
            {
                int i = indexNoeud[u];
                foreach (var lien in listeAdjacence[u])
                {
                    int j = indexNoeud[lien.Destination];
                    distances[i, j] = lien.Poids;
                    precedents[i, j] = u;
                }
            }

            // Algorithme de Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distances[i, k] + distances[k, j] < distances[i, j])
                        {
                            distances[i, j] = distances[i, k] + distances[k, j];
                            precedents[i, j] = precedents[k, j];
                        }
                    }
                }
            }

            // Affichage optionnel des distances minimales entre toutes les paires
            Console.WriteLine("\nPlus courts chemins (Floyd-Warshall) :");
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    var depart = inverseIndex[i];
                    var arrivee = inverseIndex[j];
                    var distance = distances[i, j];

                    if (double.IsInfinity(distance)) continue;

                    Console.WriteLine($" → {depart} → {arrivee} : {distance} min");
                }
            }
        }
    }
}


