using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant un écran posé par un joueur sur un défenseur.
    /// 
    /// L'écran est l'action collective la plus utilisée en basket moderne.
    /// Il sert à :
    /// - Libérer un joueur (sur ou hors ballon)
    /// - Créer un mismatch (le défenseur doit switcher)
    /// - Forcer un drop coverage (le défenseur du screener recule)
    /// 
    /// L'efficacité de l'écran dépend de :
    /// - La force et la taille du poseur d'écran (Strength)
    /// - L'IQ basket (timing et angle)
    /// - La réaction défensive (couverture P&R choisie)
    /// 
    /// Données spécifiques :
    /// - Le bénéficiaire (SecondaryPlayer)
    /// - Le type d'écran (on-ball, off-ball, flare, etc.)
    /// - L'efficacité de l'écran (le défenseur a-t-il été retardé ?)
    /// </summary>
    public class ScreenEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Le joueur qui bénéficie de l'écran.
        /// </summary>
        public Player Beneficiary
        {
            get => SecondaryPlayer!;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Type d'écran posé.
        /// </summary>
        public ScreenType ScreenStyle { get; set; }

        /// <summary>
        /// Indique si l'écran a effectivement libéré le bénéficiaire.
        /// 
        /// True si le défenseur a été retardé/bloqué.
        /// False si le défenseur a passé l'écran sans difficulté
        /// (switch propre, sortie par dessus, etc.).
        /// </summary>
        public bool DidFreeBeneficiary { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event d'écran.
        /// 
        /// L'écran est rapide (1 seconde) car c'est une action ponctuelle.
        /// Son effet sur la possession est géré par les events suivants
        /// (le bénéficiaire est désormais plus libre).
        /// </summary>
        public ScreenEvent(Player screener, Player beneficiary, ScreenType type)
        {
            Type = EventType.Screen;
            PrimaryPlayer = screener;
            SecondaryPlayer = beneficiary;
            ScreenStyle = type;
            SecondsUsed = 1;
            DidFreeBeneficiary = false; // sera défini par le moteur

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle de l'écran.
        /// 
        /// Adapte le texte selon le type d'écran et son efficacité.
        /// </summary>
        public override string GenerateDescription()
        {
            string actionText = ScreenStyle switch
            {
                ScreenType.OnBall => $"pose un écran pour {Beneficiary.Name}",
                ScreenType.OffBall => $"pose un écran off-ball pour libérer {Beneficiary.Name}",
                ScreenType.Flare => $"pose un flare screen pour {Beneficiary.Name}",
                ScreenType.Back => $"pose un back screen pour {Beneficiary.Name}",
                ScreenType.Stagger => $"participe à un double écran pour {Beneficiary.Name}",
                _ => $"pose un écran pour {Beneficiary.Name}"
            };

            return $"{PrimaryPlayer.Name} {actionText}";
        }
    }

    // ============================================================
    // ENUM ASSOCIÉ
    // ============================================================

    /// <summary>
    /// Types d'écrans possibles au basket.
    /// Chacun a un usage tactique spécifique.
    /// </summary>
    public enum ScreenType
    {
        /// <summary>
        /// Écran on-ball — posé sur le défenseur du porteur de balle.
        /// Base du pick and roll classique.
        /// </summary>
        OnBall,

        /// <summary>
        /// Écran off-ball — posé sur le défenseur d'un joueur sans balle.
        /// Libère le bénéficiaire pour recevoir une passe.
        /// </summary>
        OffBall,

        /// <summary>
        /// Flare screen — écran en éventail pour libérer un shooteur
        /// vers l'extérieur (signature des Warriors avec Curry).
        /// </summary>
        Flare,

        /// <summary>
        /// Back screen — écran dans le dos d'un défenseur,
        /// pour permettre une coupe vers le panier.
        /// </summary>
        Back,

        /// <summary>
        /// Stagger screens — double écran consécutif,
        /// souvent utilisé pour les shooteurs élite.
        /// </summary>
        Stagger
    }
}