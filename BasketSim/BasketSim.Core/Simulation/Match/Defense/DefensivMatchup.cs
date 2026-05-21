using BasketSim.Core.Models;

namespace BasketSim.Core.Simulation.Match.Defense
{
    /// <summary>
    /// Représente les assignations défensives : qui défend qui ?
    /// 
    /// Au début d'une possession, chaque défenseur est assigné à un attaquant
    /// (généralement selon les postes : PG défend PG, C défend C, etc.).
    /// 
    /// Pendant la possession, ces assignations peuvent CHANGER via :
    /// - Les switches sur les écrans (PnR coverage = Switch)
    /// - Les rotations défensives
    /// - Les box outs sur les rebonds
    /// 
    /// Cette classe maintient l'état actuel des matchups et permet
    /// de savoir à tout moment qui défend qui.
    /// 
    /// IMPORTANT : c'est une structure dynamique. Les matchups changent
    /// au cours de la possession, contrairement aux postes naturels
    /// qui sont fixes.
    /// </summary>
    public class DefensiveMatchup
    {
        // ============================================================
        // ÉTAT DES MATCHUPS
        // ============================================================

        /// <summary>
        /// Dictionnaire des assignations défensives.
        /// Clé : un attaquant
        /// Valeur : le défenseur qui le suit
        /// 
        /// Exemple : Matchups[Curry] = Holiday
        /// → Holiday défend sur Curry
        /// </summary>
        private Dictionary<Player, Player> _matchups;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Crée les matchups initiaux à partir des lineups des deux équipes.
        /// 
        /// Les assignations par défaut sont :
        /// - PG attaquant ↔ PG défenseur
        /// - SG attaquant ↔ SG défenseur
        /// - SF attaquant ↔ SF défenseur
        /// - PF attaquant ↔ PF défenseur
        /// - C attaquant ↔ C défenseur
        /// 
        /// Ces matchups peuvent être modifiés ensuite (switches, etc.).
        /// </summary>
        public DefensiveMatchup(Lineup offense, Lineup defense)
        {
            _matchups = new Dictionary<Player, Player>();

            // Assignations par poste (matchup standard)
            AssignMatchup(offense.PointGuard, defense.PointGuard);
            AssignMatchup(offense.ShootingGuard, defense.ShootingGuard);
            AssignMatchup(offense.SmallForward, defense.SmallForward);
            AssignMatchup(offense.PowerForward, defense.PowerForward);
            AssignMatchup(offense.Center, defense.Center);
        }

        /// <summary>
        /// Assigne un défenseur à un attaquant, en sécurité contre les null.
        /// </summary>
        private void AssignMatchup(Player? attacker, Player? defender)
        {
            if (attacker != null && defender != null)
            {
                _matchups[attacker] = defender;
            }
        }

        // ============================================================
        // CONSULTATION DES MATCHUPS
        // ============================================================

        /// <summary>
        /// Retourne le défenseur principal sur un attaquant donné.
        /// 
        /// Retourne null si l'attaquant n'a pas de défenseur assigné
        /// (cas rare, normalement tous les attaquants ont un défenseur).
        /// </summary>
        public Player? GetDefenderOf(Player attacker)
        {
            return _matchups.TryGetValue(attacker, out var defender) ? defender : null;
        }

        /// <summary>
        /// Retourne l'attaquant qu'un défenseur donné est censé suivre.
        /// 
        /// Inverse de GetDefenderOf : c'est la "responsabilité" du défenseur.
        /// </summary>
        public Player? GetAttackerOf(Player defender)
        {
            // Cherche dans les valeurs du dictionnaire
            return _matchups.FirstOrDefault(kvp => kvp.Value == defender).Key;
        }

        // ============================================================
        // MODIFICATION DES MATCHUPS (SWITCHES, ROTATIONS)
        // ============================================================

        /// <summary>
        /// Effectue un switch entre deux défenseurs.
        /// 
        /// Exemple : sur un PnR Curry-Green défendu par Holiday-Embiid,
        /// si la couverture est Switch :
        /// → Embiid défend maintenant Curry
        /// → Holiday défend maintenant Green
        /// 
        /// C'est cette opération qui est appelée.
        /// </summary>
        public void SwitchDefenders(Player attacker1, Player attacker2)
        {
            // Récupère les défenseurs actuels
            var defender1 = GetDefenderOf(attacker1);
            var defender2 = GetDefenderOf(attacker2);

            if (defender1 == null || defender2 == null)
                return; // un des matchups n'existe pas, on annule

            // Échange les assignations
            _matchups[attacker1] = defender2;
            _matchups[attacker2] = defender1;
        }

        /// <summary>
        /// Détermine si un défenseur est en mismatch (taille très différente
        /// de son attaquant actuel).
        /// 
        /// Un mismatch est exploitable offensivement :
        /// - Petit attaquant vs grand défenseur → peut driver et tirer
        /// - Grand attaquant vs petit défenseur → peut poster up
        /// 
        /// Seuil : différence de taille de 15cm ou plus.
        /// </summary>
        public bool IsMismatch(Player attacker)
        {
            var defender = GetDefenderOf(attacker);
            if (defender == null) return false;

            int heightDiff = Math.Abs(attacker.HeightCm - defender.HeightCm);
            return heightDiff >= 15;
        }

        /// <summary>
        /// Détermine le type de mismatch :
        /// - SmallOnBig : l'attaquant est beaucoup plus petit que son défenseur
        /// - BigOnSmall : l'attaquant est beaucoup plus grand que son défenseur
        /// - None : pas de mismatch
        /// </summary>
        public MismatchType GetMismatchType(Player attacker)
        {
            var defender = GetDefenderOf(attacker);
            if (defender == null) return MismatchType.None;

            int heightDiff = attacker.HeightCm - defender.HeightCm;

            if (heightDiff >= 15) return MismatchType.BigOnSmall;
            if (heightDiff <= -15) return MismatchType.SmallOnBig;

            return MismatchType.None;
        }
    }

    /// <summary>
    /// Types de mismatch défensif.
    /// </summary>
    public enum MismatchType
    {
        /// <summary>Pas de mismatch (tailles similaires).</summary>
        None,

        /// <summary>
        /// Attaquant beaucoup plus petit que son défenseur.
        /// Exploitable par vitesse et drives (Curry vs Gobert).
        /// </summary>
        SmallOnBig,

        /// <summary>
        /// Attaquant beaucoup plus grand que son défenseur.
        /// Exploitable par poste bas (Embiid vs Curry).
        /// </summary>
        BigOnSmall
    }
}