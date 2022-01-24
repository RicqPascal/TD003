// importation des espaces de nommage utiles
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

// Ajout de classes dans l'espace de nommage BatNav de l'application
namespace BatNav
{
    // Gestion de l'affichage de la mer pour un joueur
    public class Sea
    {
		public List<SeaElement> SeaElements = new List<SeaElement>();
		public int Row { get; set; }
		public int Col { get; set; }
		
		private MainPage MainP;

		public Sea(Grid grid, int row, int col, MainPage main)
        {
			Row = row;
			Col = col;

			MainP = main;

			int ellipseWidth = AppDef.largeurMer / row;
			int ellipseHeight = AppDef.hauteurMer /col;

			int leftMargin = 0, rightMargin = 0, topMargin = 0, bottomMargin = 0;
			Thickness thic = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
			for ( int r = 0; r < Row; r++)
            {
				for (int c = 0; c < Col; c++)
                {
					leftMargin = c * ellipseWidth;
					rightMargin = AppDef.largeurMer - ((c + 1) * ellipseWidth);
					topMargin = r * ellipseHeight;
					bottomMargin = AppDef.largeurMer - ((r + 1) * ellipseHeight);
					thic.Left = leftMargin;
					thic.Top = topMargin;
					thic.Right = rightMargin;
					thic.Bottom = bottomMargin;
					SeaElement seaElement = new SeaElement(grid, thic, r, c, ellipseWidth, ellipseHeight, 0);
					SeaElements.Add(seaElement);
				}
            }
		// ...
        }
		// méthode chargée de retouver l'élément de mer atteint
		// et qui communique la position du tir, en ligne colonne, au champ de bataille qui va étudier
		// les conséquences du tir
		public void FireAt(Ellipse ellipse)
			
		{
			// code de retour de la méthode ProcessStrike
			AppDef.State code = 0;
			// Element de mer impacté
			SeaElement impactPoint;
			try
			{
				impactPoint = SeaElements.Find(sealElement => sealElement.ellipse == ellipse);
				if (impactPoint != null)
				{
					
					// affichage coordonnées du point d'impact
					MainP.DisplayImpactPosition(impactPoint.Pos);

					
					int nb = MainPage.bSF.GetPlayerRemainStrike(MainPage.myPlayerID);
					// si plus de munition
					if (nb == 0)
					{
						MainP.displayPlayerGameStatus("Plus de munitions");
					}
					else
					{
					// gestion du tir
						code = MainPage.bSF.ProcessStrike(MainPage.myPlayerID, impactPoint);
						nb = MainPage.bSF.GetPlayerRemainStrike(MainPage.myPlayerID);
						int nbStr = MainPage.bSF.GetPlayerNbStrucks(MainPage.myPlayerID);

						MainP.displayPlayerRemainingShots(nb);
						MainP.displayPlayerCurrentImpactsNb(nbStr);



						// tir à l'eau
						if (code == AppDef.State.Afloat)
						{
							MainP.displayPlayerGameStatus("A l'eau !");
						}
						// Si touché
						else if (code == AppDef.State.Struck)
						{

							MainP.displayPlayerGameStatus("Touché !");
							if (GamesManager.GameStatus == AppDef.GameStatus.Completed || GamesManager.GameStatus == AppDef.GameStatus.Terminated)
							{
								MainP.SetActionMessage("Play again");
							}
						}
						// Si Coulé
						else if (code == AppDef.State.Sank)
						{
							MainP.displayPlayerRemainingShots(nb);
							// si la partie est terminée
							if (GamesManager.GameStatus == AppDef.GameStatus.Completed)
							{
								// notifier les clients en mode web
								// affiche gagné

								var enemyFloat = MainPage.bSF.GetEnemiesBoats(MainPage.myPlayerID);
								//var alliesFloat = MainPage.bSF.GetAlliesBoats(MainPage.myPlayerID);
								var myFloat = MainPage.bSF.GetBoats(MainPage.myPlayerID);
								MainP.DisplayPlayerBoats(myFloat ,false);
								MainP.DisplayPlayerBoats(enemyFloat, true);
								MainP.displayPlayerGameStatus("That's All, Folks!");
								MainP.SetActionMessage("Play again");
							}
							else
							{
								MainP.displayPlayerGameStatus("Coulé !");
							}
						}
						if (MainPage.bSF.GetPlayerStatus(MainPage.myPlayerID) == AppDef.PlayerStatus.Loser)
						{
							var enemyFloat = MainPage.bSF.GetEnemiesBoats(MainPage.myPlayerID);
							//var alliesFloat = MainPage.bSF.GetAlliesBoats(MainPage.myPlayerID);
							var myFloat = MainPage.bSF.GetBoats(MainPage.myPlayerID);
							MainP.DisplayPlayerBoats(myFloat, false);
							MainP.DisplayPlayerBoats(enemyFloat, true);
							MainP.displayPlayerGameStatus("Perdu..");
							// En multijoueurs, il faudra attendre que tout le monde ait épuisé ses munitions
							// pour être déclaré perdant.
						}
					}

				}
			}
			catch (Exception)
			{
				Point impactHorsZone = new Point(-1,-1);
				
			}
	
			
		}
		// redessin de la mer en bleu
		public void Repaint()
		{
			foreach (var item in SeaElements)
			{
				item.ellipse.Fill=AppDef.blueBrush;
			}
		}

	}
	
}
