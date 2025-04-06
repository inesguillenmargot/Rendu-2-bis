namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class RegimeAlimentaire
{
    public string Nom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

   /// <summary>
   /// Méthode qui ne prend rien en paramètres
   /// Méthode qui permet de retourner la liste de tous les régimes alimentaires déjà présents dans la base de données
   /// </summary>
   /// <returns></returns>
    public static List<RegimeAlimentaire> LireTous()
    {
        var liste = new List<RegimeAlimentaire>();
        
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT regimealimentaire_plat FROM RegimeAlimentaire";
        
        using var cmd = new MySqlCommand(query, conn);
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