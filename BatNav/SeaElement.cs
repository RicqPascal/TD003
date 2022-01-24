// importation des espaces de nommage utiles
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// Ajout de classes dans l'espace de nommage BatNav de l'application
namespace BatNav
{
    public class SeaElement 
    {
		
		// représentation graphique du SeaElement de type Windows.UI.Xaml.Shapes.Ellipse
		// permet de déterminer la position des évenements 'Tir'
		public Ellipse ellipse = new Ellipse();
		// ..
		// gestion de la persistance de l'affichage d'un tir
		// quand regenCycles revient à zéro la pastille rouge disparait
		public int RegenCycles { get; set; }

		// position de l'élement de mer en (ligne, colonne)
		private Point pos ;
		public Point Pos { get => pos; set => pos = value; }

		// Constructeur d'un Element de Mer
		public SeaElement(Grid grid, Thickness thic, int row, int col, int width, int height, int regenCycles)
        {
			
			RegenCycles = regenCycles;
			pos.X = col;
			pos.Y = row;

			if ( grid.Name == "enemySea" ) {
				ellipse.PointerPressed += MainPage.EngageShot;
			}
			else if (grid.Name == "mySea") {
				ellipse.AllowDrop = true;
            }
			// ..
			ellipse.Width = AppDef.largeurMer / AppDef.nbCol;
			ellipse.Height = AppDef.hauteurMer / AppDef.nbCol;
			
			ellipse.Margin = thic;
			RefreshColor(this, false);
			grid.Children.Add(ellipse);
			
		}
	
	
		// le tir en rouge s'efface après un certain  temps
		// cf: le timer event
		public async void Regen(object sender)
        {
			await CallOnMainViewUiThreadAsync(() =>
			{
				if (RegenCycles > 0) { 
					RegenCycles -= 1; 
				}

				//RefreshColor();
			});
		}
		public async void RefreshColor(object sender, bool showBoat)
        {
            await CallOnMainViewUiThreadAsync(() =>
            {
                var type = sender.GetType();


                if (showBoat)
                {
                    if (sender is ShipElement)
                    {
						var obj = (ShipElement)sender;

						if (obj.statusCode == AppDef.State.Afloat)
                        {
                            ellipse.Fill = AppDef.greenBrush;
                        }
                        else
                        {
                            ellipse.Fill = AppDef.pinkBrush;
                        }
                    }

                }
                else
                {
                    if (RegenCycles > 0)
                    {
                        ellipse.Fill = AppDef.redBrush;
                    }
                    else
                    {

                        ellipse.Fill = AppDef.blueBrush;

                    }
                }

            });
        }
        public static async Task CallOnUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler handler) =>
	await dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);

	public static async Task CallOnMainViewUiThreadAsync(DispatchedHandler handler) =>
		await CallOnUiThreadAsync(CoreApplication.MainView.CoreWindow.Dispatcher, handler);
    }
	
}
