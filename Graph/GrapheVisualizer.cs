using System;
using SkiaSharp;

namespace LivinParisVF
{
    public class GrapheVisualizer<T>
    {
        private Graphe<T> _graphe;
        private int _width = 1200;
        private int _height = 1000;
        private Dictionary<T, SKPoint> _positions;
        private Dictionary<T, int> _couleurs;

        public GrapheVisualizer(Graphe<T> graphe, Dictionary<T, int> couleurs = null)
        {
            _graphe = graphe;
            _positions = new Dictionary<T, SKPoint>();
            CalculerPositionsParCoordonnees();
            _couleurs = couleurs;
        }

        private void CalculerPositionsParCoordonnees()
        {
            double minLat = double.MaxValue, maxLat = double.MinValue;
            double minLon = double.MaxValue, maxLon = double.MinValue;

            foreach (var noeud in _graphe.GetListeAdjacence().Keys)
            {
                if (noeud is Station s)
                {
                    if (s.Latitude < minLat) minLat = s.Latitude;
                    if (s.Latitude > maxLat) maxLat = s.Latitude;
                    if (s.Longitude < minLon) minLon = s.Longitude;
                    if (s.Longitude > maxLon) maxLon = s.Longitude;
                }
            }

            double latRange = maxLat - minLat;
            double lonRange = maxLon - minLon;

            foreach (var noeud in _graphe.GetListeAdjacence().Keys)
            {
                if (noeud is Station s)
                {
                    float x = (float)((s.Longitude - minLon) / lonRange * (_width - 100)) + 50;
                    float y = (float)((maxLat - s.Latitude) / latRange * (_height - 100)) + 50;
                    _positions[noeud] = new SKPoint(x, y);
                }
            }
        }

        /// <summary>
        /// Liste de couleurs distinctes utilisées pour la coloration des sommets d'un graphe.
        /// </summary>
        private static readonly List<SKColor> CouleursDistinctes = new List<SKColor>
        {
            SKColors.Red,
            SKColors.Blue,
            //SKColors.Yellow, on l'a enlevee car pas assez visible sur le dessin
            SKColors.Green,
            SKColors.Purple,
            SKColors.Orange,
            SKColors.Brown,
            SKColors.Pink,
            SKColors.Teal,
            SKColors.Cyan
    
        };
        
        public void DessinerGraphe(string filePath)
        {
            using var bitmap = new SKBitmap(_width, _height);
            using var canvas = new SKCanvas(bitmap);
            var chemin = _graphe.GetDernierChemin();
            var liensChemin = new HashSet<(int, int)>();

            for (int i = 0; i < chemin.Count - 1; i++)
            {
                if (chemin[i] is Station s1 && chemin[i + 1] is Station s2)
                {
                    liensChemin.Add((s1.Id, s2.Id));
                    liensChemin.Add((s2.Id, s1.Id)); // graphe non orienté
                }
            }
            canvas.Clear(SKColors.White);

            var paintArrete = new SKPaint { Color = SKColors.Black, StrokeWidth = 1.5f, IsAntialias = true };
            var paintTexte = new SKPaint { Color = SKColors.White, TextSize = 10, TextAlign = SKTextAlign.Center };

            var couleursLignes = new Dictionary<string, SKColor>
            {
                { "1", SKColors.Gold },
                { "2", SKColors.DeepSkyBlue },
                { "3", SKColors.Olive },
                { "3bis", SKColors.DarkOliveGreen },
                { "4", SKColors.Purple },
                { "5", SKColors.OrangeRed },
                { "6", SKColors.LightSeaGreen },
                { "7", SKColors.Pink },
                { "7bis", SKColors.HotPink },
                { "8", SKColors.Violet },
                { "9", SKColors.YellowGreen },
                { "10", SKColors.DarkGoldenrod },
                { "11", SKColors.DarkOrange },
                { "12", SKColors.DarkGreen },
                { "13", SKColors.Teal },
                { "14", SKColors.MediumPurple }
            };

            foreach (var noeud in _graphe.GetListeAdjacence())
            {
                if (!_positions.ContainsKey(noeud.Key)) continue;
                SKPoint p1 = _positions[noeud.Key];

                foreach (var voisin in noeud.Value)
                {
                    T voisinId = voisin.Destination;
                    if (_positions.ContainsKey(voisinId))
                    {
                        SKPoint p2 = _positions[voisinId];
                        var id1 = (noeud.Key as Station)?.Id ?? -1;
                        var id2 = (voisinId as Station)?.Id ?? -1;

                        //var paint = liensChemin.Contains((id1, id2))
                            //? new SKPaint { Color = SKColors.Red, StrokeWidth = 4, IsAntialias = true }
                            //: paintArrete;

                        //canvas.DrawLine(p1, p2, paint);
                        
                        SKPaint paint;
                        if (liensChemin.Contains((id1, id2)))
                        {
                            paint = new SKPaint { Color = SKColors.Red, StrokeWidth = 4, IsAntialias = true };
                        }
                        else
                        {
                            // Utiliser la couleur de la ligne de la station de départ
                            if (noeud.Key is Station stationDepart)
                            {
                                string ligneDepart = stationDepart.Lignes[0].Trim().ToLower();
                                if (ligneDepart.StartsWith("ligne ")) ligneDepart = ligneDepart.Substring(6);
                                var couleurLigne = couleursLignes.TryGetValue(ligneDepart, out var c) ? c : SKColors.Gray;

                                paint = new SKPaint { Color = couleurLigne, StrokeWidth = 1.5f, IsAntialias = true };
                            }
                            else
                            {
                                paint = paintArrete; // fallback noir si jamais
                            }
                        }

                        canvas.DrawLine(p1, p2, paint);
                    }
                }
            }

            foreach (var (id, pos) in _positions)
            {
                if (id is Station station)
                {
                    string ligne = station.Lignes[0].Trim().ToLower();
                    if (ligne.StartsWith("ligne ")) ligne = ligne.Substring(6);

                    //SKColor couleur = couleursLignes.TryGetValue(ligne, out var c) ? c : SKColors.Gray;
                    //var paintNoeud = new SKPaint { Color = couleur, IsAntialias = true };

                    //canvas.DrawCircle(pos, 6, paintNoeud);
                    //canvas.DrawText(station.Id.ToString(), pos.X, pos.Y + 5, paintTexte);
                    SKColor couleur;
                    
                    if (_couleurs != null && _couleurs.TryGetValue(id, out int groupe))
                    {
                        couleur = CouleursDistinctes[groupe % CouleursDistinctes.Count];
                    }
                    else
                    {
                        // fallback sur la ligne si pas de coloration passée
                        string ligneStation = station.Lignes[0].Trim().ToLower();
                        if (ligneStation.StartsWith("ligne ")) ligneStation = ligneStation.Substring(6);
                        couleur = couleursLignes.TryGetValue(ligneStation, out var c) ? c : SKColors.Gray;
                    }
                    var paintNoeud = new SKPaint { Color = couleur, IsAntialias = true };
                    
                    canvas.DrawCircle(pos, 6, paintNoeud);
                    canvas.DrawText(station.Id.ToString(), pos.X, pos.Y + 5, paintTexte);
                }
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(filePath, data.ToArray());

            string cheminComplet = Path.GetFullPath(filePath);
            Console.WriteLine("Le dessin du graphe a été enregistré avec succès !");
            Console.WriteLine($"Emplacement du fichier : {cheminComplet}");
        }
    }
}
