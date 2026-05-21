using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Résout les tentatives de tir : le tir rentre-t-il ou non ?
    /// 
    /// C'est l'un des resolvers les plus importants du moteur :
    /// 50% de la simulation tourne autour de cette décision.
    /// 
    /// La formule combine plusieurs facteurs :
    /// 1. Stat de base du joueur selon la zone (Inside/Mid/Three)
    /// 2. Modificateur de la zone (hot/cold zone tendency)
    /// 3. Modificateur du contexte (catch and shoot vs fadeaway)
    /// 4. Pénalité du défenseur
    /// 5. Pénalité de fatigue
    /// 6. Bonus/malus du momentum et de la game form
    /// 
    /// IMPORTANT : Ces formules sont une PREMIÈRE VERSION.
    /// Elles seront ajustées en testant des milliers de matchs simulés.
    /// </summary>
    public class ShotResolver : IResolver
    {
        // ============================================================
        // DÉPENDANCE : SOURCE D'ALÉATOIRE
        // ============================================================
        // On reçoit un Random en paramètre du constructeur plutôt que
        // d'en créer un nouveau. Cela permet la reproductibilité avec
        // une seed contrôlée pour les tests.

        private readonly Random _random;

        /// <summary>
        /// Constructeur avec injection de la source d'aléatoire.
        /// Permet la reproductibilité des simulations en passant
        /// toujours la même seed.
        /// </summary>
        public ShotResolver(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // ============================================================
        // MÉTHODE PRINCIPALE
        // ============================================================

        /// <summary>
        /// Détermine si un tir réussit ou non.
        /// 
        /// Retourne true si le tir est marqué, false sinon.
        /// </summary>
        public bool Resolve(ShotAttemptContext context)
        {
            // Étape 1 : récupère la stat de base selon la zone
            double baseSkill = GetBaseSkillForZone(context.Shooter, context.Zone);

            // Étape 2 : applique le modificateur de tendance de zone
            double afterZoneTendency = ApplyZoneTendency(baseSkill, context);

            // Étape 3 : applique le modificateur de contexte (CnS, drive, fadeaway, etc.)
            double afterContext = ApplyContextModifier(afterZoneTendency, context.Context);

            // Étape 4 : applique la pénalité défensive
            double afterDefense = ApplyDefensivePressure(afterContext, context);

            // Étape 5 : applique les modificateurs d'état (fatigue, momentum, forme)
            double finalChance = ApplyStateModifiers(afterDefense, context);

            // Conversion finale en pourcentage (0-100) → probabilité (0-1)
            double probability = Math.Clamp(finalChance / 100.0, 0.0, 1.0);

            // Tirage aléatoire : le tir rentre si le random est inférieur à la probabilité
            return _random.NextDouble() < probability;
        }

        // ============================================================
        // ÉTAPE 1 : STAT DE BASE SELON LA ZONE
        // ============================================================

        /// <summary>
        /// Retourne la stat de base du joueur pour la zone donnée.
        /// 
        /// Les zones se groupent en 3 catégories :
        /// - Près du panier (RestrictedArea, Paint) → InsideShooting
        /// - Mi-distance (toutes les MidRange) → MidRangeShooting
        /// - 3 points (toutes les Three) → ThreePointShooting
        /// </summary>
        private double GetBaseSkillForZone(Player player, CourtZone zone)
        {
            return zone switch
            {
                CourtZone.RestrictedArea => player.InsideShooting,
                CourtZone.Paint => player.InsideShooting,

                CourtZone.MidRangeLeft => player.MidRangeShooting,
                CourtZone.MidRangeLeftCenter => player.MidRangeShooting,
                CourtZone.MidRangeCenter => player.MidRangeShooting,
                CourtZone.MidRangeRightCenter => player.MidRangeShooting,
                CourtZone.MidRangeRight => player.MidRangeShooting,

                CourtZone.ThreeCornerLeft => player.ThreePointShooting,
                CourtZone.ThreeLeftWing => player.ThreePointShooting,
                CourtZone.ThreeTop => player.ThreePointShooting,
                CourtZone.ThreeRightWing => player.ThreePointShooting,
                CourtZone.ThreeCornerRight => player.ThreePointShooting,

                _ => 50 // valeur par défaut, ne devrait jamais arriver
            };
        }

        // ============================================================
        // ÉTAPE 2 : MODIFICATEUR DE TENDANCE DE ZONE
        // ============================================================

        /// <summary>
        /// Applique le bonus/malus de "hot zone" pour cette zone.
        /// Système inspiré de NBA 2K.
        /// </summary>
        private double ApplyZoneTendency(double baseSkill, ShotAttemptContext context)
        {
            ZoneTendency tendency = context.Shooter.GetZoneTendency(context.Zone);

            // Modificateurs en valeurs absolues (pas en pourcentage)
            // pour rester sur l'échelle 0-100
            double modifier = tendency switch
            {
                ZoneTendency.Cold => -10,
                ZoneTendency.Neutral => 0,
                ZoneTendency.Hot => +5,
                ZoneTendency.Lethal => +10,
                _ => 0
            };

            return baseSkill + modifier;
        }

        // ============================================================
        // ÉTAPE 3 : MODIFICATEUR DE CONTEXTE DE TIR
        // ============================================================

        /// <summary>
        /// Applique un modificateur multiplicatif selon le type de tir.
        /// 
        /// Certains tirs sont plus faciles (catch and shoot à l'arrêt),
        /// d'autres beaucoup plus difficiles (fadeaway, heave).
        /// 
        /// Les valeurs sont des multiplicateurs appliqués à la stat
        /// déjà modifiée par la zone.
        /// </summary>
        private double ApplyContextModifier(double currentSkill, ShotContext context)
        {
            double multiplier = context switch
            {
                // === Tirs faciles ===
                ShotContext.StandStill => 1.00,        // référence
                ShotContext.SpotUp => 1.00,            // tir posé
                ShotContext.CatchAndShoot => 0.98,     // léger malus de réception

                // === Tirs en dribble ===
                ShotContext.PullUp => 0.90,            // tir après dribble
                ShotContext.StepBack => 0.85,          // recul pour créer l'espace
                ShotContext.Hesitation => 0.88,        // tir après hesi

                // === Tirs difficiles ===
                ShotContext.Fadeaway => 0.80,          // tir en se laissant tomber
                ShotContext.Turnaround => 0.83,        // tir en pivotant

                // === Pénétrations ===
                ShotContext.Drive => 0.92,             // pénétration vers le cercle
                ShotContext.Layup => 1.05,             // layup standard, plus facile
                ShotContext.ReverseLayup => 0.92,      // layup inversé, plus dur
                ShotContext.Floater => 0.85,           // floater au-dessus de la défense
                ShotContext.Eurostep => 0.95,          // eurostep bien exécuté
                ShotContext.HopStep => 0.92,           // saut latéral avant finition

                // === Dunks ===
                ShotContext.StandingDunk => 1.10,      // dunk sans élan, très haute %
                ShotContext.DrivingDunk => 1.08,       // dunk en course
                ShotContext.PosterDunk => 0.95,        // dunk au-dessus d'un défenseur

                // === Poste bas ===
                ShotContext.PostUp => 0.95,            // depuis le poste, position posée
                ShotContext.PostHook => 0.90,          // crochet en poste bas
                ShotContext.PostFadeaway => 0.78,      // fadeaway en poste bas, dur

                // === Sans ballon ===
                ShotContext.OffScreen => 0.95,         // tir en sortie d'écran
                ShotContext.FlareScreen => 0.97,       // tir après flare screen

                // === Tactiques ===
                ShotContext.PickAndPop => 1.00,        // écran puis ressort, bonne situation
                ShotContext.Transition => 1.05,        // en transition, défense pas replacée

                // === Tirs désespérés ===
                ShotContext.DeepThree => 0.70,         // très loin de la ligne
                ShotContext.Heave => 0.15,             // tir buzzer, ~3% en NBA

                _ => 1.00
            };

            return currentSkill * multiplier;
        }

        // ============================================================
        // ÉTAPE 4 : PRESSION DÉFENSIVE
        // ============================================================

        /// <summary>
        /// Applique la pénalité défensive sur la stat de tir.
        /// 
        /// Combine :
        /// - La compétence défensive du défenseur (Perimeter/Interior selon zone)
        /// - Le ContestLevel calculé en amont
        /// - L'envergure du défenseur (impact sur les tirs contestés)
        /// </summary>
        private double ApplyDefensivePressure(double currentSkill, ShotAttemptContext context)
        {
            // Si pas de défenseur, pas de pénalité (tir ouvert)
            if (context.Defender == null)
                return currentSkill;

            // Sélectionne la stat défensive pertinente selon la zone
            int defenseStat = IsInteriorZone(context.Zone)
                ? context.Defender.InteriorDefense
                : context.Defender.PerimeterDefense;

            // Calcule la pénalité de base selon la défense
            // Un défenseur à 80 défensif applique une pénalité significative
            double defenseBasePenalty = defenseStat * 0.15;

            // Module la pénalité par le niveau de contestation
            // Un tir non contesté ne subit que ~30% de la pénalité défensive
            // Un tir très contesté la subit à 130%
            double contestMultiplier = 0.3 + (context.ContestLevel / 100.0);

            // Bonus d'envergure : un défenseur à grande envergure pénalise plus
            // 200cm = baseline, chaque cm au-dessus ajoute un petit malus
            double wingspanBonus = Math.Max(0, (context.Defender.WingspanCm - 200) * 0.05);

            double totalPenalty = (defenseBasePenalty + wingspanBonus) * contestMultiplier;

            return currentSkill - totalPenalty;
        }

        /// <summary>
        /// Détermine si une zone est intérieure (raquette).
        /// Utilisé pour choisir entre InteriorDefense et PerimeterDefense.
        /// </summary>
        private bool IsInteriorZone(CourtZone zone)
        {
            return zone == CourtZone.RestrictedArea || zone == CourtZone.Paint;
        }

        // ============================================================
        // ÉTAPE 5 : MODIFICATEURS D'ÉTAT
        // ============================================================

        /// <summary>
        /// Applique les modificateurs liés à l'état du joueur :
        /// - Fatigue (pénalité progressive)
        /// - Momentum (bonus/malus selon le run actuel)
        /// - Game Form (forme du jour)
        /// </summary>
        private double ApplyStateModifiers(double currentSkill, ShotAttemptContext context)
        {
            // Pénalité de fatigue (calculée par PlayerMatchState)
            double fatigueMultiplier = context.ShooterState.GetFatigueMultiplier();
            double afterFatigue = currentSkill * fatigueMultiplier;

            // Modificateur de momentum (-100 à +100)
            // Impact léger : +/- 5 points max sur la stat
            double momentumModifier = context.ShooterState.Momentum * 0.05;

            // Modificateur de game form (0-100, neutre à 50)
            // Impact léger également : +/- 3 points max sur la stat
            double gameFormModifier = (context.ShooterState.GameForm - 50) * 0.06;

            return afterFatigue + momentumModifier + gameFormModifier;
        }
    }
}