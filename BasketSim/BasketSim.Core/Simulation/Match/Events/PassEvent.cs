using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant une passe entre coéquipiers.
    /// 
    /// La passe est l'action offensive collective la plus fondamentale.
    /// Elle peut :
    /// - Réussir → le receveur devient le nouveau porteur
    /// - Échouer (interception) → turnover, fin de possession
    /// - Mener à un panier → potentielle passe décisive (assist)
    /// 
    /// Données spécifiques :
    /// - Le receveur (SecondaryPlayer hérité)
    /// - La zone de destination
    /// - Le type de passe (chest, bounce, lob, behind back...)
    /// - Si c'est une passe potentiellement décisive
    /// </summary>
    public class PassEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Le receveur de la passe.
        /// Stocké dans SecondaryPlayer de la classe de base,
        /// exposé ici avec un nom plus explicite pour la lisibilité.
        /// </summary>
        public Player Receiver
        {
            get => SecondaryPlayer!;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Zone du terrain où la passe est destinée.
        /// </summary>
        public CourtZone TargetZone { get; set; }

        /// <summary>
        /// Type de passe (influence la difficulté et la lisibilité).
        /// </summary>
        public PassType PassStyle { get; set; }

        /// <summary>
        /// Indique si cette passe est une "passe potentiellement décisive".
        /// 
        /// Devient une vraie passe décisive (assist) si le receveur
        /// marque immédiatement (sans dribble ou avec un seul dribble).
        /// 
        /// Le moteur déterminera l'assist a posteriori en regardant
        /// la chaîne d'events.
        /// </summary>
        public bool IsPotentialAssist { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de passe entre deux joueurs.
        /// </summary>
        public PassEvent(Player passer, Player receiver, CourtZone targetZone, PassType passType)
        {
            Type = EventType.Pass;
            PrimaryPlayer = passer;
            SecondaryPlayer = receiver;
            TargetZone = targetZone;
            PassStyle = passType;
            SecondsUsed = 1; // une passe est rapide
            IsPotentialAssist = false; // sera défini par le moteur si pertinent

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle de la passe.
        /// 
        /// Inclut le passeur, le receveur, et éventuellement le style
        /// de passe si c'est notable (lob, behind back, etc.).
        /// </summary>
        public override string GenerateDescription()
        {
            string styleText = GetStyleText(PassStyle);
            string zoneText = GetZoneText(TargetZone);

            if (string.IsNullOrEmpty(styleText))
            {
                return $"{PrimaryPlayer.Name} passe à {Receiver.Name} {zoneText}";
            }

            return $"{PrimaryPlayer.Name} {styleText} pour {Receiver.Name} {zoneText}";
        }

        /// <summary>
        /// Convertit le type de passe en texte expressif.
        /// Pour les passes standard, retourne une chaîne vide
        /// (description plus naturelle).
        /// </summary>
        private string GetStyleText(PassType type)
        {
            return type switch
            {
                PassType.Chest => "",            // passe standard, pas besoin de préciser
                PassType.Bounce => "fait une passe à terre",
                PassType.Overhead => "fait une passe au-dessus de la tête",
                PassType.Lob => "envoie un lob",
                PassType.BehindTheBack => "fait une passe dans le dos",
                PassType.NoLook => "fait une passe aveugle",
                PassType.Outlet => "fait une longue passe de relance",
                _ => ""
            };
        }

        /// <summary>
        /// Convertit une zone en texte lisible.
        /// (Duplication temporaire — sera centralisée plus tard)
        /// </summary>
        private string GetZoneText(CourtZone zone)
        {
            return zone switch
            {
                CourtZone.RestrictedArea => "sous le panier",
                CourtZone.Paint => "dans la raquette",
                CourtZone.ThreeCornerLeft => "dans le corner gauche",
                CourtZone.ThreeLeftWing => "à l'aile gauche",
                CourtZone.ThreeTop => "au top of the key",
                CourtZone.ThreeRightWing => "à l'aile droite",
                CourtZone.ThreeCornerRight => "dans le corner droit",
                _ => ""
            };
        }
    }

    // ============================================================
    // ENUM ASSOCIÉ
    // ============================================================
    // On le met dans le même fichier que PassEvent car il n'est
    // utilisé que par ce type d'event.

    /// <summary>
    /// Types de passes possibles au basket.
    /// Influence la difficulté et la beauté du play-by-play.
    /// </summary>
    public enum PassType
    {
        /// <summary>Passe standard à hauteur de poitrine.</summary>
        Chest,

        /// <summary>Passe à terre — utile pour éviter les bras.</summary>
        Bounce,

        /// <summary>Passe au-dessus de la tête — pour passer sur la défense.</summary>
        Overhead,

        /// <summary>Lob — passe haute, typiquement pour un alley-oop.</summary>
        Lob,

        /// <summary>Passe dans le dos — flashy, plus risquée.</summary>
        BehindTheBack,

        /// <summary>Passe sans regarder — très risquée mais spectaculaire.</summary>
        NoLook,

        /// <summary>Longue passe de relance — typique après un rebond défensif.</summary>
        Outlet
    }
}