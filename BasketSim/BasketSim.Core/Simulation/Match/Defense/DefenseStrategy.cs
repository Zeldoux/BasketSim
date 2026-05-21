using BasketSim.Core.Models;
using BasketSim.Core.Models.Tactics;

namespace BasketSim.Core.Simulation.Match.Defense
{
    /// <summary>
    /// Détermine comment la défense réagit aux actions offensives.
    /// 
    /// Cette classe transforme les CONSIGNES tactiques de la défense
    /// (PnRCoverage, HelpDefense, ContestIntensity) en DÉCISIONS
    /// concrètes pendant une possession.
    /// 
    /// Exemple :
    /// - Tactique : PnRCoverage = Switch
    /// - Action offensive : pick and roll
    /// - Décision : effectuer un switch entre les deux défenseurs
    /// 
    /// La défense N'EST PLUS PASSIVE — elle adapte son comportement
    /// en fonction de l'attaque.
    /// </summary>
    public class DefenseStrategy
    {
        /// <summary>
        /// Source d'aléatoire pour les décisions probabilistes.
        /// </summary>
        private readonly Random _random;

        public DefenseStrategy(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // ============================================================
        // DÉCISION 1 — RÉAGIR À UN ÉCRAN
        // ============================================================

        /// <summary>
        /// Détermine la réaction défensive sur un écran (pick and roll).
        /// 
        /// Selon la couverture choisie par le coach, le résultat varie :
        /// - Switch : les défenseurs changent d'assignation
        /// - Drop : le big du screener recule, donne la mi-distance
        /// - Hedge : le big sort fort puis recule
        /// - Trap : prise à deux sur le porteur
        /// </summary>
        public ScreenReaction ReactToScreen(
            DefensiveTactics tactics,
            DefensiveMatchup matchups,
            Player ballHandler,
            Player screener)
        {
            // La couverture définie par le coach
            var coverage = tactics.PickAndRollCoverage;

            return coverage switch
            {
                PnRCoverage.Switch => HandleSwitchCoverage(matchups, ballHandler, screener),
                PnRCoverage.Drop => HandleDropCoverage(ballHandler, screener),
                PnRCoverage.Hedge or PnRCoverage.HardHedge => HandleHedgeCoverage(ballHandler),
                PnRCoverage.Trap or PnRCoverage.Blitz => HandleTrapCoverage(ballHandler),
                PnRCoverage.Ice => HandleIceCoverage(ballHandler),
                _ => new ScreenReaction { Type = ScreenReactionType.NoAction }
            };
        }

        /// <summary>
        /// Le switch : on échange les défenseurs.
        /// Conséquence : potentiel mismatch à exploiter.
        /// </summary>
        private ScreenReaction HandleSwitchCoverage(
            DefensiveMatchup matchups,
            Player ballHandler,
            Player screener)
        {
            // Effectue le switch dans les matchups
            matchups.SwitchDefenders(ballHandler, screener);

            return new ScreenReaction
            {
                Type = ScreenReactionType.Switch,
                Description = $"Switch défensif sur l'écran"
            };
        }

        /// <summary>
        /// Le drop : le big du screener recule.
        /// Conséquence : tir à mi-distance ouvert, drive contesté.
        /// </summary>
        private ScreenReaction HandleDropCoverage(Player ballHandler, Player screener)
        {
            return new ScreenReaction
            {
                Type = ScreenReactionType.Drop,
                Description = "Drop coverage — le big recule",
                MidRangeContestModifier = -25, // tir mi-distance moins contesté
                DriveContestModifier = +15     // drive plus contesté
            };
        }

        /// <summary>
        /// Le hedge : le big sort puis recule.
        /// Conséquence : le porteur ralentit, perd du temps.
        /// </summary>
        private ScreenReaction HandleHedgeCoverage(Player ballHandler)
        {
            return new ScreenReaction
            {
                Type = ScreenReactionType.Hedge,
                Description = "Hedge — le big sort fort sur le porteur",
                PullUpContestModifier = +20,  // pull-up plus contesté
                TimeUsedBonus = 2              // 2 secondes de plus consommées
            };
        }

        /// <summary>
        /// Le trap : prise à deux sur le porteur.
        /// Conséquence : forte chance de turnover, mais 4v3 derrière.
        /// </summary>
        private ScreenReaction HandleTrapCoverage(Player ballHandler)
        {
            return new ScreenReaction
            {
                Type = ScreenReactionType.Trap,
                Description = "Trap — prise à deux sur le porteur",
                TurnoverBonus = +15,           // plus de chances de turnover
                OpenPassBonus = +30            // mais si la passe sort, gros tir ouvert derrière
            };
        }

        /// <summary>
        /// Le ice : on force le porteur vers la ligne de touche.
        /// Conséquence : moins d'options offensives.
        /// </summary>
        private ScreenReaction HandleIceCoverage(Player ballHandler)
        {
            return new ScreenReaction
            {
                Type = ScreenReactionType.Ice,
                Description = "Ice — on force le porteur vers la touche",
                DriveContestModifier = +10
            };
        }

        // ============================================================
        // DÉCISION 2 — RÉAGIR À UN DRIVE
        // ============================================================

        /// <summary>
        /// Calcule le niveau d'aide défensive sur un drive vers le panier.
        /// 
        /// Plus le HelpDefense est élevé, plus il y a d'aide :
        /// - Plus de contestes sur le drive
        /// - MAIS plus de tirs ouverts à 3pts (l'aide laisse un shooteur seul)
        /// </summary>
        public int CalculateHelpOnDrive(DefensiveTactics tactics)
        {
            // L'attribut HelpDefense est entre 0 et 100
            // On le convertit en bonus de contestation sur le drive (-15 à +30)
            int helpLevel = tactics.HelpDefense;

            // Plus l'aide est élevée, plus le drive est contesté
            return (helpLevel - 30) / 2; // -15 si helpLevel=0, +35 si helpLevel=100
        }

        /// <summary>
        /// Détermine s'il y a un kickout possible après une aide défensive.
        /// 
        /// Quand un défenseur aide, il laisse son joueur ouvert.
        /// Si l'aide est forte, le kickout est plus tentant.
        /// </summary>
        public bool ShouldKickOut(DefensiveTactics tactics, Player driver)
        {
            // Probabilité de kickout = HelpDefense * Vision du driver
            double kickoutChance =
                (tactics.HelpDefense / 100.0) *
                (driver.Vision / 100.0) *
                0.7; // facteur de modulation

            return _random.NextDouble() < kickoutChance;
        }

        // ============================================================
        // DÉCISION 3 — CONTESTATION GLOBALE DES TIRS
        // ============================================================

        /// <summary>
        /// Calcule le multiplicateur de contestation global selon
        /// l'agressivité défensive choisie par le coach.
        /// </summary>
        public double GetContestMultiplier(DefensiveTactics tactics)
        {
            // ContestIntensity de 0-100 → multiplicateur 0.5 à 1.5
            return 0.5 + (tactics.ContestIntensity / 100.0);
        }
    }

    // ============================================================
    // CLASSES ET ENUMS ASSOCIÉS
    // ============================================================

    /// <summary>
    /// Résultat d'une réaction défensive sur un écran.
    /// Contient les modificateurs à appliquer aux actions suivantes.
    /// </summary>
    public class ScreenReaction
    {
        /// <summary>Type de réaction.</summary>
        public ScreenReactionType Type { get; set; }

        /// <summary>Description textuelle pour le play-by-play.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Modificateur de contestation sur tir mi-distance.</summary>
        public int MidRangeContestModifier { get; set; }

        /// <summary>Modificateur de contestation sur drive.</summary>
        public int DriveContestModifier { get; set; }

        /// <summary>Modificateur de contestation sur pull-up.</summary>
        public int PullUpContestModifier { get; set; }

        /// <summary>Bonus de probabilité de turnover.</summary>
        public int TurnoverBonus { get; set; }

        /// <summary>Bonus de probabilité de passe ouverte (kickout).</summary>
        public int OpenPassBonus { get; set; }

        /// <summary>Temps supplémentaire consommé par la réaction.</summary>
        public int TimeUsedBonus { get; set; }
    }

    /// <summary>
    /// Types de réactions défensives sur un écran.
    /// </summary>
    public enum ScreenReactionType
    {
        /// <summary>Aucune réaction particulière.</summary>
        NoAction,

        /// <summary>Switch des défenseurs.</summary>
        Switch,

        /// <summary>Drop coverage — le big recule.</summary>
        Drop,

        /// <summary>Hedge — le big sort fort puis recule.</summary>
        Hedge,

        /// <summary>Trap — prise à deux.</summary>
        Trap,

        /// <summary>Ice — on force vers la touche.</summary>
        Ice
    }
}