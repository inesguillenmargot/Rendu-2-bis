using System;
using System.Collections.Generic;
using ClosedXML.Excel;

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

        /// <summary>
        /// Retourne le dernier chemin calculé par un algorithme de plus court chemin (Dijkstra ou Bellman-Ford).
        /// </summary>
        /// <returns></returns>
        public List<T> GetDernierChemin()
        {
            return dernierChemin;
        }

        /// <summary>
        /// Ajoute un nouveau nœud au graphe s’il n’est pas déjà présent
        /// </summary>
        /// <param name="noeud"></param>
        public void AjouterNoeud(T noeud)
        {
            if (!listeAdjacence.ContainsKey(noeud))
            {
                listeAdjacence[noeud] = new List<Lien<T>>();
                indexNoeuds[noeud] = currentIndex++;
            }
        }

        /// <summary>
        /// Ajoute un lien (arête) entre deux nœuds du graphe avec un poids spécifié.
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="destination"></param>
        /// <param name="poids"></param>
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

        /// <summary>
        /// Effectue un parcours en largeur du graphe à partir d’un nœud donné.
        /// Affiche chaque nœud visité et le nombre total de nœuds parcourus.
        /// </summary>
        /// <param name="depart"></param>
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

        /// <summary>
        /// Vérifie si le graphe est connexe, c’est-à-dire si tous les nœuds sont accessibles à partir d’un nœud.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Retourne la liste d’adjacence du graphe (structure interne du graphe).
        /// </summary>
        /// <returns></returns>
        public Dictionary<T, List<Lien<T>>> GetListeAdjacence()
        {
            return listeAdjacence;
        }

        /// <summary>
        /// Vérifie et affiche les stations (nœuds) isolées n’ayant aucun voisin dans le graphe.
        /// </summary>
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

        /// <summary>
        /// Effectue un parcours en profondeur du graphe à partir d’un sommet donné.
        /// Affiche les sommets visités et le nombre total.
        /// </summary>
        /// <param name="sommetDepart"></param>
        public void ParcoursProfondeurAvecAffichage(T sommetDepart)
        {
            Console.WriteLine("Parcours en Profondeur");
            var visite = new HashSet<T>();
            int compteur = 0;

            DFS_Affichage(sommetDepart, visite, ref compteur);

            Console.WriteLine($"\nNombre total de sommets visités : {compteur}");
        }

        /// <summary>
        /// Effectue un parcours en profondeur du graphe à partir d’un sommet donné.
        /// Sans Afficher les sommets visités et le nombre total.
        /// </summary>
        /// <param name="sommet"></param>
        /// <param name="visite"></param>
        /// <param name="compteur"></param>
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

        /// <summary>
        /// Applique l’algorithme de Dijkstra pour trouver et afficher le chemin le plus court entre deux sommets.
        /// Affiche également le temps total estimé.
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="arrivee"></param>
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


        /// <summary>
        ///  Applique l’algorithme de Dijkstra pour trouver et  retourner le temps le chemin le plus court entre deux sommets.

        /// </summary>
        /// <param name="depart"></param>
        /// <param name="arrivee"></param>
        public int Dijkstra(T depart, T arrivee)
        {
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
            return tempsTotal;
        }

        /// <summary>
        /// Applique l’algorithme de Bellman-Ford pour déterminer le chemin le plus court entre deux nœuds.
        /// Détecte aussi les cycles de poids négatif. Affiche le chemin et le temps total estimé.
        /// </summary>
        /// <param name="depart"></param>
        /// <param name="arrivee"></param>
        public int BellmanFordEtAfficheChemin(T depart, T arrivee)
        {
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
                        
                    }
                }
            }

            // Si aucun chemin trouvé
            if (!precedents.ContainsKey(arrivee) && !arrivee.Equals(depart))
            {
                Console.WriteLine(" Aucun chemin trouvé entre les deux stations.");
                
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
            int tempsTotal = 0;
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
            return tempsTotal;
        }

        /// <summary>
        /// Applique l’algorithme de Floyd-Warshall pour calculer les plus courts chemins entre toutes les paires de nœuds.
        /// Affiche les distances entre tous les couples de stations.
        /// </summary>
        public int FloydWarshallEtAfficheChemin(T stationDepart, T stationArrivee)
        {
            var noeuds = listeAdjacence.Keys.ToList();
            int n = noeuds.Count;

            // Dictionnaires pour indexer les sommets
            var indexNoeud = new Dictionary<T, int>();

            for (int i = 0; i < n; i++)
            {
                indexNoeud[noeuds[i]] = i;
            }

            // Matrice des distances
            int[,] distances = new int[n, n];

            // Initialisation
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = 0;
                    }
                    else
                    {
                        distances[i, j] = int.MaxValue; // Utilisation de int.MaxValue pour l'infini
                    }
                }
            }

            // Remplir avec les poids connus
            foreach (var u in listeAdjacence.Keys)
            {
                int i = indexNoeud[u];
                foreach (var lien in listeAdjacence[u])
                {
                    int j = indexNoeud[lien.Destination];
                    distances[i, j] = lien.Poids;
                }
            }

            // Algorithme de Floyd-Warshall
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distances[i, k] != int.MaxValue && distances[k, j] != int.MaxValue &&
                            distances[i, k] + distances[k, j] < distances[i, j])
                        {
                            distances[i, j] = distances[i, k] + distances[k, j];
                        }
                    }
                }
            }

            // Index des stations de départ et d'arrivée
            int departIndex = indexNoeud[stationDepart];
            int arriveeIndex = indexNoeud[stationArrivee];

            // Temps entre la station de départ et la station d'arrivée
            int temps = distances[departIndex, arriveeIndex];

            if (temps == int.MaxValue)
            {
                Console.WriteLine("Il n'y a pas de chemin entre ces deux stations.");
                return -1; // Retourne -1 si aucune route n'existe
            }

            return temps; // Retourne le temps minimal entre les deux stations
        }

        public int ChoixMeilleurAlgo(T stationDepart, T stationArrivee)
        {
            int TempsD = Dijkstra(stationDepart, stationArrivee);
            int TempsB = BellmanFordEtAfficheChemin(stationDepart, stationArrivee);
            int TempsF=  FloydWarshallEtAfficheChemin(stationDepart,stationArrivee);
            if (TempsD <= TempsB && TempsD <= TempsF)
            {
                Console.WriteLine($"\nTemps total estimé (vérifié) : {TempsD} minutes");
                return TempsD;
            }
            else if (TempsB <= TempsD && TempsB <= TempsF)
            {
                Console.WriteLine($"\nTemps total estimé (vérifié) : {TempsB} minutes");
                return TempsB;
            }
            else
            {
                Console.WriteLine($"\nTemps total estimé (vérifié) : {TempsF} minutes");
                return TempsF;
            }
        }

        /// <summary>
        /// Applique l'algorithme de Welsh-Powell pour colorier le graphe.
        /// Associe à chaque sommet une couleur (représentée par un entier) de manière à ce que deux sommets adjacents n'aient jamais la même couleur.
        /// </summary>
        /// <returns>
        /// Un dictionnaire associe chaque sommet à un entier qui représente sa couleur et la valeur correspond au numéro de couleur utilisé pour ce sommet.
        /// </returns>
        public Dictionary<T, int> ColorierGrapheWelshPowell()
        {

            // Ordonner les sommets par degré décroissant
            var sommets = listeAdjacence.Keys
                .OrderByDescending(noeud => listeAdjacence[noeud].Count)
                .ToList();

            var couleurs = new Dictionary<T, int>();
            int couleurActuelle = 0;

            foreach (var sommet in sommets)
            {
                if (couleurs.ContainsKey(sommet)) continue;

                couleurs[sommet] = couleurActuelle;

                foreach (var autre in sommets)
                {
                    if (couleurs.ContainsKey(autre)) continue;

                    // Vérifie si aucun voisin de autre n’a la couleur actuelle
                    bool peutColorier = !listeAdjacence[autre]
                        .Any(voisin => couleurs.TryGetValue(voisin.Destination, out int c) && c == couleurActuelle);

                    if (peutColorier)
                        couleurs[autre] = couleurActuelle;
                }

                couleurActuelle++;
            }

            Console.WriteLine($"\nNombre chromatique (minimum de couleurs nécessaires) : {couleurActuelle}");

            // Affichage des groupes indépendants
            for (int c = 0; c < couleurActuelle; c++)
            {
                Console.WriteLine($"\nGroupe de couleur {c} :");
                foreach (var pair in couleurs)
                {
                    if (pair.Value == c)
                        Console.WriteLine($" - {pair.Key}");
                }
            }

            return couleurs;
        }
    }
}

