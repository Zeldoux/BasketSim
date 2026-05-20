namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Regroupe toutes les consignes tactiques défensives d'une équipe.
    /// 
    /// Cette classe centralise les décisions stratégiques en défense :
    /// - Le système défensif principal (man, zone, etc.)
    /// - La pression défensive (où, combien, sur quoi)
    /// - La couverture des pick and roll
    /// - Le niveau d'aide
    /// - L'agressivité de contestation
    /// 
    /// Le moteur de simulation lira ces paramètres à chaque possession
    /// adverse pour orienter les défenseurs.
    /// </summary>
    public class DefensiveTactics
    {
        // ============================================================
        // SYSTÈME DÉFENSIF PRINCIPAL (enum)
        // ============================================================

        /// <summary>
        /// Le système défensif principal (homme à homme, zone, etc.).
        /// Choix fondamental qui change radicalement le comportement défensif.
        /// </summary>
        public DefensiveSystem System { get; set; } = DefensiveSystem.ManToMan;

        // ============================================================
        // PRESSION DÉFENSIVE (classe hybride)
        // ============================================================

        /// <summary>
        /// Configuration de la pression défensive.
        /// Combine zone (où), intensité (combien) et cible (sur quoi).
        /// 
        /// Initialisée à une pression standard en demi-terrain.
        /// </summary>
        public DefensivePressure Pressure { get; set; }

        // ============================================================
        // COUVERTURE DES PICK AND ROLL (enum)
        // ============================================================

        /// <summary>
        /// Comment l'équipe défend les pick and roll.
        /// 
        /// Le P&R étant l'action offensive la plus utilisée en NBA,
        /// ce choix est crucial. Chaque couverture a ses forces
        /// et faiblesses selon le profil des joueurs.
        /// </summary>
        public PnRCoverage PickAndRollCoverage { get; set; } = PnRCoverage.Drop;

        // ============================================================
        // AIDE DÉFENSIVE (slider 0-100)
        // ============================================================

        private int _helpDefense;
        /// <summary>
        /// Niveau d'aide défensive (0-100).
        /// 
        /// 0 : Aucune aide — chacun défend strictement son joueur
        ///     (vulnérable aux drives mais sécurise les shooteurs)
        /// 30 : Aide légère sur les drives uniquement
        /// 50 : Aide standard, rotations classiques
        /// 70 : Aide forte, rotations agressives
        /// 100 : Aide systématique sur toute pénétration
        ///       (protège la raquette mais laisse des tirs ouverts)
        /// 
        /// Compromis fondamental :
        /// + d'aide = - de drives réussis, + de tirs ouverts contestés
        /// - d'aide = + de drives réussis, - de tirs ouverts adverses
        /// </summary>
        public int HelpDefense
        {
            get => _helpDefense;
            set => _helpDefense = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // AGRESSIVITÉ DE CONTESTATION (slider 0-100)
        // ============================================================

        private int _contestIntensity;
        /// <summary>
        /// Agressivité de contestation des tirs (0-100).
        /// 
        /// 0 : Laisser tirer pour focus sur le rebond
        ///     (équipes lentes, mauvais contesteurs)
        /// 50 : Contester de manière standard
        /// 100 : Contester agressivement chaque tir
        ///       (peut provoquer des fautes, fatigue plus rapide)
        /// 
        /// Influence :
        /// - Probabilité de réussite des tirs adverses (-)
        /// - Risque de fautes (+)
        /// - Capacité à prendre le rebond défensif (-)
        /// - Fatigue défensive (+)
        /// </summary>
        public int ContestIntensity
        {
            get => _contestIntensity;
            set => _contestIntensity = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — initialise des tactiques défensives
        /// équilibrées (homme-à-homme, pression standard, drop coverage).
        /// 
        /// Configuration sécurisée adaptée à la plupart des équipes.
        /// </summary>
        public DefensiveTactics()
        {
            System = DefensiveSystem.ManToMan;
            Pressure = new DefensivePressure();
            PickAndRollCoverage = PnRCoverage.Drop;
            HelpDefense = 50;
            ContestIntensity = 60;
        }
    }
}