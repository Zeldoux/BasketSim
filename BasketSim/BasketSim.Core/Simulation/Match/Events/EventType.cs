namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Énumère tous les types d'events possibles dans une possession.
    /// 
    /// Sert d'identifiant pour :
    /// - Filtrer les events par type dans le play-by-play
    /// - Générer des statistiques (combien de drives, combien de passes...)
    /// - Différencier les events sans avoir à faire de "type checking" en code
    /// 
    /// Chaque type correspond à une classe d'event spécifique
    /// (DribbleEvent, PassEvent, etc.).
    /// </summary>
    public enum EventType
    {
        // ============================================================
        // INITIATION DE POSSESSION
        // ============================================================

        /// <summary>
        /// Un joueur reçoit la balle au début ou pendant une possession.
        /// Marque le début effectif d'une action.
        /// </summary>
        BallReceived,

        /// <summary>
        /// Remise en jeu (inbound) — après un panier adverse,
        /// une violation ou une perte de balle hors terrain.
        /// </summary>
        Inbound,

        // ============================================================
        // MOUVEMENT AVEC BALLON
        // ============================================================

        /// <summary>
        /// Dribble — le porteur déplace la balle.
        /// Peut être une simple remontée ou un dribble créatif.
        /// </summary>
        Dribble,

        /// <summary>
        /// Pénétration vers le panier (drive).
        /// Action offensive principale qui peut mener à un tir ou une passe.
        /// </summary>
        Drive,

        // ============================================================
        // MOUVEMENT SANS BALLON
        // ============================================================

        /// <summary>
        /// Coupe vers le panier (cut) — un joueur sans ballon
        /// se déplace pour créer une option de passe.
        /// </summary>
        Cut,

        /// <summary>
        /// Mouvement off-ball — déplacement stratégique
        /// d'un joueur sans ballon (sortie d'écran, repositionnement).
        /// </summary>
        OffBallMovement,

        // ============================================================
        // INTERACTIONS ENTRE JOUEURS
        // ============================================================

        /// <summary>
        /// Passe entre coéquipiers.
        /// La pass est l'action collective la plus fondamentale.
        /// </summary>
        Pass,

        /// <summary>
        /// Écran posé par un joueur sur un défenseur.
        /// Base du pick and roll et de nombreuses actions.
        /// </summary>
        Screen,

        /// <summary>
        /// Pick and Roll — action combinée écran + déplacement.
        /// Event composé qui regroupe plusieurs sous-actions.
        /// </summary>
        PickAndRoll,

        // ============================================================
        // FINITION
        // ============================================================

        /// <summary>
        /// Tentative de tir — l'action finale de la possession.
        /// Sera résolue par ShotResolver.
        /// </summary>
        ShotAttempt,

        /// <summary>
        /// Tir réussi — généré après un ShotAttempt résolu positivement.
        /// Sert à marquer clairement les points dans le play-by-play.
        /// </summary>
        ShotMade,

        /// <summary>
        /// Tir manqué — généré après un ShotAttempt résolu négativement.
        /// </summary>
        ShotMissed,

        // ============================================================
        // RÉCUPÉRATION DE BALLE
        // ============================================================

        /// <summary>
        /// Rebond offensif — l'équipe attaquante récupère son tir manqué.
        /// </summary>
        OffensiveRebound,

        /// <summary>
        /// Rebond défensif — l'équipe défensive récupère le tir manqué.
        /// Termine la possession.
        /// </summary>
        DefensiveRebound,

        // ============================================================
        // ACTIONS DÉFENSIVES
        // ============================================================

        /// <summary>
        /// Interception (steal) — un défenseur vole la balle.
        /// Termine la possession sur un turnover.
        /// </summary>
        Steal,

        /// <summary>
        /// Contre (block) — un défenseur contre un tir.
        /// </summary>
        Block,

        /// <summary>
        /// Déflection — un défenseur dévie la balle sans la prendre.
        /// Perturbe l'attaque sans forcément la terminer.
        /// </summary>
        Deflection,

        // ============================================================
        // INCIDENTS
        // ============================================================

        /// <summary>
        /// Perte de balle non forcée (mauvaise passe, marcher, double dribble).
        /// </summary>
        Turnover,

        /// <summary>
        /// Faute commise — peut être offensive ou défensive.
        /// </summary>
        Foul,

        /// <summary>
        /// Violation — shot clock, marcher, retour en zone, etc.
        /// </summary>
        Violation
    }
}