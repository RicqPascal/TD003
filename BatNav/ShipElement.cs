// importation des espaces de nommage utiles
using Windows.Foundation;

// Ajout de classes dans l'espace de nommage BatNav de l'application
namespace BatNav
{
    // Utilisation d'une structure pour décrire un élément de bateau
    public struct ShipElement
    {
		// Status d'un élément de bateau = flotte, Touché
		public AppDef.State statusCode; 
		// Coordonnée de cet élément
		public Point elt;
		// besoin d'autre chose?
    }
	
}
