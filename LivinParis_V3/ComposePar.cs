namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class ComposePar
{
    public int RecetteId { get; set; }
    public string IngredientNom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    public void Ajouter()
    {
        
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO ComposePar (recette_id, ingredient_nom) VALUES (@RecetteId, @IngredientNom)";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", RecetteId);
        cmd.Parameters.AddWithValue("@IngredientNom", IngredientNom);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Supprimer()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"DELETE FROM ComposePar WHERE recette_id = @RecetteId AND ingredient_nom = @IngredientNom";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", RecetteId);
        cmd.Parameters.AddWithValue("@IngredientNom", IngredientNom);

        conn.Open();
        cmd.ExecuteNonQuery();
    }
    public static List<Ingredient> RecupereIngredientsParRecette(int recetteId)
    {
        var liste = new List<Ingredient>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT ingredient_nom FROM ComposePar WHERE recette_id = @RecetteId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", recetteId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            liste.Add(new Ingredient
            {
                Nom = reader.GetString("ingredient_nom")
            });
        }
        return liste;
    }
}