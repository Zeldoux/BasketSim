using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Contient toutes les informations nécessaires pour résoudre un rebond.
    /// 
    /// Quand un tir est manqué, plusieurs joueurs sont en compétition
    /// pour récupérer le rebond. Le ReboundResolver détermine qui le prend
    /// en fonction de leurs attributs et du contexte.
    /// </summary>
    public class ReboundAttemptContext
    {
        // ============================================================
        // JOUEURS EN COMPÉTITION
        // ============================================================

        /// <summary>
        /// Joueurs offensifs en compétition pour le rebond.
        /// Typiquement 1-5 joueurs selon le crash policy de l'équipe.
        /// </summary>
        public List<Player> OffensiveBoxers { get; set; }

        /// <summary>
        /// Joueurs défensifs en compétition pour le rebond.
        /// Typiquement 4-5 joueurs (presque toute l'équipe).
        /// </summary>
        public List<Player> DefensiveBoxers { get; set; }

        // ============================================================
        // CONTEXTE DU TIR MANQUÉ
        // ============================================================

        /// <summary>
        /// Zone depuis laquelle le tir manqué a été pris.
        /// Influence la zone probable du rebond :
        /// - Tirs courts → rebonds proches du panier
        /// - Tirs longs → rebonds plus éloignés
        /// </summary>
        public CourtZone ShotZone { get; set; }

        /// <summary>
        /// Le joueur qui a tenté le tir manqué.
        /// A un léger bonus de chance pour récupérer son propre rebond
        /// (il est souvent bien placé).
        /// </summary>
        public Player? OriginalShooter { get; set; }

        /// <summary>
        /// Constructeur — initialise les listes vides.
        /// </summary>
        public ReboundAttemptContext()
        {
            OffensiveBoxers = new List<Player>();
            DefensiveBoxers = new List<Player>();
        }
    }
}