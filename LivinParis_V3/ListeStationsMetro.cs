using System.Globalization;
using System.Text;

namespace LivinParis_V2;

using System;
using MySqlConnector;
public class ListeStationsMetro
{
    public int StationMetroId { get; set; }
    public string StationMetroNom { get; set; } = string.Empty;

    private static string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";
    
    /// <summary>
    /// Méthode qui récupère toutes les stations de métro
    /// </summary>
    /// <returns></returns>
    public static List<ListeStationsMetro> RecupererToutesStationsMetro()
    {
        var stations = new List<ListeStationsMetro>();
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT * FROM ListeStationsMetro";

        using var cmd = new MySqlCommand(query, conn);
        conn.Open();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var station = new ListeStationsMetro
            {
                StationMetroId = reader.GetInt32("stationmetro_id"),
                StationMetroNom = reader.GetString("stationmetro_nom")
            };
            stations.Add(station);
        }

        return stations;
    }
    
    public static ListeStationsMetro RecupererStationsMetroParId(int id)
    {
        using var conn = new MySqlConnection(connectionString);
        string query = "SELECT * FROM ListeStationsMetro WHERE stationmetro_id = @Id";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new ListeStationsMetro
            {
                StationMetroId = reader.GetInt32("stationmetro_id"),
                StationMetroNom = reader.GetString("stationmetro_nom")
            };
        }
        throw new Exception("Station de métro non trouvée.");
    }
    
    /// <summary>
    /// Méthode qui vérifie si une station (donnée par l'utilisateur - cuisinier ou client) existe
    /// </summary>
    /// <param name="nom"></param>
    /// <returns></returns>
    public static bool VérifierExistenceStationsMetroParNom(string nom)
    {
        bool exists = false;
        using var conn = new MySqlConnection(connectionString);
        //string query = "SELECT * FROM ListeStationsMetro WHERE UPPER(stationmetro_nom) = @Nom";
        string query = "SELECT * FROM ListeStationsMetro WHERE UPPER(@Nom)= UPPER(stationmetro_nom) COLLATE utf8mb4_general_ci";
        string nomstation = nom.ToUpper();
        // Supprimer les accents
        string normalizedString = nomstation.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char c in normalizedString)
        {
            // Ajouter uniquement les caractères non accentués
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        nomstation = stringBuilder.ToString();

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nom", nomstation);

        conn.Open();
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            exists = true;
        }
        return exists;
    }
    
    /// <summary>
    /// Méthode qui permet de proposer le nom d'une station de métro existente si la station de métro rentrée par l'utilisateur se prononce de la même manière qu'une station existente
    /// </summary>
    /// <param name="nom"></param>
    public static void ProposerStationSiNonExistante(ref string nom)
    {
        bool exists = VérifierExistenceStationsMetroParNom(nom);

        // Boucle jusqu'à ce que la station soit valide (soit existante, soit proposée et confirmée par l'utilisateur)
        while (!exists)
        {
            Console.WriteLine($"La station '{nom}' n'existe pas dans notre base de données.");

            // Requête SQL pour rechercher une station qui se prononce de manière similaire (avec SOUNDEx)
            using var conn = new MySqlConnection(connectionString);
            string query = "SELECT stationmetro_nom FROM ListeStationsMetro WHERE SOUNDEX(stationmetro_nom) = SOUNDEX(@Nom) LIMIT 1";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Nom", nom);

            conn.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string stationProposee = reader.GetString("stationmetro_nom");
                Console.WriteLine($"Peut-être vouliez-vous dire : {stationProposee} ?");

                // Demander à l'utilisateur s'il accepte cette suggestion
                Console.Write("Voulez-vous sélectionner cette station ? (o/n) : ");
                string reponse = Console.ReadLine()?.ToLower();

                if (reponse == "o")
                {
                    nom = stationProposee;  // Accepte la suggestion de la station
                    exists = true;  // La station est désormais valide
                }
                else
                {
                    // Redemander à l'utilisateur de saisir une nouvelle station
                    Console.Write("Veuillez entrer le nom de la station de métro correcte : ");
                    nom = Console.ReadLine() ?? "";  // Redemander la station
                }
            }
            else
            {
                Console.WriteLine("Aucune station similaire n'a été trouvée.");
                // Demander à l'utilisateur de saisir une autre station
                Console.Write("Veuillez entrer le nom de la station de métro correcte : ");
                nom = Console.ReadLine() ?? "";
            }
        }
        Console.WriteLine(" ✅ La station sélectionnée est valide.");
    }
}
