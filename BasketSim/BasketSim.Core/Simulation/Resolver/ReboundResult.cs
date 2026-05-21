using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Résultat de la résolution d'un rebond.
    /// Indique qui a pris le rebond et si c'est un rebond offensif ou défensif.
    /// </summary>
    public class ReboundResult
    {
        /// <summary>
        /// Le joueur qui a sécurisé le rebond.
        /// </summary>
        public Player Rebounder { get; set; } = null!;

        /// <summary>
        /// True si c'est un rebond offensif (l'équipe garde la balle).
        /// False si c'est un rebond défensif (changement de possession).
        /// </summary>
        public bool IsOffensiveRebound { get; set; }
    }
}