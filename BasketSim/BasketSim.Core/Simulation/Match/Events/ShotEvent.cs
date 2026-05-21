using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Event représentant une tentative de tir au panier.
    /// 
    /// C'est l'event le plus important du moteur car :
    /// - Il détermine la majorité du scoring
    /// - Il utilise ShotResolver pour calculer la réussite
    /// - Il génère les statistiques principales (FG%, 3P%, etc.)
    /// 
    /// Le ShotEvent peut être suivi de :
    /// - Un ReboundEvent (si raté)
    /// - Une fin de possession (si réussi)
    /// 
    /// Données spécifiques :
    /// - Zone et contexte du tir (deux dimensions indépendantes)
    /// - Le défenseur principal qui conteste
    /// - Le niveau de contestation
    /// - Le résultat (rentré ou raté)
    /// </summary>
    public class ShotEvent : PossessionEvent
    {
        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES — CARACTÉRISTIQUES DU TIR
        // ============================================================

        /// <summary>
        /// Zone du terrain depuis laquelle le tir est pris.
        /// Détermine la valeur du tir (2 ou 3 points) et la stat utilisée.
        /// </summary>
        public CourtZone ShotZone { get; set; }

        /// <summary>
        /// Contexte du tir (catch and shoot, fadeaway, drive, etc.).
        /// Influence la difficulté indépendamment de la zone.
        /// </summary>
        public ShotContext ShotContextType { get; set; }

        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES — DÉFENSE
        // ============================================================

        /// <summary>
        /// Le défenseur principal contestant le tir.
        /// Stocké dans SecondaryPlayer avec accès explicite.
        /// 
        /// Null possible si tir totalement ouvert (très rare en NBA).
        /// </summary>
        public Player? Contester
        {
            get => SecondaryPlayer;
            set => SecondaryPlayer = value;
        }

        /// <summary>
        /// Niveau de contestation du tir (0-100).
        /// 
        /// 0 = tir totalement ouvert (aucune main proche)
        /// 50 = contestation normale
        /// 100 = main directement devant les yeux du tireur
        /// </summary>
        public int ContestLevel { get; set; }

        // ============================================================
        // PROPRIÉTÉS SPÉCIFIQUES — RÉSULTAT
        // ============================================================

        /// <summary>
        /// Indique si le tir est rentré.
        /// 
        /// Défini par le ShotResolver après évaluation.
        /// Pour interpréter : ShotEvent.IsSuccessful est aussi mis à jour.
        /// </summary>
        public bool IsMade { get; set; }

        /// <summary>
        /// Points marqués si le tir est rentré.
        /// 0 si raté, 2 ou 3 selon la zone si réussi.
        /// 
        /// Calculé automatiquement via la propriété PointValue.
        /// </summary>
        public int PointsScored => IsMade ? PointValue : 0;

        /// <summary>
        /// Valeur du tir en points (2 ou 3).
        /// Déterminée automatiquement par la zone.
        /// </summary>
        public int PointValue => IsThreePointShot ? 3 : 2;

        /// <summary>
        /// Indique si le tir est un tir à 3 points (basé sur la zone).
        /// </summary>
        public bool IsThreePointShot =>
            ShotZone == CourtZone.ThreeCornerLeft ||
            ShotZone == CourtZone.ThreeLeftWing ||
            ShotZone == CourtZone.ThreeTop ||
            ShotZone == CourtZone.ThreeRightWing ||
            ShotZone == CourtZone.ThreeCornerRight;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — crée un event de tentative de tir.
        /// 
        /// Le résultat (IsMade) sera défini par le moteur après
        /// appel au ShotResolver.
        /// </summary>
        public ShotEvent(
            Player shooter,
            CourtZone zone,
            ShotContext context,
            Player? contester,
            int contestLevel)
        {
            Type = EventType.ShotAttempt;
            PrimaryPlayer = shooter;
            SecondaryPlayer = contester;
            ShotZone = zone;
            ShotContextType = context;
            ContestLevel = contestLevel;
            IsMade = false; // sera défini par le moteur
            SecondsUsed = 1; // un tir = 1 seconde

            Description = GenerateDescription();
        }

        // ============================================================
        // GÉNÉRATION DU TEXTE
        // ============================================================

        /// <summary>
        /// Génère une description textuelle adaptée.
        /// 
        /// Cette méthode peut être rappelée APRÈS la résolution du tir
        /// pour inclure le résultat dans la description.
        /// </summary>
        public override string GenerateDescription()
        {
            // Construit la description de l'action de tir
            string shotDescription = GetShotActionText();

            // Si le tir n'a pas encore été résolu, on reste sur l'action
            if (Type == EventType.ShotAttempt && !IsMade)
            {
                return $"{PrimaryPlayer.Name} {shotDescription}";
            }

            // Si le tir est résolu, on précise le résultat
            return IsMade
                ? $"{PrimaryPlayer.Name} {shotDescription} — RÉUSSI ({PointValue} pts)"
                : $"{PrimaryPlayer.Name} {shotDescription} — RATÉ";
        }

        /// <summary>
        /// Construit la description de l'action de tir
        /// en combinant le contexte et la zone.
        /// </summary>
        private string GetShotActionText()
        {
            // Si c'est un dunk, on le précise
            if (ShotContextType == ShotContext.StandingDunk ||
                ShotContextType == ShotContext.DrivingDunk ||
                ShotContextType == ShotContext.PosterDunk)
            {
                return $"dunke {GetShotContextSuffix()}";
            }

            // Sinon, description standard
            string shotType = IsThreePointShot ? "tire à 3 points" : "tire";
            string contextSuffix = GetShotContextSuffix();
            string zoneText = GetShotZoneText();

            return $"{shotType} {zoneText} {contextSuffix}".Trim();
        }

        /// <summary>
        /// Convertit le contexte du tir en texte expressif.
        /// </summary>
        private string GetShotContextSuffix()
        {
            return ShotContextType switch
            {
                ShotContext.CatchAndShoot => "en catch and shoot",
                ShotContext.PullUp => "en pull-up",
                ShotContext.StepBack => "en step-back",
                ShotContext.Fadeaway => "en fadeaway",
                ShotContext.Drive => "en pénétration",
                ShotContext.Layup => "en layup",
                ShotContext.Floater => "en floater",
                ShotContext.Eurostep => "en eurostep",
                ShotContext.StandingDunk => "à l'arrêt",
                ShotContext.DrivingDunk => "en course",
                ShotContext.PosterDunk => "par-dessus la défense",
                ShotContext.PostUp => "depuis le poste bas",
                ShotContext.PostHook => "en crochet",
                ShotContext.OffScreen => "en sortie d'écran",
                ShotContext.PickAndPop => "après le pick and pop",
                ShotContext.Transition => "en transition",
                ShotContext.DeepThree => "de très loin",
                ShotContext.Heave => "désespéré du milieu de terrain",
                _ => ""
            };
        }

        /// <summary>
        /// Convertit la zone du tir en texte lisible.
        /// </summary>
        private string GetShotZoneText()
        {
            return ShotZone switch
            {
                CourtZone.RestrictedArea => "au cercle",
                CourtZone.Paint => "dans la raquette",
                CourtZone.MidRangeLeft => "à mi-distance gauche",
                CourtZone.MidRangeLeftCenter => "à mi-distance aile gauche",
                CourtZone.MidRangeCenter => "à mi-distance",
                CourtZone.MidRangeRightCenter => "à mi-distance aile droite",
                CourtZone.MidRangeRight => "à mi-distance droite",
                CourtZone.ThreeCornerLeft => "depuis le corner gauche",
                CourtZone.ThreeLeftWing => "depuis l'aile gauche",
                CourtZone.ThreeTop => "du top of the key",
                CourtZone.ThreeRightWing => "depuis l'aile droite",
                CourtZone.ThreeCornerRight => "depuis le corner droit",
                _ => ""
            };
        }
    }
}