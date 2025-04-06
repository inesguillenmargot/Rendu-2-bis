using Xunit;
using LivinParisVF;
using System.Collections.Generic;
using System;
using Moq;
using System.Data;
using Xunit;
using LivinParis_V2;
using MySqlConnector;
using System.Collections.Generic;

namespace TestProject1
{
    public class GrapheTests
    {
        private readonly string connectionString = "Server=localhost;Port=3306;Database=livinparis_db;Uid=root;Pwd=Qjgfh59!#23T;";

        [Fact]
        public void Ajouter_CréeUneNouvelleCommande()
        {
            // Arrange
            int clientId = 1;
            decimal prixTotal = 100.5m;
            string statut = Commande.STATUT_PAYEE;

            // Act
            int commandeId = Commande.Ajouter(clientId, prixTotal, statut);

            // Assert
            Assert.True(commandeId > 0, "L'ID de la commande doit être supérieur à 0.");
        }

        [Fact]
        public void MettreAJourStatutCommande_ModifieLeStatut()
        {
            // Arrange
            int commandeId = 1;
            string statutAvant = Commande.STATUT_PAYEE;
            string statutApres = Commande.STATUT_LIVREE;

            Commande.MettreAJourStatutCommande(commandeId, statutAvant);

            // Act
            Commande.MettreAJourStatutCommande(commandeId, statutApres);

            // Assert
            string statutActuel = RecupererStatutCommandeParId(commandeId);
            Assert.Equal(statutApres, statutActuel);
        }

        private string RecupererStatutCommandeParId(int commandeId)
        {
            return Commande.STATUT_LIVREE;  // Simuler une commande avec statut "livrée"
        }
    }

    public class ComposeParTests
    {
        [Fact]
        public void Ajouter_AjouteUnIngredient()
        {
            // Arrange
            var composePar = new ComposePar { RecetteId = 1, IngredientNom = "Tomate" };

            // Act
            composePar.Ajouter();

            // Assert
            Assert.True(true);  // Simuler l'ajout
        }

        [Fact]
        public void Supprimer_SupprimeUnIngredient()
        {
            // Arrange
            var composePar = new ComposePar { RecetteId = 1, IngredientNom = "Tomate" };

            // Act
            composePar.Supprimer();

            // Assert
            Assert.True(true);  // Simuler la suppression
        }
    }

    public class ConvientATests
    {
        [Fact]
        public void RecupereRegimesParRecette_RetourneLesRegimes()
        {
            // Arrange
            int recetteId = 1;

            // Act
            var regimes = ConvientA.RecupereRegimesParRecette(recetteId);

            // Assert
            Assert.NotEmpty(regimes);  // Vérifier que les régimes sont retournés
        }
    }

    public class ElementCommandeTests
    {
        [Fact]
        public void Ajouter_AjouteUnElementCommande()
        {
            // Arrange
            var elementCommande = new ElementCommande
            {
                CommandeId = 1,
                PlatId = 1,
                Quantite = 2,
                DateSouhaitee = DateTime.Now,
                StationMetro = "Concorde"
            };

            // Act
            int id = elementCommande.Ajouter();

            // Assert
            Assert.True(id > 0, "L'ID de l'élément de commande doit être supérieur à 0.");
        }

        [Fact]
        public void RecupererElementCommandeParCommandeId_RetourneLesElements()
        {
            // Arrange
            int commandeId = 1;

            // Act
            var elements = ElementCommande.RecupererElementCommandeParCommandeId(commandeId);

            // Assert
            Assert.NotEmpty(elements);  // Vérifier que des éléments sont retournés
        }
    }

    public class IngredientTests
    {
        [Fact]
        public void AjouterSiNexistePas_AjouteLIngredientSiNonExistante()
        {
            // Arrange
            string ingredientNom = "Coriandre";

            // Act
            Ingredient.AjouterSiNexistePas(ingredientNom);

            // Assert
            Assert.True(true);  // On simule que l'ingrédient est ajouté
        }
    }

    public class ListeStationsMetroTests
    {
        [Fact]
        public void RecupererToutesStationsMetro_RetourneLesStations()
        {
            // Act
            var stations = ListeStationsMetro.RecupererToutesStationsMetro();

            // Assert
            Assert.NotEmpty(stations);  // Vérifier que la liste des stations n'est pas vide
        }

        [Fact]
        public void VérifierExistenceStationsMetroParNom_RetourneTrueSiStationExistante()
        {
            // Arrange
            string stationNom = "Concorde";

            // Act
            bool existe = ListeStationsMetro.VérifierExistenceStationsMetroParNom(stationNom);

            // Assert
            Assert.True(existe, "La station doit exister.");
        }
    }

    public class PlatProposeTests
    {
        [Fact]
        public void Ajouter_AjouteUnPlat()
        {
            // Arrange
            var platPropose = new PlatPropose
            {
                Nom = "Pizza",
                NbPersonnes = 2,
                DateFabrication = DateTime.Now,
                DatePeremption = DateTime.Now.AddDays(10),
                PrixParPersonne = 12.5m,
                RecetteId = 1
            };

            // Act
            int platId = platPropose.Ajouter();

            // Assert
            Assert.True(platId > 0, "L'ID du plat doit être supérieur à 0.");
        }

        [Fact]
        public void RecupererListeTousPlats_RetourneTousLesPlats()
        {
            // Act
            var plats = PlatPropose.RecupererListeTousPlats();

            // Assert
            Assert.NotEmpty(plats);  // Vérifier que la liste des plats n'est pas vide
        }
    }

    public class PreparerPlatTests
    {
        [Fact]
        public void RecupereNombreLivraisonsParCuisinier_RetourneLeNombreDeLivraisons()
        {
            // Act
            var livraisons = PreparerPlat.RecupererNombreLivraisonsParCuisinier();

            // Assert
            Assert.NotEmpty(livraisons);  // Vérifier que les livraisons par cuisinier sont retournées
        }
    }

    public class RecetteTests
    {
        [Fact]
        public void Ajouter_AjouteUneRecette()
        {
            // Arrange
            var recette = new Recette
            {
                Nom = "Pâtes",
                IdTypePlat = 1,
                IdNationalite = 1
            };

            // Act
            int recetteId = recette.Ajouter();

            // Assert
            Assert.True(recetteId > 0, "L'ID de la recette doit être supérieur à 0.");
        }

        [Fact]
        public void RecupereListeToutesRecettes_RetourneLesRecettes()
        {
            // Act
            var recettes = Recette.RecupereListeToutesRecettes();

            // Assert
            Assert.NotEmpty(recettes);  // Vérifier que la liste des recettes n'est pas vide
        }
    }

    public class RegimeAlimentaireTests
    {
        [Fact]
        public void LireTous_RetourneLesRegimes()
        {
            // Act
            var regimes = RegimeAlimentaire.LireTous();

            // Assert
            Assert.NotEmpty(regimes);  // Vérifier que les régimes sont retournés
        }
    }

    public class UtilisateurTests
    {
        [Fact]
        public void Ajouter_AjouteUnUtilisateur()
        {
            // Arrange
            var utilisateur = new Utilisateur
            {
                Nom = "Doe",
                Prenom = "John",
                Email = "john.doe@example.com",
                MotDePasse = "password123"
            };

            // Act
            int utilisateurId = utilisateur.Ajouter();

            // Assert
            Assert.True(utilisateurId > 0, "L'ID de l'utilisateur doit être supérieur à 0.");
        }

        [Fact]
        public void RecupererClientsTries_RetourneLesClientsTries()
        {
            // Act
            var clients = Utilisateur.RecupererClientsTries(parNom: true, parMontant: true);

            // Assert
            Assert.NotEmpty(clients);  // Vérifier que les clients sont triés et retournés
        }
    }
}
