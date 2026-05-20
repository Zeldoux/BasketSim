using System.Collections.Generic;

namespace BasketSim.Core.Models
{
    /// <summary>
    /// Statistiques accumulées par un joueur durant UN match.
    /// 
    /// Cette classe est purement comptable — elle stocke les actions
    /// effectuées par le joueur (tirs réussis/manqués, passes, rebonds, etc.)
    /// 
    /// Séparée de PlayerMatchState car les stats sont :
    /// - additives (on incrémente, on ne remplace pas)
    /// - utiles APRÈS le match (rapports, classements, MVP)
    /// - de nature différente de l'état (fatigue, momentum)
    /// 
    /// À la fin du match, ces stats sont conservées pour les
    /// statistiques de saison (ajout dans une future SeasonStats).
    /// </summary>
    public class PlayerGameStats
    {
        // ============================================================
        // SCORE
        // ============================================================

        /// <summary>
        /// Points marqués au total dans le match.
        /// Calculé automatiquement à partir des tirs réussis.
        /// </summary>
        public int Points => (TwoPointMade * 2) + (ThreePointMade * 3) + FreeThrowMade;

        // ============================================================
        // TIRS PAR TYPE
        // (on stocke tentés ET réussis pour pouvoir calculer les %)
        // ============================================================

        /// <summary>Tirs à 2 points tentés (toutes zones intérieures + mi-distance).</summary>
        public int TwoPointAttempted { get; set; }

        /// <summary>Tirs à 2 points réussis.</summary>
        public int TwoPointMade { get; set; }

        /// <summary>Tirs à 3 points tentés.</summary>
        public int ThreePointAttempted { get; set; }

        /// <summary>Tirs à 3 points réussis.</summary>
        public int ThreePointMade { get; set; }

        /// <summary>Lancers francs tentés.</summary>
        public int FreeThrowAttempted { get; set; }

        /// <summary>Lancers francs réussis.</summary>
        public int FreeThrowMade { get; set; }

        // ============================================================
        // TIRS DÉTAILLÉS PAR ZONE (pour analyse avancée)
        // ============================================================

        /// <summary>
        /// Tirs tentés par zone du terrain.
        /// Permet d'analyser où le joueur a pris ses tirs
        /// et de générer des heatmaps post-match.
        /// </summary>
        public Dictionary<CourtZone, int> ShotsAttemptedByZone { get; set; }

        /// <summary>
        /// Tirs réussis par zone du terrain.
        /// Permet de voir les zones chaudes/froides du joueur ce match.
        /// </summary>
        public Dictionary<CourtZone, int> ShotsMadeByZone { get; set; }

        // ============================================================
        // REBONDS
        // ============================================================

        /// <summary>Rebonds offensifs.</summary>
        public int OffensiveRebounds { get; set; }

        /// <summary>Rebonds défensifs.</summary>
        public int DefensiveRebounds { get; set; }

        /// <summary>
        /// Total des rebonds — propriété calculée pour éviter
        /// la double comptabilité (source classique de bugs).
        /// </summary>
        public int TotalRebounds => OffensiveRebounds + DefensiveRebounds;

        // ============================================================
        // CRÉATION OFFENSIVE
        // ============================================================

        /// <summary>Passes décisives (passes menant directement à un panier).</summary>
        public int Assists { get; set; }

        /// <summary>Pertes de balle.</summary>
        public int Turnovers { get; set; }

        // ============================================================
        // DÉFENSE
        // ============================================================

        /// <summary>Interceptions (balles volées à l'adversaire).</summary>
        public int Steals { get; set; }

        /// <summary>Contres réussis.</summary>
        public int Blocks { get; set; }

        // ============================================================
        // FAUTES (provoquées et subies)
        // ============================================================

        /// <summary>
        /// Fautes provoquées par ce joueur (le joueur a été fautée).
        /// Différent des fautes commises (qui est dans PlayerMatchState).
        /// </summary>
        public int FoulsDrawn { get; set; }

        // ============================================================
        // PROPRIÉTÉS CALCULÉES (utiles pour les rapports)
        // ============================================================

        /// <summary>Pourcentage de réussite aux tirs à 2 points (0 à 100).</summary>
        public double TwoPointPercentage =>
            TwoPointAttempted == 0 ? 0 : (double)TwoPointMade / TwoPointAttempted * 100;

        /// <summary>Pourcentage de réussite aux tirs à 3 points (0 à 100).</summary>
        public double ThreePointPercentage =>
            ThreePointAttempted == 0 ? 0 : (double)ThreePointMade / ThreePointAttempted * 100;

        /// <summary>Pourcentage de réussite aux lancers francs (0 à 100).</summary>
        public double FreeThrowPercentage =>
            FreeThrowAttempted == 0 ? 0 : (double)FreeThrowMade / FreeThrowAttempted * 100;

        /// <summary>
        /// Tirs totaux tentés (2pts + 3pts, lancers francs exclus).
        /// Standard de l'industrie : on ne compte pas les LF dans le "FGA".
        /// </summary>
        public int FieldGoalsAttempted => TwoPointAttempted + ThreePointAttempted;

        /// <summary>Tirs totaux réussis (2pts + 3pts).</summary>
        public int FieldGoalsMade => TwoPointMade + ThreePointMade;

        /// <summary>Pourcentage global aux tirs (FG%).</summary>
        public double FieldGoalPercentage =>
            FieldGoalsAttempted == 0 ? 0 : (double)FieldGoalsMade / FieldGoalsAttempted * 100;

        /// <summary>
        /// True Shooting Percentage (TS%) — la métrique avancée
        /// la plus utilisée pour évaluer l'efficacité offensive globale.
        /// Prend en compte les 2pts, 3pts ET lancers francs.
        /// Formule standard : Points / (2 × (FGA + 0.44 × FTA))
        /// </summary>
        public double TrueShootingPercentage
        {
            get
            {
                double denominator = 2.0 * (FieldGoalsAttempted + 0.44 * FreeThrowAttempted);
                return denominator == 0 ? 0 : Points / denominator * 100;
            }
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — initialise toutes les stats à zéro.
        /// Initialise les dictionnaires de tirs par zone pour éviter
        /// les NullReferenceException lors des incréments.
        /// </summary>
        public PlayerGameStats()
        {
            ShotsAttemptedByZone = new Dictionary<CourtZone, int>();
            ShotsMadeByZone = new Dictionary<CourtZone, int>();
        }

        // ============================================================
        // MÉTHODES D'INCRÉMENTATION
        // ============================================================

        /// <summary>
        /// Enregistre un tir tenté dans une zone donnée.
        /// Incrémente le compteur global ET le compteur par zone.
        /// 
        /// Cette méthode centralise la logique d'incrémentation
        /// pour éviter d'oublier de mettre à jour les deux compteurs.
        /// </summary>
        public void RecordShotAttempt(CourtZone zone, bool isThreePoint)
        {
            // Incrémente le compteur par zone (avec fallback à 0 si zone absente)
            ShotsAttemptedByZone.TryGetValue(zone, out int current);
            ShotsAttemptedByZone[zone] = current + 1;

            // Incrémente le compteur global selon le type
            if (isThreePoint)
                ThreePointAttempted++;
            else
                TwoPointAttempted++;
        }

        /// <summary>
        /// Enregistre un tir réussi dans une zone donnée.
        /// À appeler EN PLUS de RecordShotAttempt (un tir réussi est
        /// d'abord un tir tenté).
        /// </summary>
        public void RecordShotMade(CourtZone zone, bool isThreePoint)
        {
            ShotsMadeByZone.TryGetValue(zone, out int current);
            ShotsMadeByZone[zone] = current + 1;

            if (isThreePoint)
                ThreePointMade++;
            else
                TwoPointMade++;
        }
    }
}