namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente les zones du demi-terrain offensif d'où un tir peut être pris.
    /// Découpage inspiré des "Hot Zones" de NBA 2K, qui est un standard reconnu
    /// dans l'analyse statistique du basket-ball moderne.
    /// 
    /// L'ordre va du plus proche (sous le panier) au plus éloigné (3 points),
    /// de gauche à droite du point de vue de l'attaquant face au panier.
    /// </summary>
    public enum CourtZone
    {
        // === Zone très proche du panier ===

        // Dunks et layups, juste sous le cercle
        RestrictedArea,

        // Le reste de la raquette, hors restricted area
        Paint,

        // === Mi-distance (entre la raquette et la ligne à 3 points) ===

        // Mi-distance côté gauche (baseline gauche)
        MidRangeLeft,

        // Mi-distance gauche-centre (aile gauche)
        MidRangeLeftCenter,

        // Mi-distance face au panier (free throw line étendue)
        MidRangeCenter,

        // Mi-distance droite-centre (aile droite)
        MidRangeRightCenter,

        // Mi-distance côté droit (baseline droite)
        MidRangeRight,

        // === Zones à 3 points ===

        // Corner 3 gauche (le tir le plus proche à 3pts, ~6.70m)
        ThreeCornerLeft,

        // 3 points aile gauche (~45° du panier)
        ThreeLeftWing,

        // 3 points face au panier (top of the key)
        ThreeTop,

        // 3 points aile droite (~45° du panier)
        ThreeRightWing,

        // Corner 3 droit
        ThreeCornerRight
    }
}