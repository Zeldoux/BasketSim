using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Contient toutes les informations nécessaires pour résoudre un tir.
    /// 
    /// Cette classe regroupe les paramètres du tir en un seul objet
    /// plutôt que de passer 10+ paramètres au ShotResolver.
    /// 
    /// Avantages :
    /// - Signature de méthode plus propre
    /// - Facile d'ajouter de nouveaux paramètres sans casser le code existant
    /// - Lisibilité accrue ("context.Shooter" plutôt que "shooter")
    /// </summary>
    public class ShotAttemptContext
    {
        // ============================================================
        // ACTEURS
        // ============================================================

        /// <summary>
        /// Le joueur qui prend le tir.
        /// </summary>
        public Player Shooter { get; set; } = null!;

        /// <summary>
        /// Le défenseur principal contestant le tir.
        /// Peut être null si tir totalement ouvert (théorique).
        /// </summary>
        public Player? Defender { get; set; }

        // ============================================================
        // ÉTAT DU TIREUR
        // ============================================================

        /// <summary>
        /// État du tireur dans le match (fatigue, momentum, forme).
        /// Utilisé pour appliquer les modificateurs dynamiques.
        /// </summary>
        public PlayerMatchState ShooterState { get; set; } = null!;

        // ============================================================
        // CARACTÉRISTIQUES DU TIR
        // ============================================================

        /// <summary>
        /// Zone du terrain depuis laquelle le tir est pris.
        /// Détermine quelle stat de base utiliser (Inside/Mid/Three).
        /// </summary>
        public CourtZone Zone { get; set; }

        /// <summary>
        /// Contexte du tir (catch and shoot, drive, fadeaway, etc.).
        /// Influence la difficulté du tir.
        /// </summary>
        public ShotContext Context { get; set; }

        /// <summary>
        /// Niveau de contestation du tir (0-100).
        /// 
        /// 0 = tir totalement ouvert
        /// 50 = contestation standard
        /// 100 = main devant les yeux, tir extrêmement contesté
        /// 
        /// Calculé en amont selon la position du défenseur,
        /// son envergure, et son timing.
        /// </summary>
        public int ContestLevel { get; set; }
    }
}