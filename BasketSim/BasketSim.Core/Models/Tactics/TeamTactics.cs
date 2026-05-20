namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Regroupe l'ensemble des consignes tactiques d'une équipe.
    /// 
    /// Classe racine du système tactique qui contient les trois piliers :
    /// - Tactiques offensives (style, pace, distribution, etc.)
    /// - Tactiques défensives (système, pression, couverture, etc.)
    /// - Gestion du match (approche, shot clock)
    /// 
    /// Chaque équipe possède une instance de TeamTactics qui définit
    /// son identité de jeu. Le coach peut modifier les paramètres
    /// pendant le match (typiquement lors des timeouts).
    /// 
    /// EXEMPLE D'UTILISATION :
    /// var tactics = new TeamTactics();
    /// tactics.Offensive.Style = OffensiveStyle.Motion;
    /// tactics.Offensive.Pace = 70;
    /// tactics.Defensive.System = DefensiveSystem.ManToMan;
    /// tactics.Defensive.PickAndRollCoverage = PnRCoverage.Switch;
    /// </summary>
    public class TeamTactics
    {
        // ============================================================
        // LES TROIS PILIERS TACTIQUES
        // ============================================================

        /// <summary>
        /// Consignes offensives — style de jeu, pace, distribution, etc.
        /// </summary>
        public OffensiveTactics Offensive { get; set; }

        /// <summary>
        /// Consignes défensives — système, pression, couverture P&R, etc.
        /// </summary>
        public DefensiveTactics Defensive { get; set; }

        /// <summary>
        /// Gestion du match — approche selon le score, gestion du shot clock.
        /// Modifiée plus fréquemment que les autres pendant un match.
        /// </summary>
        public GameManagement Management { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — initialise les trois piliers avec
        /// leurs valeurs neutres. Configuration équilibrée prête à l'emploi.
        /// 
        /// Le coach peut ensuite personnaliser n'importe quel paramètre :
        /// tactics.Offensive.Pace = 80;  // jeu rapide
        /// tactics.Defensive.PickAndRollCoverage = PnRCoverage.Switch;
        /// </summary>
        public TeamTactics()
        {
            Offensive = new OffensiveTactics();
            Defensive = new DefensiveTactics();
            Management = new GameManagement();
        }
    }
}