using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant une interception de balle (steal) par un défenseur.
    /// 
    /// Un steal peut survenir :
    /// - Sur une passe (le défenseur intercepte)
    /// - Sur un dribble (le défenseur arrache la balle)
    /// - Sur un drive (le défenseur dépouille)
    /// 
    /// Conséquences :
    /// - Termine la possession offensive (turnover)
    /// - Donne la balle à l'équipe défensive
    /// - Donne souvent lieu à une transition rapide
    /// 
    /// Données spécifiques :
    /// - Le voleur (PrimaryPlayer)
    /// - La victime du vol (SecondaryPlayer)
    /// - Le contexte du vol (sur passe, sur dribble, etc.)
    /// </summary>
    public class StealEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// La victime du vol (porteur de balle ou passeur).
        /// Exposé avec un nom explicite pour la lisibilité.
        /// </summary>
        public Player Victim
        {
            get => SecondaryPlayer!;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Le contexte dans lequel le steal s'est produit.
        /// </summary>
        public StealContext Context { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de steal.
        /// 
        /// Le steal est rapide (1 seconde) car c'est une action ponctuelle.
        /// </summary>
        public StealEvent(Player stealer, Player victim, StealContext context)
        {
            Type = EventType.Steal;
            PrimaryPlayer = stealer;
            SecondaryPlayer = victim;
            Context = context;
            SecondsUsed = 1;

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle adaptée au contexte du steal.
        /// </summary>
        public override string GenerateDescription()
        {
            return Context switch
            {
                StealContext.OnPass =>
                    $"{PrimaryPlayer.Name} intercepte la passe de {Victim.Name} !",

                StealContext.OnDribble =>
                    $"{PrimaryPlayer.Name} arrache la balle à {Victim.Name} en plein dribble !",

                StealContext.OnDrive =>
                    $"{PrimaryPlayer.Name} dépouille {Victim.Name} en pénétration !",

                StealContext.OnPost =>
                    $"{PrimaryPlayer.Name} fait sauter la balle des mains de {Victim.Name} au poste !",

                _ => $"{PrimaryPlayer.Name} vole la balle à {Victim.Name}"
            };
        }
    }

    // ============================================================
    // ENUM ASSOCIÉ
    // ============================================================

    /// <summary>
    /// Contexte dans lequel un steal peut se produire.
    /// </summary>
    public enum StealContext
    {
        /// <summary>Interception d'une passe entre coéquipiers.</summary>
        OnPass,

        /// <summary>Vol de balle sur un joueur en dribble.</summary>
        OnDribble,

        /// <summary>Dépouillement d'un joueur en pénétration.</summary>
        OnDrive,

        /// <summary>Vol au poste bas (poke away).</summary>
        OnPost
    }
}