using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Résout les rebonds : qui récupère la balle après un tir manqué ?
    /// 
    /// Algorithme :
    /// 1. Chaque joueur a une "force de rebond" calculée selon ses attributs
    /// 2. On fait la somme pondérée pour chaque côté (offense vs défense)
    /// 3. On tire au sort PROPORTIONNELLEMENT à ces forces
    /// 
    /// Statistiquement, ~70-75% des rebonds vont à la défense en NBA.
    /// Notre formule doit refléter cette tendance naturellement.
    /// 
    /// Pourquoi la défense gagne plus souvent ?
    /// - Les défenseurs sont mieux placés (entre le tir et le panier)
    /// - Le boxout est plus efficace en défense
    /// - Les joueurs offensifs viennent souvent de plus loin
    /// 
    /// On modélise ça par un BIAS DÉFENSIF dans la formule.
    /// </summary>
    public class ReboundResolver : IResolver
    {
        // ============================================================
        // DÉPENDANCES
        // ============================================================

        private readonly Random _random;

        /// <summary>
        /// Bonus de chance pour les rebondeurs défensifs.
        /// Représente l'avantage naturel de positionnement.
        /// Modulable pour équilibrer le pourcentage off/def rebounds.
        /// </summary>
        private const double DefensiveBias = 1.6;

        /// <summary>
        /// Bonus pour le tireur original (souvent bien placé).
        /// </summary>
        private const double OriginalShooterBonus = 1.15;

        public ReboundResolver(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // ============================================================
        // MÉTHODE PRINCIPALE
        // ============================================================

        /// <summary>
        /// Détermine qui récupère le rebond.
        /// 
        /// Retourne un ReboundResult avec le joueur et le type (off/def).
        /// </summary>
        public ReboundResult Resolve(ReboundAttemptContext context)
        {
            // Étape 1 : calcule la force de chaque joueur en compétition
            var competitors = new List<(Player Player, double Strength, bool IsOffensive)>();

            // Ajoute les joueurs offensifs
            foreach (var player in context.OffensiveBoxers)
            {
                double strength = CalculateReboundStrength(player, isOffensive: true, context);
                competitors.Add((player, strength, true));
            }

            // Ajoute les joueurs défensifs (avec bonus défensif)
            foreach (var player in context.DefensiveBoxers)
            {
                double strength = CalculateReboundStrength(player, isOffensive: false, context);
                strength *= DefensiveBias;
                competitors.Add((player, strength, false));
            }

            // Étape 2 : tirage proportionnel aux forces
            return SelectByWeightedRandom(competitors);
        }

        // ============================================================
        // CALCUL DE LA FORCE DE REBOND
        // ============================================================

        /// <summary>
        /// Calcule la "force de rebond" d'un joueur dans le contexte donné.
        /// 
        /// Combine :
        /// - Attribut de rebond offensif ou défensif selon le côté
        /// - Boxout (capacité à mettre l'adversaire derrière)
        /// - Détente verticale
        /// - Force physique
        /// - Détermination (mental)
        /// - Hands (qualité des mains pour sécuriser la balle)
        /// </summary>
        private double CalculateReboundStrength(
            Player player,
            bool isOffensive,
            ReboundAttemptContext context)
        {
            // Stat de rebond principale selon le côté
            double mainReboundStat = isOffensive
                ? player.OffensiveRebound
                : player.DefensiveRebound;

            // Combinaison pondérée des attributs
            // Les coefficients reflètent l'importance relative de chaque stat
            double strength =
                  mainReboundStat * 2.0      // stat principale, poids fort
                + player.Boxout * 1.5        // capacité de boxout
                + player.Vertical * 0.8      // détente
                + player.Strength * 0.6      // force physique
                + player.Determination * 0.5 // mental
                + player.Hands * 0.4;        // qualité des mains

            // Bonus si c'est le tireur original (rebond offensif uniquement)
            if (isOffensive && player == context.OriginalShooter)
            {
                strength *= OriginalShooterBonus;
            }

            return strength;
        }

        // ============================================================
        // TIRAGE ALÉATOIRE PONDÉRÉ
        // ============================================================

        /// <summary>
        /// Sélectionne un joueur par tirage aléatoire proportionnel à sa force.
        /// 
        /// Algorithme du "weighted random" :
        /// 1. Calcule la somme totale des forces
        /// 2. Tire un nombre entre 0 et cette somme
        /// 3. Parcourt la liste en accumulant les forces
        /// 4. Sélectionne le joueur quand l'accumulation dépasse le tirage
        /// 
        /// Exemple :
        /// - Joueur A : force 30
        /// - Joueur B : force 20
        /// - Joueur C : force 10
        /// → Total 60
        /// → Tirage 45 → on accumule : 30 (pas atteint), 50 (atteint) → B gagne
        /// </summary>
        private ReboundResult SelectByWeightedRandom(
            List<(Player Player, double Strength, bool IsOffensive)> competitors)
        {
            double totalStrength = competitors.Sum(c => c.Strength);
            double randomValue = _random.NextDouble() * totalStrength;

            double accumulated = 0;
            foreach (var (player, strength, isOffensive) in competitors)
            {
                accumulated += strength;
                if (accumulated >= randomValue)
                {
                    return new ReboundResult
                    {
                        Rebounder = player,
                        IsOffensiveRebound = isOffensive
                    };
                }
            }

            // Fallback théorique (ne devrait jamais arriver mathématiquement)
            // Retourne le dernier joueur par sécurité
            var lastCompetitor = competitors.Last();
            return new ReboundResult
            {
                Rebounder = lastCompetitor.Player,
                IsOffensiveRebound = lastCompetitor.IsOffensive
            };
        }
    }
}