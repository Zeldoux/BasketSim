namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente le style offensif principal d'une équipe.
    /// Détermine la philosophie de jeu en attaque.
    /// 
    /// Influence fortement la simulation : chaque style favorise
    /// certains types d'actions (isolation, mouvement, pick and roll, etc.).
    /// </summary>
    public enum OffensiveStyle
    {
        /// <summary>
        /// Style équilibré — pas de préférence marquée.
        /// L'équipe prend ce que la défense donne.
        /// Bon défaut pour les équipes sans identité forte.
        /// </summary>
        Balanced,

        /// <summary>
        /// Mouvement et passes prioritaires — peu d'isolation.
        /// Beaucoup d'écrans, de coupes, de circulation de balle.
        /// Style des Warriors de Kerr, Spurs de Popovich.
        /// </summary>
        Motion,

        /// <summary>
        /// Isolation prioritaire — laisser les créateurs travailler 1-vs-1.
        /// Style des Rockets de Harden, Mavs de Doncic.
        /// </summary>
        IsolationHeavy,

        /// <summary>
        /// Pick and Roll central — l'action P&R est la base de l'attaque.
        /// Style des Jazz Stockton/Malone, Suns Nash, Hawks Trae Young.
        /// </summary>
        PickAndRollCentric,

        /// <summary>
        /// Jeu poste bas prioritaire — donner la balle aux intérieurs.
        /// Style des Lakers Shaq era, Sixers Embiid era.
        /// </summary>
        PostUpHeavy,

        /// <summary>
        /// Style inside-out — chercher l'intérieur d'abord pour kick-out à 3pts.
        /// Style hybride moderne (Nuggets Jokic).
        /// </summary>
        InsideOut
    }
}