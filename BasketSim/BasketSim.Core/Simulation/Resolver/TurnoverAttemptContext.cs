using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Contient les informations pour évaluer le risque de perte de balle.
    /// </summary>
    public class TurnoverAttemptContext
    {
        /// <summary>
        /// Le joueur qui a actuellement la balle.
        /// </summary>
        public Player BallHandler { get; set; } = null!;

        /// <summary>
        /// État du porteur (fatigue, momentum, forme).
        /// </summary>
        public PlayerMatchState BallHandlerState { get; set; } = null!;

        /// <summary>
        /// Le défenseur principal sur le porteur.
        /// </summary>
        public Player? PrimaryDefender { get; set; }

        /// <summary>
        /// Indique si la possession est sous pression du shot clock
        /// (peu de temps restant). Augmente le risque de turnover.
        /// </summary>
        public bool IsUnderShotClockPressure { get; set; }

        /// <summary>
        /// Indique si l'équipe défensive applique une pression élevée
        /// (full court press, trap, etc.). Augmente le risque.
        /// </summary>
        public bool IsUnderDefensivePressure { get; set; }
    }
}