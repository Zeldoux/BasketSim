using System;

namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente l'état dynamique d'un joueur pendant UN match spécifique.
    /// 
    /// Cette classe est volontairement séparée de Player car ces données :
    /// - changent en permanence pendant le match
    /// - sont réinitialisées au début de chaque match
    /// - ne doivent JAMAIS modifier les attributs permanents du joueur
    /// 
    /// Exemple : la fatigue de la fin du match d'hier ne doit pas
    /// affecter la fraîcheur du joueur au match d'aujourd'hui.
    /// 
    /// Chaque joueur d'une feuille de match a SON PlayerMatchState dédié.
    /// </summary>
    public class PlayerMatchState
    {
        // ============================================================
        // RÉFÉRENCE AU JOUEUR
        // ============================================================

        /// <summary>
        /// Référence vers le joueur concerné.
        /// Permet d'accéder à ses attributs permanents (Stamina, etc.)
        /// pour calculer les modificateurs en match.
        /// 
        /// Note : c'est une référence, pas une copie. Modifier
        /// Player.Stamina ici affecterait le joueur "réel" — ce qu'on
        /// ne veut surtout pas. Seul l'état dans cette classe doit
        /// être modifié pendant le match.
        /// </summary>
        public Player Player { get; private set; }

        // ============================================================
        // ÉTAT MENTAL DU MATCH
        // ============================================================

        /// <summary>
        /// Forme du jour (0-100) — générée au début du match.
        /// Mix de la Morale du joueur + part d'aléatoire pondérée
        /// par sa Consistency.
        /// 
        /// Un joueur très consistent (90+) aura une GameForm proche
        /// de sa Morale. Un joueur peu consistent (40-) peut avoir
        /// une GameForm très éloignée de sa Morale (très bonne ou
        /// très mauvaise journée).
        /// </summary>
        public int GameForm { get; set; }

        /// <summary>
        /// Momentum en direct (-100 à +100) — évolue à chaque action.
        /// 
        /// Tir réussi → +momentum
        /// Tir manqué → -momentum léger
        /// Perte de balle → -momentum
        /// Faute défensive provoquée → +momentum
        /// Steal/Block → +momentum
        /// 
        /// Influence légèrement les probabilités de réussite suivantes.
        /// Modélise le "hot hand" effect (un joueur en confiance score plus).
        /// </summary>
        public int Momentum { get; set; }

        // ============================================================
        // ÉTAT PHYSIQUE DU MATCH
        // ============================================================

        /// <summary>
        /// Fatigue actuelle (0-100) — 0 = frais, 100 = épuisé.
        /// 
        /// - Augmente quand le joueur est sur le terrain
        /// - Diminue quand le joueur est sur le banc
        /// - Vitesse d'évolution dépend du Stamina du joueur
        /// - Influence les performances (tir, défense, vitesse)
        /// </summary>
        public int CurrentFatigue { get; set; }

        // ============================================================
        // STATUT SUR LE TERRAIN
        // ============================================================

        /// <summary>
        /// Indique si le joueur est actuellement sur le terrain.
        /// false = sur le banc (ou hors feuille de match).
        /// </summary>
        public bool IsOnCourt { get; set; }

        /// <summary>
        /// Nombre de minutes jouées dans ce match.
        /// Utilisé en double pour gérer les fractions de minutes
        /// (un joueur peut entrer à 7:30 d'un quart-temps).
        /// </summary>
        public double MinutesPlayed { get; set; }

        // ============================================================
        // FAUTES
        // ============================================================

        /// <summary>
        /// Nombre de fautes commises dans ce match.
        /// À 6 fautes (NBA) ou 5 (FIBA), le joueur est exclu.
        /// </summary>
        public int FoulsCommitted { get; set; }

        /// <summary>
        /// Indique si le joueur a été exclu pour fautes.
        /// Une fois true, le joueur ne peut plus revenir sur le terrain
        /// pour le reste du match.
        /// </summary>
        public bool IsFouledOut { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur — initialise un état neutre pour un nouveau match.
        /// Le Player passé en paramètre est référencé, pas copié.
        /// 
        /// La GameForm sera calculée séparément avant le coup d'envoi
        /// par un service dédié (MatchInitializer plus tard).
        /// </summary>
        public PlayerMatchState(Player player)
        {
            // On force la présence du joueur — pas de PlayerMatchState
            // sans joueur associé (principe d'invariant de classe).
            Player = player ?? throw new ArgumentNullException(nameof(player));

            // Valeurs par défaut d'un joueur "frais" en début de match
            GameForm = 50;          // sera recalculé avant le coup d'envoi
            Momentum = 0;           // neutre au début
            CurrentFatigue = 0;     // pas fatigué
            IsOnCourt = false;      // pas sur le terrain par défaut
            MinutesPlayed = 0;
            FoulsCommitted = 0;
            IsFouledOut = false;
        }

        // ============================================================
        // MÉTHODES UTILITAIRES
        // ============================================================

        /// <summary>
        /// Calcule le multiplicateur d'efficacité du joueur en fonction
        /// de sa fatigue actuelle.
        /// 
        /// Logique :
        /// - Fatigue 0-30 : aucune pénalité (1.00)
        /// - Fatigue 30-60 : pénalité progressive (1.00 → 0.90)
        /// - Fatigue 60-100 : pénalité forte (0.90 → 0.70)
        /// 
        /// Un joueur épuisé reste donc à 70% de ses capacités,
        /// pas 0% — il joue moins bien mais joue encore.
        /// </summary>
        public double GetFatigueMultiplier()
        {
            if (CurrentFatigue <= 30)
                return 1.00;

            if (CurrentFatigue <= 60)
            {
                // Interpolation linéaire entre 1.00 (à 30) et 0.90 (à 60)
                double progression = (CurrentFatigue - 30) / 30.0;
                return 1.00 - (progression * 0.10);
            }

            // Au-delà de 60, pénalité plus marquée
            // De 0.90 (à 60) à 0.70 (à 100)
            double heavyProgression = (CurrentFatigue - 60) / 40.0;
            return 0.90 - (heavyProgression * 0.20);
        }

        /// <summary>
        /// Détermine si le joueur peut encore jouer dans ce match.
        /// false si exclu pour fautes.
        /// </summary>
        public bool CanPlay()
        {
            return !IsFouledOut;
        }
    }
}