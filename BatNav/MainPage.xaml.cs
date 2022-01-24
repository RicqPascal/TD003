using System;
using System.Collections.Generic;
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
using Windows.Media.Audio;
using Windows.System;
using BatNav;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BatNav
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

        // Une partie des définitions globales,
        // Le reste est dans le fichier Naval.cs.
        // C'est donc une classe partielle statique.
    public static partial class AppDef
    {
        // Dimensions de la Mer en pixel
        public static int largeurMer = 400;
        public static int hauteurMer = 400;
        public static double largeurMenu = 300;
        // largeur de la mer en nombre de colonnes
        public static int nbCol = 20;
        // hauteur de la mer en nombre de lignes
        public static int nbRow = 20;

        // Création de pinceaux pour le remplissage des ellipses correspondant à des 
        // élements de mer, des élements de bateau ou des tirs
        // Tirs
        public static SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.IndianRed);
        // Mer
        public static SolidColorBrush blueBrush = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
        // Visualisation des bateaux pour deboggage
        public static SolidColorBrush greenBrush = new SolidColorBrush(Windows.UI.Colors.Green);
        // Visualisation des bateaux pour deboggage
        public static SolidColorBrush pinkBrush = new SolidColorBrush(Windows.UI.Colors.DeepPink);

        // Permet de garder une référence sur l'instance de MainPage
        // pour afficher provisoirement l'emplacement des bateaux sur la mer en mode debug
        public static MainPage debugP;

        // Une partie est instanciée et prête à être jouée
        public static bool readyPlayerOne = false;
        // Etat de la partie en cours
        static public AppDef.GameStatus currentGameStatus = AppDef.GameStatus.Completed;


    }
    public static class MyExtensions
    {
        // méthodes d'extension: Pas besoin pour l'instant.
    }

    public sealed partial class MainPage : Page
    {
        // grille cf explication ci-dessous
        static public Grid myGrid;
        static public Grid enemyGrid;
        static public Sea mySea;
        static public Sea enemySea;
        // Classe qui gère le jeu (flotte, joueurs, tirs, partie)
        static public BattleShipField bSF;
        // Identifiant de la partie courante
        static public Guid bSfId;
        // ID du joueur local
        static public Guid myPlayerID;
        
        

        public MainPage()
        {
            this.InitializeComponent();

           
            // création d'une grille qui va contenir des objets enfants du type Windows.UI.Xaml.Shapes.Ellipse
            myGrid = new Grid();
            myGrid.Name = "mySea";
            // la grille est callée graphiquement sur un objet border. Cf fichier MainPage.xaml.
            seaBorder.Child = myGrid;

            // création d'une deuxième grille pour la flotte ennemie
            enemyGrid = new Grid();
            enemyGrid.Name = "enemySea";

            // ajout d'une fonction qui va gérer l'évenement du clic sur la grille (Mer) 
            enemyGrid.PointerPressed += EngageShot;
            // la grille est callée graphiquement sur un objet border. Cf fichier Xaml)
            seaBorderEnemy.Child = enemyGrid;
            
            // Création des deux mers constituées d'éllipses
            mySea = new Sea(myGrid, AppDef.nbRow, AppDef.nbCol, this);
            enemySea = new Sea(enemyGrid, AppDef.nbRow, AppDef.nbCol, this);

            // pour l'affichage des bateaux en mode debug
            AppDef.debugP = this;

            // initialisation de l'état du jeux
            InitGameFields();
        }
        // Instanciation d'une nouvelle partie
        public void PlayTheGameAgain()
        {
            try
            {
                // si la partie est terminée ou a été abortée
                if (GamesManager.GameStatus == AppDef.GameStatus.Completed || 
                    GamesManager.GameStatus == AppDef.GameStatus.Terminated)
                {
                    // on peut démarrer une nouvelle partie
                    //currentGameStatus = AppDef.GameStatus.Idle;
                    mySea.Repaint();
                    enemySea.Repaint();
                    (bSfId, myPlayerID)=GamesManager.CreateBattleShipGame("Name1");
                    bSF = GamesManager.GetBattleShipGame(bSfId);

                    if (bSF != null)
                    {
                        var playerBoats = bSF.GetBoats(myPlayerID);
                        DisplayPlayerBoats(playerBoats, false);
                        //@TODO: debug affiche les bateaux ennemis
                        var enemiesBoats = bSF.GetEnemiesBoats(myPlayerID);
                        DisplayPlayerBoats(enemiesBoats, true);


                        AppDef.readyPlayerOne = true;
                        GamesManager.GameStatus = AppDef.GameStatus.Running;

                        //@TODO100 EVENT : Gestion d'un évenement de riposte à un tir 
                        // STEP4 : inscription de la méthode de riposte qui va être activée par l'évenement
                        bSF.riposteEvent +=  HandleRiposte;

                    }

                    StartButton.Content = "Stop";
                }
                else
                {
                    StartButton.Content = "Play";
                    TextBoxStatus.Text = "";
                    GamesManager.GameStatus = AppDef.GameStatus.Terminated;
                }
            }
            catch (Exception)
            {

                throw;
            }


        }

        //@TODO100 EVENT : Gestion d'un évenement de riposte à un tir 
        // STEP3 : Création d'une méthode qui peut gérer un évenement riposte

        // Compléter ici
       
        // --

        // Pas utilisé pour l'instant 
        private void KeyPressed(object sender, KeyRoutedEventArgs e)
        {
            Windows.System.VirtualKey touche = e.Key;

            //switch (e.Key)
            //{
            //case VirtualKey.A:
            //    textBox.Text += "hello";
            //    break;
            //case VirtualKey.Enter:
            //    textBox.Text += "Enter ";
            //    break;
            //case (VirtualKey)187:
            //    textBox.Text += "=";
            //    break;

            //}

        }

        // Fait apparaitre le point d'impact en rouge et prend en charge le tir quand on clique sur une ellipse
        public static void EngageShot(object sender, PointerRoutedEventArgs e)

        {
            // si une partie est en cours d'exécution
            if (GamesManager.GameStatus == AppDef.GameStatus.Running)
            {
                // récupération du statut du joueur
                AppDef.PlayerStatus playerStatus = bSF.GetPlayerStatus(myPlayerID);

                // si le joueur courant n'a pas encore perdu la partie
                if ( playerStatus!= AppDef.PlayerStatus.Loser)
                {
                    // si on a cliqué sur une ellipse, on la peint en rouge et on appelle la méthode
                    // qui va rechercher l'élément de mer concerné (sea.FireAt() )
                    // cela permet de retrouver l
                    if (sender is Windows.UI.Xaml.Shapes.Ellipse )
                    {
                        (sender as Ellipse).Fill = AppDef.redBrush;
                        // Déclenchement du tir
                        enemySea.FireAt(sender as Ellipse);
                    }

                }
               
            }
      
        
        }

        // Affiche les coordonnées du tir en (ligne, colonne) dans les textBoxes
        public void DisplayImpactPosition(Point pos)
        {
            textBox1_Y.Text = Convert.ToString(pos.Y);
            textBox1_X.Text = Convert.ToString(pos.X);
        }
        // Affiche le nombre de tirs restant pour le joueur
        public void displayPlayerRemainingShots(int value)
        {
            textBoxRemainingShots.Text = value.ToString();
        }
        // Affihe le status de la partie en cours pour le joueur
        public void displayPlayerGameStatus(string message)
        {
            TextBoxStatus.Text = message;
        }
        // Affiche le nombre d'impacts rééalisés par le joueur
        public void displayPlayerCurrentImpactsNb(int value)
        {
            nbTotalImpacts.Text = value.ToString();
        }
        // mise à jour des champs lors du démarrage d'une nouvelle partie
        // avec les informations reçues de bd
        public void InitGameFields()
        {
            GamesManager.GameStatus = AppDef.GameStatus.Completed;
        }
        // Lancement d'une nouvelle partie quand on clique sur le bouton PLAY
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Instanciation d'une nouvelle partie
            PlayTheGameAgain();
        }
        public void SetActionMessage(string message)
        {
            StartButton.Content = message;
        }
        /// <summary>
        /// Gestion du choix du placement Automatique des bateaux
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleAutoBoatLayout(object sender, RoutedEventArgs e)
        {
            CheckBox cbAutoLayout = sender as CheckBox;

            if (cbAutoLayout.IsChecked == true )
            {
                // obtenir un placement automatique des bateaux pour ce joueur
                
                var playerBoats = bSF.GetBoats(myPlayerID);
                DisplayPlayerBoats(playerBoats, false);
                //@TODO: debug affiche les bateaux ennemis
                var enemiesBoats = bSF.GetEnemiesBoats(myPlayerID);
                DisplayPlayerBoats(enemiesBoats, true);

            }

            if ( cbAutoLayout.IsChecked == false)
            {
                // obtenir la liste des bateaux
                // mettre le jeu en attente de composition manuelle complète
                // publier la flotte du joueur local
                // 

            }

        }
        // Pour le debug, affiche le bateau.
        // Affichage des bateaux
        private void DisplayBoat(Boat boat, Sea sea)
        {
            SeaElement aPoint;
            foreach (ShipElement shipElt in boat.ShipElt)
            {
                aPoint = sea.SeaElements.Find(sealElement => sealElement.Pos == shipElt.elt);
                if (aPoint != null)
                {
                    // aPoint.ellipse.Fill = 
                    aPoint.RefreshColor(shipElt, true);
                }
            }

        }
   
        /// <summary>
        /// Affiche une flotte sur la mer
        /// </summary>
        /// <param name="playerBoats">la flotte</param>
        /// <param name="other">value: other = vrai: mer de droite other = faux : mer de gauche</param>
        public void DisplayPlayerBoats(List<Boat> playerBoats, bool other)
        {
            
            var sea = other ? enemySea : mySea;
             
            foreach (Boat boat in playerBoats)
            {
                DisplayBoat(boat, sea);
            }
        }
    }
}
