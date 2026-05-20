namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Regroupe toutes les consignes tactiques offensives d'une équipe.
    /// 
    /// Cette classe centralise les décisions stratégiques en attaque :
    /// - Le style général (motion, iso, pick and roll, etc.)
    /// - Le rythme (pace)
    /// - La distribution des touches
    /// - La sélection de tir
    /// - Le rebond offensif
    /// 
    /// Le moteur de simulation lira ces paramètres à chaque possession
    /// pour orienter les décisions des joueurs.
    /// 
    /// LIMITATIONS ACTUELLES (Projet 1 - Vague 1) :
    /// On commence avec les paramètres essentiels. Des paramètres plus fins
    /// (type de P&R, mouvement sans ballon, etc.) seront ajoutés par vagues.
    /// </summary>
    public class OffensiveTactics
    {
        // ============================================================
        // STYLE GÉNÉRAL (enum)
        // ============================================================

        /// <summary>
        /// Le style offensif principal de l'équipe.
        /// Définit la philosophie générale d'attaque.
        /// </summary>
        public OffensiveStyle Style { get; set; } = OffensiveStyle.Balanced;

        // ============================================================
        // PACE (slider 0-100)
        // ============================================================

        private int _pace;
        /// <summary>
        /// Vitesse de jeu en attaque (0-100).
        /// 
        /// 0-25 : Très lent — chaque possession est précieuse, vide le shot clock
        /// 26-45 : Lent — jeu posé en demi-terrain
        /// 46-55 : Équilibré — standard NBA
        /// 56-75 : Rapide — transition prioritaire, tirs en 14s ou moins
        /// 76-100 : Très rapide — courir à chaque possession (style Mike D'Antoni)
        /// 
        /// Influence directe sur :
        /// - Nombre de possessions par match
        /// - Pourcentage de tirs en transition vs demi-terrain
        /// - Fatigue accumulée par les joueurs
        /// </summary>
        public int Pace
        {
            get => _pace;
            set => _pace = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // RECHERCHE DU 3 POINTS (slider 0-100)
        // ============================================================

        private int _threePointTendency;
        /// <summary>
        /// Tendance à chercher le tir à 3 points (0-100).
        /// 
        /// 0 : Ne tirer à 3pts que si totalement ouvert (équipes old-school)
        /// 50 : Équilibre standard NBA moderne (~35-40% des tirs)
        /// 100 : Chercher le 3pts à chaque possession (Rockets Daryl Morey era)
        /// 
        /// Influence le ratio shots 2pts / 3pts dans la simulation.
        /// </summary>
        public int ThreePointTendency
        {
            get => _threePointTendency;
            set => _threePointTendency = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // DISTRIBUTION DES TOUCHES (classe hybride)
        // ============================================================

        /// <summary>
        /// Distribution des touches entre les joueurs de l'équipe.
        /// Classe hybride combinant style + pourcentages + références.
        /// 
        /// Initialisée à une distribution équilibrée par défaut.
        /// </summary>
        public TouchDistribution Distribution { get; set; }

        // ============================================================
        // REBOND OFFENSIF (slider 0-100)
        // ============================================================

        private int _offensiveReboundCrash;
        /// <summary>
        /// Agressivité au rebond offensif (0-100).
        /// 
        /// 0 : Tous les joueurs reculent en défense immédiatement
        ///     (priorité absolue à empêcher la transition adverse)
        /// 30 : Seul le screener/pivot va au rebond
        /// 50 : Équilibre standard (1-2 joueurs au rebond)
        /// 70 : Plusieurs joueurs vont au rebond offensif
        /// 100 : Tous les non-shooteurs crashent les boards
        ///       (style old-school, expose à la transition adverse)
        /// 
        /// Compromis fondamental :
        /// + de rebonds offensifs = + de secondes chances
        /// + d'exposition à la transition adverse
        /// </summary>
        public int OffensiveReboundCrash
        {
            get => _offensiveReboundCrash;
            set => _offensiveReboundCrash = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — initialise des tactiques offensives
        /// équilibrées et neutres, adaptées à la plupart des équipes.
        /// 
        /// Le coach peut ensuite ajuster chaque paramètre individuellement.
        /// </summary>
        public OffensiveTactics()
        {
            Style = OffensiveStyle.Balanced;
            Pace = 50;
            ThreePointTendency = 50;
            OffensiveReboundCrash = 40;
            Distribution = new TouchDistribution();
        }
    }
}