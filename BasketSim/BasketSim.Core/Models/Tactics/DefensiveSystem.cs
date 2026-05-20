namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente le système défensif principal d'une équipe.
    /// 
    /// Choix fondamental qui change radicalement le comportement
    /// défensif. La défense homme-à-homme et la zone sont des
    /// philosophies opposées qui ne se mélangent pas.
    /// </summary>
    public enum DefensiveSystem
    {
        /// <summary>
        /// Défense individuelle classique.
        /// Chaque défenseur suit son joueur attribué.
        /// Le système le plus utilisé en NBA.
        /// </summary>
        ManToMan,

        /// <summary>
        /// Zone 2-3 — 2 défenseurs en haut, 3 en bas.
        /// Protège bien la raquette, vulnérable au tir extérieur.
        /// Système Syracuse, parfois utilisé en NBA en situations.
        /// </summary>
        ZoneTwoThree,

        /// <summary>
        /// Zone 3-2 — 3 défenseurs en haut, 2 en bas.
        /// Protège bien le périmètre, vulnérable au poste bas.
        /// Moins utilisée mais efficace contre les équipes shooteuses.
        /// </summary>
        ZoneThreeTwo,

        /// <summary>
        /// Box and One — 4 défenseurs en zone (carré) + 1 sur la star.
        /// Utilisé contre les équipes avec un scoreur dominant solitaire.
        /// Rare mais efficace ponctuellement.
        /// </summary>
        BoxAndOne,

        /// <summary>
        /// Match-up zone — zone qui s'ajuste sur les joueurs.
        /// Mix de zone et d'homme à homme.
        /// Plus complexe à exécuter mais difficile à attaquer.
        /// </summary>
        MatchUpZone
    }
}