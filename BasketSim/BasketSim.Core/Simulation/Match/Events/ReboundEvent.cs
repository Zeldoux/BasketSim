using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant un rebond après un tir manqué.
    /// 
    /// Le rebond peut être :
    /// - Offensif : l'équipe attaquante récupère son tir, garde la possession
    /// - Défensif : l'équipe défensive récupère, change de possession
    /// 
    /// Le ReboundEvent suit toujours un ShotEvent manqué.
    /// Il est généré par le moteur après appel au ReboundResolver.
    /// 
    /// Données spécifiques :
    /// - Le rebondeur (PrimaryPlayer)
    /// - Type du rebond (offensif/défensif)
    /// - Zone depuis laquelle le tir manqué a été pris
    /// </summary>
    public class ReboundEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Indique si c'est un rebond offensif.
        /// True : l'équipe attaquante garde la balle
        /// False : changement de possession (rebond défensif)
        /// </summary>
        public bool IsOffensiveRebound { get; set; }

        /// <summary>
        /// Zone d'où le tir manqué a été pris.
        /// Permet de comprendre le contexte du rebond.
        /// </summary>
        public CourtZone OriginalShotZone { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de rebond.
        /// </summary>
        public ReboundEvent(Player rebounder, bool isOffensive, CourtZone shotZone)
        {
            // Le type d'event dépend de la nature du rebond
            Type = isOffensive ? EventType.OffensiveRebound : EventType.DefensiveRebound;

            PrimaryPlayer = rebounder;
            IsOffensiveRebound = isOffensive;
            OriginalShotZone = shotZone;
            SecondsUsed = 1; // rebond instantané

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description du rebond.
        /// </summary>
        public override string GenerateDescription()
        {
            if (IsOffensiveRebound)
            {
                return $"{PrimaryPlayer.Name} récupère le rebond offensif — seconde chance !";
            }

            return $"{PrimaryPlayer.Name} sécurise le rebond défensif";
        }
    }
}