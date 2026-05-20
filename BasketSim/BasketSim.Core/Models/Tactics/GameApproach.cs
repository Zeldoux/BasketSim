namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente l'approche stratégique de l'équipe selon le contexte du match.
    /// 
    /// Cette consigne peut être modifiée pendant le match par le coach
    /// (typiquement pendant les timeouts) pour adapter l'équipe à la situation
    /// du score et du temps restant.
    /// </summary>
    public enum GameApproach
    {
        /// <summary>
        /// Push the lead — pousser l'avantage quand on mène.
        /// Continuer à attaquer, ne pas baisser l'intensité.
        /// Idéal quand l'équipe est en confiance et l'adversaire faiblit.
        /// </summary>
        PushTheLead,

        /// <summary>
        /// Manage the game — gérer prudemment.
        /// Équilibre attaque-défense, éviter les risques.
        /// Approche par défaut, neutre.
        /// </summary>
        ManageTheGame,

        /// <summary>
        /// Catch up mode — accélérer pour revenir au score.
        /// Plus de risques, plus de tirs à 3pts, pression défensive.
        /// </summary>
        CatchUp,

        /// <summary>
        /// Kill the clock — vider le chrono quand on mène en fin de match.
        /// Possessions longues, pas de tirs forcés.
        /// </summary>
        KillTheClock,

        /// <summary>
        /// Conservative — jouer prudent, limiter les pertes de balle.
        /// Style défensif d'abord, attaque structurée.
        /// </summary>
        Conservative
    }
}