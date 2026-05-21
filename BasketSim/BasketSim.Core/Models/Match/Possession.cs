namespace BasketSim.Core.Models.Match
{
    /// <summary>
    /// Représente une possession en cours de simulation.
    /// 
    /// Une possession est un "événement éphémère" qui contient :
    /// - Les équipes impliquées
    /// - Le porteur de balle initial
    /// - Le contexte tactique
    /// - Les variables qui évoluent pendant la possession
    /// 
    /// Une fois la possession terminée, elle devient un PossessionResult
    /// (la version "archivée" avec ce qui s'est passé).
    /// 
    /// IMPORTANT : Possession est un objet "actif" pendant la simulation.
    /// PossessionResult est un objet "passif" après simulation.
    /// </summary>
    public class Possession
    {
        // ============================================================
        // ÉQUIPES IMPLIQUÉES
        // ============================================================

        /// <summary>
        /// L'équipe qui attaque pendant cette possession.
        /// </summary>
        public TeamMatchState AttackingTeam { get; set; }

        /// <summary>
        /// L'équipe qui défend pendant cette possession.
        /// </summary>
        public TeamMatchState DefendingTeam { get; set; }

        // ============================================================
        // ACTEURS PRINCIPAUX
        // ============================================================

        /// <summary>
        /// Le joueur qui a actuellement la balle.
        /// 
        /// Peut changer au cours de la possession (passes, dribbles entre joueurs).
        /// Au moment du tir, c'est le tireur final.
        /// </summary>
        public Player? BallHandler { get; set; }

        /// <summary>
        /// Le défenseur principal sur le porteur.
        /// Change si le porteur change.
        /// </summary>
        public Player? PrimaryDefender { get; set; }

        // ============================================================
        // CONTEXTE TEMPOREL
        // ============================================================

        /// <summary>
        /// Temps écoulé depuis le début de la possession (en secondes).
        /// 
        /// Limite : 24 secondes (shot clock).
        /// Si on atteint 24s sans tir, c'est un "shot clock violation"
        /// (perte de balle).
        /// </summary>
        public int SecondsElapsed { get; set; }

        /// <summary>
        /// Temps restant sur le shot clock.
        /// Propriété calculée pour faciliter la lecture du code.
        /// </summary>
        public int ShotClockRemaining => MatchState.ShotClockDuration - SecondsElapsed;

        // ============================================================
        // INFORMATIONS DE CONTEXTE
        // ============================================================

        /// <summary>
        /// Numéro de la possession dans le match (1, 2, 3...).
        /// Permet d'identifier précisément cette possession.
        /// </summary>
        public int PossessionNumber { get; set; }

        /// <summary>
        /// Indique si cette possession est en transition (contre-attaque).
        /// 
        /// Une possession en transition a généralement :
        /// - Une défense pas encore replacée
        /// - Plus de chances de tir ouvert
        /// - Souvent un layup/dunk ou un 3 points
        /// </summary>
        public bool IsTransition { get; set; }

        /// <summary>
        /// Indique si cette possession est une "seconde chance"
        /// (suite à un rebond offensif).
        /// 
        /// Les secondes chances ont des stats différentes :
        /// + de tirs forcés
        /// + de tirs près du panier
        /// </summary>
        public bool IsSecondChance { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — initialise une nouvelle possession.
        /// 
        /// Le BallHandler et le PrimaryDefender seront définis ensuite
        /// par le PossessionEngine en fonction de la tactique
        /// (qui touche la balle en premier ?).
        /// </summary>
        public Possession(TeamMatchState attacking, TeamMatchState defending)
        {
            AttackingTeam = attacking ?? throw new ArgumentNullException(nameof(attacking));
            DefendingTeam = defending ?? throw new ArgumentNullException(nameof(defending));

            SecondsElapsed = 0;
            IsTransition = false;
            IsSecondChance = false;
        }
    }
}