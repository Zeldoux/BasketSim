using BasketSim.Core.Models;
using BasketSim.Core.Models.Tactics;

namespace BasketSim.Core.Simulation.Match.Defense
{
    /// <summary>
    /// Applique les ajustements défensifs sur les calculs offensifs.
    /// 
    /// Cette classe centralise la logique qui modifie les paramètres
    /// d'une action offensive (tir, drive, passe) selon les choix
    /// défensifs et l'état des matchups.
    /// 
    /// C'est ici qu'on transforme "Gobert défend Curry" en "Curry tire
    /// avec un ContestLevel +30 et un malus de zone".
    /// </summary>
    public class DefenseAdjuster
    {
        // ============================================================
        // AJUSTEMENT DU CONTEST LEVEL POUR UN TIR
        // ============================================================

        /// <summary>
        /// Calcule le niveau de contestation final pour un tir,
        /// en tenant compte :
        /// - Du défenseur direct (matchup)
        /// - De la couverture défensive choisie
        /// - De l'agressivité de contestation (ContestIntensity)
        /// - Des mismatchs éventuels
        /// </summary>
        public int CalculateShotContest(
            Player shooter,
            Player? defender,
            CourtZone zone,
            DefensiveTactics tactics,
            ScreenReaction? lastScreenReaction = null)
        {
            // Pas de défenseur → tir totalement ouvert
            if (defender == null) return 0;

            // === Base : compétence défensive selon la zone ===
            int defenseStat = IsInteriorZone(zone)
                ? defender.InteriorDefense
                : defender.PerimeterDefense;

            // Contestation de base : 50 + (defense_stat - 50) / 2
            // Un défenseur à 80 défense → contest = 65
            // Un défenseur à 50 défense → contest = 50
            // Un défenseur à 30 défense → contest = 40
            int baseContest = 50 + (defenseStat - 50) / 2;

            // === Modulation par l'agressivité de contestation ===
            double contestMultiplier = 0.5 + (tactics.ContestIntensity / 100.0);
            int adjustedContest = (int)(baseContest * contestMultiplier);

            // === Bonus d'envergure du défenseur ===
            // Au-delà de 200cm d'envergure, chaque cm en plus ajoute du contest
            int wingspanBonus = Math.Max(0, (defender.WingspanCm - 200) / 3);
            adjustedContest += wingspanBonus;

            // === Modificateurs de la couverture P&R (si applicable) ===
            if (lastScreenReaction != null)
            {
                // Si on est en zone mi-distance et la réaction était un Drop
                if (IsMidRangeZone(zone))
                {
                    adjustedContest += lastScreenReaction.MidRangeContestModifier;
                }

                // Si c'est un pull-up et qu'il y a eu Hedge
                // Note : on simplifie ici, le contexte de pull-up devrait
                // être passé en paramètre dans une version plus avancée
            }

            // === Pénalité si mismatch favorable à l'attaquant ===
            int heightDiff = shooter.HeightCm - defender.HeightCm;
            if (heightDiff >= 15)
            {
                // Attaquant beaucoup plus grand → contest réduit
                adjustedContest -= 15;
            }
            else if (heightDiff <= -15)
            {
                // Attaquant beaucoup plus petit → contest légèrement augmenté
                // mais pas trop, car les petits sont rapides
                adjustedContest += 5;
            }

            // Clamp dans les bornes valides
            return Math.Clamp(adjustedContest, 0, 100);
        }

        // ============================================================
        // AJUSTEMENT DE LA PROBABILITÉ DE STEAL
        // ============================================================

        /// <summary>
        /// Calcule un bonus de probabilité de steal selon le contexte défensif.
        /// </summary>
        public double GetStealChanceModifier(
            DefensiveTactics tactics,
            bool isOnPass,
            bool isUnderTrap)
        {
            double modifier = 1.0;

            // La pression défensive augmente les chances de steal
            modifier *= 1.0 + (tactics.Pressure.Intensity / 200.0);

            // Sur une passe, les steals sont plus fréquents
            if (isOnPass)
            {
                modifier *= 1.3;
            }

            // Si on est en trap, les steals sont beaucoup plus probables
            if (isUnderTrap)
            {
                modifier *= 2.5;
            }

            return modifier;
        }

        // ============================================================
        // HELPERS — IDENTIFICATION DES ZONES
        // ============================================================

        /// <summary>
        /// Détermine si une zone est intérieure (raquette).
        /// </summary>
        private bool IsInteriorZone(CourtZone zone)
        {
            return zone == CourtZone.RestrictedArea || zone == CourtZone.Paint;
        }

        /// <summary>
        /// Détermine si une zone est à mi-distance.
        /// </summary>
        private bool IsMidRangeZone(CourtZone zone)
        {
            return zone == CourtZone.MidRangeLeft
                || zone == CourtZone.MidRangeLeftCenter
                || zone == CourtZone.MidRangeCenter
                || zone == CourtZone.MidRangeRightCenter
                || zone == CourtZone.MidRangeRight;
        }
    }
}