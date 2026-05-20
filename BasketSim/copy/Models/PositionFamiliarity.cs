namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente le niveau d'aisance d'un joueur à un poste donné.
    /// Inspiré du système Position Familiarity de Football Manager,
    /// adapté au basket-ball.
    /// 
    /// Lorsqu'un joueur évolue à un poste où il est moins à l'aise,
    /// ses attributs sont effectivement réduits pour le calcul de
    /// performance dans la simulation.
    /// 
    /// L'ordre des valeurs va du plus efficace au moins efficace.
    /// </summary>
    public enum PositionFamiliarity
    {
        /// <summary>
        /// Poste naturel du joueur.
        /// 100% des attributs sont utilisés sans pénalité.
        /// </summary>
        Natural,

        /// <summary>
        /// Poste secondaire, le joueur peut y évoluer efficacement.
        /// Environ 95% des attributs sont utilisés.
        /// Exemple : un Power Forward jouant Center en small ball.
        /// </summary>
        Accomplished,

        /// <summary>
        /// Le joueur peut tenir le poste sans drame.
        /// Environ 85% des attributs sont utilisés.
        /// Exemple : un Small Forward forcé en Shooting Guard.
        /// </summary>
        Competent,

        /// <summary>
        /// Le joueur est en difficulté à ce poste.
        /// Environ 70% des attributs sont utilisés.
        /// Exemple : un Center essayant de jouer Small Forward.
        /// </summary>
        Awkward,

        /// <summary>
        /// Le joueur est totalement inadapté à ce poste.
        /// Environ 50% des attributs sont utilisés.
        /// Exemple : un pivot pur essayant de jouer meneur.
        /// </summary>
        Ineffective
    }
}