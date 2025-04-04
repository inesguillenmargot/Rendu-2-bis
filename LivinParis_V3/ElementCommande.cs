namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class ElementCommande
{
    public int CommandeDetailId { get; set; }
    public int Quantite { get; set; }
    public DateTime Date { get; set; }
    public string StationMetro { get; set; } = string.Empty;
    public int CommandeId { get; set; }
    public int PlatId { get; set; }

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public int Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO ElementCommande (commande_quantite, commande_date, commande_stationmetro, commande_id, plat_id) VALUES (@Quantite, @Date, @Station, @CommandeId, @PlatId); SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Quantite", Quantite);
        cmd.Parameters.AddWithValue("@Date", Date);
        cmd.Parameters.AddWithValue("@Station", StationMetro);
        cmd.Parameters.AddWithValue("@CommandeId", CommandeId);
        cmd.Parameters.AddWithValue("@PlatId", PlatId);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static void Supprimer(int id)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "DELETE FROM ElementCommande WHERE commandedetail_id = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
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
                Date = reader.GetDateTime("commande_date"),
                StationMetro = reader.GetString("commande_stationmetro"),
                CommandeId = reader.GetInt32("commande_id"),
                PlatId = reader.GetInt32("plat_id")
            });
        }
        return elementsCommande;
    }
}