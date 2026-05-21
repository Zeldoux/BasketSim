using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Résout les risques de perte de balle au cours d'une possession.
    /// 
    /// Les pertes de balle ont deux origines principales :
    /// 1. Erreur du porteur (mauvaise passe, marcher, double dribble)
    /// 2. Interception/steal par un défenseur
    /// 
    /// Statistiquement en NBA :
    /// - ~13-15 turnovers par équipe par match
    /// - ~8 sont des steals (interceptions)
    /// - ~5-7 sont des erreurs non forcées
    /// 
    /// Donc environ 50-60% des turnovers sont des steals.
    /// </summary>
    public class TurnoverResolver : IResolver
    {
        private readonly Random _random;

        /// <summary>
        /// Probabilité de base d'un turnover par possession (en %).
        /// Calibré pour donner ~13 turnovers par 100 possessions
        /// (correspondant aux moyennes NBA).
        /// </summary>
        private const double BaseTurnoverChance = 13.0;

        public TurnoverResolver(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // ============================================================
        // MÉTHODE PRINCIPALE
        // ============================================================

        /// <summary>
        /// Détermine si la possession actuelle se termine par un turnover.
        /// </summary>
        public TurnoverResult Resolve(TurnoverAttemptContext context)
        {
            // Calcule la probabilité finale de turnover
            double turnoverChance = CalculateTurnoverChance(context);

            // Tirage aléatoire
            double roll = _random.NextDouble() * 100;
            bool isTurnover = roll < turnoverChance;

            // Si pas de turnover, retourne tôt
            if (!isTurnover)
            {
                return new TurnoverResult { IsTurnover = false };
            }

            // Détermine si c'est un steal ou une erreur non forcée
            bool isSteal = DetermineIfSteal(context);

            return new TurnoverResult
            {
                IsTurnover = true,
                IsSteal = isSteal,
                CauseDefender = isSteal ? context.PrimaryDefender : null
            };
        }

        // ============================================================
        // CALCUL DE LA PROBABILITÉ
        // ============================================================

        /// <summary>
        /// Calcule la probabilité finale de turnover pour cette possession.
        /// Combine attributs du porteur, du défenseur, et facteurs de pression.
        /// </summary>
        private double CalculateTurnoverChance(TurnoverAttemptContext context)
        {
            double chance = BaseTurnoverChance;

            // === Modification par les attributs du porteur ===

            // Ball handling : plus c'est élevé, moins de turnovers
            // Un joueur à 90 BH réduit le risque de ~40%
            // Un joueur à 30 BH l'augmente de ~50%
            double ballHandlingFactor = (60 - context.BallHandler.BallHandling) * 0.10;
            chance += ballHandlingFactor;

            // Vision : aide pour les passes (moins de mauvais choix)
            double visionFactor = (60 - context.BallHandler.Vision) * 0.05;
            chance += visionFactor;

            // === Modification par la défense ===

            if (context.PrimaryDefender != null)
            {
                // Capacité d'interception du défenseur
                double stealFactor = context.PrimaryDefender.Steal * 0.08;
                chance += stealFactor;

                // OnBallDefense : pression sur le porteur
                double pressureFactor = context.PrimaryDefender.OnBallDefense * 0.05;
                chance += pressureFactor;
            }

            // === Modification par les facteurs contextuels ===

            // Pression du shot clock : +30% si peu de temps
            if (context.IsUnderShotClockPressure)
            {
                chance *= 1.30;
            }

            // Pression défensive (press, trap) : +50%
            if (context.IsUnderDefensivePressure)
            {
                chance *= 1.50;
            }

            // === Modification par l'état du porteur ===

            // Fatigue : plus le joueur est fatigué, plus il fait d'erreurs
            double fatigueMultiplier = 2.0 - context.BallHandlerState.GetFatigueMultiplier();
            chance *= fatigueMultiplier;

            // Clamp pour rester dans des bornes raisonnables (0-50%)
            return Math.Clamp(chance, 0, 50);
        }

        // ============================================================
        // DÉTERMINATION STEAL VS ERREUR NON FORCÉE
        // ============================================================

        /// <summary>
        /// Détermine si la perte de balle est un steal du défenseur
        /// ou une erreur non forcée du porteur.
        /// 
        /// Plus le défenseur est bon en steal, plus c'est probable
        /// que ce soit une interception.
        /// </summary>
        private bool DetermineIfSteal(TurnoverAttemptContext context)
        {
            // Pas de défenseur ? Forcément une erreur non forcée
            if (context.PrimaryDefender == null)
                return false;

            // Probabilité de steal basée sur l'attribut Steal du défenseur
            // Un défenseur à 80 Steal a ~70% de chance que ce soit un steal
            // Un défenseur à 30 Steal a ~30% de chance
            double stealProbability = context.PrimaryDefender.Steal / 100.0 + 0.20;
            stealProbability = Math.Clamp(stealProbability, 0, 0.85);

            return _random.NextDouble() < stealProbability;
        }
    }
}