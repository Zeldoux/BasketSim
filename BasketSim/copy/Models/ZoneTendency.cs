namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente la "hot zone" d'un joueur dans une zone donnée du terrain.
    /// Modélise le fait qu'un joueur peut être particulièrement efficace
    /// ou inefficace depuis certaines positions.
    /// 
    /// Inspiré directement du système Hot Zones de NBA 2K.
    /// La valeur "Neutral" est celle par défaut pour toutes les zones
    /// où le joueur n'a pas de spécificité.
    /// </summary>
    public enum ZoneTendency
    {
        // Zone froide — le joueur tire mal depuis cette zone (-10 à la stat)
        Cold,

        // Zone neutre — pas de modificateur, niveau standard du joueur
        Neutral,

        // Zone chaude — le joueur tire bien depuis cette zone (+5)
        Hot,

        // Zone létale — spécialité du joueur, performances élite (+10)
        Lethal
    }
}