namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente la distribution des touches au sein de l'équipe.
    /// 
    /// Classe hybride combinant :
    /// - Un STYLE catégoriel (enum DistributionStyle)
    ///   qui détermine la philosophie générale de distribution
    /// - Des SLIDERS pour quantifier précisément les pourcentages
    /// 
    /// Cette approche permet de modéliser à la fois la philosophie
    /// ("on joue centré sur la star") et la mesure exacte
    /// ("la star prend 45% des touches précisément").
    /// 
    /// EXEMPLE D'UTILISATION :
    /// - Style = StarCentric
    /// - PrimaryStarUsage = 45
    /// - SecondaryUsage = 25
    /// → La star touche 45% des possessions, le 2e joueur 25%,
    ///   le reste se partage les 30% restants.
    /// </summary>
    public class TouchDistribution
    {
        // ============================================================
        // STYLE CATÉGORIEL
        // ============================================================

        /// <summary>
        /// Le style de distribution général.
        /// Détermine la philosophie d'attaque (star-centric, balanced, etc.).
        /// 
        /// Le moteur utilisera ce style pour les comportements
        /// par défaut quand les pourcentages ne sont pas précisés.
        /// </summary>
        public DistributionStyle Style { get; set; } = DistributionStyle.Balanced;

        // ============================================================
        // SLIDERS QUANTITATIFS (0-100)
        // ============================================================
        // Note : on utilise des propriétés avec backing fields pour
        // pouvoir appliquer le clamp (validation à la création).

        private int _primaryStarUsage;
        /// <summary>
        /// Pourcentage de possessions où le joueur principal a la balle.
        /// 
        /// Valeurs typiques selon le style :
        /// - Balanced : 18-22%
        /// - StarCentric : 35-50%
        /// - TwoStarShare : 25-30%
        /// - PlaymakerCentric : 25-35% (le meneur touche beaucoup)
        /// - PostHub : 25-35% (le pivot touche beaucoup)
        /// </summary>
        public int PrimaryStarUsage
        {
            get => _primaryStarUsage;
            set => _primaryStarUsage = Math.Clamp(value, 0, 100);
        }

        private int _secondaryStarUsage;
        /// <summary>
        /// Pourcentage de possessions pour le joueur secondaire.
        /// 
        /// Pertinent surtout pour TwoStarShare où il s'agit
        /// du deuxième scoreur. Pour les autres styles, c'est
        /// le rôle de support principal.
        /// </summary>
        public int SecondaryStarUsage
        {
            get => _secondaryStarUsage;
            set => _secondaryStarUsage = Math.Clamp(value, 0, 100);
        }

        // ============================================================
        // RÉFÉRENCES AUX JOUEURS CONCERNÉS
        // ============================================================

        /// <summary>
        /// Référence vers le joueur principal pour cette distribution.
        /// 
        /// Nullable car certains styles n'ont pas de joueur "star" :
        /// dans Balanced, tous sont relativement égaux.
        /// 
        /// Le moteur de match utilisera cette référence pour donner
        /// la balle au bon joueur lors des possessions star-centric.
        /// </summary>
        public Player? PrimaryStar { get; set; }

        /// <summary>
        /// Référence vers le joueur secondaire.
        /// 
        /// Pertinent surtout pour TwoStarShare (la deuxième star),
        /// PlaymakerCentric (le scoreur principal après le meneur),
        /// PostHub (le scoreur extérieur secondaire).
        /// </summary>
        public Player? SecondaryStar { get; set; }

        // ============================================================
        // CONSTRUCTEURS
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — distribution équilibrée.
        /// Tous les joueurs auront une utilisation similaire (~20% chacun).
        /// </summary>
        public TouchDistribution()
        {
            Style = DistributionStyle.Balanced;
            PrimaryStarUsage = 20;
            SecondaryStarUsage = 20;
        }

        /// <summary>
        /// Constructeur avec style — applique les valeurs typiques
        /// pour le style choisi. Pratique pour créer rapidement
        /// une distribution cohérente.
        /// </summary>
        public TouchDistribution(DistributionStyle style)
        {
            Style = style;

            // Applique les valeurs typiques selon le style
            // Le coach peut ensuite affiner via les setters
            switch (style)
            {
                case DistributionStyle.StarCentric:
                    PrimaryStarUsage = 40;
                    SecondaryStarUsage = 22;
                    break;

                case DistributionStyle.TwoStarShare:
                    PrimaryStarUsage = 28;
                    SecondaryStarUsage = 26;
                    break;

                case DistributionStyle.PlaymakerCentric:
                    PrimaryStarUsage = 30;  // le meneur
                    SecondaryStarUsage = 22; // le scoreur principal
                    break;

                case DistributionStyle.PostHub:
                    PrimaryStarUsage = 28;  // le pivot
                    SecondaryStarUsage = 22;
                    break;

                case DistributionStyle.Balanced:
                default:
                    PrimaryStarUsage = 20;
                    SecondaryStarUsage = 20;
                    break;
            }
        }
    }
}