namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class Ingredient
{
    public string Nom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    /// <summary>
    /// Méthode qui prend en paramètres le nom de l'ingrédient
    /// Méthode qui permet d'ajouter à la base de données l'ingrédient si celui-ci n'existe pas déjà (la vérification est faite)
    /// </summary>
    /// <param name="nom"></param>
    public static void AjouterSiNexistePas(string nom)
    {
        using var conn = new MySqlConnection(connectionString);
        string checkQuery = "SELECT COUNT(*) FROM Ingredients WHERE ingredient_nom = @Nom";
        using var checkCmd = new MySqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@Nom", nom);

        conn.Open();
        var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

        if (!exists)
        {
            string insertQuery = "INSERT INTO Ingredients (ingredient_nom) VALUES (@Nom)";
            using var insertCmd = new MySqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@Nom", nom);
            insertCmd.ExecuteNonQuery();
        }
    }
}