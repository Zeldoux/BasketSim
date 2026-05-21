namespace BasketSim.Core.Models
{
    /// <summary>
    /// Statistiques agrégées d'une équipe durant un match.
    /// 
    /// À ne pas confondre avec PlayerGameStats : ces stats sont
    /// au niveau de l'ÉQUIPE entière (somme des actions de tous
    /// les joueurs + statistiques propres à l'équipe).
    /// 
    /// Certaines stats ne peuvent exister qu'au niveau équipe :
    /// - Fautes d'équipe par quart-temps (bonus aux LF)
    /// - Points par quart-temps
    /// - Possessions
    /// </summary>
    public class TeamGameStats
    {
        // ============================================================
        // SCORE
        // ============================================================

        /// <summary>
        /// Points marqués au total dans le match.
        /// Calculé automatiquement à partir des tirs réussis.
        /// </summary>
        public int Points => (TwoPointMade * 2) + (ThreePointMade * 3) + FreeThrowMade;

        /// <summary>
        /// Points marqués par quart-temps (index 0-3 pour Q1-Q4, plus
        /// éventuellement des prolongations en index 4+).
        /// 
        /// Utile pour les rapports détaillés et l'analyse du momentum.
        /// </summary>
        public List<int> PointsByQuarter { get; set; }

        // ============================================================
        // TIRS PAR TYPE
        // ============================================================

        /// <summary>Tirs à 2 points tentés (somme de l'équipe).</summary>
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
        // REBONDS
        // ============================================================

        /// <summary>Rebonds offensifs (somme équipe).</summary>
        public int OffensiveRebounds { get; set; }

        /// <summary>Rebonds défensifs (somme équipe).</summary>
        public int DefensiveRebounds { get; set; }

        /// <summary>Total des rebonds (propriété calculée).</summary>
        public int TotalRebounds => OffensiveRebounds + DefensiveRebounds;

        // ============================================================
        // CRÉATION OFFENSIVE
        // ============================================================

        /// <summary>Passes décisives totales.</summary>
        public int Assists { get; set; }

        /// <summary>Pertes de balle totales.</summary>
        public int Turnovers { get; set; }

        // ============================================================
        // DÉFENSE
        // ============================================================

        /// <summary>Interceptions totales.</summary>
        public int Steals { get; set; }

        /// <summary>Contres réussis.</summary>
        public int Blocks { get; set; }

        // ============================================================
        // FAUTES (stats propres à l'équipe)
        // ============================================================

        /// <summary>
        /// Fautes commises par quart-temps.
        /// 
        /// IMPORTANT : en NBA, à partir de la 5ème faute d'équipe dans
        /// un quart-temps, l'équipe adverse passe en "bonus" : chaque
        /// faute donne 2 lancers francs.
        /// 
        /// Cette stat est donc cruciale pour la simulation.
        /// </summary>
        public List<int> FoulsByQuarter { get; set; }

        /// <summary>
        /// Total des fautes commises sur le match (propriété calculée).
        /// </summary>
        public int TotalFouls => FoulsByQuarter.Sum();

        // ============================================================
        // POSSESSIONS
        // ============================================================

        /// <summary>
        /// Nombre de possessions de l'équipe dans le match.
        /// 
        /// Stat avancée utile pour calculer l'efficacité offensive
        /// (Points / 100 Possessions) qui est plus représentative
        /// que le score brut.
        /// </summary>
        public int Possessions { get; set; }

        // ============================================================
        // PROPRIÉTÉS CALCULÉES (métriques avancées)
        // ============================================================

        /// <summary>Pourcentage de réussite aux tirs à 2 points.</summary>
        public double TwoPointPercentage =>
            TwoPointAttempted == 0 ? 0 : (double)TwoPointMade / TwoPointAttempted * 100;

        /// <summary>Pourcentage de réussite aux tirs à 3 points.</summary>
        public double ThreePointPercentage =>
            ThreePointAttempted == 0 ? 0 : (double)ThreePointMade / ThreePointAttempted * 100;

        /// <summary>Pourcentage de réussite aux lancers francs.</summary>
        public double FreeThrowPercentage =>
            FreeThrowAttempted == 0 ? 0 : (double)FreeThrowMade / FreeThrowAttempted * 100;

        /// <summary>Tirs totaux tentés (FGA).</summary>
        public int FieldGoalsAttempted => TwoPointAttempted + ThreePointAttempted;

        /// <summary>Tirs totaux réussis (FGM).</summary>
        public int FieldGoalsMade => TwoPointMade + ThreePointMade;

        /// <summary>Pourcentage global aux tirs (FG%).</summary>
        public double FieldGoalPercentage =>
            FieldGoalsAttempted == 0 ? 0 : (double)FieldGoalsMade / FieldGoalsAttempted * 100;

        /// <summary>
        /// Offensive Rating — points marqués pour 100 possessions.
        /// Métrique avancée standard pour évaluer l'efficacité offensive.
        /// </summary>
        public double OffensiveRating =>
            Possessions == 0 ? 0 : (double)Points / Possessions * 100;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut.
        /// Initialise les listes par quart-temps avec 4 zéros
        /// (un par quart-temps standard).
        /// </summary>
        public TeamGameStats()
        {
            // Initialise avec 4 quarts-temps standard
            // Des prolongations ajouteront des entrées au besoin
            PointsByQuarter = new List<int> { 0, 0, 0, 0 };
            FoulsByQuarter = new List<int> { 0, 0, 0, 0 };
        }
    }
}