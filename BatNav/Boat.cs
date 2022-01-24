// importation des espaces de nommage utiles
using System;
using Windows.Foundation;

// Ajout de classes dans l'espace de nommage BatNav de l'application
namespace BatNav
{
    public class Boat
	{
		// Type de bateau
		private string name;
		public string Name { get=>name; }
		// propriétaire du bateau
		private Guid owner;
		public Guid Owner {get => owner;}
		// le vaisseau flotte encore, est touché ou est coulé
		public AppDef.State Afloat { get; set; }
		// coordonnées de la proue, avant du bateau
		public Point BowPostion { get => ShipElt[0].elt; }
		// taille du bateau en nombre d'éléments
		public int Size { get => size; set => size = value; }
		public AppDef.Cap Cap { get; set; }
		// allure du bateau.
		// On peut prévoir une implémentation où les bateau se déplacent !
        public float Pace { get; set; }

		
		// Tableau d'élements de bateau qui constituent le corps du bateau
		public ShipElement[] ShipElt;
		private int size;
		// constructeur 
		public Boat(string name, int size, int pace, Guid owner )
		{
			// que met-on?
			this.name = name;
			this.Afloat = AppDef.State.Afloat;
			this.Size = size;
			this.Pace = pace;
			this.owner = owner;
			ShipElt = new ShipElement[size];
		}

    }
	
}
