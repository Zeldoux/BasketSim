namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Regroupe les consignes de gestion du match.
    /// 
    /// Ces paramètres sont indépendants de l'attaque/défense pure
    /// et concernent la gestion du tempo, du score et du contexte.
    /// 
    /// Plus que les consignes tactiques, ces paramètres sont conçus
    /// pour être MODIFIÉS PENDANT LE MATCH (lors des timeouts) en
    /// fonction de l'évolution du score et du temps restant.
    /// </summary>
    public class GameManagement
    {
        // ============================================================
        // APPROCHE GÉNÉRALE (enum)
        // ============================================================

        /// <summary>
        /// L'approche stratégique générale par rapport au score.
        /// 
        /// Change typiquement plusieurs fois pendant un match :
        /// - Début : ManageTheGame
        /// - Si l'équipe mène : PushTheLead ou Conservative
        /// - Si l'équipe est menée : CatchUp
        /// - Fin de match avec avance : KillTheClock
        /// </summary>
        public GameApproach Approach { get; set; } = GameApproach.ManageTheGame;

        // ============================================================
        // GESTION DU SHOT CLOCK (slider 0-100)
        // ============================================================

        private int _shotClockTendency;
        /// <summary>
        /// Tendance d'utilisation du shot clock (0-100).
        /// 
        /// 0 : Early offense extrême — chercher le tir dans les 7 premières secondes
        ///     (style transition pure, Showtime Lakers)
        /// 30 : Early offense standard (tir entre 7-15s)
        /// 50 : Standard — utilisation normale du shot clock (10-18s)
        /// 70 : Patience — chercher le meilleur tir possible (15-22s)
        /// 100 : Late clock — vider le shot clock systématiquement (20-24s)
        ///       (utilisé pour la dernière possession de quart, ou en KillTheClock)
        /// 
        /// Influence directement le nombre de possessions par match.
        /// </summary>
        public int ShotClockTendency
        {
            get => _shotClockTendency;
            set => _shotClockTendency = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — gestion neutre du match.
        /// Approche équilibrée, utilisation standard du shot clock.
        /// </summary>
        public GameManagement()
        {
            Approach = GameApproach.ManageTheGame;
            ShotClockTendency = 50;
        }
    }
}