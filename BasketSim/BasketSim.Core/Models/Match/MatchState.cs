namespace BasketSim.Core.Models.Match
{
    /// <summary>
    /// Représente l'état global d'un match à un instant donné.
    /// 
    /// Cette classe est le "tableau d'affichage" central :
    /// - Quel quart-temps on joue
    /// - Combien de temps il reste
    /// - Quelle équipe a la possession
    /// - Quel est le score
    /// 
    /// Le MatchEngine met à jour cet état à chaque possession.
    /// Le MatchState ne contient PAS les stats détaillées des joueurs
    /// (qui sont dans PlayerGameStats) ni les états des équipes
    /// (qui sont dans TeamMatchState).
    /// 
    /// Le MatchState est la "vue d'ensemble" du moment présent.
    /// </summary>
    public class MatchState
    {
        // ============================================================
        // RÉFÉRENCES AUX ÉTATS DES DEUX ÉQUIPES
        // ============================================================

        /// <summary>
        /// État de l'équipe à domicile pendant ce match.
        /// Contient son lineup actuel, ses timeouts, ses stats, etc.
        /// </summary>
        public TeamMatchState HomeTeam { get; set; }

        /// <summary>
        /// État de l'équipe à l'extérieur pendant ce match.
        /// </summary>
        public TeamMatchState AwayTeam { get; set; }

        // ============================================================
        // TEMPS ET QUART-TEMPS
        // ============================================================

        /// <summary>
        /// Quart-temps en cours.
        /// 
        /// 1, 2, 3, 4 = quarts-temps standards
        /// 5, 6, 7... = prolongations successives
        /// 
        /// Standard NBA : un match a 4 quart-temps de 12 minutes.
        /// Une prolongation dure 5 minutes.
        /// </summary>
        public int CurrentQuarter { get; set; }

        /// <summary>
        /// Secondes restantes dans le quart-temps en cours.
        /// 
        /// Décrémente à chaque possession en fonction du temps
        /// utilisé pour cette possession.
        /// 
        /// Quand cette valeur arrive à 0, on passe au quart-temps suivant
        /// (ou en prolongation si nécessaire).
        /// </summary>
        public int SecondsRemainingInQuarter { get; set; }

        // ============================================================
        // POSSESSION
        // ============================================================

        /// <summary>
        /// Indique quelle équipe a actuellement la possession de balle.
        /// 
        /// true = HomeTeam a la balle
        /// false = AwayTeam a la balle
        /// 
        /// Représenté en bool plutôt qu'enum pour simplicité — il n'y
        /// a que deux équipes, pas besoin d'un type plus complexe.
        /// </summary>
        public bool HomeTeamHasBall { get; set; }

        /// <summary>
        /// Numéro de la possession en cours (1, 2, 3...).
        /// 
        /// Compteur global pour le match entier, utile pour :
        /// - Identifier une possession spécifique dans les logs
        /// - Calculer les stats avancées (offensive rating)
        /// - Le debug
        /// </summary>
        public int PossessionNumber { get; set; }

        // ============================================================
        // PROPRIÉTÉS CALCULÉES (utilitaires)
        // ============================================================

        /// <summary>
        /// Référence l'équipe qui a actuellement la possession.
        /// Évite d'écrire "HomeTeamHasBall ? HomeTeam : AwayTeam" partout.
        /// </summary>
        public TeamMatchState AttackingTeam =>
            HomeTeamHasBall ? HomeTeam : AwayTeam;

        /// <summary>
        /// Référence l'équipe qui défend actuellement.
        /// </summary>
        public TeamMatchState DefendingTeam =>
            HomeTeamHasBall ? AwayTeam : HomeTeam;

        /// <summary>
        /// Indique si le match est en prolongation.
        /// </summary>
        public bool IsOvertime => CurrentQuarter > 4;

        /// <summary>
        /// Indique si on est dans les 3 dernières minutes du match.
        /// Important pour la règle des timeouts en NBA.
        /// </summary>
        public bool IsClutchTime =>
            CurrentQuarter >= 4 && SecondsRemainingInQuarter <= 180;

        /// <summary>
        /// Indique si le match est terminé.
        /// 
        /// Le match se termine quand :
        /// - On est dans le 4ème quart-temps (ou plus)
        /// - Le temps est écoulé
        /// - Le score n'est pas à égalité (sinon prolongation)
        /// </summary>
        public bool IsMatchOver
        {
            get
            {
                // Le match ne peut pas être fini avant la fin du Q4
                if (CurrentQuarter < 4)
                    return false;

                // Temps pas encore écoulé
                if (SecondsRemainingInQuarter > 0)
                    return false;

                // Temps écoulé : on vérifie l'égalité
                // Si égalité → prolongation, donc pas fini
                return HomeTeam.Stats.Points != AwayTeam.Stats.Points;
            }
        }

        // ============================================================
        // CONSTANTES NBA
        // ============================================================

        /// <summary>
        /// Durée d'un quart-temps standard en secondes (12 minutes).
        /// </summary>
        public const int RegularQuarterDuration = 12 * 60; // 720 secondes

        /// <summary>
        /// Durée d'une prolongation en secondes (5 minutes).
        /// </summary>
        public const int OvertimeDuration = 5 * 60; // 300 secondes

        /// <summary>
        /// Durée maximale d'une possession (shot clock NBA).
        /// </summary>
        public const int ShotClockDuration = 24;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — initialise un match prêt à commencer.
        /// 
        /// Au début d'un match :
        /// - Quart-temps 1
        /// - Temps complet
        /// - Possession à déterminer (sera décidée par le saut entre deux)
        /// - Aucune possession encore jouée
        /// </summary>
        public MatchState(TeamMatchState homeTeam, TeamMatchState awayTeam)
        {
            HomeTeam = homeTeam ?? throw new ArgumentNullException(nameof(homeTeam));
            AwayTeam = awayTeam ?? throw new ArgumentNullException(nameof(awayTeam));

            CurrentQuarter = 1;
            SecondsRemainingInQuarter = RegularQuarterDuration;
            HomeTeamHasBall = true; // par défaut, sera ajusté par le saut entre deux
            PossessionNumber = 0;
        }

        // ============================================================
        // MÉTHODES UTILITAIRES
        // ============================================================

        /// <summary>
        /// Inverse la possession entre les deux équipes.
        /// Appelé après chaque possession qui se termine sans rebond offensif.
        /// </summary>
        public void SwitchPossession()
        {
            HomeTeamHasBall = !HomeTeamHasBall;
        }

        /// <summary>
        /// Avance au quart-temps suivant.
        /// Réinitialise le temps restant en fonction du nouveau quart-temps
        /// (12 min pour les quarts réguliers, 5 min pour les prolongations).
        /// </summary>
        public void AdvanceToNextQuarter()
        {
            CurrentQuarter++;

            // Si on entre en prolongation (Q5+), durée différente
            SecondsRemainingInQuarter = IsOvertime
                ? OvertimeDuration
                : RegularQuarterDuration;

            // Réinitialise les compteurs de timeouts du quart-temps
            HomeTeam.ResetQuarterlyTimeouts();
            AwayTeam.ResetQuarterlyTimeouts();
        }

        /// <summary>
        /// Retourne le score sous forme de chaîne lisible.
        /// Utile pour le debug et le play-by-play.
        /// </summary>
        public string GetScoreString()
        {
            return $"{HomeTeam.Team.Code} {HomeTeam.Stats.Points} - {AwayTeam.Stats.Points} {AwayTeam.Team.Code}";
        }

        /// <summary>
        /// Retourne une description lisible du moment actuel du match.
        /// Format : "Q3 - 07:42 - LAL 65 - 58 BOS"
        /// </summary>
        public override string ToString()
        {
            int minutes = SecondsRemainingInQuarter / 60;
            int seconds = SecondsRemainingInQuarter % 60;
            string quarterLabel = IsOvertime
                ? $"OT{CurrentQuarter - 4}"
                : $"Q{CurrentQuarter}";

            return $"{quarterLabel} - {minutes:D2}:{seconds:D2} - {GetScoreString()}";
        }
    }
}