using System;
using MySqlConnector;
using LivinParis_V2;
using System.Text;
using LivinParisVF;
using OfficeOpenXml;
using System.Linq;
using ClosedXML.Excel;

public class Program
{
    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    static void Main(string[] args)
    {
        string cheminExcel = @"C:\Users\guill\RiderProjects\LivinParis_V3\Graph\bin\Debug\net8.0\MetroParis (4).xlsx";
        var graphe = ChargementGraphe.ChargerGrapheDepuisExcel(cheminExcel);
        var visualiseur = new GrapheVisualizer<Station>(graphe);
        visualiseur.DessinerGraphe("graphe_paris.png");
        
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        string[] options = { "S'inscrire", "Se connecter", "Exit" };
        int selectedIndex = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Menu principal ===\n");

            for (int i = 0; i < options.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.Write("👉 ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.Write("   ");
                    Console.ResetColor();
                }
                Console.WriteLine(options[i]);
            }

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % options.Length;
                    break;
                case ConsoleKey.Enter:
                    Console.ResetColor();
                    switch (selectedIndex)
                    {
                        case 0:
                            InscriptionUtilisateur();
                            break;
                        case 1:
                            ConnexionUtilisateur();
                            break;
                        case 2:
                            Console.WriteLine("\nAu revoir !");
                            return;
                    }
                    break;
            }
        }
    }

    static void InscriptionUtilisateur()
    {
        Console.Clear();
        Console.WriteLine("=== Formulaire d'inscription ===\n");

        var utilisateur = new Utilisateur();

        Console.Write("Nom : ");
        utilisateur.Nom = Console.ReadLine() ?? "";

        Console.Write("Prénom : ");
        utilisateur.Prenom = Console.ReadLine() ?? "";

        utilisateur.Telephone = LireTelephoneValide();

        Console.Write("Email : ");
        utilisateur.Email = Console.ReadLine() ?? "";

        utilisateur.MotDePasse = LireMotDePasseValide();

        Console.Write("Station métro proche : ");
        utilisateur.MetroProche = Console.ReadLine() ?? "";

        Console.Write("Rue : ");
        utilisateur.Rue = Console.ReadLine() ?? "";

        Console.Write("Numéro de rue : ");
        utilisateur.NumRue = int.Parse(Console.ReadLine() ?? "0");

        utilisateur.CodePostal = LireCodePostalValide();

        Console.Write("Ville : ");
        utilisateur.Ville = Console.ReadLine() ?? "";

        string[] roles = { "Client", "Cuisinier", "Client + Cuisinier" };
        int roleIndex = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Formulaire d'inscription (suite) ===\n");
            Console.WriteLine("Sélectionnez votre rôle :\n");

            for (int i = 0; i < roles.Length; i++)
            {
                if (i == roleIndex)
                {
                    Console.Write("👉 ");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.Write("   ");
                    Console.ResetColor();
                }

                Console.WriteLine(roles[i]);
            }

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    roleIndex = (roleIndex - 1 + roles.Length) % roles.Length;
                    break;
                case ConsoleKey.DownArrow:
                    roleIndex = (roleIndex + 1) % roles.Length;
                    break;
                case ConsoleKey.Enter:
                    Console.ResetColor();
                    utilisateur.Type = roles[roleIndex];
                    utilisateur.EstClient = utilisateur.Type.Contains("Client");
                    utilisateur.EstCuisinier = utilisateur.Type.Contains("Cuisinier");
                    break;
            }
            if (key == ConsoleKey.Enter)
            {
                break;
            }
        }
        utilisateur.EstClient = utilisateur.Type.ToLower().Contains("client");
        utilisateur.EstCuisinier = utilisateur.Type.ToLower().Contains("cuisinier");
        if (utilisateur.EstClient)
        {
            string[] clientTypes = { "Particulier", "Entreprise" };
            int clientIndex = 0;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Client : êtes-vous un particulier ou une entreprise ? ===\n");

                for (int i = 0; i < clientTypes.Length; i++)
                {
                    if (i == clientIndex)
                    {
                        Console.Write("👉 ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.Write("   ");
                        Console.ResetColor();
                    }

                    Console.WriteLine(clientTypes[i]);
                }

                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        clientIndex = (clientIndex - 1 + clientTypes.Length) % clientTypes.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        clientIndex = (clientIndex + 1) % clientTypes.Length;
                        break;
                    case ConsoleKey.Enter:
                        Console.ResetColor();
                        utilisateur.EstParticulier = clientTypes[clientIndex] == "Particulier";
                        break;
                }

                if (key == ConsoleKey.Enter)
                {
                    break;
                }
            }

            if (!utilisateur.EstParticulier)
            {
                Console.Write("Nom de l'entreprise : ");
                utilisateur.EntrepriseNom = Console.ReadLine() ?? "";

                Console.Write("Nom du référent : ");
                utilisateur.EntrepriseNomReferent = Console.ReadLine() ?? "";
            }
        }

        try
        {
            int id = utilisateur.Ajouter();
            utilisateur.UtilisateurId = id;

            Console.WriteLine("\n✅ Utilisateur inscrit avec succès !");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Erreur lors de l'inscription : " + ex.Message);
        }

        Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
        Console.ReadKey();
    }
    static void ConnexionUtilisateur()
    {
        Console.Clear();
        Console.WriteLine("=== Connexion ===\n");

        Console.Write("Email : ");
        string email = Console.ReadLine() ?? "";

        Console.Write("Mot de passe : ");
        string motdepasse = Console.ReadLine() ?? "";

        object? result = null;

        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT utilisateur_id FROM Utilisateur WHERE utilisateur_email = @Email AND utilisateur_mdp = @Mdp";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@Mdp", motdepasse);

        try
        {
            conn.Open();
            result = cmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Erreur de connexion à la base : " + ex.Message);
        }

        if (result != null)
        {
            int utilisateurId = Convert.ToInt32(result);

            var utilisateur = Utilisateur.RecupererParId(utilisateurId);

            Console.WriteLine($"\n✅ Connexion réussie ! Bienvenue {utilisateur.Prenom} {utilisateur.Nom}");

            if (utilisateur.EstCuisinier && !utilisateur.EstClient)
            {
                MenuCuisinier(utilisateurId);
            }
            else if (utilisateur.EstClient && !utilisateur.EstCuisinier)
            {
                MenuClient(utilisateurId);
            }
            else if (utilisateur.EstCuisinier && utilisateur.EstClient)
            {
                string[] roles = { "Espace Cuisinier", "Espace Client" };
                int roleIndex = 0;
                ConsoleKey key;

                do
                {
                    Console.Clear();
                    Console.WriteLine("=== Choisissez un rôle pour cette session ===\n");
                    for (int i = 0; i < roles.Length; i++)
                    {
                        if (i == roleIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("👉 ");
                        }
                        else
                        {
                            Console.Write("   ");
                            Console.ResetColor();
                        }
                        Console.WriteLine(roles[i]);
                    }
                    Console.ResetColor();

                    key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            roleIndex = (roleIndex - 1 + roles.Length) % roles.Length;
                            break;
                        case ConsoleKey.DownArrow:
                            roleIndex = (roleIndex + 1) % roles.Length;
                            break;
                    }
                } while (key != ConsoleKey.Enter);

                if (roleIndex == 0)
                    MenuCuisinier(utilisateurId);
                else
                    MenuClient(utilisateurId);
            }
        }
        else
        {
            Console.WriteLine("\n❌ Identifiants incorrects.");
        }

        Console.WriteLine("\nAppuyez sur une touche pour revenir au menu...");
        Console.ReadKey();
    }
    static string LireTelephoneValide()
    {
        string? telephone;
        do
        {
            Console.Write("Téléphone (commence par 0 et 10 chiffres) : ");
            telephone = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(telephone) &&
                telephone.Length == 10 &&
                telephone.StartsWith('0') &&
                telephone.All(char.IsDigit))
            {
                return telephone;
            }

            Console.WriteLine("❌ Numéro invalide. Réessaye.");
        }
        while (true);
    }
    static int LireCodePostalValide()
    {
        string? saisie;
        do
        {
            Console.Write("Code postal (5 chiffres) : ");
            saisie = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(saisie) &&
                saisie.Length == 5 &&
                saisie.All(char.IsDigit))
            {
                return int.Parse(saisie);
            }

            Console.WriteLine("❌ Code postal invalide. Réessaye.");
        }
        while (true);
    }
    static string LireMotDePasseValide()
    {
        string? motDePasse;
        do
        {
            Console.Write("Mot de passe (min. 8 caractères, 1 majuscule, 1 minuscule, 1 chiffre) : ");
            motDePasse = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(motDePasse) &&
                motDePasse.Length >= 8 &&
                motDePasse.Any(char.IsUpper) &&
                motDePasse.Any(char.IsLower) &&
                motDePasse.Any(char.IsDigit))
            {
                return motDePasse;
            }

            Console.WriteLine("❌ Mot de passe trop faible !!");
        }
        while (true);
    }
    static void MenuCuisinier(int cuisinierId)
    {
        string[] options = {
            "Ajouter un plat",
            "Ajouter un plat du jour",
            "Voir tous vos plats",
            "Voir le plat du jour",
            "Voir vos plats réalisés par fréquence",
            "Voir vos clients servis",
            "Modifier les données du cuisinier",
            "Supprimer mon compte",
            "Lancer la livraison",
            "Noter le client",
            "⬅ Retour"
        };

        int selected = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Espace Cuisinier ===\n");

            for (int i = 0; i < options.Length; i++)
            {
                Console.Write(i == selected ? "👉 " : "   ");
                Console.ForegroundColor = (i == selected) ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.WriteLine(options[i]);
            }
            Console.ResetColor();

            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Length) % options.Length;
                    break;
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Length;
                    break;
                case ConsoleKey.Enter:
                    Console.Clear();
                    switch (selected)
                    {
                        case 0: // Ajouter un plat
                            AjouterPlat(cuisinierId, platDuJour: false);
                            break;

                        case 1: // Ajouter un plat du jour
                            AjouterPlat(cuisinierId, platDuJour: true);
                            break;

                        case 2: // Liste plats proposés par le cuisinier
                            var plats = PlatPropose.RecupererPlatsParCuisinier(cuisinierId);
                            Console.WriteLine("=== Vos plats proposés ===\n");
                            foreach (var p in plats)
                                Console.WriteLine($"🍽️ {p.Nom} - {p.PrixParPersonne}€ - {p.DateFabrication:dd/MM} → {p.DatePeremption:dd/MM}");
                            break;

                        case 3: // Liste plat du jour
                            var platDuJour = PlatPropose.RecuperePlatDuJour(cuisinierId);
                            Console.WriteLine("=== Plat du jour ===\n");
                            if (platDuJour != null)
                                Console.WriteLine($"🌟 {platDuJour.Nom} - {platDuJour.PrixParPersonne}€ - {platDuJour.DateFabrication:dd/MM}");
                            else
                                Console.WriteLine("Aucun plat du jour trouvé pour aujourd'hui.");
                            break;

                        case 4: // Plats préparés par fréquence
                            var freq = PreparerPlat.RecupereLesPlatsParFrequence(cuisinierId);
                            Console.WriteLine("=== Plats par fréquence ===\n");
                            foreach (var kv in freq)
                                Console.WriteLine($"📦 {kv.Key.Nom} : {kv.Value} commandes");
                            break;

                        case 5: // Liste clients servis
                            var clients = PreparerPlat.RecupereClientsServis(cuisinierId);
                            Console.WriteLine("=== Clients servis ===\n");
                            foreach (var c in clients)
                                Console.WriteLine($"👤 {c.Prenom} {c.Nom} - {c.Email}");
                            break;

                        case 6: // Modif des données du cuisinier
                            ModifierDonneesCuisinier(cuisinierId);
                            break;
                            
                        case 7: // Supprimer ton compte
                            Console.Write("\n⚠️ Es-tu sûr de vouloir supprimer ton compte ? (o/n) : ");
                            var confirmation = Console.ReadLine()?.ToLower();
                            if (confirmation == "o")
                            {
                                Utilisateur.SupprimerParId(cuisinierId);
                                Console.WriteLine("✅ Ton compte a bien été supprimé.");
                                Console.WriteLine("Appuie sur une touche pour quitter...");
                                Console.ReadKey();
                                return; // quitte le menu cuisinier
                            }
                            else
                            {
                                Console.WriteLine("❌ Suppression annulée.");
                                Console.ReadKey();
                            }
                            break;
                        case 8: // Lancer la livraison
                            LancerLivraison(cuisinierId);
                            break;

                        case 9: // Noter le client
                            NoterClient(cuisinierId);
                            break;

                        case 10: // Retourner au menu principal
                            return;
                    }
                    Console.WriteLine("\nAppuie sur une touche pour continuer...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    static void MenuClient(int clientId)
    {
        string[] options = {
            "Passer une commande",
            "Voir ses commandes",
            "Voir le prix de ses commandes",
            "Voir les statistiques de mes commandes",
            "Modifier les données du client",
            "Supprimer mon compte",
            "⬅ Retour"
        };

        int selected = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Espace Client ===\n");

            for (int i = 0; i < options.Length; i++)
            {
                Console.Write(i == selected ? "👉 " : "   ");
                Console.ForegroundColor = (i == selected) ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.WriteLine(options[i]);
            }
            Console.ResetColor();

            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Length) % options.Length;
                    break;
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Length;
                    break;
                case ConsoleKey.Enter:
                    Console.Clear();
                    switch (selected)
                    {
                        case 0: //Passer une commande
                            var platsDispos = PlatPropose.RecupererListeTousPlats();  // Récupère tous les plats dispo
                            var platsCommandes = new List<(PlatPropose plat, DateTime dateLivraison, string stationMetro, int quantite)>();
                            var selectionnes = new HashSet<int>();  // Pour éviter les sélections multiples du même plat --> demande lors de la commande le nb de plats toto nécessaire
                            int index = 0;
                            ConsoleKey touche;

                            while (true)
                            {
                                Console.Clear();
                                Console.WriteLine("=== Sélectionner vos plats ===");

                                // Affiche les plats disponibles
                                for (int i = 0; i < platsDispos.Count; i++)
                                {
                                    Console.Write(i == index ? "👉 " : "   ");
                                    Console.ForegroundColor = selectionnes.Contains(i) ? ConsoleColor.Green : ConsoleColor.Gray;
                                    Console.WriteLine($"{platsDispos[i].Nom} - {platsDispos[i].PrixParPersonne}€");
                                }
                                Console.ResetColor();

                                // Affiche à la fin de la liste ce qu'il faut faire pour terminer sa sélection de plats
                                Console.WriteLine("   Appuyez sur 'T' pour terminer la sélection");

                                // Navigation dans le menu
                                touche = Console.ReadKey(true).Key;
                                switch (touche)
                                {
                                    case ConsoleKey.UpArrow:
                                        index = (index - 1 + platsDispos.Count) % platsDispos.Count;
                                        break;
                                    case ConsoleKey.DownArrow:
                                        index = (index + 1) % platsDispos.Count;
                                        break;
                                    case ConsoleKey.Enter:
                                        // Sélectionner un plat
                                        if (!selectionnes.Contains(index))
                                        {
                                            Console.Clear();
                                            Console.WriteLine($"Pour le plat {platsDispos[index].Nom}, entre la date de livraison (jj-MM-aaaa) : ");
                                            DateTime dateLivraison = DateTime.Parse(Console.ReadLine() ?? DateTime.Today.ToString("yyyy-MM-dd"));
                                            
                                            Console.Write("Station de métro pour la livraison : ");
                                            string station = Console.ReadLine() ?? "";

                                            // Appel de la méthode ProposerStationSiNonExistante en passant station par référence
                                            ListeStationsMetro.ProposerStationSiNonExistante(ref station);

                                            // Ensuite, station contiendra la valeur correcte après la vérification ou proposition
                                            Console.WriteLine($"La station de métro choisie pour la livraison est : {station}");


                                            // Demande la quantité souhaitée pour le plat
                                            int quantite;
                                            while (true)
                                            {
                                                Console.Write($"Combien de {platsDispos[index].Nom} souhaitez-vous ? ");
                                                if (int.TryParse(Console.ReadLine(), out quantite) && quantite > 0)
                                                {
                                                    break;
                                                }
                                                else
                                                {
                                                    Console.WriteLine("⚠️ Quantité invalide. Veuillez entrer un nombre entier positif.");
                                                }
                                            }

                                            platsCommandes.Add((platsDispos[index], dateLivraison, station, quantite));
                                            selectionnes.Add(index);
                                            Console.WriteLine("✅ Plat ajouté !");
                                            Thread.Sleep(800);
                                        }
                                        else
                                        {
                                            Console.WriteLine("⚠️ Ce plat est déjà dans la commande.");
                                            Thread.Sleep(800);
                                        }
                                        break;
                                    case ConsoleKey.T:
                                        // Si l'utilisateur appuie sur "T", terminer la sélection
                                        if (platsCommandes.Count == 0)
                                        {
                                            Console.WriteLine("❌ Aucun plat sélectionné.");
                                            break;
                                        }

                                        decimal total = platsCommandes.Sum(p => p.plat.PrixParPersonne * p.quantite);
                                        Console.WriteLine($"\n Total à payer : {total}€");

                                        Console.Write(" Veux-tu procéder au paiement ? (o/n) - attention sans paiement la commande est annulée : ");
                                        string? paiement = Console.ReadLine()?.ToLower();

                                        if (paiement == "o")
                                        {
                                            // Ajouter la commande avant l'ajout des plats
                                            int commandeId = Commande.Ajouter(clientId, total);

                                            // Ajout des plats sélectionnés à la commande
                                            foreach (var commande in platsCommandes)
                                            {
                                                Commande.AjouterPlat(commandeId, commande.plat.PlatId, commande.dateLivraison, commande.stationMetro, commande.quantite);
                                            }
                                            Console.WriteLine("✅ Commande passée avec succès !");
                                            
                                            // Mise à jour du statut de la commande à "payée mais non livrée"
                                            Commande.MettreAJourStatutCommande(commandeId, "payée mais non livrée");

                                            // Demande d'un avis et d'une note après le paiement = fin de l'expérience utilisateur
                                            Console.WriteLine("\nMerci pour votre commande ! Veuillez laisser un avis : ");
                                            string avisClient = Console.ReadLine() ?? "";

                                            Console.WriteLine("Veuillez donner une note de 1 à 5 : ");
                                            decimal noteClient;
                                            while (!decimal.TryParse(Console.ReadLine(), out noteClient) || noteClient < 1 || noteClient > 5)
                                            {
                                                Console.WriteLine("⚠️ Veuillez entrer une note valide entre 1 et 5.");
                                            }

                                            // Mise à jour de l'avis et de la note dans la base de données
                                            Commande.MettreAJourAvisEtNote(commandeId, avisClient, noteClient);
                                        }
                                        else
                                        {
                                            Console.WriteLine("❌ Paiement annulé.");
                                        }
                                        return;  // Sortie du menu après avoir passé la commande 
                                }
                            }
                            break;
                        
                        case 1: // Commandes passées
                            var commandes = Commande.RecupereCommandesParClient(clientId);
                            Console.WriteLine("=== Vos commandes passées ===\n");

                            if (commandes.Count == 0)
                            {
                                Console.WriteLine("❌ Aucune commande trouvée.");
                            }
                            else
                            {
                                foreach (var commande in commandes)
                                {
                                    Console.WriteLine($"Commande #{commande.CommandeId} | Statut : {commande.Statut} | Total : {commande.PrixTotal} €");

                                    var plats = Commande.RecupererPlatsParCommande(commande.CommandeId);
                                    foreach (var (plat, date, station) in plats)
                                    {
                                        Console.WriteLine($"   {plat.Nom} - {plat.PrixParPersonne}€ | Livraison : {date:dd/MM} à {station}");
                                    }

                                    Console.WriteLine();
                                }
                            }

                            Console.WriteLine("Appuie sur une touche pour continuer...");
                            Console.ReadKey();
                            break;

                        case 2: // Statistiques prix commande
                            AfficherStatistiquesPrixCommandesClient(clientId);
                            break;

                        case 3: // Statistiques commande (moyenne note client, cuisinier etc)
                            AfficherStatistiquesCommandes(clientId);
                            break;

                        case 4: // Modifier les données du client
                            ModifierDonneesClient(clientId);
                            break;
                        case 5: // Suppression compte
                            Console.Write("⚠️ Es-tu sûr de vouloir supprimer ton compte ? (o/n) : ");
                            var confirm = Console.ReadLine()?.ToLower();
                            if (confirm == "o")
                            {
                                Utilisateur.SupprimerParId(clientId);
                                Console.WriteLine("✅ Ton compte a bien été supprimé.");
                                Console.WriteLine("Appuie sur une touche pour quitter...");
                                Console.ReadKey();
                                return;
                            }
                            else
                            {
                                Console.WriteLine("❌ Suppression annulée.");
                                Console.ReadKey();
                            }
                            break;

                        case 6:
                            return;
                    }
                    Console.WriteLine("\nAppuie sur une touche pour continuer...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    static void AjouterPlat(int cuisinierId, bool platDuJour = false)
    {
        Console.Clear();
        Console.WriteLine("=== Ajouter un plat ===");

        var plat = new PlatPropose();

        Console.Write("Nom du plat : ");
        plat.Nom = Console.ReadLine() ?? "";

        Console.Write("Nombre de personnes : ");
        plat.NbPersonnes = int.Parse(Console.ReadLine() ?? "0");

        if (platDuJour)
        {
            plat.DateFabrication = DateTime.Today;
        }
        else
        {
            Console.Write("Date de fabrication (jj-MM-aaaa) : ");
            plat.DateFabrication = DateTime.Parse(Console.ReadLine() ?? "");
        }

        Console.Write("Date de péremption (jj-MM-aaaa) : ");
        plat.DatePeremption = DateTime.Parse(Console.ReadLine() ?? "");

        Console.Write("Prix par personne : ");
        plat.PrixParPersonne = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("Chemin vers la photo : ");
        plat.Photos = Console.ReadLine() ?? "";

        // Affichage des recettes existantes
        var recettes = Recette.RecupereListeToutesRecettes();
        Console.WriteLine("\n=== Recettes disponibles ===");
        for (int i = 0; i < recettes.Count; i++)
        {
            Console.WriteLine($"{recettes[i].RecetteId} - {recettes[i].Nom}");
        }
        Console.WriteLine("0 - Nouvelle recette");

        Console.Write("\nChoisissez l’ID de la recette à associer : ");
        int recetteId = int.Parse(Console.ReadLine() ?? "0");

        if (recetteId == 0)
        {
            // Crée une nouvelle recette
            var nouvelleRecette = new Recette();
            Console.Write("Nom de la recette : ");
            nouvelleRecette.Nom = Console.ReadLine() ?? "";

            string[] typesPlat = { "Entrée", "Plat", "Dessert" };
            int typeIndex = 0;
            ConsoleKey keyType;

            do
            {
                Console.Clear();
                Console.WriteLine("=== Choisissez le type de plat ===\n");

                for (int i = 0; i < typesPlat.Length; i++)
                {
                    Console.Write(i == typeIndex ? "👉 " : "   ");
                    Console.ForegroundColor = (i == typeIndex) ? ConsoleColor.Cyan : ConsoleColor.Gray;
                    Console.WriteLine(typesPlat[i]);
                }
                Console.ResetColor();

                keyType = Console.ReadKey(true).Key;
                switch (keyType)
                {
                    case ConsoleKey.UpArrow:
                        typeIndex = (typeIndex - 1 + typesPlat.Length) % typesPlat.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        typeIndex = (typeIndex + 1) % typesPlat.Length;
                        break;
                }
            }
            while (keyType != ConsoleKey.Enter);
            nouvelleRecette.IdTypePlat = typeIndex + 1;
            
            string[] nationalites = {
                "Italienne",
                "Française",
                "Japonaise",
                "Chinoise",
                "Mexicaine",
                "Indienne",
                "Thaïlandaise",
                "Espagnole",
                "Turque",
                "Vietnamienne"
            };

            int nationaliteIndex = 0;
            ConsoleKey keyNat;
            do
            {
                Console.Clear();
                Console.WriteLine("=== Choisissez la nationalité de la recette ===\n");

                for (int i = 0; i < nationalites.Length; i++)
                {
                    Console.Write(i == nationaliteIndex ? "👉 " : "   ");
                    Console.ForegroundColor = (i == nationaliteIndex) ? ConsoleColor.Yellow : ConsoleColor.Gray;
                    Console.WriteLine(nationalites[i]);
                }
                Console.ResetColor();

                keyNat = Console.ReadKey(true).Key;
                switch (keyNat)
                {
                    case ConsoleKey.UpArrow:
                        nationaliteIndex = (nationaliteIndex - 1 + nationalites.Length) % nationalites.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        nationaliteIndex = (nationaliteIndex + 1) % nationalites.Length;
                        break;
                }
            } while (keyNat != ConsoleKey.Enter);
            string nomNationalite = nationalites[nationaliteIndex];
            nouvelleRecette.IdNationalite = Nationalite.AjouteNationaliteSiNexistePas(nomNationalite);

            // Ingrédients
            Console.Write("Nombre d’ingrédients : ");
            int nbIngr = int.Parse(Console.ReadLine() ?? "0");
            for (int i = 0; i < nbIngr; i++)
            {
                Console.Write($"Nom ingrédient {i + 1} : ");
                string nom = Console.ReadLine() ?? "";
                nouvelleRecette.Ingredients.Add(new Ingredient { Nom = nom });
            }

            // Régimes alimentaires
            Console.Write("Nombre de régimes alimentaires : ");
            int nbRegimes = int.Parse(Console.ReadLine() ?? "0");
            string[] regimesDisponibles = { "Sans gluten", "Viande Halal", "Végétarien", "Sans lactose", "Sans arachides", "Végan" };
            var regimesChoisis = new List<string>();
            int regimeIndex = 0;

            for (int i = 0; i < nbRegimes; i++)
            {
                ConsoleKey key;
                do
                {
                    Console.Clear();
                    Console.WriteLine($"=== Choix du régime alimentaire n°{i + 1} ===");
                    for (int j = 0; j < regimesDisponibles.Length; j++)
                    {
                        if (j == regimeIndex)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("👉 ");
                        }
                        else
                        {
                            Console.Write("   ");
                            Console.ResetColor();
                        }
                        Console.WriteLine(regimesDisponibles[j]);
                    }
                    Console.ResetColor();

                    key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            regimeIndex = (regimeIndex - 1 + regimesDisponibles.Length) % regimesDisponibles.Length;
                            break;
                        case ConsoleKey.DownArrow:
                            regimeIndex = (regimeIndex + 1) % regimesDisponibles.Length;
                            break;
                        case ConsoleKey.Enter:
                            string choix = regimesDisponibles[regimeIndex];
                            if (!regimesChoisis.Contains(choix))
                            {
                                regimesChoisis.Add(choix);
                                Console.WriteLine($"\n✅ {choix} ajouté !");
                            }
                            else
                            {
                                Console.WriteLine("\n⚠️ Ce régime a déjà été sélectionné.");
                                i--; // pour recommencer ce tour
                            }
                            Thread.Sleep(1000); // petite pause visuelle
                            break;
                    }
                } while (key != ConsoleKey.Enter);
            }
            recetteId = nouvelleRecette.Ajouter();
        }
        plat.RecetteId = recetteId;
        int platId = plat.Ajouter();

        var lien = new PreparerPlat { UtilisateurId = cuisinierId, PlatId = platId };
        lien.Ajouter();

        Console.WriteLine("\n✅ Plat ajouté avec succès !");
        Console.WriteLine("\n=== Vos plats mis à jour ===\n");
        var plats = PlatPropose.RecupererPlatsParCuisinier(cuisinierId);
        foreach (var p in plats)
            Console.WriteLine($" {p.Nom} - {p.PrixParPersonne}€ - {p.DateFabrication:dd/MM} → {p.DatePeremption:dd/MM}");
        Console.WriteLine("Appuie sur une touche pour continuer...");
        Console.ReadKey();
    }
    static void AfficherStatistiquesPrixCommandesClient(int clientId)
    {
        Console.Clear();
        Console.WriteLine("=== Statistiques de vos commandes ===\n");

        var commandes = Commande.RecupereCommandesParClient(clientId);

        if (commandes.Count == 0)
        {
            Console.WriteLine("❌ Vous n'avez passé aucune commande.");
        }
        else
        {
            int nombreCommandes = commandes.Count;
            decimal moyenne = Commande.CalculerMoyennePrixParClient(clientId);
            var commandeMax = commandes.OrderByDescending(c => c.PrixTotal).First();

            Console.WriteLine($"Nombre total de commandes : {nombreCommandes}");
            Console.WriteLine($"Moyenne des prix des commandes : {moyenne:F2} €");
            Console.WriteLine($"Commande la plus chère : Commande #{commandeMax.CommandeId} avec un total de {commandeMax.PrixTotal:F2} €");
        }

        Console.WriteLine("\nAppuyez sur une touche pour revenir...");
        Console.ReadKey();
    }
    public static void AfficherStatistiquesCommandes(int utilisateurId)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        // Nombre de commandes par nationalité
        string query1 = @" SELECT n.nationalite_plat AS nationalite_plat, COUNT(*) AS NombreDeCommandes  FROM Commande c JOIN PlatPropose p ON c.commande_id = p.plat_id JOIN Recette r ON p.recette_id = r.recette_id  JOIN Nationalite n ON r.nationalite_id_plat = n.nationalite_id_plat  WHERE c.utilisateur_id = @UtilisateurId  GROUP BY n.nationalite_plat";
        using (var cmd1 = new MySqlCommand(query1, conn))
        {
            cmd1.Parameters.AddWithValue("@UtilisateurId", utilisateurId);

            using var reader1 = cmd1.ExecuteReader();
            Console.WriteLine("=== Nombre de commandes par nationalité ===");
            while (reader1.Read())
            {
                string nationalite = reader1.GetString("nationalite_plat");
                int nombreCommandes = reader1.GetInt32("NombreDeCommandes");
                Console.WriteLine($"{nationalite} : {nombreCommandes} commandes");
            }
        }

        // Demande à l'utilisateur de saisir la période sur laquelle il veut filtrer les commandes
        DateTime dateDebut;
        DateTime dateFin;

        // Saisie de la date de début
        while (true)
        {
            Console.Write("\nVeuillez entrer la date de début (yyyy-MM-dd) : ");
            string? dateDebutStr = Console.ReadLine();
            if (DateTime.TryParse(dateDebutStr, out dateDebut))
            {
                break;
            }
            else
            {
                Console.WriteLine("❌ Format de date invalide. Réessayez.");
            }
        }

        // Saisie de la date de fin
        while (true)
        {
            Console.Write("\nVeuillez entrer la date de fin (yyyy-MM-dd) : ");
            string? dateFinStr = Console.ReadLine();
            if (DateTime.TryParse(dateFinStr, out dateFin))
            {
                break;
            }
            else
            {
                Console.WriteLine("❌ Format de date invalide. Réessayez.");
            }
        }

        // Nombre de commandes sur la période donnée par l'utilisateur
        string query2 = @"SELECT COUNT(*) AS NombreDeCommandes FROM Commande c JOIN ElementCommande ec ON c.commande_id = ec.commande_id WHERE c.utilisateur_id = @UtilisateurId   AND ec.commande_date BETWEEN @DateDebut AND @DateFin"; // Utilisation de la colonne commande_date dans ElementCommande

        using (var cmd2 = new MySqlCommand(query2, conn))
        {
            cmd2.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
            cmd2.Parameters.AddWithValue("@DateDebut", dateDebut); // Date de début entrée par l'utilisateur
            cmd2.Parameters.AddWithValue("@DateFin", dateFin); // Date de fin entrée par l'utilisateur

            var nombreCommandesPeriodiques = Convert.ToInt32(cmd2.ExecuteScalar());
            Console.WriteLine($"\nNombre de commandes entre {dateDebut:yyyy-MM-dd} et {dateFin:yyyy-MM-dd} : {nombreCommandesPeriodiques}");
        }

        // Moyenne des notes données par le client aux cuisiniers
        string query3 = @"SELECT AVG(commande_notecuisinier) AS MoyenneNotesCuisinier FROM Commande WHERE utilisateur_id = @UtilisateurId";

        using (var cmd3 = new MySqlCommand(query3, conn))
        {
            cmd3.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
            var moyenneNotesCuisinier = cmd3.ExecuteScalar();
            Console.WriteLine($"Moyenne des notes données par le client aux cuisiniers : {moyenneNotesCuisinier}");
        }

        // Moyenne des notes données par les cuisiniers au client
        string query4 = @"SELECT AVG(commande_noteclient) AS MoyenneNotesClient  FROM Commande  WHERE utilisateur_id = @UtilisateurId";

        using (var cmd4 = new MySqlCommand(query4, conn))
        {
            cmd4.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
            var moyenneNotesClient = cmd4.ExecuteScalar();
            Console.WriteLine($"Moyenne des notes données par les cuisiniers au client : {moyenneNotesClient}");
        }

        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
        Console.ReadKey();
    }
    static void ModifierDonneesCuisinier(int cuisinierId)
    {
        var utilisateur = Utilisateur.RecupererParId(cuisinierId); // Récupère les info du cuisinier
        Console.Clear();
        Console.WriteLine("=== Modifier les données du cuisinier ===\n");

        // Affichage des données actuelles --> permet de les modifier
        Console.WriteLine($"Nom actuel : {utilisateur.Nom}");
        Console.Write("Nouveau nom (laisser vide pour conserver l'actuel) : ");
        string nouveauNom = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauNom)) utilisateur.Nom = nouveauNom;

        Console.WriteLine($"Prénom actuel : {utilisateur.Prenom}");
        Console.Write("Nouveau prénom (laisser vide pour conserver l'actuel) : ");
        string nouveauPrenom = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauPrenom)) utilisateur.Prenom = nouveauPrenom;

        Console.WriteLine($"Téléphone actuel : {utilisateur.Telephone}");
        Console.Write("Nouveau téléphone (laisser vide pour conserver l'actuel) : ");
        string nouveauTelephone = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauTelephone)) utilisateur.Telephone = nouveauTelephone;

        Console.WriteLine($"Email actuel : {utilisateur.Email}");
        Console.Write("Nouveau email (laisser vide pour conserver l'actuel) : ");
        string nouveauEmail = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauEmail)) utilisateur.Email = nouveauEmail;

        Console.WriteLine($"Adresse actuelle : {utilisateur.Rue} {utilisateur.NumRue}, {utilisateur.Ville} {utilisateur.CodePostal}");
        Console.Write("Nouvelle adresse (laisser vide pour conserver l'actuel) : ");
        string nouvelleAdresse = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouvelleAdresse)) utilisateur.Rue = nouvelleAdresse;

        Console.WriteLine($"Station métro actuelle : {utilisateur.MetroProche}");
        Console.Write("Nouvelle station métro (laisser vide pour conserver l'actuel) : ");
        string nouvelleStationMetro = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouvelleStationMetro)) utilisateur.MetroProche = nouvelleStationMetro;

        // Mise à jour de l'utilisateur dans la BDD
        try
        {
            // Mise à jour dans la BDD
            using var conn = new MySqlConnection(connectionString);
            string query = @"UPDATE Utilisateur SET  utilisateur_nom = @Nom,  utilisateur_prenom = @Prenom, utilisateur_telephone = @Telephone, utilisateur_email = @Email, utilisateur_rue = @Rue,  utilisateur_num_rue = @NumRue,  utilisateur_codepostal = @CodePostal,  utilisateur_ville = @Ville,  utilisateur_metroproche = @Metro  WHERE utilisateur_id = @UtilisateurId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nom", utilisateur.Nom);
            cmd.Parameters.AddWithValue("@Prenom", utilisateur.Prenom);
            cmd.Parameters.AddWithValue("@Telephone", utilisateur.Telephone);
            cmd.Parameters.AddWithValue("@Email", utilisateur.Email);
            cmd.Parameters.AddWithValue("@Rue", utilisateur.Rue);
            cmd.Parameters.AddWithValue("@NumRue", utilisateur.NumRue);
            cmd.Parameters.AddWithValue("@CodePostal", utilisateur.CodePostal);
            cmd.Parameters.AddWithValue("@Ville", utilisateur.Ville);
            cmd.Parameters.AddWithValue("@Metro", utilisateur.MetroProche);
            cmd.Parameters.AddWithValue("@UtilisateurId", utilisateur.UtilisateurId);

            conn.Open();
            cmd.ExecuteNonQuery();
            
            Console.WriteLine("\n✅ Données modifiées avec succès !");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Erreur lors de la mise à jour des données : " + ex.Message);
        }

        Console.WriteLine("\nAppuyez sur une touche pour retourner au menu...");
        Console.ReadKey();
    }
    static void ModifierDonneesClient(int clientId)
    {
        var utilisateur = Utilisateur.RecupererParId(clientId);  // Récupère les infos du client
        Console.Clear();
        Console.WriteLine("=== Modifier les données du client ===\n");

        // Affichage des données actuelles --> permet de les modifier
        Console.WriteLine($"Nom actuel : {utilisateur.Nom}");
        Console.Write("Nouveau nom (laisser vide pour conserver l'actuel) : ");
        string nouveauNom = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauNom)) utilisateur.Nom = nouveauNom;

        Console.WriteLine($"Prénom actuel : {utilisateur.Prenom}");
        Console.Write("Nouveau prénom (laisser vide pour conserver l'actuel) : ");
        string nouveauPrenom = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauPrenom)) utilisateur.Prenom = nouveauPrenom;

        Console.WriteLine($"Téléphone actuel : {utilisateur.Telephone}");
        Console.Write("Nouveau téléphone (laisser vide pour conserver l'actuel) : ");
        string nouveauTelephone = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauTelephone)) utilisateur.Telephone = nouveauTelephone;

        Console.WriteLine($"Email actuel : {utilisateur.Email}");
        Console.Write("Nouveau email (laisser vide pour conserver l'actuel) : ");
        string nouveauEmail = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouveauEmail)) utilisateur.Email = nouveauEmail;

        Console.WriteLine($"Adresse actuelle : {utilisateur.Rue} {utilisateur.NumRue}, {utilisateur.Ville} {utilisateur.CodePostal}");
        Console.Write("Nouvelle adresse (laisser vide pour conserver l'actuel) : ");
        string nouvelleAdresse = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouvelleAdresse)) utilisateur.Rue = nouvelleAdresse;

        Console.WriteLine($"Station métro actuelle : {utilisateur.MetroProche}");
        Console.Write("Nouvelle station métro (laisser vide pour conserver l'actuel) : ");
        string nouvelleStationMetro = Console.ReadLine() ?? "";
        if (!string.IsNullOrEmpty(nouvelleStationMetro)) utilisateur.MetroProche = nouvelleStationMetro;

        // Mise à jour de l'utilisateur dans la BDD
        try
        {
            // Mise à jour dans la BDD
            using var conn = new MySqlConnection(connectionString);
            string query = @"UPDATE Utilisateur SET  utilisateur_nom = @Nom,  utilisateur_prenom = @Prenom, utilisateur_telephone = @Telephone, utilisateur_email = @Email, utilisateur_rue = @Rue,  utilisateur_num_rue = @NumRue,  utilisateur_codepostal = @CodePostal,  utilisateur_ville = @Ville,  utilisateur_metroproche = @Metro  WHERE utilisateur_id = @UtilisateurId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nom", utilisateur.Nom);
            cmd.Parameters.AddWithValue("@Prenom", utilisateur.Prenom);
            cmd.Parameters.AddWithValue("@Telephone", utilisateur.Telephone);
            cmd.Parameters.AddWithValue("@Email", utilisateur.Email);
            cmd.Parameters.AddWithValue("@Rue", utilisateur.Rue);
            cmd.Parameters.AddWithValue("@NumRue", utilisateur.NumRue);
            cmd.Parameters.AddWithValue("@CodePostal", utilisateur.CodePostal);
            cmd.Parameters.AddWithValue("@Ville", utilisateur.Ville);
            cmd.Parameters.AddWithValue("@Metro", utilisateur.MetroProche);
            cmd.Parameters.AddWithValue("@UtilisateurId", utilisateur.UtilisateurId);

            conn.Open();
            cmd.ExecuteNonQuery();

            Console.WriteLine("\n✅ Données modifiées avec succès !");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ Erreur lors de la mise à jour des données : " + ex.Message);
        }

        Console.WriteLine("\nAppuyez sur une touche pour retourner au menu...");
        Console.ReadKey();
    }
    /*public static void LancerLivraison(int cuisinierId)
{
    // Récupérer toutes les commandes non encore livrées
    var commandesEnCours = Commande.RecupereCommandesParCuisinierEtStatut(cuisinierId, "payée mais non livrée");

    Console.Clear();
    Console.WriteLine("=== Lancer la livraison ===\n");

    if (commandesEnCours.Count == 0)
    {
        Console.WriteLine("❌ Aucune commande à livrer.");
        return;
    }

    for (int i = 0; i < commandesEnCours.Count; i++)
    {
        Console.WriteLine($"{i + 1}. Commande #{commandesEnCours[i].CommandeId} | Client : {commandesEnCours[i].UtilisateurId}");
    }

    Console.Write("Sélectionner une commande à livrer : ");
    int choix = int.Parse(Console.ReadLine() ?? "0");

    if (choix > 0 && choix <= commandesEnCours.Count)
    {
        int commandeId = commandesEnCours[choix - 1].CommandeId;

        // Récupérer la station de métro du cuisinier
        string stationMetroCuisinier = Utilisateur.RecupererParId(cuisinierId).MetroProche;

        // Récupérer la station de métro du client à partir des éléments de la commande
        var elementCommande = ElementCommande.RecupererElementCommandeParCommandeId(commandeId).FirstOrDefault();
        string stationMetroClient = elementCommande?.StationMetro ?? ""; // Si aucun élément, on retourne une chaîne vide

        if (string.IsNullOrEmpty(stationMetroClient))
        {
            Console.WriteLine("❌ La station de métro du client n'a pas été trouvée.");
            return;
        }

        // Créez le graphe avec les stations du métro (exemple simplifié)
        var graph = new Graphe<string>( Ajoutez ici le nombre de stations ou la logique d'initialisation );

        // Ajoutez les stations et leurs connexions dans le graphe ici, en utilisant vos données de la base de données
        // Par exemple : graph.AjouterNoeud("Station A");
        // Et ajoutez les liens avec graph.AjouterLien("Station A", "Station B", poids);

        // Utilisez l'algorithme de Dijkstra pour obtenir les distances entre les stations
        var distances = graph.Dijkstra(stationMetroCuisinier);

        if (distances.ContainsKey(stationMetroClient))
        {
            double tempsDeTrajet = distances[stationMetroClient]; // En minutes ou en distance (à adapter)

            // Capture l'heure de début du lancement de la livraison
            DateTime heureDeDebut = DateTime.Now;

            // Affichage de l'heure de début de la livraison
            Console.WriteLine($"⏱ Heure de lancement : {heureDeDebut}");

            // Mise à jour de l'heure de début dans la base de données pour la commande
            Commande.MettreAJourHeureLancementLivraison(commandeId, heureDeDebut);

            // Attendre que le temps de trajet soit écoulé (simulation de l'attente)
            double tempsEcoule = (DateTime.Now - heureDeDebut).TotalMinutes;

            if (tempsEcoule >= tempsDeTrajet)
            {
                // Mettre à jour le statut de la commande à "livrée"
                Commande.MettreAJourStatutCommande(commandeId, "livrée");

                Console.WriteLine($"✅ Livraison de la commande #{commandeId} lancée et terminée.");
            }
            else
            {
                Console.WriteLine("❌ Le temps de livraison n'est pas encore écoulé.");
            }
        }
        else
        {
            Console.WriteLine("❌ Impossible de calculer le temps de trajet entre les stations.");
        }
    }
    else
    {
        Console.WriteLine("❌ Sélection invalide.");
    }

    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
    Console.ReadKey();
}*/
    /*static void LancerLivraison(int cuisinierId)
{
    // Récupérer toutes les commandes non encore livrées
    var commandesEnCours = Commande.RecupereCommandesParCuisinierEtStatut(cuisinierId, "payée mais non livrée");

    Console.Clear();
    Console.WriteLine("=== Lancer la livraison ===\n");

    if (commandesEnCours.Count == 0)
    {
        Console.WriteLine("❌ Aucune commande à livrer.");
        return;
    }

    for (int i = 0; i < commandesEnCours.Count; i++)
    {
        Console.WriteLine($"{i + 1}. Commande #{commandesEnCours[i].CommandeId} | Client : {commandesEnCours[i].UtilisateurId}");
    }

    Console.Write("Sélectionner une commande à livrer : ");
    int choix = int.Parse(Console.ReadLine() ?? "0");

    if (choix > 0 && choix <= commandesEnCours.Count)
    {
        int commandeId = commandesEnCours[choix - 1].CommandeId;

        // Récupérer la station de métro du cuisinier
        string stationMetroCuisinier = Utilisateur.RecupererParId(cuisinierId).MetroProche;

        // Récupérer la station de métro du client à partir des éléments de la commande
        var elementCommande = ElementCommande.RecupererElementCommandeParCommandeId(commandeId).FirstOrDefault();
        string stationMetroClient = elementCommande?.StationMetro ?? ""; // Si aucun élément, on retourne une chaîne vide

        if (string.IsNullOrEmpty(stationMetroClient))
        {
            Console.WriteLine("❌ La station de métro du client n'a pas été trouvée.");
            return;
        }

        // Charger le graphe à partir du fichier Excel
        string cheminExcel = @"C:\Users\guill\RiderProjects\LivinParis_V3\Graph\bin\Debug\net8.0\MetroParis (4).xlsx";
        var graphe = ChargementGraphe.ChargerGrapheDepuisExcel2(cheminExcel);

        // Utilisez l'algorithme de Dijkstra pour obtenir les distances entre les stations
        var distances = graphe.Dijkstra(stationMetroCuisinier);

        if (distances.ContainsKey(stationMetroClient))
        {
            double tempsDeTrajet = distances[stationMetroClient]; // En minutes ou en distance (à adapter)

            // Capture l'heure de début du lancement de la livraison
            DateTime heureDeDebut = DateTime.Now;

            // Affichage de l'heure de début de la livraison
            Console.WriteLine($"⏱ Heure de lancement : {heureDeDebut}");

            // Mise à jour de l'heure de début dans la base de données pour la commande
            Commande.MettreAJourHeureLancementLivraison(commandeId, heureDeDebut);

            // Attendre que le temps de trajet soit écoulé (simulation de l'attente)
            double tempsEcoule = (DateTime.Now - heureDeDebut).TotalMinutes;

            if (tempsEcoule >= tempsDeTrajet)
            {
                // Mettre à jour le statut de la commande à "livrée"
                Commande.MettreAJourStatutCommande(commandeId, "livrée");

                Console.WriteLine($"✅ Livraison de la commande #{commandeId} lancée et terminée.");
            }
            else
            {
                Console.WriteLine("❌ Le temps de livraison n'est pas encore écoulé.");
            }
        }
        else
        {
            Console.WriteLine("❌ Impossible de calculer le temps de trajet entre les stations.");
        }
    }
    else
    {
        Console.WriteLine("❌ Sélection invalide.");
    }

    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
    Console.ReadKey();
}*/
    /*static void LancerLivraison(int cuisinierId) 
{
    // Récupérer toutes les commandes non encore livrées
    var commandesEnCours = Commande.RecupereCommandesParCuisinierEtStatut(cuisinierId, "payée mais non livrée");

    Console.Clear();
    Console.WriteLine("=== Lancer la livraison ===\n");

    if (commandesEnCours.Count == 0)
    {
        Console.WriteLine("❌ Aucune commande à livrer.");
        return;
    }

    // Afficher toutes les commandes disponibles
    for (int i = 0; i < commandesEnCours.Count; i++)
    {
        Console.WriteLine($"{i + 1}. Commande #{commandesEnCours[i].CommandeId} | Client : {commandesEnCours[i].UtilisateurId}");
    }

    // Demander à l'utilisateur de sélectionner une commande à livrer
    int choix;
    while (true)
    {
        Console.Write("Sélectionner une commande à livrer (entrer le numéro de commande) : ");
        string input = Console.ReadLine();

        // Vérifier si l'entrée est un nombre et si ce nombre est dans la plage valide
        if (int.TryParse(input, out choix) && choix > 0 && choix <= commandesEnCours.Count)
        {
            break;  // Sortir de la boucle si l'entrée est valide
        }
        else
        {
            Console.WriteLine("❌ Sélection invalide. Veuillez entrer un numéro de commande valide.");
        }
    }

    int commandeId = commandesEnCours[choix - 1].CommandeId;

    // Récupérer la station de métro du cuisinier
    string stationMetroCuisinier = Utilisateur.RecupererParId(cuisinierId).MetroProche;

    // Récupérer la station de métro du client à partir des éléments de la commande
    var elementCommande = ElementCommande.RecupererElementCommandeParCommandeId(commandeId).FirstOrDefault();
    string stationMetroClient = elementCommande?.StationMetro ?? ""; // Si aucun élément, on retourne une chaîne vide

    if (string.IsNullOrEmpty(stationMetroClient))
    {
        Console.WriteLine("❌ La station de métro du client n'a pas été trouvée.");
        return;
    }

    // Charger le graphe à partir du fichier Excel
    string cheminExcel = @"C:\Users\guill\RiderProjects\LivinParis_V3\Graph\bin\Debug\net8.0\MetroParis (4).xlsx";
    var graphe = ChargementGraphe.ChargerGrapheDepuisExcel2(cheminExcel);

    // Utilisez l'algorithme de Dijkstra pour obtenir les distances entre les stations
    var distances = graphe.Dijkstra(stationMetroCuisinier);

    if (distances.ContainsKey(stationMetroClient))
    {
        double tempsDeTrajet = distances[stationMetroClient]; // En minutes ou en distance (à adapter)

        // Capture l'heure de début du lancement de la livraison
        DateTime heureDeDebut = DateTime.Now;

        // Affichage de l'heure de début de la livraison
        Console.WriteLine($"⏱ Heure de lancement : {heureDeDebut}");

        // Mise à jour de l'heure de début dans la base de données pour la commande
        Commande.MettreAJourHeureLancementLivraison(commandeId, heureDeDebut);

        // Attendre que le temps de trajet soit écoulé (simulation de l'attente)
        double tempsEcoule = (DateTime.Now - heureDeDebut).TotalMinutes;

        if (tempsEcoule >= tempsDeTrajet)
        {
            // Mettre à jour le statut de la commande à "livrée"
            Commande.MettreAJourStatutCommande(commandeId, "livrée");

            Console.WriteLine($"✅ Livraison de la commande #{commandeId} lancée et terminée.");
        }
        else
        {
            Console.WriteLine("❌ Le temps de livraison n'est pas encore écoulé.");
        }
    }
    else
    {
        Console.WriteLine("❌ Impossible de calculer le temps de trajet entre les stations.");
    }

    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
    Console.ReadKey();
}*/

static void LancerLivraison(int cuisinierId)
{
    // Charger le fichier Excel avec ClosedXML
    string cheminExcel = @"C:\Users\guill\RiderProjects\LivinParis_V3\Graph\bin\Debug\net8.0\MetroParis (4).xlsx";
    var workbook = new XLWorkbook(cheminExcel);
    
    // Récupérer la première feuille du fichier Excel
    var worksheet = workbook.Worksheet(1);

    // Charger les données du graphe depuis le fichier Excel (ici, la station et ses liens)
    var graphData = worksheet.RowsUsed().Skip(1)
        .Select(row =>
        {
            string station = row.Cell(1).Value.ToString();
            string lien = row.Cell(2).Value.ToString();
            double poids = 0;

            try
            {
                // Tenter de convertir la valeur en double (poids / temps de trajet)
                poids = Convert.ToDouble(row.Cell(3).Value);
            }
            catch (InvalidCastException)
            {
                // Si la conversion échoue, on peut ignorer ou gérer autrement
                Console.WriteLine($"Avertissement : Impossible de convertir la cellule en nombre pour la station {station}.");
            }

            return new
            {
                Station = station,
                Lien = lien,
                Poids = poids
            };
        }).ToList();

    // Construire la liste d'adjacence pour le graphe
    var listeAdjacence = new Dictionary<string, List<(string Destination, double Poids)>>();

    foreach (var data in graphData)
    {
        if (!listeAdjacence.ContainsKey(data.Station))
        {
            listeAdjacence[data.Station] = new List<(string, double)>();
        }

        // Ajout des voisins
        listeAdjacence[data.Station].Add((data.Lien, data.Poids));
    }

    // Récupérer toutes les commandes non encore livrées
    var commandesEnCours = Commande.RecupereCommandesParCuisinierEtStatut(cuisinierId, "payée mais non livrée");

    Console.Clear();
    Console.WriteLine("=== Lancer la livraison ===\n");

    if (commandesEnCours.Count == 0)
    {
        Console.WriteLine("❌ Aucune commande à livrer.");
        return;
    }

    // Afficher toutes les commandes disponibles
    for (int i = 0; i < commandesEnCours.Count; i++)
    {
        Console.WriteLine($"{i + 1}. Commande #{commandesEnCours[i].CommandeId} | Client : {commandesEnCours[i].UtilisateurId}");
    }

    // Demander à l'utilisateur de sélectionner une commande à livrer
    int choix;
    while (true)
    {
        Console.WriteLine("=== Liste de toutes les commandes ===");
        List<Commande> toutesLesCommandes = ToutesLesCommandes();
        // Afficher toutes les commandes avec leur ID et leur client
        for (int i = 0; i < toutesLesCommandes.Count; i++) // toutesLesCommandes représente la liste de toutes les commandes
        {
            Console.WriteLine($"{i + 1}. Commande #{toutesLesCommandes[i].CommandeId} | Client : {toutesLesCommandes[i].UtilisateurId}");
        }

        Console.Write("Sélectionner une commande à livrer (entrer le numéro de commande) : ");
        string input = Console.ReadLine();

        // Vérifier si l'entrée est un nombre et si ce nombre est dans la plage valide des indices
        if (int.TryParse(input, out choix) && choix > 0 && choix <= toutesLesCommandes.Count)
        {
            // Récupérer l'ID de la commande choisie
            int commandelId = toutesLesCommandes[choix - 1].CommandeId;

            // Si l'ID est valide, sortir de la boucle
            break;  
        }
        else
        {
            Console.WriteLine("❌ Sélection invalide. Veuillez entrer un numéro de commande valide.");
        }
    }


    // Utiliser l'indice corrigé pour obtenir la commande
    int commandeId = commandesEnCours[choix - 1].CommandeId;

    // Récupérer la station de métro du cuisinier
    string stationMetroCuisinier = Utilisateur.RecupererParId(cuisinierId).MetroProche;

    // Récupérer la station de métro du client à partir des éléments de la commande
    var elementCommande = ElementCommande.RecupererElementCommandeParCommandeId(commandeId).FirstOrDefault();
    string stationMetroClient = elementCommande?.StationMetro ?? ""; // Si aucun élément, on retourne une chaîne vide

    if (string.IsNullOrEmpty(stationMetroClient))
    {
        Console.WriteLine("❌ La station de métro du client n'a pas été trouvée.");
        return;
    }

    // Exécuter l'algorithme de Dijkstra pour calculer les temps de trajet
    var distances = Dijkstra(stationMetroCuisinier, listeAdjacence);

    if (distances.ContainsKey(stationMetroClient))
    {
        double tempsDeTrajet = distances[stationMetroClient]; // Le temps de trajet en minutes ou en distance

        // Capture l'heure de début du lancement de la livraison
        DateTime heureDeDebut = DateTime.Now;

        // Affichage de l'heure de début de la livraison
        Console.WriteLine($"⏱ Heure de lancement : {heureDeDebut}");

        // Mise à jour de l'heure de début dans la base de données pour la commande
        Commande.MettreAJourHeureLancementLivraison(commandeId, heureDeDebut);

        // Attendre que le temps de trajet soit écoulé (simulation de l'attente)
        double tempsEcoule = (DateTime.Now - heureDeDebut).TotalMinutes;

        if (tempsEcoule >= tempsDeTrajet)
        {
            // Mettre à jour le statut de la commande à "livrée"
            Commande.MettreAJourStatutCommande(commandeId, "livrée");

            Console.WriteLine($"✅ Livraison de la commande #{commandeId} lancée et terminée.");
        }
        else
        {
            Console.WriteLine("❌ Le temps de livraison n'est pas encore écoulé.");
        }
    }
    else
    {
        Console.WriteLine("❌ Impossible de calculer le temps de trajet entre les stations.");
    }

    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
    Console.ReadKey();
}

// Méthode Dijkstra sans classe, directement utilisée dans la méthode LancerLivraison
/*public static Dictionary<string, double> Dijkstra(string source, Dictionary<string, List<(string Destination, double Poids)>> listeAdjacence)
{
    var distances = new Dictionary<string, double>(); // Stocke la distance minimale
    var parents = new Dictionary<string, string>();   // Pour reconstruire le chemin
    var priorityQueue = new SortedDictionary<double, string>(); // File de priorité

    // Initialisation
    foreach (var noeud in listeAdjacence.Keys)
    {
        distances[noeud] = double.MaxValue; // Distance infinie
        parents[noeud] = null;              // Pas de parent initialement
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

    return distances; // Retourne les distances minimales
}*/
    public static Dictionary<string, double> Dijkstra(string source, Dictionary<string, List<(string Destination, double Poids)>> listeAdjacence)
    {
        var distances = new Dictionary<string, double>(); // Stocke la distance minimale
        var parents = new Dictionary<string, string>();   // Pour reconstruire le chemin
        var priorityQueue = new SortedDictionary<double, string>(); // File de priorité

        // Initialisation
        foreach (var noeud in listeAdjacence.Keys)
        {
            distances[noeud] = double.MaxValue; // Distance infinie
            parents[noeud] = null;              // Pas de parent initialement
        }
        distances[source] = 0;  // La distance de la source à elle-même est 0
        priorityQueue[0] = source;

        while (priorityQueue.Count > 0)
        {
            var currentNode = priorityQueue.First().Value;
            priorityQueue.Remove(priorityQueue.First().Key);

            // Vérifier si la station existe dans le dictionnaire avant d'accéder à ses voisins
            if (listeAdjacence.ContainsKey(currentNode))
            {
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
            else
            {
                Console.WriteLine($"❌ La station {currentNode} n'existe pas dans le graphe.");
            }
        }

        return distances; // Retourne les distances minimales
    }
    // Méthode pour récupérer toutes les commandes depuis la base de données
        public static List<Commande> RecupererToutesLesCommandes()
        {
            List<Commande> commandes = new List<Commande>();

            // Connexion à la base de données MySQL
            string connectionString = "Server=localhost;Database=LivinParis;User ID=root;Password=;"; // Remplace avec tes infos de connexion

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Commande WHERE Statut != 'livrée'"; // Filtrer pour ne récupérer que les commandes non livrées
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var commande = new Commande
                            {
                                CommandeId = reader.GetInt32("CommandeId"),
                                PrixTotal = reader.GetDecimal("PrixTotal"),
                                Statut = reader.GetString("Statut"),
                                AvisClient = reader.GetString("AvisClient"),
                                NoteClient = reader.GetDecimal("NoteClient"),
                                NoteCuisinier = reader.GetDecimal("NoteCuisinier"),
                                UtilisateurId = reader.GetInt32("UtilisateurId")
                            };
                            commandes.Add(commande);
                        }
                    }
                }
            }

            return commandes;
        }

        // Méthode pour mettre à jour le statut d'une commande à "livrée"
        public static void MettreAJourStatutCommande(int commandeId, string nouveauStatut)
        {
            string connectionString = "Server=localhost;Database=LivinParis;User ID=root;Password=;"; // Remplace avec tes infos de connexion

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Commande SET Statut = @Statut WHERE CommandeId = @CommandeId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Statut", nouveauStatut);
                    cmd.Parameters.AddWithValue("@CommandeId", commandeId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Méthode pour lancer la livraison
        public static void LancerLivraison()
        {
            // Récupérer toutes les commandes non livrées
            var commandes = RecupererToutesLesCommandes();

            if (commandes.Count == 0)
            {
                Console.WriteLine("❌ Aucune commande à livrer.");
                return;
            }

            // Afficher la liste des commandes
            Console.WriteLine("=== Liste des commandes à livrer ===");
            foreach (var commande in commandes)
            {
                Console.WriteLine($"{commande.CommandeId}. Commande #{commande.CommandeId} | Client : {commande.UtilisateurId} | Statut : {commande.Statut}");
            }

            // Demander au cuisinier de sélectionner une commande
            Console.Write("Sélectionner une commande à livrer (entrer le numéro de commande) : ");
            int choixCommande;

            // Vérifier la validité de l'entrée
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out choixCommande) && commandes.Exists(c => c.CommandeId == choixCommande && c.Statut != "livrée"))
                {
                    break;  // Si l'entrée est valide, sortir de la boucle
                }
                else
                {
                    Console.WriteLine("❌ Sélection invalide. Veuillez entrer un numéro de commande valide.");
                }
            }

            // Trouver la commande sélectionnée
            var commandeChoisie = commandes.Find(c => c.CommandeId == choixCommande);

            // Mettre à jour le statut de la commande à "livrée"
            MettreAJourStatutCommande(commandeChoisie.CommandeId, "livrée");

            // Afficher un message de confirmation
            Console.WriteLine($"✅ La commande #{commandeChoisie.CommandeId} a été livrée !");
        }


    static void NoterClient(int cuisinierId)
    {
        // Récupérer toutes les commandes livrées par le cuisinier
        var commandesLivrees = Commande.RecupereCommandesParCuisinierEtStatut(cuisinierId, "livrée");

        Console.Clear();
        Console.WriteLine("=== Noter le client ===\n");

        if (commandesLivrees.Count == 0)
        {
            Console.WriteLine("❌ Aucune commande livrée à noter.");
            return;
        }

        for (int i = 0; i < commandesLivrees.Count; i++)
        {
            Console.WriteLine($"{i + 1}. Commande #{commandesLivrees[i].CommandeId} | Client : {commandesLivrees[i].UtilisateurId}");
        }

        Console.Write("Sélectionner une commande à noter : ");
        int choix = int.Parse(Console.ReadLine() ?? "0");

        if (choix > 0 && choix <= commandesLivrees.Count)
        {
            int commandeId = commandesLivrees[choix - 1].CommandeId;
            Console.Write("Donnez une note au client (1 à 5) : ");
            decimal noteClient = decimal.Parse(Console.ReadLine() ?? "0");

            if (noteClient < 1 || noteClient > 5)
            {
                Console.WriteLine("❌ La note doit être comprise entre 1 et 5.");
                return;
            }

            // Mise à jour de la note du client dans la commande
            Commande.MettreAJourNoteCuisinier(commandeId, noteClient);

            Console.WriteLine($"✅ Vous avez noté le client pour la commande #{commandeId} avec une note de {noteClient}/5.");
        }
        else
        {
            Console.WriteLine("❌ Sélection invalide.");
        }

        Console.WriteLine("\nAppuyez sur une touche pour continuer...");
        Console.ReadKey();
    }
    public static void MettreAJourNoteCuisinier(int commandeId, decimal noteCuisinier)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE Commande SET commande_notecuisinier = @NoteCuisinier WHERE commande_id = @CommandeId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@NoteCuisinier", noteCuisinier);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    public static List<Commande> ToutesLesCommandes()
    {
        // Simuler la récupération des commandes depuis une base de données ou une autre source de données.
        // Remplace cette partie par l'accès à ta base de données ou à ta collection de commandes.

        List<Commande> commandes = new List<Commande>();

        // Exemple de commandes (à remplacer par des données réelles)
        commandes.Add(new Commande { CommandeId = 1, UtilisateurId = 9, Statut = "payée mais non livrée" });
        commandes.Add(new Commande { CommandeId = 2, UtilisateurId = 15, Statut = "livrée" });
        commandes.Add(new Commande { CommandeId = 3, UtilisateurId = 10, Statut = "payée mais non livrée" });
        commandes.Add(new Commande { CommandeId = 4, UtilisateurId = 12, Statut = "en attente" });
    
        // Retourner la liste des commandes
        return commandes;
    }
}
