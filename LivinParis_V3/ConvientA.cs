namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class ConvientA
{
    public int RecetteId { get; set; }
    public string RegimeAlimentaire { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public void Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO ConvientA (recette_id, regimealimentaire_plat) VALUES (@RecetteId, @RegimeAlimentaire)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", RecetteId);
        cmd.Parameters.AddWithValue("@Regime", RegimeAlimentaire);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public static List<RegimeAlimentaire> RecupereRegimesParRecette(int recetteId)
    {
        var liste = new List<RegimeAlimentaire>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT regimealimentaire_plat FROM ConvientA WHERE recette_id = @RecetteId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", recetteId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new RegimeAlimentaire
                {
                    Nom = reader.GetString("regimealimentaire_plat")
                });
        }

        return liste;
    }
}