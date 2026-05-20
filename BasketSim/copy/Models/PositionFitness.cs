namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente l'évaluation complète d'un joueur à un poste donné.
    /// 
    /// Cette structure est le résultat d'un calcul fait par
    /// PositionFamiliarityCalculator. Elle agrège :
    /// - Le niveau de familiarity du joueur à ce poste
    /// - L'overall rating effectif (modulé par la familiarity)
    /// - Le multiplicateur d'efficacité appliqué
    /// 
    /// Utilisée pour évaluer si un joueur peut jouer à un poste,
    /// et avec quelle efficacité.
    /// </summary>
    public class PositionFitness
    {
        /// <summary>
        /// Le poste évalué.
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Niveau d'aisance du joueur à ce poste.
        /// Calculé en fonction de ses attributs physiques et techniques.
        /// </summary>
        public PositionFamiliarity Familiarity { get; set; }

        /// <summary>
        /// Overall rating du joueur à ce poste, après application
        /// du multiplicateur de familiarity.
        /// Échelle 0-100.
        /// </summary>
        public int EffectiveOverall { get; set; }

        /// <summary>
        /// Overall "brut" du joueur si on appliquait les pondérations
        /// du poste SANS la pénalité de familiarity.
        /// Utile pour comprendre le potentiel théorique du joueur
        /// au poste, avant la pénalité d'adaptation.
        /// </summary>
        public int RawOverall { get; set; }

        /// <summary>
        /// Multiplicateur appliqué aux attributs (0.50 à 1.00).
        /// Reflète le niveau de familiarity.
        /// </summary>
        public double EffectivenessMultiplier { get; set; }

        /// <summary>
        /// Retourne une description lisible de la fitness à ce poste.
        /// Utile pour les logs et l'affichage console du Projet 1.
        /// </summary>
        public override string ToString()
        {
            return $"{Position} : {Familiarity} (Overall {EffectiveOverall}, brut {RawOverall})";
        }
    }
}