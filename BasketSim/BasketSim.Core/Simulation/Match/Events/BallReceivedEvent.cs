using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant la réception de la balle par un joueur.
    /// 
    /// C'est généralement le PREMIER event d'une possession :
    /// - Après une remise en jeu suite à un panier adverse
    /// - Après une récupération de rebond défensif
    /// - Après une interception
    /// 
    /// Peut aussi apparaître en milieu de possession, après une passe
    /// (mais dans ce cas le PassEvent gère déjà l'arrivée de la balle).
    /// 
    /// Données spécifiques :
    /// - La zone du terrain où le joueur reçoit la balle
    /// </summary>
    public class BallReceivedEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES
        // ============================================================

        /// <summary>
        /// Zone du terrain où le joueur reçoit la balle.
        /// Utile pour suivre la position initiale et générer
        /// un play-by-play réaliste.
        /// </summary>
        public CourtZone ReceptionZone { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de réception de balle.
        /// Définit automatiquement le Type et la durée typique (1 seconde).
        /// </summary>
        public BallReceivedEvent(Player receiver, CourtZone zone)
        {
            Type = EventType.BallReceived;
            PrimaryPlayer = receiver;
            ReceptionZone = zone;
            SecondsUsed = 1; // réception : ~1 seconde

            // Génère immédiatement la description
            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle pour le play-by-play.
        /// 
        /// Exemples :
        /// - "Curry reçoit la balle au top of the key"
        /// - "Jokic reçoit la balle dans la raquette"
        /// </summary>
        public override string GenerateDescription()
        {
            string zoneText = GetZoneText(ReceptionZone);
            return $"{PrimaryPlayer.Name} reçoit la balle {zoneText}";
        }

        /// <summary>
        /// Convertit une zone du terrain en texte lisible pour le play-by-play.
        /// </summary>
        private string GetZoneText(CourtZone zone)
        {
            return zone switch
            {
                CourtZone.RestrictedArea => "sous le panier",
                CourtZone.Paint => "dans la raquette",
                CourtZone.MidRangeLeft => "à mi-distance côté gauche",
                CourtZone.MidRangeLeftCenter => "à mi-distance aile gauche",
                CourtZone.MidRangeCenter => "à mi-distance face au panier",
                CourtZone.MidRangeRightCenter => "à mi-distance aile droite",
                CourtZone.MidRangeRight => "à mi-distance côté droit",
                CourtZone.ThreeCornerLeft => "dans le corner gauche",
                CourtZone.ThreeLeftWing => "à 3 points aile gauche",
                CourtZone.ThreeTop => "au top of the key",
                CourtZone.ThreeRightWing => "à 3 points aile droite",
                CourtZone.ThreeCornerRight => "dans le corner droit",
                _ => "sur le terrain"
            };
        }
    }
}