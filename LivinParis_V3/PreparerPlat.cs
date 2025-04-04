namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class PreparerPlat
{
    public int UtilisateurId { get; set; }
    public int PlatId { get; set; }

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public void Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO PreparerPlat (utilisateur_id, plat_id) VALUES (@UtilisateurId, @PlatId)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UtilisateurId", UtilisateurId);
        cmd.Parameters.AddWithValue("@PlatId", PlatId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    public static Dictionary<int, int> RecupererNombreLivraisonsParCuisinier()
    {
        var livraisonsParCuisinier = new Dictionary<int, int>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT utilisateur_id, COUNT(commandedetail_id) AS nb_livraisons FROM PreparerPlat JOIN ElementCommande ON PreparerPlat.plat_id = ElementCommande.plat_id GROUP BY utilisateur_id";
        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int cuisinierId = reader.GetInt32("utilisateur_id");
            int nbLivraisons = reader.GetInt32("nb_livraisons");

            livraisonsParCuisinier[cuisinierId] = nbLivraisons;
        }
        return livraisonsParCuisinier;
    }
    public static (int utilisateurId, int nbLivraisons) RecupereTopCuisinierParLivraison()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT utilisateur_id, COUNT(commandedetail_id) AS nb_livraisons FROM PreparerPlat JOIN ElementCommande ON PreparerPlat.plat_id = ElementCommande.plat_id GROUP BY utilisateur_id ORDER BY nb_livraisons DESC LIMIT 1";
        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            int id = reader.GetInt32("utilisateur_id");
            int total = reader.GetInt32("nb_livraisons");
            return (id, total);
        }
        return (0, 0); // Aucun cuisinier trouvé
    }
    /// <summary>
    /// Si juste RecupereTousLesClientsServies(cuisinierId) alors ça prend depuis l'inscription de celui-ci)
    /// </summary>
    /// <param name="cuisinierId"></param>
    /// <param name="dateDebut"></param>
    /// <param name="dateFin"></param>
    /// <returns></returns>
    public static List<Utilisateur> RecupereClientsServis(int cuisinierId, DateTime? dateDebut = null, DateTime? dateFin = null)
    {
        var clients = new List<Utilisateur>();
        using var conn = new MySqlConnection(connectionString);
        var conditions = new List<string> { "pp.utilisateur_id = @CuisinierId" };
        if (dateDebut != null)
            conditions.Add("ec.commande_date >= @DateDebut");
        if (dateFin != null)
            conditions.Add("ec.commande_date <= @DateFin");
        string whereClause = "WHERE " + string.Join(" AND ", conditions);
        string query = $@" SELECT DISTINCT u.* FROM PreparerPlat pp JOIN PlatPropose p ON pp.plat_id = p.plat_id JOIN ElementCommande ec ON ec.plat_id = p.plat_id JOIN Commande c ON ec.commande_id = c.commande_id JOIN Utilisateur u ON c.utilisateur_id = u.utilisateur_id {whereClause}";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CuisinierId", cuisinierId);
        if (dateDebut != null) cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
        if (dateFin != null) cmd.Parameters.AddWithValue("@DateFin", dateFin);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            clients.Add(new Utilisateur
            {
                UtilisateurId = reader.GetInt32("utilisateur_id"),
                Nom = reader.GetString("utilisateur_nom"),
                Prenom = reader.GetString("utilisateur_prenom"),
                Email = reader.GetString("utilisateur_email"),
                Telephone = reader.GetString("utilisateur_telephone"),
                Rue = reader.GetString("utilisateur_rue"),
                NumRue = reader.GetInt32("utilisateur_num_rue"),
                CodePostal = reader.GetInt32("utilisateur_codepostal"),
                Ville = reader.GetString("utilisateur_ville"),
                Type = reader.GetString("utilisateur_type"),
                MetroProche = reader.GetString("utilisateur_metroproche")
            });
        }
        return clients;
    }
    public static Dictionary<PlatPropose, int> RecupereLesPlatsParFrequence(int cuisinierId)
    {
        var resultats = new Dictionary<PlatPropose, int>();

        using var conn = new MySqlConnection(connectionString);
        string query = @" SELECT p.*, COUNT(ec.commandedetail_id) AS frequence FROM PreparerPlat pp JOIN PlatPropose p ON pp.plat_id = p.plat_id LEFT JOIN ElementCommande ec ON p.plat_id = ec.plat_id WHERE pp.utilisateur_id = @CuisinierId GROUP BY p.plat_id ORDER BY frequence DESC";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CuisinierId", cuisinierId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var plat = new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = reader.GetInt32("recette_id")
            };
            int frequence = reader.GetInt32("frequence");
            resultats[plat] = frequence;
        }
        return resultats;
    }
}