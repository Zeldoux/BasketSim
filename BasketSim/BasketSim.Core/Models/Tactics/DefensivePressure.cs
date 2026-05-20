namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente la pression défensive de l'équipe.
    /// 
    /// Classe hybride combinant :
    /// - Un TYPE catégoriel (zone de pression — terrain entier, demi-terrain, etc.)
    /// - Un SLIDER d'intensité (0-100)
    /// - Une CIBLE catégorielle (sur quoi se concentre la pression)
    /// 
    /// EXEMPLE D'UTILISATION :
    /// - Type = HalfCourt
    /// - Intensity = 70
    /// - Target = OnBall
    /// → Pression standard en demi-terrain, forte intensité, ciblée sur le porteur.
    /// </summary>
    public class DefensivePressure
    {
        // ============================================================
        // ZONE DE PRESSION (enum)
        // ============================================================

        /// <summary>
        /// Définit où la pression défensive commence sur le terrain.
        /// </summary>
        public PressureZone Zone { get; set; } = PressureZone.HalfCourt;

        // ============================================================
        // INTENSITÉ (slider 0-100)
        // ============================================================

        private int _intensity;
        /// <summary>
        /// Intensité globale de la pression défensive.
        /// 
        /// 0 = laxiste (pas de pression réelle)
        /// 30 = pression molle
        /// 50 = pression standard
        /// 70 = pression forte
        /// 100 = pression maximale (fatigue rapidement les défenseurs)
        /// 
        /// Influence :
        /// - La probabilité de provoquer des pertes de balle (+)
        /// - La fatigue des défenseurs (+)
        /// - Le risque de fautes (+)
        /// - L'espacement laissé aux tirs ouverts (-)
        /// </summary>
        public int Intensity
        {
            get => _intensity;
            set => _intensity = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // CIBLE DE LA PRESSION (enum)
        // ============================================================

        /// <summary>
        /// Définit où se concentre l'intensité de la pression.
        /// </summary>
        public PressureTarget Target { get; set; } = PressureTarget.Balanced;

        // ============================================================
        // CONSTRUCTEURS
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — pression standard en demi-terrain.
        /// Valeurs neutres adaptées à la plupart des équipes.
        /// </summary>
        public DefensivePressure()
        {
            Zone = PressureZone.HalfCourt;
            Intensity = 50;
            Target = PressureTarget.Balanced;
        }

        /// <summary>
        /// Constructeur paramétré pour créer rapidement
        /// une configuration de pression spécifique.
        /// </summary>
        public DefensivePressure(PressureZone zone, int intensity, PressureTarget target)
        {
            Zone = zone;
            Intensity = intensity;  // clampé automatiquement par le setter
            Target = target;
        }
    }

    // ============================================================
    // ENUMS ASSOCIÉS
    // ============================================================
    // On les met dans le même fichier car ils sont étroitement liés
    // à DefensivePressure et n'ont pas vocation à être réutilisés ailleurs.

    /// <summary>
    /// Définit la zone du terrain où commence la pression défensive.
    /// </summary>
    public enum PressureZone
    {
        /// <summary>
        /// Pas de pression — l'équipe attend en demi-terrain défensif.
        /// Stratégie conservatrice, conserve la fraîcheur.
        /// </summary>
        NoPress,

        /// <summary>
        /// Pression légère après la ligne médiane.
        /// Standard pour la plupart des équipes.
        /// </summary>
        HalfCourt,

        /// <summary>
        /// Pression dès le trois-quarts de terrain.
        /// Forme intermédiaire entre demi-terrain et full court.
        /// </summary>
        ThreeQuarterCourt,

        /// <summary>
        /// Full Court Press — pression sur tout le terrain.
        /// Très énergivore mais peut désorganiser l'adversaire.
        /// Souvent utilisé en fin de match pour revenir au score.
        /// </summary>
        FullCourt
    }

    /// <summary>
    /// Définit où se concentre l'intensité de la pression.
    /// </summary>
    public enum PressureTarget
    {
        /// <summary>
        /// Pression équilibrée — sur le porteur ET les lignes de passe.
        /// Approche par défaut, polyvalente.
        /// </summary>
        Balanced,

        /// <summary>
        /// Pression sur le porteur de balle uniquement.
        /// Empêche le dribble facile mais laisse les passes possibles.
        /// </summary>
        OnBall,

        /// <summary>
        /// Pression sur les lignes de passe (deny).
        /// Empêche les passes faciles mais laisse le dribble.
        /// </summary>
        OnPasses,

        /// <summary>
        /// Pression spécifique sur les shooteurs adverses.
        /// Empêche les tirs ouverts, laisse plus de drive.
        /// Utilisé contre les équipes très axées 3pts.
        /// </summary>
        OnShooters,

        /// <summary>
        /// Pression spécifique sur la star adverse.
        /// Empêche la star de toucher la balle facilement.
        /// Box-and-one ou junk defense.
        /// </summary>
        OnStar
    }
}