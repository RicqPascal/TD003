// importation des espaces de nommage utiles
using System.Timers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using System.Threading;

// Ajout de classes dans l'espace de nommage BatNav de l'application
namespace BatNav
{
    public static partial class AppDef
	{
		// temps au bout duquel l'impact d'un tir est effacé	
		public static int eraseEllipseDelay = 1000;
		public static int regenCyclesInitialCredit = 5;
		
		// définit l'orientation du bateau et donc la position de la proue
		public enum Cap
		{
			W,  // Ouest
			N,	// Nord
			E,	// Est
			S	// Sud
		}

		// pas utilisé, définit le sens d'avance
		public enum Machinery
		{
			Ahead,	// avant
			Astern,	// arrière
			Stop	// stop
		}
		// pas utilisé, définit le régime des moteurs
		public enum Power
		{
			Full,
			Half,
			Slow
		}
		// Etat d'un bateau, d'un tir
		public enum State
		{
			// Flotte, A l'eau
			Afloat = 1, 
			// Touché
			Struck = 2, 
			// Coulé 
			Sank =3,
			// refusé : plus de munition par exemple
			Denied = 4
		}

		// Etat d'une partie
		public enum GameStatus
		{
			// En pause
			Idle,
			// Partie en cours
			Running,
			// Terminé avant la fin
			Terminated,
			// Terminé normalement
			Completed
			// Prévoir de mettre en place une sauvegarde et la possibilité de reprendre une partie
		}
		public enum PlayerStatus
		{
			// Gagnant
			Winner,
			// Perdant
			Loser,
			// pas encore fixé
			NotSet
		}
	}
	
}
