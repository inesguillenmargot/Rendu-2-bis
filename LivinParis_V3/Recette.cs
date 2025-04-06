namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class Recette
{
    public int RecetteId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int IdTypePlat { get; set; }
    public int IdNationalite { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public List<RegimeAlimentaire> RegimesAlimentaire { get; set; } = new List<RegimeAlimentaire>();

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    /// <summary>
    /// Méthode qui ne prend rien en paramètres
    /// Méthode qui permet d'ajouter une nouvelle recette dans la base de données
    /// </summary>
    /// <returns></returns>
    public int Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO Recette (recette_nom, type_id_plat, nationalite_id_plat) VALUES (@Nom, @TypePlat, @Nationalite);  SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nom", Nom);
        cmd.Parameters.AddWithValue("@TypePlat", IdTypePlat);
        cmd.Parameters.AddWithValue("@Nationalite", IdNationalite);

        conn.Open();
        int recetteIdTemporaire = Convert.ToInt32(cmd.ExecuteScalar());
        
        int i = 0;
        while (i < Ingredients.Count)
        {
            Ingredient.AjouterSiNexistePas(Ingredients[i].Nom);
            //Console.WriteLine(Ingredients[i].Nom);
            query = @"INSERT INTO ComposePar (recette_Id, ingredient_nom) VALUES (@recetteIdTemporaire, @Ingredient);";
            using var cmd2 = new MySqlCommand(query, conn);
            cmd2.Parameters.AddWithValue("@recetteIdTemporaire", recetteIdTemporaire);
            cmd2.Parameters.AddWithValue("@Ingredient", Ingredients[i].Nom);
            //conn.Open();
            //cmd2.ExecuteScalar();
            cmd2.ExecuteNonQuery();
            i++;
        }
        i = 0;
        while (i < RegimesAlimentaire.Count)
        {
            query = @"INSERT INTO ConvientA (recette_Id, regimealimentaire_plat) VALUES (@recetteIdTemporaire, @RegimeAlimentaire);";
            using var cmd2 = new MySqlCommand(query, conn);
            cmd2.Parameters.AddWithValue("@recetteIdTemporaire", recetteIdTemporaire);
            cmd2.Parameters.AddWithValue("@RegimeAlimentaire", RegimesAlimentaire[i].Nom);
            //conn.Open();
            //cmd2.ExecuteScalar();
            cmd2.ExecuteNonQuery();
            i++;
        }
        return recetteIdTemporaire;
    }
    
    /// <summary>
    /// Méthode qui ne prend rien en paramètres
    /// Méthode qui permet de retourner la liste de toutes les recettes déjà présentes dans la base de données
    /// </summary>
    /// <returns></returns>
    public static List<Recette> RecupereListeToutesRecettes()
    {
        var recettes = new List<Recette>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT recette_id, recette_nom, type_id_plat, nationalite_id_plat FROM Recette";

        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int recetteIdTemporaire = reader.GetInt32("recette_id");
            List<Ingredient> ingredientsTemporaire = new List<Ingredient>();
            List<RegimeAlimentaire> regimeAlimentaireTemporaire = new List<RegimeAlimentaire>();
            ingredientsTemporaire = ComposePar.RecupereIngredientsParRecette(recetteIdTemporaire);
            regimeAlimentaireTemporaire = ConvientA.RecupereRegimesParRecette(recetteIdTemporaire);

            recettes.Add(new Recette
            {
                RecetteId = recetteIdTemporaire,
                Nom = reader.GetString("recette_nom"),
                IdTypePlat = reader.GetInt32("type_id_plat"),
                IdNationalite = reader.GetInt32("nationalite_id_plat"),
                Ingredients = ingredientsTemporaire, //ComposePar.RecupereIngredientsParRecette(RecetteId),
                RegimesAlimentaire = regimeAlimentaireTemporaire //ConvientA.RecupereRegimesParRecette(RecetteId)
            });
        }
        return recettes;
    }
    public static Recette RecupereRecetteParId(int recetteId)
    {
        var recette = new Recette();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT recette_id, recette_nom, type_id_plat, nationalite_id_plat FROM Recette WHERE recette_id = @RecetteId";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@RecetteId", recetteId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            recette = new Recette
            {
                RecetteId = reader.GetInt32("recette_id"),
                Nom = reader.GetString("recette_nom"),
                IdTypePlat = reader.GetInt32("type_id_plat"),
                IdNationalite = reader.GetInt32("nationalite_id_plat")
            };
        }

        recette.Ingredients = ComposePar.RecupereIngredientsParRecette(recetteId);
        recette.RegimesAlimentaire = ConvientA.RecupereRegimesParRecette(recetteId);

        return recette;
    }
}