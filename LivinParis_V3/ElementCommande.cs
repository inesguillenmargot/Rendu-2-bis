namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class ElementCommande
{
    public int CommandeDetailId { get; set; }
    public int Quantite { get; set; }
    public DateTime DateSouhaitee { get; set; }
    public string StationMetro { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public DateTime DateDebutLivraison { get; set; }
    public int DureeLivraison { get; set; }
    public int CommandeId { get; set; }
    public int PlatId { get; set; }

    
    
    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public int Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO ElementCommande (commande_quantite, commande_date_souhaitee, commande_stationmetro, commande_statut, commande_date_debut_livraison, commande_duree_livraison, commande_id, plat_id) VALUES (@Quantite, @DateSouhaitee, @Station, @Statut, @DateDebutLivraison, @DureeLivraison, @CommandeId, @PlatId); SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Quantite", Quantite);
        cmd.Parameters.AddWithValue("@DateSouhaitee", DateSouhaitee);
        cmd.Parameters.AddWithValue("@Station", StationMetro);
        cmd.Parameters.AddWithValue("@Statut", Statut);
        cmd.Parameters.AddWithValue("@DateDebutLivraison", DateDebutLivraison);
        cmd.Parameters.AddWithValue("@DureeLivraison", DureeLivraison);
        cmd.Parameters.AddWithValue("@CommandeId", CommandeId);
        cmd.Parameters.AddWithValue("@PlatId", PlatId);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id de la commande
    /// Méthode qui permet de retourner une liste des éléments de commande d'une commande
    /// </summary>
    /// <param name="commandeId"></param>
    /// <returns></returns>
    public static List<ElementCommande> RecupererElementCommandeParCommandeId(int commandeId)
    {
        var elementsCommande = new List<ElementCommande>();

        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT * FROM ElementCommande WHERE commande_id = @CommandeId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CommandeId", commandeId);

        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            elementsCommande.Add(new ElementCommande
            {
                CommandeDetailId = reader.GetInt32("commandedetail_id"),
                Quantite = reader.GetInt32("commande_quantite"),
                DateSouhaitee = reader.GetDateTime("commande_date_souhaitee"),
                StationMetro = reader.GetString("commande_stationmetro"),
                Statut = reader.GetString("commande_statut"),
                //DateDebutLivraison = reader.GetDateTime("commande_date_debut_livraison"),
                DateDebutLivraison = new DateTime(),
                //DureeLivraison = reader.GetInt32("commande_duree_livraison"),
                DureeLivraison = 0,
                CommandeId = reader.GetInt32("commande_id"),
                PlatId = reader.GetInt32("plat_id")
            });
        }
        return elementsCommande;
    }
    /// <summary>
    /// Cette méthode ne prend pas de paramètres. Elle permet de mettre à jour les élements "statut", "date de debut de livraison", "la durée de la livraison" et "Id de l'élement de commande".
    /// </summary>
    public void MettreAJourLivraison()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE ElementCommande SET commande_statut = @Statut, commande_date_debut_livraison = @DateDebut, commande_duree_livraison = @DureeLivraison WHERE commandedetail_id = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Statut", Statut);
        cmd.Parameters.AddWithValue("@DateDebut", DateDebutLivraison);
        cmd.Parameters.AddWithValue("@DureeLivraison", DureeLivraison);
        cmd.Parameters.AddWithValue("@Id", CommandeDetailId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Cette méthode ne prend pas de paramètres. Elle permet de mettre à jour tous les éléments de la table ElementCommande
    /// </summary>
    public static void PasserElementCommandeALivrer()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"UPDATE ElementCommande SET commande_statut = @Statut WHERE TIMESTAMPADD(MINUTE, commande_duree_livraison,commande_date_debut_livraison)<sysdate()";
        
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Statut", "Livrée");

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}