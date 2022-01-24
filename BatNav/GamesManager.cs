using System;
using System.Collections.Generic;


namespace BatNav
{
    // Gestionnaire de parties
   
    public static class GamesManager
	{
		// Liste des parties en cours
		private static List<BattleShipField> bSfList;
		public static AppDef.GameStatus GameStatus { get; set;}

		private static List<Gamer> gamersList;

		// ID du gamer viruel pour jouer contre l'ordinateur
		private static Guid _cyberPlayer;


		//constructeur de la classe GamesManager
		static GamesManager()
		{
			gamersList = new List<Gamer>();
			bSfList = new List<BattleShipField>();


			// mode contre l'ordinateur
			// création d'un gamer ennemi virtuel
			_cyberPlayer = CreateGamer("Skynet");

		}

		// Création d'un gamer par son pseudo
		// l'instanciation du gamer va lui attribuer un identifiant unique Guid
		static Guid CreateGamer(string pseudo)
        {
			Gamer newGamer;
			// Si le Gamer n'existe pas déjà
			if ((newGamer=gamersList.Find(g => g.Pseudo == pseudo)) == null)
			{
				// On le crée
				newGamer = new Gamer(pseudo);
				gamersList.Add(newGamer);
			};
			return (newGamer.ID);
        }
		// Création d'une partie et ajout dans la liste des parties
		public static Tuple<Guid, Guid> CreateBattleShipGame(string playerOnePseudo)
        {
			// Le créateur de la partie est le premier joueur 
			// il sera inscrit par défaut dans la liste A des joueurs alliés
			var gamerID = CreateGamer(playerOnePseudo);
			BattleShipField newBsF = new BattleShipField(1, gamerID);
			// ajout du joueur ennemi virtuel
			newBsF.JoinBattle(_cyberPlayer, true);
			// ajout de la partie à la liste des parties
			bSfList.Add(newBsF);
			// renvoie l'ID du créateur et l'ID de la partie
			// @TODO030 TUPLES
			// Compléter ici
			//
			// --
        }

		// obtient une partie par son ID dans la liste des parties
		public static BattleShipField GetBattleShipGame(Guid GameID)
        {
			return (bSfList.Find(b => b.GameID == GameID));
        }

		// obtention du pseudo d'un gamer identifié par son ID
		public static string GetGamerPseudo(Guid gamerId)
        {
			Gamer gamer = gamersList.Find(g => g.ID == gamerId);

            if (gamer != null)
            {
				return (gamer.Pseudo);
            }
			return (String.Empty);
        }
	}
}
