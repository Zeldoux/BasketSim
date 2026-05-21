namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente les 5 joueurs actuellement sur le terrain.
    /// 
    /// Le Lineup change au cours du match via les rotations et les
    /// remplacements. Chaque slot correspond à un poste TACTIQUE
    /// (pas forcément le poste naturel du joueur).
    /// 
    /// IMPORTANT : un joueur peut occuper un slot différent de son
    /// poste naturel. Par exemple, mettre un Power Forward dans
    /// le slot Center pour jouer "small ball".
    /// Le moteur utilisera PositionFamiliarityCalculator pour ajuster
    /// les performances en fonction de cette différence.
    /// </summary>
    public class Lineup
    {
        // ============================================================
        // LES 5 SLOTS DU LINEUP
        // ============================================================

        /// <summary>
        /// Joueur occupant le poste de meneur (PG).
        /// </summary>
        public Player? PointGuard { get; set; }

        /// <summary>
        /// Joueur occupant le poste d'arrière (SG).
        /// </summary>
        public Player? ShootingGuard { get; set; }

        /// <summary>
        /// Joueur occupant le poste d'ailier (SF).
        /// </summary>
        public Player? SmallForward { get; set; }

        /// <summary>
        /// Joueur occupant le poste d'ailier fort (PF).
        /// </summary>
        public Player? PowerForward { get; set; }

        /// <summary>
        /// Joueur occupant le poste de pivot (C).
        /// </summary>
        public Player? Center { get; set; }

        // ============================================================
        // MÉTHODES D'ACCÈS GROUPÉ
        // ============================================================

        /// <summary>
        /// Retourne la liste des 5 joueurs du lineup dans l'ordre des postes.
        /// Filtre les slots vides (null).
        /// </summary>
        public List<Player> GetAllPlayers()
        {
            var players = new List<Player>();

            if (PointGuard != null) players.Add(PointGuard);
            if (ShootingGuard != null) players.Add(ShootingGuard);
            if (SmallForward != null) players.Add(SmallForward);
            if (PowerForward != null) players.Add(PowerForward);
            if (Center != null) players.Add(Center);

            return players;
        }

        /// <summary>
        /// Retourne le joueur occupant un poste tactique donné.
        /// Retourne null si le slot est vide.
        /// </summary>
        public Player? GetPlayerAt(Position position)
        {
            return position switch
            {
                Position.PointGuard => PointGuard,
                Position.ShootingGuard => ShootingGuard,
                Position.SmallForward => SmallForward,
                Position.PowerForward => PowerForward,
                Position.Center => Center,
                _ => null
            };
        }

        /// <summary>
        /// Place un joueur dans un slot tactique donné.
        /// Remplace le joueur précédemment dans ce slot s'il y en avait un.
        /// </summary>
        public void SetPlayerAt(Position position, Player? player)
        {
            switch (position)
            {
                case Position.PointGuard: PointGuard = player; break;
                case Position.ShootingGuard: ShootingGuard = player; break;
                case Position.SmallForward: SmallForward = player; break;
                case Position.PowerForward: PowerForward = player; break;
                case Position.Center: Center = player; break;
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Vérifie que le lineup est complet (5 joueurs sur le terrain).
        /// 
        /// Un lineup incomplet ne devrait jamais arriver pendant un match,
        /// sauf cas extrême (toute l'équipe exclue pour fautes — théorique).
        /// </summary>
        public bool IsComplete()
        {
            return PointGuard != null
                && ShootingGuard != null
                && SmallForward != null
                && PowerForward != null
                && Center != null;
        }

        /// <summary>
        /// Vérifie qu'aucun joueur n'apparaît deux fois dans le lineup.
        /// Sécurité contre les erreurs de manipulation.
        /// </summary>
        public bool HasNoDuplicates()
        {
            var players = GetAllPlayers();
            return players.Count == players.Distinct().Count();
        }

        /// <summary>
        /// Vérifie que le lineup est valide :
        /// - Complet (5 joueurs)
        /// - Sans doublons
        /// </summary>
        public bool IsValid()
        {
            return IsComplete() && HasNoDuplicates();
        }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut — lineup vide.
        /// Tous les slots sont initialisés à null.
        /// </summary>
        public Lineup()
        {
            // Les propriétés sont déjà null par défaut (nullable references)
        }
    }
}