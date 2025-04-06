namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class TypePlat
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    /// <summary>
    /// Méthode qui ne prend rien en paramètres
    /// Méthode qui récupère tous les types de plats et qui les retourne sous forme de liste
    /// </summary>
    /// <returns></returns>
    public static List<TypePlat> LireTous()
    {
        var liste = new List<TypePlat>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT type_plat_id, type_plat FROM Type_Plat";
        using var cmd = new MySqlCommand(query, conn);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new TypePlat
            {
                Id = reader.GetInt32("type_plat_id"),
                Nom = reader.GetString("type_plat")
            });
        }
        return liste;
    }

    /// <summary>
    /// Méthode qui prend en paramètres l'id du plat
    /// Méthode qui permet de retourner le nom du plat en fonction de son id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string RecupereNomPlatParId(int id)
    {
        string typePlat = string.Empty;
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT type_plat FROM Type_Plat WHERE type_plat_id = @Id";
        
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        conn.Open();

        var result = cmd.ExecuteScalar();
        if (result != null)
        {
            typePlat = result.ToString();
        }
        return typePlat;
    }
}