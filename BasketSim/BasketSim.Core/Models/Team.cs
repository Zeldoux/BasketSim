using BasketSim.Core.Models.Tactics;

namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente une équipe de basket de manière permanente.
    /// 
    /// Cette classe contient uniquement les caractéristiques durables :
    /// - Identité de l'équipe
    /// - Roster complet (joueurs sous contrat)
    /// - Tactiques par défaut
    /// - Identité du coach
    /// 
    /// L'état dynamique pendant un match (lineup actuel, timeouts restants,
    /// stats du match) est stocké dans TeamMatchState.
    /// 
    /// Cette séparation permet à une même équipe de jouer plusieurs matchs
    /// sans pollution de données entre les rencontres.
    /// </summary>
    public class Team
    {
        // ============================================================
        // IDENTITÉ
        // ============================================================

        /// <summary>
        /// Nom complet de l'équipe (ex: "Los Angeles Lakers").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ville de l'équipe (ex: "Los Angeles").
        /// Séparée du nom car certaines villes ont plusieurs équipes
        /// (LA Lakers vs LA Clippers).
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Code court de 3 lettres (ex: "LAL", "GSW", "BOS").
        /// Standard NBA, utile pour les tableaux et l'affichage condensé.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        // ============================================================
        // ROSTER
        // ============================================================

        /// <summary>
        /// Liste des joueurs sous contrat avec l'équipe.
        /// 
        /// Capacité standard NBA : 15 joueurs maximum.
        /// La validation de cette limite se fera plus tard quand
        /// on implémentera les contrats et la gestion de roster.
        /// 
        /// Pour le Projet 1, on accepte n'importe quel nombre de joueurs
        /// (idéalement 12-15 pour pouvoir simuler des rotations).
        /// </summary>
        public List<Player> Roster { get; set; }

        // ============================================================
        // COACHING
        // ============================================================

        /// <summary>
        /// Nom du coach principal de l'équipe.
        /// 
        /// Volontairement simplifié pour le Projet 1 (juste une string).
        /// Une classe Coach complète avec personnalité tactique,
        /// expérience, et préférences sera créée plus tard.
        /// </summary>
        public string CoachName { get; set; } = string.Empty;

        // ============================================================
        // TACTIQUES PAR DÉFAUT
        // ============================================================

        /// <summary>
        /// Tactiques par défaut de l'équipe.
        /// 
        /// Ces tactiques définissent l'identité de jeu de l'équipe.
        /// Pendant un match, le coach peut les ajuster via TeamMatchState,
        /// mais ces valeurs restent la base de retour.
        /// 
        /// Exemple : les Warriors auront un Pace élevé et un système
        /// SwitchEverything par défaut. Pendant un match, le coach
        /// peut temporairement passer en Drop coverage pour s'adapter.
        /// </summary>
        public TeamTactics Tactics { get; set; }

        // ============================================================
        // CHIMIE D'ÉQUIPE
        // ============================================================

        private int _teamChemistry;
        /// <summary>
        /// Alchimie d'équipe (0-100).
        /// 
        /// Représente la cohésion globale du groupe.
        /// Influence légèrement :
        /// - Les passes décisives (chemistry élevée = plus de assists)
        /// - Les rotations défensives (chemistry élevée = meilleures aides)
        /// - Le moral général en cas de difficulté
        /// 
        /// Pour le Projet 1, c'est une valeur statique.
        /// Plus tard, elle évoluera selon les résultats, transferts, etc.
        /// </summary>
        public int TeamChemistry
        {
            get => _teamChemistry;
            set => _teamChemistry = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut.
        /// Initialise un roster vide, des tactiques neutres et une
        /// chimie d'équipe moyenne (50).
        /// </summary>
        public Team()
        {
            Roster = new List<Player>();
            Tactics = new TeamTactics();
            TeamChemistry = 50;
        }

        // ============================================================
        // MÉTHODES UTILITAIRES
        // ============================================================

        /// <summary>
        /// Retourne l'overall moyen de l'équipe basé sur ses 8 meilleurs
        /// joueurs (rotation principale typique en NBA).
        /// 
        /// Plus représentatif que la moyenne de tout le roster
        /// (qui serait tirée vers le bas par les joueurs de fin de banc).
        /// </summary>
        public int GetTeamOverall()
        {
            if (Roster.Count == 0)
                return 0;

            // Calcule l'overall naturel de chaque joueur et prend les 8 meilleurs
            // (ou moins si le roster a moins de 8 joueurs)
            int playersToConsider = Math.Min(8, Roster.Count);

            var topOveralls = Roster
                .Select(p => Simulation.RatingCalculator.GetNaturalOverall(p))
                .OrderByDescending(o => o)
                .Take(playersToConsider)
                .ToList();

            return (int)Math.Round(topOveralls.Average());
        }

        /// <summary>
        /// Retourne une représentation lisible de l'équipe.
        /// Utile pour le debug et l'affichage console.
        /// </summary>
        public override string ToString()
        {
            return $"{City} {Name} ({Code})";
        }
    }
}