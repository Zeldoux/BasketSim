using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Events
{
    /// <summary>
    /// Classe de base abstraite pour tous les events d'une possession.
    /// 
    /// Définit les propriétés communes à TOUS les events :
    /// - Le type (pour identification)
    /// - Le ou les joueurs impliqués
    /// - Le temps consommé
    /// - La description textuelle (play-by-play)
    /// 
    /// Chaque type d'event concret hérite de cette classe et ajoute
    /// ses propriétés spécifiques (ex: ShotEvent ajoute Zone et ShotContext).
    /// 
    /// IMPORTANT : "abstract" signifie qu'on ne peut pas instancier
    /// directement un PossessionEvent. Il faut créer un type concret
    /// (PassEvent, ShotEvent, etc.). C'est exactement ce qu'on veut.
    /// </summary>
    public abstract class PossessionEvent
    {
        // ============================================================
        // IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Le type de cet event.
        /// Doit être défini par chaque classe fille dans son constructeur.
        /// </summary>
        public EventType Type { get; protected set; }

        // ============================================================
        // ACTEURS
        // ============================================================

        /// <summary>
        /// Le joueur principal de l'event.
        /// 
        /// Selon le type d'event :
        /// - PassEvent : le passeur
        /// - DribbleEvent : le dribbleur
        /// - ShotEvent : le tireur
        /// - ScreenEvent : le poseur d'écran
        /// </summary>
        public Player PrimaryPlayer { get; set; } = null!;

        /// <summary>
        /// Joueur secondaire de l'event, si applicable.
        /// 
        /// Selon le type d'event :
        /// - PassEvent : le receveur
        /// - DriveEvent : le défenseur principal
        /// - ShotEvent : le contesteur
        /// - ScreenEvent : le joueur pour qui l'écran est posé
        /// 
        /// Null si l'event ne concerne qu'un joueur.
        /// </summary>
        public Player? SecondaryPlayer { get; set; }

        // ============================================================
        // CONTEXTE TEMPOREL
        // ============================================================

        /// <summary>
        /// Secondes consommées par cet event.
        /// 
        /// Typiquement :
        /// - Pass : 1 seconde
        /// - Dribble : 2-5 secondes
        /// - Screen : 1 seconde
        /// - Drive : 2-4 secondes
        /// - Shot : 1 seconde
        /// </summary>
        public int SecondsUsed { get; set; }

        /// <summary>
        /// Temps absolu écoulé dans la possession au moment de cet event.
        /// Permet de reconstruire la séquence chronologique.
        /// </summary>
        public int PossessionTimeStamp { get; set; }

        // ============================================================
        // RÉSULTAT
        // ============================================================

        /// <summary>
        /// Indique si l'event a "réussi" dans son objectif.
        /// 
        /// La signification dépend du type :
        /// - PassEvent : la passe a atteint son destinataire
        /// - DriveEvent : le drive a abouti à une finition ou kickout
        /// - ShotEvent : le tir est rentré
        /// - ScreenEvent : l'écran a libéré le bénéficiaire
        /// 
        /// Pour les events sans notion de succès (ex: BallReceived),
        /// cette propriété est généralement true par défaut.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        // ============================================================
        // PLAY-BY-PLAY
        // ============================================================

        /// <summary>
        /// Description textuelle de l'event pour le play-by-play.
        /// 
        /// Exemples :
        /// - "Curry reçoit la balle au top of the key"
        /// - "Green pose un écran sur le défenseur de Curry"
        /// - "Curry passe à Klay dans le corner gauche"
        /// - "Klay tire à 3 points... RÉUSSI !"
        /// 
        /// Cette propriété est remplie par chaque event concret
        /// dans sa méthode GenerateDescription().
        /// </summary>
        public string Description { get; set; } = string.Empty;

        // ============================================================
        // MÉTHODES VIRTUELLES
        // ============================================================

        /// <summary>
        /// Génère la description textuelle de l'event.
        /// 
        /// "virtual" signifie que les classes filles PEUVENT
        /// surcharger cette méthode (et elles doivent le faire
        /// pour donner du contexte spécifique).
        /// 
        /// Pour l'instant, on retourne une description générique.
        /// Chaque classe fille la remplacera par une version dédiée.
        /// </summary>
        public virtual string GenerateDescription()
        {
            return $"[{Type}] {PrimaryPlayer?.Name ?? "Unknown"}";
        }

        // ============================================================
        // MÉTHODE POUR LE DEBUG
        // ============================================================

        /// <summary>
        /// Représentation lisible de l'event pour le debug.
        /// Format : [TimeStamp] Type - Description
        /// </summary>
        public override string ToString()
        {
            return $"[{PossessionTimeStamp:D2}s] {Description}";
        }
    }
}