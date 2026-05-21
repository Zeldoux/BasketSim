namespace BasketSim.Core.Simulation.Resolvers
{
    /// <summary>
    /// Interface de marquage pour identifier les classes qui résolvent
    /// des actions de simulation.
    /// 
    /// Cette interface est volontairement vide — chaque resolver a sa
    /// propre signature de méthode parce que chacun résout une chose
    /// différente :
    /// - ShotResolver retourne un bool (tir réussi ou raté)
    /// - ReboundResolver retourne un Player
    /// - TurnoverResolver retourne un bool
    /// 
    /// Le rôle de cette interface est de :
    /// 1. Marquer ces classes comme appartenant à la même famille
    /// 2. Documenter l'architecture du système
    /// 3. Permettre des futures évolutions (logging commun, par ex.)
    /// 
    /// On pourrait imaginer une interface plus stricte avec une méthode
    /// commune, mais cela compliquerait inutilement le système pour
    /// gagner peu — chaque resolver a vraiment besoin de signatures
    /// spécifiques à son domaine.
    /// </summary>
    public interface IResolver
    {
        // Interface de marquage — pas de méthode commune
    }
}