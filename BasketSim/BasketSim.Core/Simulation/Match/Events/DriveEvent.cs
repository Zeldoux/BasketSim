using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant une pénétration vers le panier (drive).
    /// 
    /// Le drive est l'action offensive la plus dynamique :
    /// - Le porteur attaque le cercle en dribble
    /// - Le défenseur tente de l'arrêter
    /// - Issue possible :
    ///   * Finition au cercle (devient un ShotEvent en RestrictedArea)
    ///   * Kickout (passe à un coéquipier ouvert)
    ///   * Floater (devient un ShotEvent en Paint)
    ///   * Faute provoquée
    ///   * Échec (perte de balle, contre)
    /// 
    /// Cet event représente LE DRIVE LUI-MÊME, pas sa conclusion.
    /// La conclusion sera un autre event (Shot, Pass, etc.) qui suit
    /// immédiatement dans la séquence.
    /// 
    /// Données spécifiques :
    /// - Le défenseur principal contré
    /// - La zone de départ du drive
    /// - L'issue du drive (finition, kickout, etc.)
    /// </summary>
    public class DriveEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Le défenseur principal sur le porteur pendant le drive.
        /// Stocké dans SecondaryPlayer, exposé avec un nom explicite.
        /// </summary>
        public Player Defender
        {
            get => SecondaryPlayer!;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Zone du terrain d'où le drive est initié.
        /// Influence la difficulté (drive depuis le top vs depuis le corner).
        /// </summary>
        public CourtZone StartZone { get; set; }

        /// <summary>
        /// Indique comment le drive se conclut.
        /// 
        /// Cette propriété est définie par le moteur de simulation
        /// après évaluation des chances du driver vs son défenseur.
        /// </summary>
        public DriveOutcome Outcome { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de drive.
        /// 
        /// Durée typique : 2-3 secondes.
        /// L'outcome est initialisé à InProgress et sera défini par le moteur.
        /// </summary>
        public DriveEvent(Player driver, Player defender, CourtZone startZone)
        {
            Type = EventType.Drive;
            PrimaryPlayer = driver;
            SecondaryPlayer = defender;
            StartZone = startZone;
            Outcome = DriveOutcome.InProgress;
            SecondsUsed = 2;

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle adaptée à l'outcome.
        /// 
        /// Le texte est ajusté quand l'outcome est défini par le moteur,
        /// donc cette méthode peut être rappelée a posteriori.
        /// </summary>
        public override string GenerateDescription()
        {
            return Outcome switch
            {
                DriveOutcome.InProgress =>
                    $"{PrimaryPlayer.Name} attaque le panier face à {Defender.Name}",

                DriveOutcome.FinishedAtRim =>
                    $"{PrimaryPlayer.Name} pénètre jusqu'au cercle",

                DriveOutcome.PulledFloater =>
                    $"{PrimaryPlayer.Name} se relève pour un floater",

                DriveOutcome.KickedOut =>
                    $"{PrimaryPlayer.Name} attaque puis ressort la balle",

                DriveOutcome.DrewFoul =>
                    $"{PrimaryPlayer.Name} provoque la faute en pénétrant",

                DriveOutcome.Blocked =>
                    $"{PrimaryPlayer.Name} tente le drive mais est contré par {Defender.Name}",

                DriveOutcome.Stripped =>
                    $"{PrimaryPlayer.Name} perd la balle en pénétrant",

                _ => $"{PrimaryPlayer.Name} attaque le panier"
            };
        }
    }

    // ============================================================
    // ENUM ASSOCIÉ
    // ============================================================

    /// <summary>
    /// Issues possibles d'un drive vers le panier.
    /// </summary>
    public enum DriveOutcome
    {
        /// <summary>État initial — le drive est en cours d'évaluation.</summary>
        InProgress,

        /// <summary>
        /// Le driver atteint le cercle pour une finition (layup/dunk).
        /// Un ShotEvent en RestrictedArea suivra.
        /// </summary>
        FinishedAtRim,

        /// <summary>
        /// Le driver s'arrête pour un floater dans la raquette.
        /// Un ShotEvent en Paint suivra.
        /// </summary>
        PulledFloater,

        /// <summary>
        /// Le driver ressort la balle (kickout) à un coéquipier ouvert.
        /// Un PassEvent suivra.
        /// </summary>
        KickedOut,

        /// <summary>
        /// Le driver provoque une faute défensive.
        /// Va aux lancers francs (à gérer plus tard).
        /// </summary>
        DrewFoul,

        /// <summary>
        /// Le tir au terme du drive est contré.
        /// Termine la possession (sauf rebond offensif).
        /// </summary>
        Blocked,

        /// <summary>
        /// Le porteur perd la balle pendant le drive (steal).
        /// Termine la possession sur turnover.
        /// </summary>
        Stripped
    }
}