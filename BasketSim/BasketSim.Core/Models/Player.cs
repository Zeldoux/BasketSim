using System.Collections.Generic;

namespace BasketSim.Core.Models
{
    /// <summary>
    /// Représente un joueur de basket avec tous ses attributs permanents.
    /// 
    /// IMPORTANT : Cette classe ne contient QUE les caractéristiques durables du joueur.
    /// - L'état en cours de match (fatigue, momentum) est dans PlayerMatchState
    /// - Les statistiques d'un match sont dans PlayerGameStats
    /// 
    /// Cette séparation suit le principe "une responsabilité par classe" :
    /// les attributs d'un joueur ne doivent pas dépendre d'un match en particulier.
    /// 
    /// Toutes les notes de compétence sont sur une échelle de 0 à 100.
    /// (Pourra être migrée vers 0-200 plus tard, façon Football Manager,
    /// quand la progression de carrière sera implémentée.)
    /// </summary>
    public class Player
    {
        // ============================================================
        // IDENTITÉ
        // ============================================================

        /// <summary>Nom complet du joueur (ex: "Stephen Curry")</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Âge du joueur en années</summary>
        public int Age { get; set; }

        /// <summary>Taille en centimètres</summary>
        public int HeightCm { get; set; }

        /// <summary>Poids en kilogrammes</summary>
        public int WeightKg { get; set; }

        /// <summary>
        /// Envergure (wingspan) en centimètres.
        /// Souvent plus importante que la taille au basket moderne.
        /// Impact sur la défense, le rebond et le tir contesté.
        /// </summary>
        public int WingspanCm { get; set; }

        /// <summary>
        /// Poste naturel du joueur (celui où il est le plus à l'aise).
        /// Pour évaluer un joueur à un autre poste, utiliser
        /// PositionFamiliarityCalculator.
        /// </summary>
        public Position Position { get; set; }

        // ============================================================
        // ATTRIBUTS TECHNIQUES — TIR
        // ============================================================

        /// <summary>Tirs près du panier (toutes finitions intérieures).</summary>
        public int InsideShooting { get; set; }

        /// <summary>Tirs mi-distance (tous les tirs entre raquette et ligne 3pts).</summary>
        public int MidRangeShooting { get; set; }

        /// <summary>Tirs à 3 points (toutes zones derrière la ligne).</summary>
        public int ThreePointShooting { get; set; }

        /// <summary>Lancers francs — compétence indépendante du jeu en mouvement.</summary>
        public int FreeThrow { get; set; }

        /// <summary>
        /// Toucher de balle — finesse des finitions difficiles.
        /// Crucial pour : floaters, layups contestés, finitions douces.
        /// Exemples de joueurs avec gros Touch : Larry Bird, Sabonis, Jokic.
        /// </summary>
        public int Touch { get; set; }

        /// <summary>
        /// QI de tir — capacité à prendre le bon tir au bon moment.
        /// Influence la sélection de tir et évite les tirs forcés.
        /// </summary>
        public int ShotIQ { get; set; }

        /// <summary>
        /// Tendances de tir par zone du terrain.
        /// Permet de modéliser les "hot zones" et "cold zones" de chaque joueur.
        /// 
        /// Les zones non spécifiées sont considérées comme Neutral.
        /// Accès via la méthode GetZoneTendency() qui gère le fallback.
        /// </summary>
        public Dictionary<CourtZone, ZoneTendency> ZoneTendencies { get; set; }

        // ============================================================
        // ATTRIBUTS TECHNIQUES — CRÉATION OFFENSIVE
        // ============================================================

        /// <summary>Maîtrise du dribble, protection de balle, résistance aux pertes.</summary>
        public int BallHandling { get; set; }

        /// <summary>Qualité d'exécution des passes (précision, force, timing).</summary>
        public int Passing { get; set; }

        /// <summary>
        /// Vision du jeu — capacité à VOIR les bonnes passes.
        /// Séparé de Passing car certains joueurs voient les passes
        /// mais ne les exécutent pas bien (ou inversement).
        /// Exemples avec grosse Vision : LeBron, Magic Johnson, Jokic.
        /// </summary>
        public int Vision { get; set; }

        /// <summary>
        /// Capacité à conclure au cercle SOUS CONTACT.
        /// Différent du InsideShooting : c'est la capacité à finir
        /// MALGRÉ la pression défensive et les contacts.
        /// </summary>
        public int Finishing { get; set; }

        /// <summary>
        /// Mouvement sans ballon — courses, écrans, coupures backdoor.
        /// Caractéristique des purs shooteurs qui jouent sans ballon.
        /// Exemples : Stephen Curry, Klay Thompson, Reggie Miller.
        /// </summary>
        public int OffBallMovement { get; set; }

        /// <summary>
        /// Moves au poste bas — drop step, jump hook, fadeaway poste.
        /// Spécialité des intérieurs traditionnels.
        /// Exemples : Hakeem Olajuwon, Tim Duncan, Joel Embiid.
        /// </summary>
        public int PostMoves { get; set; }

        /// <summary>
        /// Créativité offensive — capacité à inventer des moves,
        /// à improviser dans des situations bloquées.
        /// Exemples : Kyrie Irving, James Harden, Luka Doncic.
        /// </summary>
        public int Creativity { get; set; }

        // ============================================================
        // ATTRIBUTS TECHNIQUES — DÉFENSE
        // ============================================================

        /// <summary>Défense sur l'extérieur — contre shooteurs et dribbleurs.</summary>
        public int PerimeterDefense { get; set; }

        /// <summary>Défense intérieure — dans la raquette, contre drives et postes bas.</summary>
        public int InteriorDefense { get; set; }

        /// <summary>
        /// Défense directe sur le porteur de balle.
        /// Capacité à rester devant son joueur, contester le tir.
        /// </summary>
        public int OnBallDefense { get; set; }

        /// <summary>
        /// Défense en aide, rotations, anticipation des passes.
        /// Crucial pour les schémas défensifs modernes (switch, help).
        /// </summary>
        public int OffBallDefense { get; set; }

        /// <summary>Capacité à intercepter passes et voler la balle au dribble.</summary>
        public int Steal { get; set; }

        /// <summary>Capacité à contrer les tirs.</summary>
        public int Block { get; set; }

        // ============================================================
        // ATTRIBUTS TECHNIQUES — REBOND
        // ============================================================

        /// <summary>Rebond offensif — récupérer ses propres tirs ratés.</summary>
        public int OffensiveRebound { get; set; }

        /// <summary>Rebond défensif — sécuriser les tirs ratés de l'adversaire.</summary>
        public int DefensiveRebound { get; set; }

        /// <summary>
        /// Boxout — capacité à mettre l'adversaire derrière soi au rebond.
        /// Fondamental, souvent plus important que la détente brute.
        /// </summary>
        public int Boxout { get; set; }

        // ============================================================
        // ATTRIBUTS PHYSIQUES
        // ============================================================

        /// <summary>Vitesse de pointe en course en ligne droite.</summary>
        public int Speed { get; set; }

        /// <summary>
        /// Accélération — vitesse de démarrage et premier pas.
        /// Différente du Speed : un joueur peut être rapide en course
        /// mais lent à démarrer (ou inversement).
        /// </summary>
        public int Acceleration { get; set; }

        /// <summary>Force physique — impact dans contacts, screens, postes bas.</summary>
        public int Strength { get; set; }

        /// <summary>Endurance — vitesse à laquelle le joueur fatigue ou récupère.</summary>
        public int Stamina { get; set; }

        /// <summary>Agilité — changements de direction, esquives, dribbles complexes.</summary>
        public int Agility { get; set; }

        /// <summary>Équilibre — résistance aux contacts, finitions déséquilibrées.</summary>
        public int Balance { get; set; }

        /// <summary>Détente verticale — impact sur dunks, contres, rebonds.</summary>
        public int Vertical { get; set; }

        /// <summary>
        /// Qualité des mains — saisir les passes difficiles, sécuriser les rebonds.
        /// Un joueur avec mauvaises mains laisse échapper la balle souvent.
        /// </summary>
        public int Hands { get; set; }

        // ============================================================
        // ATTRIBUTS MENTAUX
        // ============================================================

        /// <summary>QI basket général — bonnes décisions, lecture globale du jeu.</summary>
        public int BasketballIQ { get; set; }

        /// <summary>
        /// Lecture du jeu offensif — placement, timing des passes,
        /// reconnaissance des défenses adverses.
        /// </summary>
        public int OffensiveAwareness { get; set; }

        /// <summary>
        /// Lecture du jeu défensif — anticipation, rotations,
        /// reconnaissance des systèmes offensifs adverses.
        /// </summary>
        public int DefensiveAwareness { get; set; }

        /// <summary>
        /// Calme sous pression — performance dans les situations stressantes.
        /// Influence les lancers francs en fin de match et les tirs clutch.
        /// </summary>
        public int Composure { get; set; }

        /// <summary>
        /// Détermination — refus de lâcher, intensité dans l'effort.
        /// Influence les rebonds disputés, les sprints retour défense.
        /// </summary>
        public int Determination { get; set; }

        /// <summary>
        /// Intensité de l'effort général au cours du match.
        /// Influence la fatigue (gros work rate = fatigue plus rapide).
        /// </summary>
        public int WorkRate { get; set; }

        /// <summary>
        /// Leadership — influence positive sur les coéquipiers.
        /// Bonus de moral et de momentum à toute l'équipe.
        /// </summary>
        public int Leadership { get; set; }

        /// <summary>Discipline — gestion des fautes, sang-froid sous pression.</summary>
        public int Discipline { get; set; }

        /// <summary>Performance dans les moments décisifs (fin de match serré).</summary>
        public int Clutch { get; set; }

        /// <summary>
        /// Régularité match en match.
        /// Un joueur avec faible Consistency aura plus de variation
        /// dans sa GameForm (parfois 95, parfois 60).
        /// </summary>
        public int Consistency { get; set; }

        // ============================================================
        // ÉTAT LONG TERME (évolue sur la saison)
        // ============================================================

        /// <summary>
        /// Morale du joueur — évolue selon les résultats des matchs.
        /// Bonne perf et victoire → augmente. Mauvaise perf, défaites → diminue.
        /// Influence la GameForm en début de match.
        /// </summary>
        public int Morale { get; set; }

        /// <summary>
        /// Confiance générale du joueur — évolue plus lentement que la morale.
        /// Reflète l'état d'esprit profond du joueur sur la saison.
        /// </summary>
        public int Confidence { get; set; }

        /// <summary>
        /// Potentiel maximal du joueur — note plafond qu'il peut atteindre
        /// avec l'expérience et l'entraînement.
        /// (Utilisé pour la progression dans les versions futures.)
        /// </summary>
        public int Potential { get; set; }

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        /// <summary>
        /// Constructeur par défaut.
        /// Initialise le dictionnaire ZoneTendencies vide pour éviter
        /// les NullReferenceException lors de l'accès aux tendances.
        /// </summary>
        public Player()
        {
            ZoneTendencies = new Dictionary<CourtZone, ZoneTendency>();
        }

        // ============================================================
        // MÉTHODES UTILITAIRES
        // ============================================================

        /// <summary>
        /// Retourne la tendance du joueur pour une zone donnée.
        /// Si la zone n'est pas définie, retourne Neutral par défaut.
        /// 
        /// Cette méthode encapsule l'accès au dictionnaire pour éviter
        /// les vérifications "ContainsKey" partout dans le code.
        /// Principe DRY : la logique de fallback est centralisée ici.
        /// </summary>
        public ZoneTendency GetZoneTendency(CourtZone zone)
        {
            // TryGetValue est plus performant que ContainsKey + indexeur
            // car il ne fait qu'une seule recherche dans le dictionnaire.
            return ZoneTendencies.TryGetValue(zone, out var tendency)
                ? tendency
                : ZoneTendency.Neutral;
        }
    }
}