using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant une perte de balle.
    /// 
    /// Couvre TOUTES les pertes de balle SAUF les steals
    /// (qui ont leur propre StealEvent).
    /// 
    /// Causes possibles :
    /// - Mauvaise passe (out of bounds)
    /// - Marcher (travelling)
    /// - Double dribble
    /// - Faute offensive (charge)
    /// - Violation des 24 secondes
    /// - Violation des 8 secondes (retour en zone)
    /// - Balle qui sort en touche
    /// 
    /// Un steal n'est PAS un turnover de ce type — il a son propre event
    /// car il met en valeur le défenseur. Le turnover ici représente
    /// une erreur "non forcée" du porteur ou de l'équipe attaquante.
    /// 
    /// Données spécifiques :
    /// - Le fauteur (PrimaryPlayer)
    /// - La cause exacte du turnover
    /// </summary>
    public class TurnoverEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// La cause spécifique du turnover.
        /// Influence le play-by-play et les stats détaillées.
        /// </summary>
        public TurnoverCause Cause { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de turnover.
        /// </summary>
        public TurnoverEvent(Player culprit, TurnoverCause cause)
        {
            Type = EventType.Turnover;
            PrimaryPlayer = culprit;
            Cause = cause;
            SecondsUsed = 1;

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description selon la cause du turnover.
        /// </summary>
        public override string GenerateDescription()
        {
            return Cause switch
            {
                TurnoverCause.BadPass =>
                    $"{PrimaryPlayer.Name} fait une mauvaise passe — perte de balle",

                TurnoverCause.OutOfBounds =>
                    $"{PrimaryPlayer.Name} envoie la balle en touche",

                TurnoverCause.Travelling =>
                    $"{PrimaryPlayer.Name} marche — violation",

                TurnoverCause.DoubleDribble =>
                    $"{PrimaryPlayer.Name} commet un double dribble",

                TurnoverCause.OffensiveFoul =>
                    $"{PrimaryPlayer.Name} commet une faute offensive (charge)",

                TurnoverCause.ShotClockViolation =>
                    $"{PrimaryPlayer.Name} laisse échapper le shot clock — violation des 24s",

                TurnoverCause.BackcourtViolation =>
                    $"{PrimaryPlayer.Name} commet un retour en zone",

                TurnoverCause.LostBall =>
                    $"{PrimaryPlayer.Name} perd la balle bêtement",

                _ => $"{PrimaryPlayer.Name} perd la balle"
            };
        }
    }

    // ============================================================
    // ENUM ASSOCIÉ
    // ============================================================

    /// <summary>
    /// Causes possibles d'un turnover non forcé.
    /// </summary>
    public enum TurnoverCause
    {
        /// <summary>Mauvaise passe (interceptée ou hors limites).</summary>
        BadPass,

        /// <summary>La balle sort en touche du fait du porteur.</summary>
        OutOfBounds,

        /// <summary>Marcher (travelling).</summary>
        Travelling,

        /// <summary>Double dribble.</summary>
        DoubleDribble,

        /// <summary>Faute offensive (charge, écran illégal).</summary>
        OffensiveFoul,

        /// <summary>Shot clock expiré (24 secondes).</summary>
        ShotClockViolation,

        /// <summary>Retour en zone arrière.</summary>
        BackcourtViolation,

        /// <summary>Perte de balle sans cause précise (mishandle).</summary>
        LostBall
    }
}