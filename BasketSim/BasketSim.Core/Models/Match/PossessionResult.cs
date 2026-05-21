namespace BasketSim.Core.Models.Match
{
    /// <summary>
    /// Représente le résultat final d'une possession terminée.
    /// 
    /// Contrairement à Possession (objet actif pendant la simulation),
    /// PossessionResult est l'archive de ce qui s'est passé :
    /// - Quelle équipe attaquait
    /// - Qui a fait quoi
    /// - Quel a été l'outcome final
    /// - Combien de points ont été marqués
    /// 
    /// Ces résultats sont utiles pour :
    /// - Générer le play-by-play
    /// - Mettre à jour les stats
    /// - Analyser le match a posteriori
    /// </summary>
    public class PossessionResult
    {
        // ============================================================
        // CONTEXTE
        // ============================================================

        /// <summary>
        /// Numéro de la possession dans le match.
        /// </summary>
        public int PossessionNumber { get; set; }

        /// <summary>
        /// Équipe qui attaquait.
        /// </summary>
        public TeamMatchState AttackingTeam { get; set; }

        /// <summary>
        /// Équipe qui défendait.
        /// </summary>
        public TeamMatchState DefendingTeam { get; set; }

        /// <summary>
        /// Temps utilisé par la possession (en secondes).
        /// </summary>
        public int SecondsUsed { get; set; }

        // ============================================================
        // RÉSULTAT
        // ============================================================

        /// <summary>
        /// Le résultat final de la possession.
        /// Détermine ce qui se passe ensuite (changement de possession ou rebond offensif).
        /// </summary>
        public PossessionOutcome Outcome { get; set; }

        /// <summary>
        /// Points marqués pendant cette possession.
        /// 0 si la possession s'est terminée sans panier.
        /// 1, 2 ou 3 selon le type de tir réussi.
        /// 
        /// Peut être > 3 si combinaison panier + lancers francs
        /// (mais on gérera ça plus tard avec la mécanique des fautes).
        /// </summary>
        public int PointsScored { get; set; }

        // ============================================================
        // ACTEURS PRINCIPAUX
        // ============================================================

        /// <summary>
        /// Joueur principal de l'action offensive.
        /// Typiquement le tireur (si tir) ou le porteur (si turnover).
        /// </summary>
        public Player? PrimaryAttacker { get; set; }

        /// <summary>
        /// Joueur défensif principal sur cette action.
        /// Le défenseur du tireur, ou celui qui a fait l'interception/contre.
        /// </summary>
        public Player? PrimaryDefender { get; set; }

        /// <summary>
        /// Si la possession a impliqué une passe décisive,
        /// référence vers le passeur.
        /// Null si pas d'assist.
        /// </summary>
        public Player? Assister { get; set; }

        /// <summary>
        /// Si la possession s'est terminée par un rebond,
        /// référence vers le rebondeur.
        /// Null si pas de rebond (turnover, panier rentré).
        /// </summary>
        public Player? Rebounder { get; set; }

        // ============================================================
        // DÉTAILS DU TIR (si applicable)
        // ============================================================

        /// <summary>
        /// Zone du terrain depuis laquelle le tir a été pris.
        /// Null si la possession ne s'est pas terminée par un tir.
        /// </summary>
        public CourtZone? ShotZone { get; set; }

        /// <summary>
        /// Contexte du tir (catch and shoot, drive, fadeaway, etc.).
        /// Null si la possession ne s'est pas terminée par un tir.
        /// </summary>
        public ShotContext? ShotContext { get; set; }

        // ============================================================
        // PROPRIÉTÉS CALCULÉES
        // ============================================================

        /// <summary>
        /// Indique si la possession s'est terminée par un tir réussi.
        /// </summary>
        public bool WasMadeShot => Outcome == PossessionOutcome.MadeShot;

        /// <summary>
        /// Indique si la possession garde la balle à l'équipe attaquante.
        /// True uniquement sur un rebond offensif.
        /// </summary>
        public bool RetainsPossession =>
            Outcome == PossessionOutcome.MissedShotOffensiveRebound;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut.
        /// Les propriétés seront remplies au fur et à mesure
        /// par le PossessionEngine pendant la simulation.
        /// </summary>
        public PossessionResult(TeamMatchState attacking, TeamMatchState defending)
        {
            AttackingTeam = attacking ?? throw new ArgumentNullException(nameof(attacking));
            DefendingTeam = defending ?? throw new ArgumentNullException(nameof(defending));

            PointsScored = 0;
            SecondsUsed = 0;
        }
    }
}