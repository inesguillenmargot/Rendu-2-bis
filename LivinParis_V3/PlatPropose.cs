namespace LivinParis_V2;

using System;
using System.Collections.Generic;
using MySqlConnector;

public class PlatPropose
{
    public int PlatId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int NbPersonnes { get; set; }
    public DateTime DateFabrication { get; set; }
    public DateTime DatePeremption { get; set; }
    public decimal PrixParPersonne { get; set; }
    public string Photos { get; set; } = string.Empty;
    public int RecetteId { get; set; }

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

    /// <summary>
    /// Méthode permettant d'ajouter un plat à la base de données
    /// </summary>
    /// <returns></returns>
    public int Ajouter()
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @"INSERT INTO PlatPropose (plat_nom, plat_nbpersonnes, plat_datefabrication, plat_dateperemption, plat_prixpp, plat_photos, recette_id) VALUES ( @Nom, @NbPersonnes, @DateFab, @DatePeremption, @PrixPP, @Photos, @RecetteId); SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nom", Nom);
        cmd.Parameters.AddWithValue("@NbPersonnes", NbPersonnes);
        cmd.Parameters.AddWithValue("@DateFab", DateFabrication);
        cmd.Parameters.AddWithValue("@DatePeremption", DatePeremption);
        cmd.Parameters.AddWithValue("@PrixPP", PrixParPersonne);
        cmd.Parameters.AddWithValue("@Photos", Photos);
        cmd.Parameters.AddWithValue("@RecetteId", RecetteId);

        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
    
    /// <summary>
    /// Méthode qui permet de récupérer la liste de tous les plats disponibles 
    /// </summary>
    /// <returns></returns>
    public static List<PlatPropose> RecupererListeTousPlats()
    {
        var plats = new List<PlatPropose>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT * FROM PlatPropose";

        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int recetteId = reader.GetInt32("recette_id");

            plats.Add(new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = recetteId
            });
        }
        return plats;
    }
    
    public static PlatPropose RecupererPlatParId(int id)
    {
        PlatPropose plat = new PlatPropose();

        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT * FROM PlatPropose WHERE plat_id = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            int recetteId = reader.GetInt32("recette_id");

            plat = new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = recetteId
            };
        }
        return plat;
    }
    public static List<PlatPropose> RecupererPlatsDisponiblesPourDate(DateTime date)
    {
        var platsDisponibles = new List<PlatPropose>();
        using var conn = new MySqlConnection(connectionString);
        string query = @"SELECT * FROM PlatPropose WHERE @Date BETWEEN plat_datefabrication AND plat_dateperemption";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Date", date);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int recetteId = reader.GetInt32("recette_id");

            platsDisponibles.Add(new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = recetteId
            });
        }
        return platsDisponibles;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id du cuisinier (int) et qui permet de retourner la liste des plats qu'un cuisinier propose
    /// </summary>
    /// <param name="cuisinierId"></param>
    /// <returns></returns>
    public static List<PlatPropose> RecupererPlatsParCuisinier(int cuisinierId)
    {
        var plats = new List<PlatPropose>();

        using var conn = new MySqlConnection(connectionString);
        string query = @" SELECT * FROM PlatPropose WHERE plat_id IN (SELECT plat_id FROM PreparerPlat WHERE utilisateur_id = @CuisinierId);";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CuisinierId", cuisinierId);

        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {

            plats.Add(new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = reader.GetInt32("recette_id")
            });
        }
        return plats;
    }
    
    /// <summary>
    /// Méthode qui prend en paramètres l'id du cuisinier (int) et qui permet de retourner le plat du jour qu'un cuisinier propose
    /// </summary>
    /// <param name="cuisinierId"></param>
    /// <returns></returns>
    public static PlatPropose? RecuperePlatDuJour(int cuisinierId)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = @" SELECT p.*  FROM PlatPropose p JOIN PreparerPlat pp ON p.plat_id = pp.plat_id WHERE pp.utilisateur_id = @CuisinierId AND p.plat_datefabrication = CURDATE() ORDER BY p.plat_datefabrication DESC LIMIT 1";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@CuisinierId", cuisinierId);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new PlatPropose
            {
                PlatId = reader.GetInt32("plat_id"),
                Nom = reader.GetString("plat_nom"),
                NbPersonnes = reader.GetInt32("plat_nbpersonnes"),
                DateFabrication = reader.GetDateTime("plat_datefabrication"),
                DatePeremption = reader.GetDateTime("plat_dateperemption"),
                PrixParPersonne = reader.GetDecimal("plat_prixpp"),
                Photos = reader.GetString("plat_photos"),
                RecetteId = reader.GetInt32("recette_id")
            };
        }
        return null;
    }
}