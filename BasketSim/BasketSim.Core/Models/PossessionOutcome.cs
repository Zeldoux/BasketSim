namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente le résultat final d'une possession offensive.
    /// Chaque possession se termine forcément par l'une de ces issues.
    /// 
    /// Note : la zone et le contexte du tir sont stockés séparément
    /// dans PossessionResult. Cet enum ne décrit que ce qu'il s'est
    /// passé à la fin de la possession.
    /// </summary>
    public enum PossessionOutcome
    {
        // Le tir est rentré — l'équipe marque
        MadeShot,

        // Tir manqué, l'équipe défensive récupère le rebond
        // → possession suivante pour la défense
        MissedShotDefensiveRebound,

        // Tir manqué, l'équipe offensive récupère le rebond
        // → la même équipe garde la possession (deuxième chance)
        MissedShotOffensiveRebound,

        // Perte de balle (mauvaise passe, marcher, double dribble...)
        // → possession suivante pour la défense
        Turnover,

        // Faute défensive sur le tireur — lancers francs à venir
        // La possession s'arrête sur la faute
        Foul,

        // Tir contré par la défense
        // Note : un contre peut être suivi d'un rebond, mais on simplifie
        // ici en considérant le contre comme un outcome final
        Block,

        // Interception par la défense (vol de balle)
        // → possession suivante pour la défense
        Steal
    }
}