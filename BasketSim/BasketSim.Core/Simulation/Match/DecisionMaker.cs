using BasketSim.Core.Models;
using BasketSim.Core.Models.Tactics;

namespace BasketSim.Core.Simulation.Match
{
    /// <summary>
    /// Le "cerveau" du moteur — prend les décisions à chaque phase
    /// d'une possession.
    /// 
    /// Le DecisionMaker répond à des questions du type :
    /// - Qui doit recevoir la balle au début ?
    /// - Le porteur doit-il passer, dribbler, ou tirer ?
    /// - Si passer, à qui ?
    /// - Si tirer, depuis quelle zone et avec quel contexte ?
    /// 
    /// Les décisions sont influencées par :
    /// - Les attributs des joueurs (un meneur passe plus qu'un pivot)
    /// - Les tactiques de l'équipe (style motion vs iso)
    /// - Le contexte (shot clock, score, momentum)
    /// - L'aléatoire (avec seed pour reproductibilité)
    /// 
    /// IMPORTANT : Cette classe contient la "personnalité" du jeu.
    /// C'est ici que les profils de coachs et de joueurs se manifestent.
    /// </summary>
    public class DecisionMaker
    {
        // ============================================================
        // DÉPENDANCES
        // ============================================================

        /// <summary>
        /// Source d'aléatoire — injectée pour la reproductibilité.
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// Constructeur avec injection de la source d'aléatoire.
        /// </summary>
        public DecisionMaker(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        // ============================================================
        // DÉCISION 1 — QUI REÇOIT LA BALLE AU DÉBUT ?
        // ============================================================

        /// <summary>
        /// Choisit le joueur qui reçoit la balle en début de possession.
        /// 
        /// Logique :
        /// - 70% du temps : c'est le meneur (poste 1) — comportement standard
        /// - 30% du temps : un autre joueur selon le contexte
        /// 
        /// Pour les transitions, c'est souvent le rebondeur défensif
        /// ou le voleur qui pousse la balle directement.
        /// </summary>
        public Player ChooseInitialBallHandler(Lineup lineup, bool isTransition)
        {
            // 70% du temps, le PG reçoit la balle
            double roll = _random.NextDouble();
            if (roll < 0.70 && lineup.PointGuard != null)
            {
                return lineup.PointGuard;
            }

            // Sinon, choix parmi les guards (PG ou SG)
            var guards = new List<Player>();
            if (lineup.PointGuard != null) guards.Add(lineup.PointGuard);
            if (lineup.ShootingGuard != null) guards.Add(lineup.ShootingGuard);

            if (guards.Count > 0)
            {
                return guards[_random.Next(guards.Count)];
            }

            // Cas extrême : aucun guard, on prend le premier joueur dispo
            return lineup.GetAllPlayers().First();
        }

        // ============================================================
        // DÉCISION 2 — QUELLE ACTION OFFENSIVE PRENDRE ?
        // ============================================================

        /// <summary>
        /// Décide de l'action principale à effectuer en phase Organization.
        /// 
        /// Le choix dépend du style offensif de l'équipe et des attributs
        /// du porteur.
        /// </summary>
        public OffensiveAction ChooseOffensiveAction(DecisionContext context)
        {
            var tactics = context.Possession.AttackingTeam.CurrentTactics.Offensive;
            var ballHandler = context.BallHandler;

            // Pondération de base selon le style de l'équipe
            var weights = GetActionWeightsForStyle(tactics.Style);

            // Ajustement selon les attributs du porteur
            AdjustWeightsForPlayer(weights, ballHandler);

            // Ajustement selon le shot clock
            AdjustWeightsForShotClock(weights, context.ShotClockRemaining);

            // Sélection par tirage pondéré
            return SelectByWeights(weights);
        }

        /// <summary>
        /// Retourne les poids d'action de base selon le style offensif.
        /// </summary>
        private Dictionary<OffensiveAction, double> GetActionWeightsForStyle(OffensiveStyle style)
        {
            return style switch
            {
                OffensiveStyle.PickAndRollCentric => new Dictionary<OffensiveAction, double>
                {
                    { OffensiveAction.PickAndRoll, 0.50 },
                    { OffensiveAction.Isolation, 0.15 },
                    { OffensiveAction.Motion, 0.15 },
                    { OffensiveAction.PostUp, 0.10 },
                    { OffensiveAction.DirectShot, 0.10 }
                },

                OffensiveStyle.IsolationHeavy => new Dictionary<OffensiveAction, double>
                {
                    { OffensiveAction.Isolation, 0.50 },
                    { OffensiveAction.PickAndRoll, 0.20 },
                    { OffensiveAction.Motion, 0.10 },
                    { OffensiveAction.PostUp, 0.10 },
                    { OffensiveAction.DirectShot, 0.10 }
                },

                OffensiveStyle.Motion => new Dictionary<OffensiveAction, double>
                {
                    { OffensiveAction.Motion, 0.45 },
                    { OffensiveAction.PickAndRoll, 0.25 },
                    { OffensiveAction.Isolation, 0.10 },
                    { OffensiveAction.PostUp, 0.10 },
                    { OffensiveAction.DirectShot, 0.10 }
                },

                OffensiveStyle.PostUpHeavy => new Dictionary<OffensiveAction, double>
                {
                    { OffensiveAction.PostUp, 0.45 },
                    { OffensiveAction.Motion, 0.20 },
                    { OffensiveAction.PickAndRoll, 0.15 },
                    { OffensiveAction.Isolation, 0.10 },
                    { OffensiveAction.DirectShot, 0.10 }
                },

                OffensiveStyle.InsideOut => new Dictionary<OffensiveAction, double>
                {
                    { OffensiveAction.PostUp, 0.35 },
                    { OffensiveAction.Motion, 0.25 },
                    { OffensiveAction.PickAndRoll, 0.20 },
                    { OffensiveAction.Isolation, 0.10 },
                    { OffensiveAction.DirectShot, 0.10 }
                },

                _ => new Dictionary<OffensiveAction, double> // Balanced
                {
                    { OffensiveAction.PickAndRoll, 0.25 },
                    { OffensiveAction.Motion, 0.25 },
                    { OffensiveAction.Isolation, 0.20 },
                    { OffensiveAction.PostUp, 0.15 },
                    { OffensiveAction.DirectShot, 0.15 }
                }
            };
        }

        /// <summary>
        /// Ajuste les poids d'action selon les attributs du porteur.
        /// </summary>
        private void AdjustWeightsForPlayer(Dictionary<OffensiveAction, double> weights, Player player)
        {
            // Bons isolateurs : ball handlers créatifs
            if (player.BallHandling >= 80 && player.Creativity >= 75)
            {
                weights[OffensiveAction.Isolation] *= 1.5;
            }

            // Bons posters : intérieurs avec PostMoves
            if (player.PostMoves >= 75 && (player.Position == Position.Center
                || player.Position == Position.PowerForward))
            {
                weights[OffensiveAction.PostUp] *= 1.4;
            }

            // Shooteurs élites favorisent les tirs directs (si ouverts)
            if (player.ThreePointShooting >= 90)
            {
                weights[OffensiveAction.DirectShot] *= 1.3;
            }
        }

        /// <summary>
        /// Ajuste les poids selon le shot clock.
        /// </summary>
        private void AdjustWeightsForShotClock(Dictionary<OffensiveAction, double> weights, int shotClock)
        {
            if (shotClock < 5)
            {
                // Très peu de temps : tir forcé probable
                weights[OffensiveAction.DirectShot] *= 3.0;
                weights[OffensiveAction.Isolation] *= 2.0;
                weights[OffensiveAction.PickAndRoll] *= 0.3;
                weights[OffensiveAction.Motion] *= 0.2;
                weights[OffensiveAction.PostUp] *= 0.3;
            }
            else if (shotClock < 10)
            {
                // Temps modéré : on accélère un peu
                weights[OffensiveAction.DirectShot] *= 1.5;
                weights[OffensiveAction.Isolation] *= 1.3;
                weights[OffensiveAction.Motion] *= 0.7;
            }
        }

        // ============================================================
        // DÉCISION 3 — À QUI PASSER LA BALLE ?
        // ============================================================

        /// <summary>
        /// Choisit le receveur d'une passe parmi les coéquipiers.
        /// </summary>
        public Player ChoosePassReceiver(DecisionContext context)
        {
            var lineup = context.Possession.AttackingTeam.CurrentLineup;
            var allPlayers = lineup.GetAllPlayers();

            // Retire le porteur de la liste des receveurs possibles
            var candidates = allPlayers.Where(p => p != context.BallHandler).ToList();

            if (candidates.Count == 0)
            {
                // Cas dégénéré : pas de receveur possible
                return context.BallHandler;
            }

            // Calcule un poids pour chaque candidat
            var weights = new Dictionary<Player, double>();
            var tactics = context.Possession.AttackingTeam.CurrentTactics.Offensive;

            foreach (var candidate in candidates)
            {
                double weight = 1.0;

                // Boost si c'est la star principale
                if (candidate == tactics.Distribution.PrimaryStar)
                {
                    weight *= 2.5;
                }
                // Boost si c'est la star secondaire
                else if (candidate == tactics.Distribution.SecondaryStar)
                {
                    weight *= 1.8;
                }

                // Boost selon l'overall du joueur (les meilleurs reçoivent plus)
                double overallFactor = candidate.ThreePointShooting / 100.0 + 0.5;
                weight *= overallFactor;

                weights[candidate] = weight;
            }

            return SelectFromDictionary(weights);
        }

        // ============================================================
        // DÉCISION 4 — TIRER OU CONTINUER L'ACTION ?
        // ============================================================

        /// <summary>
        /// Décide si le porteur de balle doit tirer maintenant
        /// ou continuer l'action (passer, driver, dribbler).
        /// </summary>
        public bool ShouldShoot(DecisionContext context, CourtZone fromZone)
        {
            // Probabilité de base de tirer
            double shootChance = 0.40;

            // Modifié par le shot clock
            if (context.ShotClockRemaining < 5)
                shootChance = 0.85; // tir presque forcé
            else if (context.ShotClockRemaining < 10)
                shootChance = 0.55;

            // Modifié par le nombre d'actions déjà effectuées
            shootChance += context.ActionsCount * 0.10;

            // Modifié par la zone (proche = plus de tirs)
            if (fromZone == CourtZone.RestrictedArea)
                shootChance += 0.30;
            else if (fromZone == CourtZone.Paint)
                shootChance += 0.20;

            // Modifié par les attributs du tireur dans cette zone
            int shootingSkill = GetSkillForZone(context.BallHandler, fromZone);
            double skillBonus = (shootingSkill - 60) / 200.0;
            shootChance += skillBonus;

            // Clamp pour rester réaliste
            shootChance = Math.Clamp(shootChance, 0.05, 0.95);

            return _random.NextDouble() < shootChance;
        }

        /// <summary>
        /// Récupère la stat de tir du joueur pour une zone donnée.
        /// </summary>
        private int GetSkillForZone(Player player, CourtZone zone)
        {
            return zone switch
            {
                CourtZone.RestrictedArea or CourtZone.Paint => player.InsideShooting,
                CourtZone.ThreeCornerLeft or CourtZone.ThreeLeftWing or
                CourtZone.ThreeTop or CourtZone.ThreeRightWing or
                CourtZone.ThreeCornerRight => player.ThreePointShooting,
                _ => player.MidRangeShooting
            };
        }

        // ============================================================
        // HELPERS — SÉLECTION PONDÉRÉE
        // ============================================================

        /// <summary>
        /// Sélectionne un élément par tirage pondéré dans un dictionnaire.
        /// </summary>
        private T SelectFromDictionary<T>(Dictionary<T, double> weights) where T : notnull
        {
            double total = weights.Values.Sum();
            double roll = _random.NextDouble() * total;
            double cumulative = 0;

            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (cumulative >= roll)
                    return kvp.Key;
            }

            // Fallback (ne devrait jamais arriver)
            return weights.Keys.First();
        }

        /// <summary>
        /// Version typée pour les OffensiveAction.
        /// </summary>
        private OffensiveAction SelectByWeights(Dictionary<OffensiveAction, double> weights)
        {
            return SelectFromDictionary(weights);
        }
    }
}