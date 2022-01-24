using System;
using System.Timers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace BatNav
{
	// La classe BattleShipField est le moteur du jeu de bataille 
	// elle ne doit pas interférer avec le rendu graphique.
	// elle permet d'instancier une partie
	public class BattleShipField  
	{
		// identifiant de la partie
		private Guid gameID;

		// Déclaration d'un timer pour faire repasser les points d'impact au bleu
		private System.Timers.Timer aTimer;
		// Liste des éléments de mer qui ont été touchés qui doivent repasser au 'bleu' au bout d'un certain temps
		// Utilisation d'une collection de données Safe-Thread afin d'éviter les conflits d'accès concurrents
		// il faut détruire manuellement cette collection quand elle n'est plus utilisée (méthode Dispose dans finalizer)
		public BlockingCollection<SeaElement> impactedSeaElements = new BlockingCollection<SeaElement>();

		// générateur de nombres aléatoires
		private Random val = new Random();
		// niveau de jeu 
		// Level : joue sur le nombre de tirs autorisés par exemple
		public int Level { get; set; }
		public int NbStrikes { get => _nbStrikes ; }
		private int _nbStrikes = 100;
		// obtention de l'ID de cette bataille
        public Guid GameID { get => gameID; }

		//@TODO100 EVENT : Gestion d'un évenement de riposte à une tir 
		// STEP1 : Création d'une variable du type event args
		
		// compléter ici


		//
        // Liste des joueurs du camp A
        private List<Player> playersListA = new List<Player>();
		// Liste des joueurs du camp B
		private List<Player> playersListB = new List<Player>();

		// La flotte qui contient les vaisseaux de tous les joueurs.
		// si un bateau est touché, il faut le retrouver et marquer un de ses éléments 'touché' et
		// vérifier s'il n'est pas coulé
		public List<Boat> boatList = new List<Boat>();


		// constructeur 
		public BattleShipField(int level, Guid playerOneID)
		{
			// création d'un nouvel ID pour cette partie
			gameID = Guid.NewGuid();
			// fixation du niveau de jeu choisi
			Level = level;
			

			switch (Level)
			{
				case 1:
					_nbStrikes = 70;
					AppDef.eraseEllipseDelay = 3000;
					break;
				case 2:
					_nbStrikes = 60;
					AppDef.eraseEllipseDelay = 2000;
					break;
				default:
					_nbStrikes = 30;
					break;
			}

			string newPlayerPseudo = GamesManager.GetGamerPseudo(playerOneID);

			var player = new Player(newPlayerPseudo, playerOneID, NbStrikes, AppDef.PlayerStatus.NotSet);
			playersListA.Add(player);
			
			this.CreatePlayerFloat(playerOneID);
			this.SetTimer();


		}
		// finaliseur de la classe BattleShip
		~BattleShipField()
		{
			// destruction du timer
			aTimer.Stop();
			aTimer.Dispose();

			// destruction de la liste des éléments de mer impactés
			try
			{
				foreach (SeaElement item in impactedSeaElements.GetConsumingEnumerable()) { }
				impactedSeaElements.Dispose();
			}
			catch (Exception)
			{

				throw;
			}
		}
		private void SetTimer()
		{
			// Création d'un timer avec un interval de 10 s
			aTimer = new System.Timers.Timer(AppDef.eraseEllipseDelay);
			aTimer.Elapsed += RegenAll;
			//aTimer.SynchronizingObject = this;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
		}

		public Guid RequestPlayerID(string name)
		{
			Guid playerID;
			Player player;
			List<Player> playersList;
			// si le joueur est dans la liste A on renvoie la liste A sinon on cherche dans la liste B
			playersList = (playersListA.Find(p => p.Pseudo == name) != null) ? playersListA : playersListB;
			player = playersList.Find(play => play.Pseudo.Equals(name));
			if (player != null)
			{
				return player.ID;
			}
			playerID = new Guid();
			return playerID;
		}

		// inscription d'un nouveau joueur allié ou ennemi
		// @TODO040 LISTES GENERIQUES
		 public void JoinBattle(Guid gamerID, bool enemy)
        {
			// compléter ici

		}
		// calcul d'un cap aléatoire
		private AppDef.Cap RandomCap()
		{
			AppDef.Cap rndCap;

			int cap = val.Next(0, 3);
			rndCap = (AppDef.Cap)cap;

			return rndCap;
		}
		// calcul d'une position aléatoire
		private Point RandomPos()
		{
			Point pos = new Point
			{
				X = val.Next(0, AppDef.nbCol),
				Y = val.Next(0, AppDef.nbRow)
			};

			return pos;
		}


		// recherche un emplacement disponible sur la mer et fixe la position d'un bateau
		private void SetPostion(Boat boat)
		{
			// Pour plus tard, tester si la mer à des dimensions largement supérieures à celle d'un bateau.

			// On fixe une orientation aléatoire
			boat.Cap = RandomCap();

			Point tmpPoint;

			// position non encore déterminée
			Boolean put = false;

			while (!put)
			{
				// calcul d'une position aléatoire
				tmpPoint = RandomPos();

				// On décale le bateau pour le faire tenir sur la mer le cas échéant


				// Si le bateau fait route vers l'ouest et qu'il déborde à droite, on le décale à gauche
				if (boat.Cap == AppDef.Cap.W && (tmpPoint.X + boat.Size > AppDef.nbCol))
				{
					tmpPoint.X -= (tmpPoint.X + boat.Size) - AppDef.nbCol;
				}
				else
				{
					// Si le bateau fait route vers le nord et qu'il déborde en-bas, on le décale vers le haut
					if (boat.Cap == AppDef.Cap.N && (tmpPoint.Y + boat.Size > AppDef.nbRow))
					{
						tmpPoint.Y -= (tmpPoint.Y + boat.Size) - AppDef.nbRow;
					}
					else
					{
						// Si le bateau fait route vers l'Est et qu'il déborde à gauche, on le décale vers la droite
						if (boat.Cap == AppDef.Cap.E && (tmpPoint.X - boat.Size + 1 < 0))
						{
							tmpPoint.X = boat.Size - 1;
						}
						else
						{
							// Si le bateau fait route vers le Sud et qu'il déborde en-haut, on le décale vers le bas
							if (boat.Cap == AppDef.Cap.S && (tmpPoint.Y - boat.Size + 1 < 0))
							{
								tmpPoint.Y = boat.Size - 1;
							}
						}
					}

				}
				// On calcule les coordonnées de tous les éléments du bateau

				int incX = 0;
				int incY = 0;
				switch (boat.Cap)
				{
					case AppDef.Cap.W:
						incX = 1;
						break;
					case AppDef.Cap.N:
						incY = 1;
						break;
					case AppDef.Cap.E:
						incX = -1;
						break;
					case AppDef.Cap.S:
						incY = -1;
						break;
					default:
						break;
				}
				for (int i = 0; i < boat.Size; i++)
				{
					boat.ShipElt[i].statusCode = AppDef.State.Afloat;
					boat.ShipElt[i].elt.X = tmpPoint.X + i * incX;
					boat.ShipElt[i].elt.Y = tmpPoint.Y + i * incY;
				}
				// On déclare le bateau placé
				put = true;

				// On recherche une collision possible avec un autre bâtiment
				// s'il y a collision
				if (boatList.Find(aBoat => CheckCollision(aBoat, boat)) != null)
				{
					// il faut chercher une autre position
					// et ça peut durer longtemps si la mer est trop petite et qu'il y trop de bateaux
					// pour l'instant cela n'est pas géré.
					// il faudrait mettre en place des gardes-fous
					put = false;
				}

			}

		}

		// Recherche si deux bateaux ont au moins un élement qui possède les mêmes coordonnées
		private Boolean CheckCollision(Boat boat1, Boat boat2)
		{
			Boolean collision = false;
			int idx1 = 0;
			int idx2 = 0;

			while (!collision && boat1.Afloat == AppDef.State.Afloat && boat2.Afloat == AppDef.State.Afloat && idx1 < boat1.Size)
			{
				idx2 = 0;
				while (!collision && idx2 < boat2.Size)
				{
					if (boat1.ShipElt[idx1].elt == boat2.ShipElt[idx2].elt)
					{
						collision = true;
					}
					else
					{
						idx2 += 1;
					}
				}
				idx1 += 1;
			}

			return collision;
		}

		

		// Création de l'ensemble des bateaux d'un joueur
		private void CreatePlayerFloat(Guid playerID)
		{
			Boat tmpBoat;
			

			
				tmpBoat = new Boat("Porte-avions", 5, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
				tmpBoat = new Boat("Destroyer", 4, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
				tmpBoat = new Boat("Frégate", 3, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
				tmpBoat = new Boat("Frégate", 3, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
				tmpBoat = new Boat("Sous-Marin", 2, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
				tmpBoat = new Boat("Sous-Marin", 2, 0, playerID);
				SetPostion(tmpBoat);
				boatList.Add(tmpBoat);
				//DisplayBoat(tmpBoat);
			
		}




		// Gestion du tir effectué par un joueur sur un élément de mer :
		// il faut mettre à jour l'état des bateaux
		// Mettre en place la gestion du level et des tirs restants pour les joueurs
		// et décider de la fin de la partie
		public AppDef.State ProcessStrike(Guid playerID, SeaElement strikeElt)
		{

			// status du tir
			AppDef.State code = AppDef.State.Afloat; // A l'eau
			// Identification du joueur
			Player thePlayer;
			List<Player> playersList = playersListA.Concat(playersListB).ToList();
			if ((thePlayer = playersList.Find(player => player.ID == playerID)) != null)
			{
				if (thePlayer.RemainStrike < 1)
				{
					// le tir n'est pas traité
					return code = AppDef.State.Denied;
				}
				thePlayer.RemainStrike -= 1;
				// remettre le nombre de cycles de l'élément de mer au max
				strikeElt.RegenCycles = AppDef.regenCyclesInitialCredit;
				//@TODO200
				// si l'élément de mer impacté a déjà été ajouté alors ne plus l'ajouter dans cette liste.
				// compléter ici
				

				// -- 

			}
			// Recherche d'un impact sur un bateau
			Point impactPoint = strikeElt.Pos;
			Boolean struck = false;
			Boolean allPlayerBoatsSank = false;
			Guid loserID;
			int nbBoats = boatList.Count;
			int i = 0;
			while (!struck && i < nbBoats)
			{
				Boat boat = boatList.ElementAt(i);
				int j = 0;
				while (!struck && j < boat.Size)
				{
					// si un elt de bateau est touché
					if (boat.ShipElt[j].elt == impactPoint)
					{
						// si bateau déjà touché à cet endroit on considère que c'est un tir à l'eau
						if (boat.ShipElt[j].statusCode == AppDef.State.Struck)
						{
							code = AppDef.State.Afloat;
							struck = true;
						}
						else
						{
							struck = true;
							thePlayer.NbStruck += 1;
							code = AppDef.State.Struck;
							boat.ShipElt[j].statusCode = AppDef.State.Struck;

							// regarde si le bateau est coulé
							Boolean sank = true;
							for (int e = 0; e < boat.Size; e++)
							{
								if (boat.ShipElt[e].statusCode == AppDef.State.Afloat)
								{
									sank = false;
								}
							}
							// si le bateau est coulé
							if (sank)
							{
								// alors déclare bateau coulé
								boat.Afloat = AppDef.State.Sank;
								code = AppDef.State.Sank;
								// regarde si toute la flotte du joueur est coulée
								Boat floatingBoat;
								// on recherche un bateau qui flotte encore pour ce joueur
								floatingBoat = boatList.Find(fboat => fboat.Afloat == AppDef.State.Afloat && fboat.Owner == boat.Owner);
								if (floatingBoat == null)
								{
									// ce joueur à perdu
									allPlayerBoatsSank = true;
									loserID = boat.Owner;
									// on déclare ce joueur perdant
									// et tous les autres gagnants
									// sauf s'il n'y a qu'un seul joueur qui tire sur ses bateaux :-D
									if (playersList.Count == 1)
									{
										thePlayer.Status = AppDef.PlayerStatus.Winner;
										// en fait, c'est un vrai loser !
									}
									else
									{
										foreach (Player player in playersList)
										{
											if (player.ID == loserID)
											{
												player.Status = AppDef.PlayerStatus.Loser;
											}
											else
											{
												player.Status = AppDef.PlayerStatus.Winner;
											}
										}
									}
								}
							}
						}

					}
					j += 1;
				}
				i += 1;
			}



			// Tous les bateaux d'un joueur sont coulés : GameOver
			if (allPlayerBoatsSank)
			{
				GamesManager.GameStatus = AppDef.GameStatus.Completed;
			}
			if (thePlayer.RemainStrike <= 0)
			{
				// En MULTI JOUEUR
				// on regarde s'il reste des bateaux non coulés qui ne lui appartiennent pas
				Boat floatingBoat = boatList.Find(fboat => fboat.Afloat == AppDef.State.Afloat && fboat.Owner == thePlayer.ID);

				// EN MONO JOUEUR: Tant qu'il reste un bateau
				// Boat floatingBoat = boatList.Find(fboat => fboat.Afloat == AppDef.State.Afloat);

				// si le joueur n'a plus de munition et qu'il reste un bateau ennemi
				// alors il est déclaré Perdant.
				if (floatingBoat != null)
				{
					thePlayer.Status = AppDef.PlayerStatus.Loser;
				}
			}

			// Expérimental : risposte à l'aide d'un évenement
			//@TODO100 EVENT : Gestion d'un évenement de riposte à un tir 
			// STEP2 : Emission d'un évenement riposte

			

			// résultat du tir
			return code;
		}
		// Obtention du Status d'un joueur
		public AppDef.PlayerStatus GetPlayerStatus(Guid playerID)
		{
			AppDef.PlayerStatus status = AppDef.PlayerStatus.NotSet;
			Player thePlayer;
			List<Player> playersList;
			// si le joueur est dans la liste A on renvoie la liste A sinon on cherche dans la liste B
			playersList = (playersListA.Find(p => p.ID == playerID) != null) ? playersListA : playersListB;
			thePlayer = playersList.Find(player => player.ID == playerID);
			if (thePlayer != null)
			{
				status = thePlayer.Status;
			}
			return status;
		}
		// renvoie le nombre de tirs restants pour un joueur
		public int GetPlayerRemainStrike(Guid playerID)
		{
			int nbStrikes = 0;
			Player thePlayer;
			List<Player> playersList;
			// si le joueur est dans la liste A on renvoie la liste A sinon on cherche dans la liste B
			playersList = (playersListA.Find(p => p.ID == playerID) != null) ? playersListA : playersListB;
			thePlayer = playersList.Find(player => player.ID == playerID);
			if (thePlayer != null)
			{
				nbStrikes = thePlayer.RemainStrike;
			}
			return nbStrikes;
		}
		public int GetPlayerNbStrucks(Guid playerID)
		{
			int nbStrucks = 0;
			Player thePlayer;
			List<Player> playersList;
			// si le joueur est dans la liste A on renvoie la liste A sinon on cherche dans la liste B
			playersList = (playersListA.Find(p => p.ID == playerID) != null) ? playersListA : playersListB;
			thePlayer = playersList.Find(player => player.ID == playerID);
			if (thePlayer != null)
			{
				nbStrucks = thePlayer.NbStruck;
			}
			return nbStrucks;
		}
		// récupération de la liste des bateaux d'un joueur
		public List<Boat> GetBoats(Guid playerID)
		{
			return (from boat in boatList where boat.Owner == playerID select boat).ToList();
		}
		// récupération de la liste des bateaux de la flotte ennemie
		public List<Boat> GetEnemiesBoats(Guid playerID)
		{
			List<Player> playerList;
			// si le joueur est dans la liste A on renvoie la liste B du camp adverse
			playerList = (playersListA.Find(p => p.ID == playerID) != null) ? playersListB : playersListA;

			// pour tous les bateaux appartenants à des joueurs de cette liste
			var playerIds = (from player in playerList select player.ID).ToList();
			List<Player> tmp = (from player in playerList select player).ToList();

			return (from boat in boatList where  playerIds.Contains( boat.Owner )  select boat).ToList();
		}
		public List<Boat> GetAlliesBoats(Guid playerID)
		{
			List<Player> playerList;
			// si le joueur est dans la liste A on renvoie la liste A du camp des alliés
			playerList = (playersListA.Find(p => p.ID == playerID) != null) ? playersListB : playersListA;

			// pour tout les bateaux appartenants à des joueurs de cette liste
			var playerIds = from player in playerList select playerID;


			return (from boat in boatList where playerIds.Contains(boat.Owner) select boat).ToList();
		}

		// régénération de tous les éléments de mer atteints par un tir
		// Cette méthode sera déclenchée par l'évenenement "temps écoulé" du timer
		private void RegenAll(Object source, ElapsedEventArgs e)
		{
			//
			Boolean test = true;

			// Pour tous les éléments de mer touchés
			foreach (var seaElt in impactedSeaElements)
			{

				seaElt.RegenCycles -= 1;

				// si le nombres de cycles est écoulé, 
				if (seaElt.RegenCycles <= 0)
				{
					// alors, on retire un element de la (liste) file, celui-ci en principe
					// est normalement le plus ancien

					// nb:  la variable 'inline' tmp passée en référence (out) et assignée par la méthode TryTake
					test = impactedSeaElements.TryTake(out SeaElement tmp);
					// au cas où le compteur serait devenu négatif, on le remet à zéro
					seaElt.RegenCycles = 0;
				}

				// on actualise la couleur le cas échéant
				// seaElt.RefreshColor(source, false);
				seaElt.RefreshColor(seaElt, false);
			}

		}


	}

}
