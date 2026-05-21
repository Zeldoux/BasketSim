using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant un dribble — déplacement du porteur de balle.
    /// 
    /// Utilisé pour :
    /// - La remontée de balle (du backcourt vers le frontcourt)
    /// - Le déplacement d'une zone à une autre
    /// - L'attaque du porteur face à son défenseur
    /// 
    /// Le dribble peut être interrompu par un steal — c'est géré
    /// par le moteur, pas dans cet event lui-même.
    /// 
    /// Données spécifiques :
    /// - Zone de départ et zone d'arrivée
    /// - Indication si c'est une remontée de balle ou un dribble offensif
    /// </summary>
    public class DribbleEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Zone du terrain d'où part le dribble.
        /// </summary>
        public CourtZone StartZone { get; set; }

        /// <summary>
        /// Zone du terrain où arrive le dribble.
        /// Peut être identique à StartZone (dribble sur place).
        /// </summary>
        public CourtZone EndZone { get; set; }

        /// <summary>
        /// Indique si ce dribble est une remontée de balle
        /// (du backcourt vers le frontcourt).
        /// 
        /// Les remontées :
        /// - Prennent plus de temps (3-5 sec)
        /// - Sont plus sensibles aux interceptions en cas de pression
        /// - Marquent le début effectif de l'organisation offensive
        /// </summary>
        public bool IsBallAdvance { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de dribble.
        /// La durée est calculée en fonction du type de dribble.
        /// </summary>
        public DribbleEvent(Player dribbler, CourtZone start, CourtZone end, bool isBallAdvance)
        {
            Type = EventType.Dribble;
            PrimaryPlayer = dribbler;
            StartZone = start;
            EndZone = end;
            IsBallAdvance = isBallAdvance;

            // Durée typique :
            // - Remontée de balle : 3-5 secondes
            // - Dribble offensif court : 1-2 secondes
            SecondsUsed = isBallAdvance ? 4 : 2;

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle adaptée au contexte.
        /// 
        /// Différencie remontée vs dribble offensif pour le réalisme
        /// du play-by-play.
        /// </summary>
        public override string GenerateDescription()
        {
            if (IsBallAdvance)
            {
                return $"{PrimaryPlayer.Name} remonte la balle";
            }

            // Dribble offensif : on précise la destination
            string destination = GetZoneText(EndZone);
            return $"{PrimaryPlayer.Name} dribble {destination}";
        }

        /// <summary>
        /// Convertit une zone du terrain en texte lisible.
        /// (Méthode dupliquée de BallReceivedEvent — sera centralisée plus tard)
        /// </summary>
        private string GetZoneText(CourtZone zone)
        {
            return zone switch
            {
                CourtZone.RestrictedArea => "vers le panier",
                CourtZone.Paint => "dans la raquette",
                CourtZone.MidRangeLeft => "vers mi-distance gauche",
                CourtZone.MidRangeLeftCenter => "vers mi-distance aile gauche",
                CourtZone.MidRangeCenter => "vers mi-distance centre",
                CourtZone.MidRangeRightCenter => "vers mi-distance aile droite",
                CourtZone.MidRangeRight => "vers mi-distance droite",
                CourtZone.ThreeCornerLeft => "vers le corner gauche",
                CourtZone.ThreeLeftWing => "vers l'aile gauche à 3pts",
                CourtZone.ThreeTop => "au top of the key",
                CourtZone.ThreeRightWing => "vers l'aile droite à 3pts",
                CourtZone.ThreeCornerRight => "vers le corner droit",
                _ => "sur le terrain"
            };
        }
    }
}