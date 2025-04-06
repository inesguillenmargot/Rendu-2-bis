namespace LivinParis_V2;

using System;
using MySqlConnector;

public class Commande
{
    public int CommandeId { get; set; }
    public decimal PrixTotal { get; set; }
    public string Statut { get; set; } = string.Empty;
    public string AvisClient { get; set; } = string.Empty;
    public decimal NoteClient { get; set; }
    public decimal NoteCuisinier { get; set; }
    public int UtilisateurId { get; set; }

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id du client, le prix total de la commande et le statut qui doit être à : payée
    /// Méthode permettant d'ajouter une commande à la base de données
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="prixTotal"></param>
    /// <param name="statut"></param>
    /// <returns></returns>
    public static int Ajouter(int clientId, decimal prixTotal, string statut = "payée")
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO Commande (commande_prixtotal, commande_statut, utilisateur_id)  VALUES (@Prix, @Statut, @UtilisateurId);  SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Prix", prixTotal);
        cmd.Parameters.AddWithValue("@Statut", statut);
        cmd.Parameters.AddWithValue("@UtilisateurId", clientId);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande, l'id du plat, la date de livraison, la station de métro ainsi que la quantité de plat souhaitée
    /// Méthode qui permet d'ajouter à la base de données un élément de commande (différents pour chaque plat)
    /// </summary>
    /// <param name="commandeId"></param>
    /// <param name="platId"></param>
    /// <param name="dateLivraison"></param>
    /// <param name="stationMetro"></param>
    /// <param name="quantite"></param>
    public static void AjouterPlat(int commandeId, int platId, DateTime dateLivraison, string stationMetro, int quantite)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO ElementCommande (commande_quantite, commande_date_souhaitee, commande_stationmetro,commande_statut, commande_date_debut_livraison, commande_duree_livraison, commande_id, plat_id) VALUES (@Quantite, @Date, @Station,@Statut, @DateDebutLivraison, @DureeLivraison, @CommandeId, @PlatId)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Quantite", quantite);
        cmd.Parameters.AddWithValue("@Date", dateLivraison);
        cmd.Parameters.AddWithValue("@Station", stationMetro);
        cmd.Parameters.AddWithValue("@Statut", "A traiter");
        cmd.Parameters.AddWithValue("@DateDebutLivraison", null);
        cmd.Parameters.AddWithValue("@DureeLivraison", null);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);
        cmd.Parameters.AddWithValue("@PlatId", platId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur
    /// Méthode qui permet de retourner une liste des commandes faites par un client
    /// </summary>
    /// <param name="utilisateurId"></param>
    /// <returns></returns>
    public static List<Commande> RecupereCommandesParClient(int utilisateurId)
    {
        var commandes = new List<Commande>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT * FROM Commande WHERE utilisateur_id = @UtilisateurId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", utilisateurId);

        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            commandes.Add(new Commande
            {
                CommandeId = reader.GetInt32("commande_id"),
                PrixTotal = reader.IsDBNull(reader.GetOrdinal("commande_prixtotal")) ? 0 : reader.GetDecimal("commande_prixtotal"),
                Statut = reader.GetString("commande_statut"),
                AvisClient = reader.IsDBNull(reader.GetOrdinal("commande_avisClient")) ? string.Empty : reader.GetString("commande_avisClient"),
                // Vérification pour la note client, si NULL, retourner 0
                NoteClient = reader.IsDBNull(reader.GetOrdinal("commande_noteclient")) ? 0 : reader.GetDecimal("commande_noteclient"),
                NoteCuisinier = reader.IsDBNull(reader.GetOrdinal("commande_notecuisinier")) ? 0 : reader.GetDecimal("commande_notecuisinier"),
                UtilisateurId = utilisateurId
            });
        }
        return commandes;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres deux dates (une période)
    /// Méthode qui retourne une liste de de toutes les commandes passées entre un certaine période
    /// </summary>
    /// <param name="dateDebut"></param>
    /// <param name="dateFin"></param>
    /// <returns></returns>
    public static List<Commande> RecupereToutesLesCommandesParPeriode(DateTime dateDebut, DateTime dateFin)
    {
        var commandes = new List<Commande>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT DISTINCT c.* FROM Commande c JOIN ElementCommande ec ON c.commande_id = ec.commande_id WHERE ec.commande_date BETWEEN @DateDebut AND @DateFin";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
        cmd.Parameters.AddWithValue("@DateFin", dateFin);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            commandes.Add(new Commande
            {
                CommandeId = reader.GetInt32("commande_id"),
                PrixTotal = reader.GetDecimal("commande_prixtotal"),
                Statut = reader.GetString("commande_statut"),
                AvisClient = reader.GetString("commande_avisClient"),
                NoteClient = reader.GetDecimal("commande_noteclient"),
                NoteCuisinier = reader.GetDecimal("commande_notecuisinier"),
                UtilisateurId = reader.GetInt32("utilisateur_id")
            });
        }
        return commandes;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur
    /// Méthode qui permet de retourner la moyenne du prix des commandes fites par un client
    /// </summary>
    /// <param name="utilisateurId"></param>
    /// <returns></returns>
    public static decimal CalculerMoyennePrixParClient(int utilisateurId)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT AVG(commande_prixtotal) FROM Commande WHERE utilisateur_id = @UtilisateurId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
        conn.Open();

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToDecimal(result) : 0;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur et la nationalité des plats commandés
    /// Méthode qui permet de retourner une liste des commandes faites par un client selon la nationalité des plats commandés
    /// </summary>
    /// <param name="utilisateurId"></param>
    /// <param name="nationalite"></param>
    /// <returns></returns>
    public static List<Commande> RecupererCommandesParClientEtNationalite(int utilisateurId, string nationalite)
    {
        var commandes = new List<Commande>();

        using var conn = new MySqlConnection(connectionString);
        string query = @" SELECT DISTINCT c.* FROM Commande c JOIN ElementCommande ec ON c.commande_id = ec.commande_id JOIN PlatPropose p ON ec.plat_id = p.plat_id JOIN Recette r ON p.recette_id = r.recette_id WHERE c.utilisateur_id = @UtilisateurId AND r.nationalite_plat = @Nationalite";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
        cmd.Parameters.AddWithValue("@Nationalite", nationalite);

        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            commandes.Add(new Commande
            {
                CommandeId = reader.GetInt32("commande_id"),
                PrixTotal = reader.GetDecimal("commande_prixtotal"),
                Statut = reader.GetString("commande_statut"),
                AvisClient = reader.GetString("commande_avisClient"),
                NoteClient = reader.GetDecimal("commande_noteclient"),
                NoteCuisinier = reader.GetDecimal("commande_notecuisinier"),
                UtilisateurId = reader.GetInt32("utilisateur_id")
            });
        }
        return commandes;
    }
   
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande
    /// Méthode qui permet de retourner la liste des plats d'une commande
    /// </summary>
    /// <param name="commandeId"></param>
    /// <returns></returns>
    public static List<(PlatPropose Plat, DateTime DateLivraison, string StationMetro)> RecupererPlatsParCommande(int commandeId)
    {
        var liste = new List<(PlatPropose, DateTime, string)>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT cc.plat_id, cc.commande_date_souhaitee, cc.commande_stationmetro, p.plat_nom, p.plat_prixpp FROM ElementCommande cc JOIN PlatPropose p ON p.plat_id = cc.plat_id WHERE cc.commande_id = @CommandeId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var plat = new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp")
            };

            DateTime date = reader.GetDateTime("commande_date_souhaitee");
            string station = reader.GetString("commande_stationmetro");

            liste.Add((plat, date, station));
        }
        return liste;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur(ici client)
    /// Méthode qui permet de retourner la moyenne des notes attribuées aux cusiniers par le client
    /// </summary>
    /// <param name="utilisateurId"></param>
    /// <returns></returns>
    public static decimal CalculerMoyenneNoteClient(int utilisateurId)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT AVG(commande_noteclient) FROM Commande WHERE utilisateur_id = @UtilisateurId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
        conn.Open();

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToDecimal(result) : 0;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur (ici cuisinier)
    /// Méthode qui permet de retourner la moyenne des notes attribuées aux clients par le cuisinier
    /// </summary>
    /// <param name="utilisateurId"></param>
    /// <returns></returns>
    public static decimal CalculerMoyenneNoteCuisinier(int utilisateurId)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT AVG(commande_notecuisinier) FROM Commande WHERE utilisateur_id = @UtilisateurId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", utilisateurId);
        conn.Open();

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToDecimal(result) : 0;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande, l'avis du client et la note du client
    /// Méthode qui permet de mettre à jour dans la base de données l'avis et la note donnée par le client au cuisinier
    /// </summary>
    /// <param name="commandeId"></param>
    /// <param name="avisClient"></param>
    /// <param name="noteClient"></param>
    public static void MettreAJourAvisEtNote(int commandeId, string avisClient, decimal noteClient)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE Commande SET commande_avisClient = @AvisClient, commande_noteclient = @NoteClient WHERE commande_id = @CommandeId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@AvisClient", avisClient);
        cmd.Parameters.AddWithValue("@NoteClient", noteClient);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande et le nouveau statut de celle-ci
    /// Méthode qui permet de mettre à jour dans la base de données le statut de la commande à payée mais non livrée
    /// </summary>
    /// <param name="commandeId"></param>
    /// <param name="nouveauStatut"></param>
    public static void MettreAJourStatutCommande(int commandeId, string nouveauStatut)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE Commande SET commande_statut = @Statut WHERE commande_id = @CommandeId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Statut", nouveauStatut);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande et la note que le cuisinier met au client
    /// Méthode qui permet de mettre à jour dans la base de données la note que le cuisinier attribue aux clients qu'il sert
    /// </summary>
    /// <param name="commandeId"></param>
    /// <param name="noteCuisinier"></param>
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
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de l'utilisateur(cuisinier), la statut de la commande et la date de celle-ci
    /// Méthode qui permet de retourner une liste des commandes pour une cuisinier en fonction du statut de celles-ci et de leur date
    /// </summary>
    /// <param name="cuisinierId"></param>
    /// <param name="statut"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public static List<Commande> RecupereCommandesParCuisinierEtStatutEtDate(int cuisinierId, string statut, DateTime date)
    {
        var commandes = new List<Commande>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT c.* FROM Commande c JOIN ElementCommande ec ON c.commande_id = ec.commande_id JOIN PlatPropose p ON ec.plat_id = p.plat_id JOIN PreparerPlat pp ON p.plat_id = pp.plat_id WHERE pp.utilisateur_id = @CuisinierId AND c.commande_statut = @Statut AND DATE(ec.commande_date_souhaitee) = DATE(@Date)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CuisinierId", cuisinierId);
        cmd.Parameters.AddWithValue("@Statut", statut);
        cmd.Parameters.AddWithValue("@Date", date);

        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            commandes.Add(new Commande
            {
                CommandeId = reader.GetInt32("commande_id"),
                PrixTotal = reader.GetDecimal("commande_prixtotal"),
                Statut = reader.GetString("commande_statut"),
                AvisClient = reader.IsDBNull(reader.GetOrdinal("commande_avisClient")) ? string.Empty : reader.GetString("commande_avisClient"),
                NoteClient = reader.IsDBNull(reader.GetOrdinal("commande_noteclient")) ? 0 : reader.GetDecimal("commande_noteclient"),
                NoteCuisinier = reader.IsDBNull(reader.GetOrdinal("commande_notecuisinier")) ? 0 : reader.GetDecimal("commande_notecuisinier"),
                UtilisateurId = reader.GetInt32("utilisateur_id")
            });
        }

        return commandes;
    }
    
    /// <summary>
    /// Méthode qui ne prend pas de paramètres
    /// Méthode qui permet de mettre à jour dans la base de données le statut de la commande à Livrée si tous les éléments de commande de cette commande sont mis au statut livrée
    /// </summary>
    public static void MettreAJourStatutCommande()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE Commande a SET a.commande_statut = @Statut WHERE NOT EXISTS (SELECT null FROM ElementCommande b WHERE b.commande_statut<>'Livrée' AND a.commande_id=b.commande_id)";
        
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Statut", "Livrée");

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}