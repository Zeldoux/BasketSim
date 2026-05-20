namespace BasketSim.Core.Models.Tactics
{
    /// <summary>
    /// Représente la couverture défensive sur les pick and roll.
    /// 
    /// Le pick and roll est l'action offensive la plus utilisée
    /// en NBA moderne — comment le défendre est une décision
    /// tactique cruciale qui peut faire/défaire une équipe.
    /// 
    /// Chaque couverture a ses forces et faiblesses contre
    /// différents types de joueurs.
    /// </summary>
    public enum PnRCoverage
    {
        /// <summary>
        /// Drop coverage — le big recule sous le panier.
        /// Protège la raquette mais laisse des tirs mi-distance ouverts.
        /// Idéal avec un big lent mais grand (Gobert, Adams).
        /// </summary>
        Drop,

        /// <summary>
        /// Hedge — le big sort fort sur le porteur puis recule.
        /// Bonne polyvalence mais demande un big athlétique.
        /// </summary>
        Hedge,

        /// <summary>
        /// Hard Hedge — le big traque agressivement le porteur.
        /// Force la perte de balle mais expose la raquette si pas bien exécuté.
        /// </summary>
        HardHedge,

        /// <summary>
        /// Switch — changement d'assignation entre les deux défenseurs.
        /// Demande un roster polyvalent (tous capables de défendre partout).
        /// Style Warriors Death Lineup.
        /// </summary>
        Switch,

        /// <summary>
        /// Trap — prise à deux systématique sur le porteur.
        /// Force la balle hors du porteur clé mais expose le 4-contre-3 derrière.
        /// </summary>
        Trap,

        /// <summary>
        /// Ice coverage — force le porteur vers la ligne de touche.
        /// Limite les options du porteur mais demande discipline.
        /// </summary>
        Ice,

        /// <summary>
        /// Blitz — deux défenseurs sortent fort sur le porteur.
        /// Variante agressive du trap.
        /// </summary>
        Blitz
    }
}