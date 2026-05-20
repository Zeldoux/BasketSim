using BasketSim.Core.Models;
using System;
using System.Collections.Generic;

namespace BasketSim.Core.Simulation
{
    /// <summary>
    /// Définit les pondérations des attributs pour chaque poste de basket.
    /// 
    /// Cette classe contient UNIQUEMENT des données de configuration —
    /// aucune logique de calcul. Le calcul lui-même est dans RatingCalculator.
    /// 
    /// Cette séparation permet d'ajuster les pondérations facilement
    /// sans toucher au moteur de calcul.
    /// 
    /// PHILOSOPHIE DES POIDS :
    /// - 2.0 : attribut CRITIQUE pour le poste (ex: BallHandling pour PG)
    /// - 1.5 : attribut IMPORTANT pour le poste
    /// - 1.0 : attribut UTILE mais standard
    /// - 0.0 : attribut NON PRIS EN COMPTE pour ce poste
    /// 
    /// Aucun attribut n'a un poids négatif. On ne pénalise jamais —
    /// on valorise plus ou moins selon le poste.
    /// </summary>
    public static class PositionWeights
    {
        // ============================================================
        // NOMS DES ATTRIBUTS (constantes)
        // ============================================================
        // On utilise des constantes string pour les clés du dictionnaire.
        // Avantage : si on renomme un attribut, le compilateur nous alerte
        // partout où la constante est utilisée (refactoring sûr).

        public const string InsideShooting = nameof(InsideShooting);
        public const string MidRangeShooting = nameof(MidRangeShooting);
        public const string ThreePointShooting = nameof(ThreePointShooting);
        public const string FreeThrow = nameof(FreeThrow);
        public const string Touch = nameof(Touch);
        public const string ShotIQ = nameof(ShotIQ);

        public const string BallHandling = nameof(BallHandling);
        public const string Passing = nameof(Passing);
        public const string Vision = nameof(Vision);
        public const string Finishing = nameof(Finishing);
        public const string OffBallMovement = nameof(OffBallMovement);
        public const string PostMoves = nameof(PostMoves);
        public const string Creativity = nameof(Creativity);

        public const string PerimeterDefense = nameof(PerimeterDefense);
        public const string InteriorDefense = nameof(InteriorDefense);
        public const string OnBallDefense = nameof(OnBallDefense);
        public const string OffBallDefense = nameof(OffBallDefense);
        public const string Steal = nameof(Steal);
        public const string Block = nameof(Block);

        public const string OffensiveRebound = nameof(OffensiveRebound);
        public const string DefensiveRebound = nameof(DefensiveRebound);
        public const string Boxout = nameof(Boxout);

        public const string Speed = nameof(Speed);
        public const string Acceleration = nameof(Acceleration);
        public const string Strength = nameof(Strength);
        public const string Stamina = nameof(Stamina);
        public const string Agility = nameof(Agility);
        public const string Balance = nameof(Balance);
        public const string Vertical = nameof(Vertical);
        public const string Hands = nameof(Hands);

        public const string BasketballIQ = nameof(BasketballIQ);
        public const string OffensiveAwareness = nameof(OffensiveAwareness);
        public const string DefensiveAwareness = nameof(DefensiveAwareness);

        // ============================================================
        // FOURCHETTES DE TAILLE IDÉALES PAR POSTE (en cm)
        // ============================================================
        // Utilisées par PositionFamiliarityCalculator pour appliquer
        // une pénalité physique aux postes inadaptés à la taille du joueur.

        /// <summary>
        /// Retourne la fourchette de taille idéale pour un poste.
        /// Hors fourchette = pénalité progressive de familiarity.
        /// </summary>
        public static (int Min, int Max) GetIdealHeightRange(Position position)
        {
            return position switch
            {
                Position.PointGuard => (180, 200),
                Position.ShootingGuard => (190, 205),
                Position.SmallForward => (195, 210),
                Position.PowerForward => (200, 215),
                Position.Center => (205, 225),
                _ => (180, 220) // fallback générique
            };
        }

        // ============================================================
        // PONDÉRATIONS PAR POSTE
        // ============================================================

        /// <summary>
        /// Retourne le dictionnaire des pondérations pour un poste donné.
        /// Les attributs absents du dictionnaire ne sont pas pris en compte
        /// dans le calcul de l'overall à ce poste.
        /// </summary>
        public static Dictionary<string, double> GetWeights(Position position)
        {
            return position switch
            {
                Position.PointGuard => PointGuardWeights,
                Position.ShootingGuard => ShootingGuardWeights,
                Position.SmallForward => SmallForwardWeights,
                Position.PowerForward => PowerForwardWeights,
                Position.Center => CenterWeights,
                _ => throw new ArgumentException($"Poste non supporté : {position}")
            };
        }

        // ============================================================
        // POINT GUARD (Meneur)
        // ============================================================
        // Profil : organisateur, premier porteur de balle, scoring/passing
        // Stats critiques : ball handling, passing, vision, 3pts, vitesse
        // Stats importantes : défense périmètre, IQ basket, finishing
        // ============================================================

        private static readonly Dictionary<string, double> PointGuardWeights = new()
        {
            // CRITIQUES — le poste ne peut pas exister sans ces stats
            { BallHandling,        2.0 },
            { Passing,             2.0 },
            { Vision,              2.0 },
            { ThreePointShooting,  1.5 },
            { Speed,               1.5 },
            { Acceleration,        1.5 },

            // IMPORTANTES
            { PerimeterDefense,    1.2 },
            { OnBallDefense,       1.2 },
            { Steal,               1.2 },
            { BasketballIQ,        1.2 },
            { OffensiveAwareness,  1.2 },
            { ShotIQ,              1.2 },
            { Agility,             1.2 },

            // UTILES (standard)
            { MidRangeShooting,    1.0 },
            { Finishing,           1.0 },
            { Touch,               1.0 },
            { Creativity,          1.0 },
            { FreeThrow,           1.0 },
            { DefensiveAwareness,  1.0 },
            { Stamina,             1.0 },
            { Balance,             1.0 },
        };

        // ============================================================
        // SHOOTING GUARD (Arrière)
        // ============================================================
        // Profil : scoreur extérieur, second porteur, jeu sans ballon
        // Stats critiques : tir 3pts, mi-distance, mouvement sans ballon
        // Stats importantes : ball handling, défense périmètre
        // ============================================================

        private static readonly Dictionary<string, double> ShootingGuardWeights = new()
        {
            // CRITIQUES
            { ThreePointShooting,  2.0 },
            { MidRangeShooting,    1.8 },
            { OffBallMovement,     1.8 },
            { ShotIQ,              1.5 },

            // IMPORTANTES
            { BallHandling,        1.5 },
            { PerimeterDefense,    1.3 },
            { OnBallDefense,       1.2 },
            { Steal,               1.2 },
            { Speed,               1.2 },
            { Acceleration,        1.2 },
            { Agility,             1.2 },
            { FreeThrow,           1.2 },

            // UTILES
            { InsideShooting,      1.0 },
            { Finishing,           1.0 },
            { Touch,               1.0 },
            { Passing,             1.0 },
            { Vision,              1.0 },
            { OffensiveAwareness,  1.0 },
            { DefensiveAwareness,  1.0 },
            { BasketballIQ,        1.0 },
            { Stamina,             1.0 },
            { Balance,             1.0 },
        };

        // ============================================================
        // SMALL FORWARD (Ailier)
        // ============================================================
        // Profil : le plus polyvalent, "wing" moderne
        // Stats critiques : tir 3pts, défense polyvalente, athlétisme
        // Stats importantes : à peu près tout — c'est un couteau suisse
        // ============================================================

        private static readonly Dictionary<string, double> SmallForwardWeights = new()
        {
            // CRITIQUES
            { ThreePointShooting,  1.7 },
            { PerimeterDefense,    1.5 },
            { OnBallDefense,       1.5 },
            { OffBallDefense,      1.5 },

            // IMPORTANTES
            { MidRangeShooting,    1.3 },
            { Finishing,           1.3 },
            { InsideShooting,      1.2 },
            { BallHandling,        1.2 },
            { Strength,            1.2 },
            { Speed,               1.2 },
            { Acceleration,        1.2 },
            { Vertical,            1.2 },
            { DefensiveRebound,    1.2 },
            { BasketballIQ,        1.2 },

            // UTILES
            { Passing,             1.0 },
            { Vision,              1.0 },
            { Steal,               1.0 },
            { Block,               1.0 },
            { ShotIQ,              1.0 },
            { Touch,               1.0 },
            { Agility,             1.0 },
            { Balance,             1.0 },
            { OffensiveAwareness,  1.0 },
            { DefensiveAwareness,  1.0 },
            { OffBallMovement,     1.0 },
            { FreeThrow,           1.0 },
            { Stamina,             1.0 },
        };

        // ============================================================
        // POWER FORWARD (Ailier fort)
        // ============================================================
        // Profil : pont entre intérieur et extérieur, "stretch four" moderne
        // Stats critiques : rebond, défense intérieure, finitions au cercle
        // Stats importantes : tir mi-distance/3pts (modern PF)
        // ============================================================

        private static readonly Dictionary<string, double> PowerForwardWeights = new()
        {
            // CRITIQUES
            { InsideShooting,      1.7 },
            { Finishing,           1.7 },
            { DefensiveRebound,    1.7 },
            { InteriorDefense,     1.5 },
            { Strength,            1.5 },

            // IMPORTANTES
            { OffensiveRebound,    1.3 },
            { Boxout,              1.3 },
            { Block,               1.3 },
            { OffBallDefense,      1.3 },
            { PostMoves,           1.3 },
            { ThreePointShooting,  1.2 }, // modern PF (Draymond, Tatum-style)
            { MidRangeShooting,    1.2 },
            { Vertical,            1.2 },

            // UTILES
            { PerimeterDefense,    1.0 },
            { OnBallDefense,       1.0 },
            { Touch,               1.0 },
            { Hands,               1.0 },
            { Speed,               1.0 },
            { Agility,             1.0 },
            { Balance,             1.0 },
            { BasketballIQ,        1.0 },
            { DefensiveAwareness,  1.0 },
            { OffensiveAwareness,  1.0 },
            { FreeThrow,           1.0 },
            { Stamina,             1.0 },
            { Passing,             1.0 },
        };

        // ============================================================
        // CENTER (Pivot)
        // ============================================================
        // Profil : ancre défensive, dominateur intérieur, ou unicorn moderne
        // Stats critiques : défense intérieure, rebond, finitions
        // Stats importantes : tir extérieur (unicorn), passing (Jokic-style)
        // ============================================================

        private static readonly Dictionary<string, double> CenterWeights = new()
        {
            // CRITIQUES
            { InteriorDefense,     2.0 },
            { DefensiveRebound,    1.8 },
            { Boxout,              1.7 },
            { InsideShooting,      1.7 },
            { Block,               1.6 },
            { Strength,            1.5 },

            // IMPORTANTES
            { OffensiveRebound,    1.4 },
            { Finishing,           1.4 },
            { PostMoves,           1.4 },
            { Vertical,            1.3 },
            { Hands,               1.3 },
            { OffBallDefense,      1.2 },
            { DefensiveAwareness,  1.2 },

            // UTILES — incluant les stats "unicorn" modernes
            { ThreePointShooting,  1.0 }, // pour les Wembanyama, Jokic, Towns
            { MidRangeShooting,    1.0 },
            { Touch,               1.0 },
            { Passing,             1.0 }, // pour les Jokic, Sabonis
            { Vision,              1.0 },
            { BasketballIQ,        1.0 },
            { OffensiveAwareness,  1.0 },
            { FreeThrow,           1.0 },
            { Stamina,             1.0 },
            { Balance,             1.0 },
            { Agility,             1.0 },
        };
    }
}