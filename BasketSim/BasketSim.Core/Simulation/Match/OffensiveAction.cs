namespace BasketSim.Core.Simulation.Match
{
    /// <summary>
    /// Types d'actions offensives que peut décider le DecisionMaker.
    /// 
    /// Chaque action correspond à un pattern de jeu différent
    /// et déclenche une séquence d'events spécifique dans le PossessionEngine.
    /// 
    /// Utilisé par :
    /// - DecisionMaker : choisit l'action à prendre selon la tactique
    /// - PossessionEngine : exécute le pattern correspondant
    /// </summary>
    public enum OffensiveAction
    {
        /// <summary>
        /// Pick and Roll classique — un écran pour libérer le porteur.
        /// 
        /// Action collective la plus utilisée en NBA moderne.
        /// Génère typiquement : Screen → Drive ou Pass → Shot
        /// </summary>
        PickAndRoll,

        /// <summary>
        /// Isolation — le porteur attaque seul son défenseur.
        /// 
        /// Style des stars créatives (Doncic, Harden, Kyrie).
        /// Génère typiquement : Dribble → Drive ou Shot
        /// </summary>
        Isolation,

        /// <summary>
        /// Motion — passes et mouvements collectifs sans schéma fixe.
        /// 
        /// Style des équipes coachées (Spurs, Warriors Kerr era).
        /// Génère typiquement : Pass → Pass → Pass → Shot
        /// </summary>
        Motion,

        /// <summary>
        /// Post Up — jeu poste bas pour un intérieur dominant.
        /// 
        /// Style traditionnel (Olajuwon, Duncan) ou moderne (Jokic).
        /// Génère typiquement : Pass au poste → PostUp → Shot
        /// </summary>
        PostUp,

        /// <summary>
        /// Direct Shot — tir immédiat sans construction.
        /// 
        /// Utilisé quand le tir est trop ouvert pour ne pas le prendre,
        /// ou en fin de shot clock.
        /// Génère typiquement : juste un Shot
        /// </summary>
        DirectShot
    }
}