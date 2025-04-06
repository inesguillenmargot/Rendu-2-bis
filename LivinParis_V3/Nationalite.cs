namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class Nationalite
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public void Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO Nationalité (nationalite_plat) VALUES (@Nom)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nom", Nom);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public static List<Nationalite> RecupereListeNationalites()
    {
        var liste = new List<Nationalite>();

        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT nationalite_plat, nationalite_id_plat FROM Nationalité";

        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new Nationalite
            {
                Nom = reader.GetString("nationalite_plat"),
                Id = reader.GetInt32("nationalite_id_plat")
            });
        }
        return liste;
    }

    public static string RecupereNationaliteParId(int id)
    {
        string nationalite = string.Empty;
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT nationalite_plat FROM Nationalité WHERE nationalite_id_plat = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        conn.Open();

        var result = cmd.ExecuteScalar();
        if (result != null)
        {
            nationalite = result.ToString();
        }
        return nationalite;
    }
    
    /// <summary>
    /// Méthode qui permet de permet de vérifier si une nationalité existe et si elle n'existe pas de la rajouter dans la base de données
    /// </summary>
    /// <param name="nom"></param>
    /// <returns></returns>
    public static int AjouteNationaliteSiNexistePas(string nom)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        // Vérification existance
        var checkCmd = new MySqlCommand("SELECT nationalite_id_plat FROM Nationalite WHERE nationalite_plat = @Nom", conn);
        checkCmd.Parameters.AddWithValue("@Nom", nom);
        var result = checkCmd.ExecuteScalar();

        if (result != null)
            return Convert.ToInt32(result);

        // Sinon ajout de celle-ci
        var insertCmd = new MySqlCommand("INSERT INTO Nationalite (nationalite_plat) VALUES (@Nom); SELECT LAST_INSERT_ID();", conn);
        insertCmd.Parameters.AddWithValue("@Nom", nom);
        return Convert.ToInt32(insertCmd.ExecuteScalar());
    }
}