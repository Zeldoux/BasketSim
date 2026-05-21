using BasketSim.Core.Models;
using BasketSim.Core.Models.Match;

namespace BasketSim.Core.Simulation.Match
{
    /// <summary>
    /// Contient toutes les informations dont a besoin le DecisionMaker
    /// pour choisir une action.
    /// 
    /// Cette classe regroupe le contexte de la décision en cours :
    /// - Quelle possession est en train de se jouer
    /// - Quel joueur a la balle
    /// - Quelle phase on est en train de résoudre
    /// - Combien de temps il reste sur le shot clock
    /// - Quelle est la dernière action effectuée
    /// 
    /// Plutôt que de passer 10 paramètres au DecisionMaker, on les
    /// regroupe dans cette structure pour clarté.
    /// </summary>
    public class DecisionContext
    {
        // ============================================================
        // CONTEXTE DE LA POSSESSION
        // ============================================================

        /// <summary>
        /// La possession en cours.
        /// Contient les références aux équipes, le porteur, etc.
        /// </summary>
        public Possession Possession { get; set; } = null!;

        /// <summary>
        /// La phase actuelle de la possession.
        /// </summary>
        public PossessionPhase CurrentPhase { get; set; }

        // ============================================================
        // ACTEURS ACTUELS
        // ============================================================

        /// <summary>
        /// Le joueur qui a actuellement la balle.
        /// </summary>
        public Player BallHandler { get; set; } = null!;

        /// <summary>
        /// Le défenseur principal sur le porteur.
        /// Calculé en fonction du matchup défensif.
        /// </summary>
        public Player? PrimaryDefender { get; set; }

        /// <summary>
        /// Position actuelle de la balle sur le terrain.
        /// </summary>
        public CourtZone CurrentBallZone { get; set; }

        // ============================================================
        // ÉTAT DU SHOT CLOCK
        // ============================================================

        /// <summary>
        /// Secondes restantes sur le shot clock.
        /// 
        /// Influence fortement les décisions :
        /// - > 15s : on a le temps, on construit l'action
        /// - 8-15s : on commence à chercher le tir
        /// - < 8s : pression, on doit tirer rapidement
        /// - < 4s : tir forcé probable
        /// </summary>
        public int ShotClockRemaining { get; set; }

        // ============================================================
        // HISTORIQUE
        // ============================================================

        /// <summary>
        /// Nombre d'actions déjà effectuées dans cette possession.
        /// 
        /// Utile pour éviter les possessions trop longues
        /// et imiter le comportement réel (3-5 actions par possession).
        /// </summary>
        public int ActionsCount { get; set; }

        /// <summary>
        /// Indique si la possession est une seconde chance
        /// (suite à un rebond offensif).
        /// 
        /// Les secondes chances ont des comportements différents :
        /// - Plus de tirs forcés
        /// - Plus de tirs près du panier
        /// - Moins de patience
        /// </summary>
        public bool IsSecondChance { get; set; }
    }
}