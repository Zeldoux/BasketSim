namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente les 5 postes traditionnels au basket-ball.
    /// L'ordre suit la numérotation classique (1 à 5).
    /// </summary>
    public enum Position
    {
        // Le "1" — le meneur, organise le jeu, dribble et distribue
        PointGuard,

        // Le "2" — l'arrière, principalement scoreur extérieur
        ShootingGuard,

        // Le "3" — l'ailier, polyvalent (scoring, défense, rebond)
        SmallForward,

        // Le "4" — l'ailier fort, joue plus près du panier
        PowerForward,

        // Le "5" — le pivot, joue dans la raquette, gros rebondeur
        Center
    }
}

