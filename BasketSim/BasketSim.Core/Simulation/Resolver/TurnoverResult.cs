using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Résultat de la résolution d'un risque de turnover.
    /// </summary>
    public class TurnoverResult
    {
        /// <summary>
        /// True si la possession se termine par une perte de balle.
        /// </summary>
        public bool IsTurnover { get; set; }

        /// <summary>
        /// True si la perte de balle est due à une interception
        /// (steal) plutôt qu'à une faute du porteur.
        /// Si true, CauseDefender contient l'intercepteur.
        /// </summary>
        public bool IsSteal { get; set; }

        /// <summary>
        /// Le défenseur qui a provoqué la perte de balle.
        /// Null si la perte est due à une faute du porteur (passe ratée, marcher...)
        /// </summary>
        public Player? CauseDefender { get; set; }
    }
}