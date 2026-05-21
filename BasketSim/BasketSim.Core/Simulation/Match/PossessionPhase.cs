namespace BasketSim.Core.Simulation.Match
{
    /// <summary>
    /// Représente les différentes phases d'une possession de basket.
    /// 
    /// Une possession suit généralement ces phases dans l'ordre :
    /// 1. Initiation (réception de la balle)
    /// 2. Advancement (remontée vers le frontcourt)
    /// 3. Organization (mise en place de l'attaque)
    /// 4. Actions (passes, drives, écrans)
    /// 5. Resolution (tir ou turnover)
    /// 6. PostResolution (rebond ou changement de possession)
    /// 
    /// Le PossessionEngine utilise ces phases pour structurer
    /// la simulation et savoir quelles décisions prendre.
    /// </summary>
    public enum PossessionPhase
    {
        /// <summary>
        /// Début de possession — un joueur récupère la balle.
        /// </summary>
        Initiation,

        /// <summary>
        /// Remontée de la balle vers le frontcourt.
        /// </summary>
        Advancement,

        /// <summary>
        /// Mise en place de l'attaque — choix du type d'action principal.
        /// </summary>
        Organization,

        /// <summary>
        /// Exécution des actions offensives (passes, drives, écrans).
        /// Cette phase est ITÉRATIVE : plusieurs actions peuvent
        /// se succéder avant la résolution.
        /// </summary>
        Actions,

        /// <summary>
        /// Résolution de la possession (tir tenté ou turnover).
        /// </summary>
        Resolution,

        /// <summary>
        /// Suite après résolution :
        /// - Si tir réussi → fin de possession
        /// - Si tir raté → rebond
        /// - Si rebond offensif → retour à Actions
        /// - Si rebond défensif → fin de possession
        /// </summary>
        PostResolution,

        /// <summary>
        /// La possession est terminée.
        /// État final du PossessionEngine.
        /// </summary>
        Ended
    }
}