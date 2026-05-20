using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation
{
    /// <summary>
    /// Calcule l'overall rating d'un joueur à un poste donné.
    /// 
    /// Cette classe est le point d'entrée principal pour évaluer
    /// le niveau d'un joueur. Elle combine :
    /// 
    /// - Les pondérations par poste (PositionWeights)
    /// - La familiarity au poste (PositionFamiliarityCalculator)
    /// 
    /// Le résultat est un PositionFitness complet qui contient :
    /// - L'overall effectif (avec pénalité de familiarity)
    /// - L'overall brut (sans pénalité, pour analyse)
    /// - Le niveau de familiarity
    /// - Le multiplicateur appliqué
    /// 
    /// PHILOSOPHIE :
    /// Un overall n'est pas une "note de valeur" absolue d'un joueur,
    /// mais une note de PERFORMANCE à un poste donné. Wembanyama est
    /// excellent en Center et mauvais en Point Guard — ce ne sont pas
    /// des notes "morales", ce sont des notes d'efficacité.
    /// </summary>
    public static class RatingCalculator
    {
        // ============================================================
        // MÉTHODES PUBLIQUES PRINCIPALES
        // ============================================================

        /// <summary>
        /// Calcule l'overall rating effectif d'un joueur à un poste donné.
        /// 
        /// C'est la méthode "raccourcie" qui retourne juste le nombre.
        /// Pour avoir tous les détails du calcul, utiliser GetFitness().
        /// </summary>
        public static int GetOverallAt(Player player, Position position)
        {
            return GetFitness(player, position).EffectiveOverall;
        }

        /// <summary>
        /// Calcule l'overall rating du joueur à son poste naturel.
        /// Raccourci pour GetOverallAt(player, player.Position).
        /// </summary>
        public static int GetNaturalOverall(Player player)
        {
            return GetOverallAt(player, player.Position);
        }

        /// <summary>
        /// Calcule l'évaluation complète d'un joueur à un poste donné.
        /// Retourne un PositionFitness avec tous les détails du calcul.
        /// 
        /// C'est la méthode "complète" — à utiliser quand on veut
        /// afficher les détails ou comprendre pourquoi un overall
        /// est ce qu'il est.
        /// </summary>
        public static PositionFitness GetFitness(Player player, Position position)
        {
            // Étape 1 : calcule l'overall brut basé sur les pondérations
            int rawOverall = ComputeRawOverall(player, position);

            // Étape 2 : détermine la familiarity du joueur à ce poste
            PositionFamiliarity familiarity =
                PositionFamiliarityCalculator.Calculate(player, position);

            // Étape 3 : récupère le multiplicateur d'efficacité
            double multiplier =
                PositionFamiliarityCalculator.GetEffectivenessMultiplier(familiarity);

            // Étape 4 : applique le multiplicateur pour obtenir l'overall effectif
            // Math.Round arrondi au plus proche entier (3.5 → 4)
            int effectiveOverall = (int)Math.Round(rawOverall * multiplier);

            // Construit et retourne le résultat complet
            return new PositionFitness
            {
                Position = position,
                Familiarity = familiarity,
                RawOverall = rawOverall,
                EffectiveOverall = effectiveOverall,
                EffectivenessMultiplier = multiplier
            };
        }

        /// <summary>
        /// Calcule la fitness du joueur sur TOUS les postes.
        /// Retourne un dictionnaire poste → fitness.
        /// 
        /// Utile pour :
        /// - Afficher la "fiche joueur" complète
        /// - Identifier les postes alternatifs viables
        /// - Évaluer la polyvalence d'un joueur
        /// </summary>
        public static Dictionary<Position, PositionFitness> GetFitnessAtAllPositions(Player player)
        {
            var result = new Dictionary<Position, PositionFitness>();

            // Itère sur toutes les valeurs de l'enum Position
            foreach (Position position in Enum.GetValues<Position>())
            {
                result[position] = GetFitness(player, position);
            }

            return result;
        }

        /// <summary>
        /// Retourne le meilleur poste d'un joueur (celui où son overall
        /// effectif est le plus élevé).
        /// 
        /// Note : ce n'est pas toujours le poste naturel. Un joueur peut
        /// avoir un meilleur overall à un poste secondaire si ses stats
        /// y sont mieux valorisées.
        /// </summary>
        public static PositionFitness GetBestPosition(Player player)
        {
            var allFitness = GetFitnessAtAllPositions(player);

            // OrderByDescending pour avoir le meilleur en premier, puis First()
            return allFitness.Values
                .OrderByDescending(f => f.EffectiveOverall)
                .First();
        }

        // ============================================================
        // CALCUL DE L'OVERALL BRUT
        // ============================================================

        /// <summary>
        /// Calcule l'overall brut d'un joueur à un poste donné,
        /// sans appliquer la pénalité de familiarity.
        /// 
        /// Formule : moyenne pondérée des stats du joueur,
        /// avec les poids définis dans PositionWeights pour ce poste.
        /// 
        /// Seules les stats avec un poids > 0 sont prises en compte.
        /// Les autres stats existent et sont utilisées par le moteur
        /// de simulation, mais ne comptent pas pour l'overall.
        /// </summary>
        private static int ComputeRawOverall(Player player, Position position)
        {
            // Récupère les pondérations pour ce poste
            var weights = PositionWeights.GetWeights(position);

            double weightedSum = 0;
            double totalWeight = 0;

            // Pour chaque stat pondérée, calcule sa contribution
            foreach (var (statName, weight) in weights)
            {
                // Récupère la valeur du joueur pour cette stat
                int statValue = GetPlayerAttributeValue(player, statName);

                // Ajoute la contribution pondérée à la somme
                weightedSum += statValue * weight;
                totalWeight += weight;
            }

            // Protection contre la division par zéro (cas théorique impossible
            // si PositionWeights est correctement configuré, mais on protège quand même)
            if (totalWeight == 0)
                return 0;

            // Moyenne pondérée, arrondie à l'entier le plus proche
            return (int)Math.Round(weightedSum / totalWeight);
        }

        // ============================================================
        // HELPER — RÉCUPÉRATION D'UN ATTRIBUT PAR NOM
        // ============================================================
        //
        // Cette méthode est dupliquée depuis PositionFamiliarityCalculator.
        // C'est volontaire pour l'instant — on accepte cette duplication
        // car la centraliser créerait une dépendance circulaire ou
        // forcerait une refactorisation prématurée.
        //
        // Si à l'avenir on a un troisième endroit qui en a besoin,
        // il sera temps de créer un PlayerAttributeAccessor dédié.
        // ============================================================

        /// <summary>
        /// Récupère la valeur d'un attribut d'un joueur via son nom.
        /// Fait le lien entre les noms (string) utilisés dans PositionWeights
        /// et les propriétés réelles de la classe Player.
        /// </summary>
        private static int GetPlayerAttributeValue(Player player, string statName)
        {
            return statName switch
            {
                // Tir
                PositionWeights.InsideShooting => player.InsideShooting,
                PositionWeights.MidRangeShooting => player.MidRangeShooting,
                PositionWeights.ThreePointShooting => player.ThreePointShooting,
                PositionWeights.FreeThrow => player.FreeThrow,
                PositionWeights.Touch => player.Touch,
                PositionWeights.ShotIQ => player.ShotIQ,

                // Création offensive
                PositionWeights.BallHandling => player.BallHandling,
                PositionWeights.Passing => player.Passing,
                PositionWeights.Vision => player.Vision,
                PositionWeights.Finishing => player.Finishing,
                PositionWeights.OffBallMovement => player.OffBallMovement,
                PositionWeights.PostMoves => player.PostMoves,
                PositionWeights.Creativity => player.Creativity,

                // Défense
                PositionWeights.PerimeterDefense => player.PerimeterDefense,
                PositionWeights.InteriorDefense => player.InteriorDefense,
                PositionWeights.OnBallDefense => player.OnBallDefense,
                PositionWeights.OffBallDefense => player.OffBallDefense,
                PositionWeights.Steal => player.Steal,
                PositionWeights.Block => player.Block,

                // Rebond
                PositionWeights.OffensiveRebound => player.OffensiveRebound,
                PositionWeights.DefensiveRebound => player.DefensiveRebound,
                PositionWeights.Boxout => player.Boxout,

                // Physique
                PositionWeights.Speed => player.Speed,
                PositionWeights.Acceleration => player.Acceleration,
                PositionWeights.Strength => player.Strength,
                PositionWeights.Stamina => player.Stamina,
                PositionWeights.Agility => player.Agility,
                PositionWeights.Balance => player.Balance,
                PositionWeights.Vertical => player.Vertical,
                PositionWeights.Hands => player.Hands,

                // Mental
                PositionWeights.BasketballIQ => player.BasketballIQ,
                PositionWeights.OffensiveAwareness => player.OffensiveAwareness,
                PositionWeights.DefensiveAwareness => player.DefensiveAwareness,

                _ => 0
            };
        }
    }
}