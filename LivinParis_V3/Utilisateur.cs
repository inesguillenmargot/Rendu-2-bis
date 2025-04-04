namespace LivinParis_V2;

using System;
using MySqlConnector;

public class Utilisateur
{
    public int UtilisateurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public bool EstClient { get; set; }
    public bool EstParticulier { get; set; }
    public string EntrepriseNom { get; set; } = string.Empty;
    public string EntrepriseNomReferent { get; set; } = string.Empty;
    public bool EstCuisinier { get; set; }
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string MetroProche { get; set; } = string.Empty;
    public string Rue { get; set; } = string.Empty;
    public int NumRue { get; set; }
    public int CodePostal { get; set; }
    public string Ville { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public int Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO Utilisateur (utilisateur_nom, utilisateur_prenom, utilisateur_est_client, utilisateur_est_particulier, entreprise_nom, entreprise_nom_referent, utilisateur_est_cuisinier, utilisateur_telephone, utilisateur_email, utilisateur_mdp, utilisateur_type, utilisateur_metroproche, utilisateur_rue, utilisateur_num_rue, utilisateur_codepostal, utilisateur_ville) VALUES (@Nom, @Prenom, @EstClient, @EstParticulier, @EntrepriseNom, @EntrepriseReferent,  @EstCuisinier, @Telephone, @Email, @Mdp, @Type, @Metro, @Rue, @NumRue, @CodePostal, @Ville); SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nom", Nom);
        cmd.Parameters.AddWithValue("@Prenom", Prenom);
        cmd.Parameters.AddWithValue("@EstClient", EstClient);
        cmd.Parameters.AddWithValue("@EstParticulier", EstParticulier);
        cmd.Parameters.AddWithValue("@EntrepriseNom", EntrepriseNom);
        cmd.Parameters.AddWithValue("@EntrepriseReferent", EntrepriseNomReferent);
        cmd.Parameters.AddWithValue("@EstCuisinier", EstCuisinier);
        cmd.Parameters.AddWithValue("@Telephone", Telephone);
        cmd.Parameters.AddWithValue("@Email", Email);
        cmd.Parameters.AddWithValue("@Mdp", MotDePasse);
        cmd.Parameters.AddWithValue("@Type", Type);
        cmd.Parameters.AddWithValue("@Metro", MetroProche);
        cmd.Parameters.AddWithValue("@Rue", Rue);
        cmd.Parameters.AddWithValue("@NumRue", NumRue);
        cmd.Parameters.AddWithValue("@CodePostal", CodePostal);
        cmd.Parameters.AddWithValue("@Ville", Ville);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    /// <summary>
    /// si parNom = false alors pas trié par nom mais si parRue = vraie, trié par rue. Cela permet de pouvoir trier simultanément par rue, par nom et par montant
    /// </summary>
    /// <param name="parNom"></param>
    /// <param name="parRue"></param>
    /// <param name="parMontant"></param>
    /// <returns></returns>
    public static List<(Utilisateur client, decimal totalAchats)> RecupererClientsTries(bool parNom = false, bool parRue = false, bool parMontant = false)
    {
        var clients = new List<(Utilisateur, decimal)>();
        using var conn = new MySqlConnection(connectionString);
        string query = @" SELECT u.*, COALESCE(SUM(c.commande_prixtotal), 0) AS total_achats FROM Utilisateur u LEFT JOIN Commande c ON u.utilisateur_id = c.utilisateur_id WHERE u.utilisateur_est_client = true GROUP BY u.utilisateur_id";

        var orderBy = new List<string>();
        if (parNom) orderBy.Add("u.utilisateur_nom ASC, u.utilisateur_prenom ASC");
        if (parRue) orderBy.Add("u.utilisateur_rue ASC");
        if (parMontant) orderBy.Add("total_achats DESC");

        if (orderBy.Count > 0)
        {
            query += " ORDER BY " + string.Join(", ", orderBy);
        }

        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var client = new Utilisateur
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
            };
            decimal total = reader.GetDecimal("total_achats");
            clients.Add((client, total));
        }
        return clients;
    }
    public static Utilisateur RecupererParId(int id)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT * FROM Utilisateur WHERE utilisateur_id = @Id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Utilisateur
            {
                UtilisateurId = reader.GetInt32("utilisateur_id"),
                Nom = reader.GetString("utilisateur_nom"),
                Prenom = reader.GetString("utilisateur_prenom"),
                Telephone = reader.GetString("utilisateur_telephone"),
                Email = reader.GetString("utilisateur_email"),
                MotDePasse = reader.GetString("utilisateur_mdp"),
                Type = reader.GetString("utilisateur_type"),
                MetroProche = reader.GetString("utilisateur_metroproche"),
                Rue = reader.GetString("utilisateur_rue"),
                NumRue = reader.GetInt32("utilisateur_num_rue"),
                CodePostal = reader.GetInt32("utilisateur_codepostal"),
                Ville = reader.GetString("utilisateur_ville"),
                EstClient = reader.GetBoolean("utilisateur_est_client"),
                EstCuisinier = reader.GetBoolean("utilisateur_est_cuisinier"),
                EstParticulier = reader.GetBoolean("utilisateur_est_particulier"),
                EntrepriseNom = reader.IsDBNull(reader.GetOrdinal("entreprise_nom")) ? "" : reader.GetString("entreprise_nom"),
                EntrepriseNomReferent = reader.IsDBNull(reader.GetOrdinal("entreprise_nom_referent")) ? "" : reader.GetString("entreprise_nom_referent")
            };
        }
        throw new Exception("Utilisateur non trouvé.");
    }
    public static void SupprimerParId(int utilisateurId)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "DELETE FROM Utilisateur WHERE utilisateur_id = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", utilisateurId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}
