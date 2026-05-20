using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation
{
    /// <summary>
    /// Calcule le niveau de familiarity d'un joueur à un poste donné.
    /// 
    /// Inspiré du système Position Familiarity de Football Manager,
    /// adapté au basket-ball avec trois critères combinés :
    /// 
    /// 1. DISTANCE AU POSTE NATUREL
    ///    Plus le poste demandé est proche du poste naturel,
    ///    plus la familiarity est élevée.
    /// 
    /// 2. ADÉQUATION PHYSIQUE (taille)
    ///    Un joueur trop grand pour un petit poste, ou trop petit
    ///    pour un grand poste, voit sa familiarity plafonnée.
    /// 
    /// 3. ADÉQUATION TECHNIQUE
    ///    Si les stats clés du poste sont trop faibles, plafonnement
    ///    également. Un pivot sans ball handling ne sera jamais meneur.
    /// 
    /// La familiarity finale est le MINIMUM des trois critères :
    /// un seul critère bloquant suffit à limiter le joueur.
    /// </summary>
    public static class PositionFamiliarityCalculator
    {
        // ============================================================
        // MULTIPLICATEURS D'EFFICACITÉ PAR NIVEAU
        // ============================================================
        // Définissent le pourcentage des attributs utilisés selon
        // la familiarity. Utilisé par RatingCalculator pour appliquer
        // la pénalité de performance.

        /// <summary>
        /// Retourne le multiplicateur d'efficacité associé à un niveau
        /// de familiarity. Valeurs entre 0.50 et 1.00.
        /// </summary>
        public static double GetEffectivenessMultiplier(PositionFamiliarity familiarity)
        {
            return familiarity switch
            {
                PositionFamiliarity.Natural => 1.00,
                PositionFamiliarity.Accomplished => 0.95,
                PositionFamiliarity.Competent => 0.85,
                PositionFamiliarity.Awkward => 0.70,
                PositionFamiliarity.Ineffective => 0.50,
                _ => 0.50
            };
        }

        // ============================================================
        // CALCUL PRINCIPAL
        // ============================================================

        /// <summary>
        /// Calcule la familiarity d'un joueur à un poste donné.
        /// 
        /// Combine trois critères en prenant le minimum :
        /// - Distance au poste naturel
        /// - Adéquation physique (taille)
        /// - Adéquation technique (stats clés)
        /// </summary>
        public static PositionFamiliarity Calculate(Player player, Position targetPosition)
        {
            // Si on évalue le joueur à son poste naturel,
            // c'est automatiquement Natural — pas besoin de calcul
            if (player.Position == targetPosition)
                return PositionFamiliarity.Natural;

            // Calcule les trois plafonds
            var byDistance = CalculateByPositionalDistance(player.Position, targetPosition);
            var bySize = CalculateBySize(player, targetPosition);
            var byTechnique = CalculateByTechnique(player, targetPosition);

            // Le minimum des trois gagne
            // (le critère le plus bloquant détermine le résultat)
            return MinOf(byDistance, bySize, byTechnique);
        }

        // ============================================================
        // CRITÈRE 1 — DISTANCE AU POSTE NATUREL
        // ============================================================

        /// <summary>
        /// Calcule la familiarity en fonction de la distance entre
        /// le poste naturel et le poste demandé.
        /// 
        /// Les postes sont numérotés de 1 (PG) à 5 (C).
        /// La distance absolue détermine la familiarity maximale.
        /// </summary>
        private static PositionFamiliarity CalculateByPositionalDistance(
            Position naturalPosition,
            Position targetPosition)
        {
            // Conversion en numéro de poste (1 à 5)
            int natural = (int)naturalPosition;
            int target = (int)targetPosition;
            int distance = Math.Abs(target - natural);

            return distance switch
            {
                0 => PositionFamiliarity.Natural,        // même poste
                1 => PositionFamiliarity.Accomplished,   // poste adjacent
                2 => PositionFamiliarity.Competent,      // saute un poste
                3 => PositionFamiliarity.Awkward,        // 3 postes d'écart
                _ => PositionFamiliarity.Ineffective     // PG vs C
            };
        }

        // ============================================================
        // CRITÈRE 2 — ADÉQUATION PHYSIQUE (TAILLE)
        // ============================================================

        /// <summary>
        /// Calcule la familiarity maximale autorisée par la taille du joueur.
        /// 
        /// Plus le joueur est hors de la fourchette idéale du poste,
        /// plus la familiarity est plafonnée bas.
        /// 
        /// La pénalité est progressive : un joueur de 2cm hors fourchette
        /// n'est presque pas pénalisé, un joueur de 20cm hors fourchette
        /// est lourdement limité.
        /// </summary>
        private static PositionFamiliarity CalculateBySize(Player player, Position targetPosition)
        {
            var (min, max) = PositionWeights.GetIdealHeightRange(targetPosition);

            // Si le joueur est dans la fourchette, aucune limite physique
            if (player.HeightCm >= min && player.HeightCm <= max)
                return PositionFamiliarity.Natural;

            // Calcule l'écart en centimètres par rapport à la fourchette
            int deviation = player.HeightCm < min
                ? min - player.HeightCm    // trop petit
                : player.HeightCm - max;   // trop grand

            // Pénalité progressive selon l'écart
            // 1-5cm   → Accomplished (presque pas pénalisé)
            // 6-10cm  → Competent
            // 11-20cm → Awkward
            // 21cm+   → Ineffective (Wembanyama en meneur, par exemple)
            return deviation switch
            {
                <= 5 => PositionFamiliarity.Accomplished,
                <= 10 => PositionFamiliarity.Competent,
                <= 20 => PositionFamiliarity.Awkward,
                _ => PositionFamiliarity.Ineffective
            };
        }

        // ============================================================
        // CRITÈRE 3 — ADÉQUATION TECHNIQUE
        // ============================================================

        /// <summary>
        /// Calcule la familiarity maximale autorisée par les compétences
        /// techniques du joueur pour le poste demandé.
        /// 
        /// On évalue les "stats critiques" du poste (celles avec un poids
        /// élevé dans PositionWeights) et on regarde leur moyenne.
        /// 
        /// Plus cette moyenne est basse, plus la familiarity est plafonnée :
        /// un pivot sans ball handling ne deviendra jamais meneur,
        /// peu importe son entraînement.
        /// </summary>
        private static PositionFamiliarity CalculateByTechnique(Player player, Position targetPosition)
        {
            // Récupère les stats critiques de ce poste (celles avec poids >= 1.5)
            var weights = PositionWeights.GetWeights(targetPosition);
            var criticalStats = weights
                .Where(kvp => kvp.Value >= 1.5)
                .Select(kvp => kvp.Key)
                .ToList();

            // Si pour une raison quelconque il n'y a pas de stats critiques,
            // on considère qu'il n'y a pas de limite technique
            if (criticalStats.Count == 0)
                return PositionFamiliarity.Natural;

            // Calcule la moyenne du joueur sur ces stats critiques
            double sum = 0;
            foreach (var statName in criticalStats)
            {
                sum += GetPlayerAttributeValue(player, statName);
            }
            double averageCritical = sum / criticalStats.Count;

            // Plafonnement selon la moyenne des stats critiques
            // 70+ → Natural (les stats permettent le poste)
            // 55+ → Accomplished
            // 40+ → Competent
            // 25+ → Awkward
            // <25 → Ineffective (totalement inadapté techniquement)
            return averageCritical switch
            {
                >= 70 => PositionFamiliarity.Natural,
                >= 55 => PositionFamiliarity.Accomplished,
                >= 40 => PositionFamiliarity.Competent,
                >= 25 => PositionFamiliarity.Awkward,
                _ => PositionFamiliarity.Ineffective
            };
        }

        // ============================================================
        // HELPER — RÉCUPÉRATION D'UN ATTRIBUT PAR NOM
        // ============================================================

        /// <summary>
        /// Récupère la valeur d'un attribut d'un joueur via son nom.
        /// 
        /// Cette méthode fait le lien entre les noms des stats dans
        /// PositionWeights (chaînes) et les propriétés réelles du Player.
        /// 
        /// IMPORTANT : si on ajoute un attribut dans Player.cs, il faut
        /// l'ajouter ici aussi pour qu'il soit reconnu. C'est une limitation
        /// du langage — on pourrait utiliser la réflexion mais c'est plus
        /// lent et moins lisible.
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

        // ============================================================
        // HELPER — MIN DE PLUSIEURS FAMILIARITIES
        // ============================================================

        /// <summary>
        /// Retourne le minimum (la valeur la plus pénalisante) entre
        /// plusieurs niveaux de familiarity.
        /// 
        /// Note : "minimum" ici = niveau le PLUS BAS dans la hiérarchie,
        /// donc le PLUS PÉNALISANT (Ineffective > Awkward > ... > Natural
        /// en termes de pénalité).
        /// 
        /// Comme l'enum est défini de Natural (0) à Ineffective (4),
        /// "minimum" mathématique = niveau le plus pénalisant.
        /// </summary>
        private static PositionFamiliarity MinOf(params PositionFamiliarity[] values)
        {
            // On utilise Max sur les valeurs int de l'enum
            // car Ineffective (4) est plus pénalisant que Natural (0)
            int maxPenalty = values.Max(v => (int)v);
            return (PositionFamiliarity)maxPenalty;
        }
    }
}