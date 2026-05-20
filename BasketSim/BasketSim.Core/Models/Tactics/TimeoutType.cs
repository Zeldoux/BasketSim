namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente les types de timeouts disponibles en NBA.
    /// 
    /// Chaque type a une durée et un impact différent.
    /// La gestion des timeouts est un aspect important du coaching :
    /// - 7 timeouts par match maximum
    /// - Maximum 4 dans le 4ème quart
    /// - Maximum 2 dans les 3 dernières minutes
    /// </summary>
    public enum TimeoutType
    {
        /// <summary>
        /// Full timeout — 75 secondes.
        /// Le coach peut faire un vrai discours, changer plusieurs choses.
        /// Récupération physique notable pour les joueurs.
        /// </summary>
        Full,

        /// <summary>
        /// Short timeout — 20 secondes.
        /// Pour donner une consigne rapide ou un play.
        /// Peu de récupération physique.
        /// </summary>
        Short,

        /// <summary>
        /// Mandatory timeout — déclenché automatiquement par la TV.
        /// Le coach n'a pas choisi de le prendre mais peut en profiter.
        /// Durée standard d'environ 90 secondes.
        /// </summary>
        Mandatory,

        /// <summary>
        /// 20-second timeout — utilisé en fin de quart pour réorganiser.
        /// Une opportunité tactique sans utiliser un full timeout.
        /// </summary>
        TwentySecond
    }
}