using BasketSim.Core.Models.Tactics;

namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente l'état dynamique d'une équipe pendant UN match spécifique.
    /// 
    /// Pendant qu'une Team est l'identité permanente (roster, identité,
    /// tactiques par défaut), TeamMatchState contient toutes les données
    /// qui évoluent au cours d'un match :
    /// 
    /// - Le Lineup actuellement sur le terrain
    /// - Les timeouts restants et leur historique
    /// - Les tactiques courantes (peuvent différer des tactiques par défaut)
    /// - Les stats accumulées
    /// - Le momentum d'équipe
    /// 
    /// À la fin du match, le TeamMatchState peut être archivé ou jeté.
    /// La Team reste intacte pour le match suivant.
    /// </summary>
    public class TeamMatchState
    {
        // ============================================================
        // RÉFÉRENCE À L'ÉQUIPE
        // ============================================================

        /// <summary>
        /// Référence vers l'équipe concernée.
        /// Permet d'accéder aux infos permanentes (nom, roster, etc.).
        /// 
        /// Private set : on ne change jamais l'équipe d'un TeamMatchState
        /// après création (invariant de classe).
        /// </summary>
        public Team Team { get; private set; }

        // ============================================================
        // LINEUP ACTUEL
        // ============================================================

        /// <summary>
        /// Le 5 actuellement sur le terrain.
        /// Change pendant le match via les rotations et remplacements.
        /// </summary>
        public Lineup CurrentLineup { get; set; }

        // ============================================================
        // ÉTATS DES JOUEURS
        // ============================================================

        /// <summary>
        /// État individuel de chaque joueur du roster pendant ce match.
        /// 
        /// Indexé par le joueur lui-même (référence) pour un accès rapide.
        /// Contient la fatigue, le momentum, les fautes, etc. de chaque joueur.
        /// </summary>
        public Dictionary<Player, PlayerMatchState> PlayerStates { get; set; }

        // ============================================================
        // STATS DU MATCH
        // ============================================================

        /// <summary>
        /// Statistiques accumulées par l'équipe pendant ce match.
        /// </summary>
        public TeamGameStats Stats { get; set; }

        /// <summary>
        /// Statistiques de chaque joueur pendant ce match.
        /// Indexé par le joueur (référence).
        /// </summary>
        public Dictionary<Player, PlayerGameStats> PlayerStats { get; set; }

        // ============================================================
        // TIMEOUTS (règles NBA)
        // ============================================================

        /// <summary>
        /// Nombre de timeouts restants pour le match.
        /// 
        /// Règle NBA : 7 timeouts par match maximum.
        /// </summary>
        public int TimeoutsRemaining { get; set; }

        /// <summary>
        /// Nombre de timeouts utilisés dans le quart-temps en cours.
        /// 
        /// Règle NBA : maximum 4 timeouts dans le 4ème quart-temps.
        /// </summary>
        public int TimeoutsUsedInCurrentQuarter { get; set; }

        /// <summary>
        /// Nombre de timeouts utilisés dans les 3 dernières minutes
        /// du match.
        /// 
        /// Règle NBA : maximum 2 timeouts dans les 3 dernières minutes.
        /// Cette règle empêche les coachs d'étirer artificiellement
        /// la fin de match.
        /// </summary>
        public int TimeoutsUsedInLastThreeMinutes { get; set; }

        // ============================================================
        // TACTIQUES COURANTES
        // ============================================================

        /// <summary>
        /// Tactiques actuellement appliquées par l'équipe.
        /// 
        /// Au début du match, c'est une COPIE des tactiques par défaut
        /// de Team.Tactics. Pendant le match, le coach peut modifier
        /// ces tactiques (typiquement lors des timeouts).
        /// 
        /// Les tactiques de Team.Tactics ne sont JAMAIS modifiées —
        /// elles restent la base de l'équipe pour les matchs futurs.
        /// </summary>
        public TeamTactics CurrentTactics { get; set; }

        // ============================================================
        // MOMENTUM D'ÉQUIPE
        // ============================================================

        private int _teamMomentum;
        /// <summary>
        /// Momentum collectif de l'équipe (-100 à +100).
        /// 
        /// - +50 et plus : équipe en feu (run de 8-0, ambiance favorable)
        /// - 0 : neutre
        /// - -50 et moins : équipe en difficulté (run adverse, démoralisée)
        /// 
        /// Évolue à chaque possession importante :
        /// - Tir clutch réussi → +
        /// - Perte de balle stupide → -
        /// - Faute technique → -
        /// - Run de l'équipe → ++
        /// 
        /// Influence légèrement les performances individuelles.
        /// </summary>
        public int TeamMomentum
        {
            get => _teamMomentum;
            set => _teamMomentum = Math.Clamp(value, -100, 100);
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — initialise l'état pour le début d'un match.
        /// 
        /// - Référence l'équipe
        /// - Copie ses tactiques par défaut comme tactiques courantes
        /// - Initialise les timeouts au max NBA (7)
        /// - Crée les états individuels pour chaque joueur du roster
        /// - Initialise les stats à zéro
        /// </summary>
        public TeamMatchState(Team team)
        {
            // Validation : on ne peut pas créer un TeamMatchState sans équipe
            Team = team ?? throw new ArgumentNullException(nameof(team));

            // Initialise le lineup vide (sera rempli par le coach avant le match)
            CurrentLineup = new Lineup();

            // Initialise les stats à zéro
            Stats = new TeamGameStats();

            // Crée un PlayerMatchState et PlayerGameStats pour chaque joueur du roster
            PlayerStates = new Dictionary<Player, PlayerMatchState>();
            PlayerStats = new Dictionary<Player, PlayerGameStats>();

            foreach (var player in team.Roster)
            {
                PlayerStates[player] = new PlayerMatchState(player);
                PlayerStats[player] = new PlayerGameStats();
            }

            // Timeouts NBA standard
            TimeoutsRemaining = 7;
            TimeoutsUsedInCurrentQuarter = 0;
            TimeoutsUsedInLastThreeMinutes = 0;

            // Copie les tactiques par défaut comme tactiques courantes
            // Note : pour l'instant on assigne la référence — pour une vraie copie
            // indépendante, il faudrait implémenter un système de clonage.
            // À voir plus tard si nécessaire.
            CurrentTactics = team.Tactics;

            // Momentum neutre au début
            TeamMomentum = 0;
        }

        // ============================================================
        // MÉTHODES UTILITAIRES
        // ============================================================

        /// <summary>
        /// Détermine si l'équipe peut utiliser un timeout maintenant.
        /// Vérifie les règles NBA :
        /// - Timeouts restants > 0
        /// - Pas plus de 4 timeouts dans le 4ème quart-temps
        /// - Pas plus de 2 timeouts dans les 3 dernières minutes
        /// </summary>
        public bool CanCallTimeout(int currentQuarter, int secondsRemaining)
        {
            // Plus aucun timeout disponible
            if (TimeoutsRemaining <= 0)
                return false;

            // Q4 : maximum 4 timeouts dans ce quart-temps
            if (currentQuarter == 4 && TimeoutsUsedInCurrentQuarter >= 4)
                return false;

            // Dernières 3 minutes : maximum 2 timeouts
            if (currentQuarter == 4 && secondsRemaining <= 180
                && TimeoutsUsedInLastThreeMinutes >= 2)
                return false;

            return true;
        }

        /// <summary>
        /// Enregistre l'utilisation d'un timeout.
        /// Met à jour les compteurs pour le respect des règles NBA.
        /// </summary>
        public void RecordTimeoutUsed(int currentQuarter, int secondsRemaining)
        {
            TimeoutsRemaining--;
            TimeoutsUsedInCurrentQuarter++;

            if (currentQuarter == 4 && secondsRemaining <= 180)
            {
                TimeoutsUsedInLastThreeMinutes++;
            }
        }

        /// <summary>
        /// Réinitialise le compteur de timeouts du quart-temps.
        /// À appeler au début de chaque nouveau quart-temps.
        /// </summary>
        public void ResetQuarterlyTimeouts()
        {
            TimeoutsUsedInCurrentQuarter = 0;
        }
    }
}