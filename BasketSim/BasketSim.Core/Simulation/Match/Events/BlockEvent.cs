using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant un contre (block) d'un tir par un défenseur.
    /// 
    /// Le block survient pendant un tir :
    /// - Le défenseur intercepte la trajectoire du ballon
    /// - Le tir est considéré comme raté
    /// - La balle peut être :
    ///   * Récupérée par n'importe quelle équipe (jouer le rebond)
    ///   * Sortie en touche (remise en jeu)
    /// 
    /// IMPORTANT : Le BlockEvent suit toujours un ShotEvent.
    /// Le ShotEvent.IsMade sera false, et le BlockEvent indique
    /// la cause de cet échec.
    /// 
    /// Données spécifiques :
    /// - Le contreur (PrimaryPlayer)
    /// - Le tireur contré (SecondaryPlayer)
    /// - Si la balle reste en jeu après le contre
    /// </summary>
    public class BlockEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Le tireur dont le tir a été contré.
        /// Exposé avec un nom explicite.
        /// </summary>
        public Player Shooter
        {
            get => SecondaryPlayer!;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Indique si la balle reste en jeu après le contre.
        /// 
        /// True : la balle est jouable, un rebond va se disputer
        /// False : la balle est sortie en touche, remise en jeu
        /// 
        /// Statistiquement, ~60% des contres laissent la balle en jeu.
        /// </summary>
        public bool BallStaysInPlay { get; set; }

        /// <summary>
        /// Zone d'où le tir contré a été pris.
        /// Utile pour les stats et la narration.
        /// </summary>
        public CourtZone ShotZone { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de contre.
        /// 
        /// Le contre est instantané (1 seconde).
        /// </summary>
        public BlockEvent(Player blocker, Player shooter, CourtZone shotZone, bool ballStaysInPlay)
        {
            Type = EventType.Block;
            PrimaryPlayer = blocker;
            SecondaryPlayer = shooter;
            ShotZone = shotZone;
            BallStaysInPlay = ballStaysInPlay;
            SecondsUsed = 1;

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description du contre adaptée au contexte.
        /// </summary>
        public override string GenerateDescription()
        {
            string ballState = BallStaysInPlay
                ? "la balle reste en jeu"
                : "la balle sort en touche";

            return $"{PrimaryPlayer.Name} CONTRE le tir de {Shooter.Name} ! ({ballState})";
        }
    }
}