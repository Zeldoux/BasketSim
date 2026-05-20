namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente le style de distribution des touches au sein de l'équipe.
    /// 
    /// Définit qui touche la balle et combien.
    /// Influence fortement le profil offensif d'une équipe.
    /// </summary>
    public enum DistributionStyle
    {
        /// <summary>
        /// Distribution équilibrée — tous les joueurs touchent la balle.
        /// Style "beautiful game" des Spurs, Warriors des bonnes années.
        /// Demande un roster avec plusieurs créateurs.
        /// </summary>
        Balanced,

        /// <summary>
        /// Star-centric — la star domine la possession.
        /// 35-50% des actions passent par elle.
        /// Style Harden Rockets, Doncic Mavs, Westbrook Thunder.
        /// </summary>
        StarCentric,

        /// <summary>
        /// Two-star share — partage entre deux stars.
        /// Chacune prend ~25-30% des actions.
        /// Style Curry/Klay, Shaq/Kobe, LeBron/Wade.
        /// </summary>
        TwoStarShare,

        /// <summary>
        /// Playmaker-centric — un meneur distribue à tous les autres.
        /// Le meneur a la balle mais ne shoote pas forcément beaucoup.
        /// Style Nash Suns, Stockton Jazz, Magic Lakers.
        /// </summary>
        PlaymakerCentric,

        /// <summary>
        /// Post hub — le pivot est le centre du jeu.
        /// Distribue depuis le poste haut/bas.
        /// Style Jokic Nuggets, Sabonis Kings.
        /// </summary>
        PostHub
    }
}