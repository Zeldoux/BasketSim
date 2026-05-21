using BasketSim.Core.Models;
using BasketSim.Core.Models.Match;
using BasketSim.Core.Simulation.Match.Defense;
using BasketSim.Core.Simulation.Match.Events;
using BasketSim.Core.Simulation.Resolvers;

namespace BasketSim.Core.Simulation.Match
{
    /// <summary>
    /// Le moteur principal qui simule une possession complète.
    /// 
    /// Le PossessionEngine orchestre toutes les phases d'une possession
    /// avec une défense ACTIVE qui s'adapte aux actions offensives.
    /// 
    /// Phases :
    /// 1. Initiation : le porteur reçoit la balle, matchups initiaux
    /// 2. Advancement : remontée de balle (risque de steal)
    /// 3. Organization : choix de l'action principale
    /// 4. Actions : exécution avec réactions défensives
    /// 5. Resolution : tir avec contestation calculée selon la défense
    /// 6. PostResolution : rebond si nécessaire
    /// 
    /// Composants utilisés :
    /// - DecisionMaker : choix tactiques offensifs
    /// - DefenseStrategy : décisions défensives (réaction aux écrans, etc.)
    /// - DefenseAdjuster : modificateurs défensifs sur les calculs
    /// - DefensiveMatchup : qui défend qui (dynamique)
    /// - ShotResolver, ReboundResolver, TurnoverResolver : résolution finale
    /// </summary>
    public class PossessionEngine
    {
        // ============================================================
        // DÉPENDANCES (INJECTION)
        // ============================================================

        private readonly Random _random;
        private readonly DecisionMaker _decisionMaker;
        private readonly DefenseStrategy _defenseStrategy;
        private readonly DefenseAdjuster _defenseAdjuster;
        private readonly ShotResolver _shotResolver;
        private readonly ReboundResolver _reboundResolver;
        private readonly TurnoverResolver _turnoverResolver;

        // ============================================================
        // ÉTAT INTERNE (lors d'une simulation)
        // ============================================================

        /// <summary>
        /// Liste des events accumulés pendant la simulation en cours.
        /// </summary>
        private List<PossessionEvent> _currentEvents = new();

        /// <summary>
        /// Matchups défensifs actuels pour la possession en cours.
        /// Peut être modifié pendant la possession (switches).
        /// </summary>
        private DefensiveMatchup? _currentMatchups;

        /// <summary>
        /// Dernière réaction défensive (sur un écran).
        /// Influence les calculs des actions suivantes.
        /// </summary>
        private ScreenReaction? _lastScreenReaction;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        public PossessionEngine(Random random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));

            _decisionMaker = new DecisionMaker(_random);
            _defenseStrategy = new DefenseStrategy(_random);
            _defenseAdjuster = new DefenseAdjuster();
            _shotResolver = new ShotResolver(_random);
            _reboundResolver = new ReboundResolver(_random);
            _turnoverResolver = new TurnoverResolver(_random);
        }

        // ============================================================
        // MÉTHODE PRINCIPALE
        // ============================================================

        /// <summary>
        /// Simule une possession complète et retourne le résultat.
        /// </summary>
        public PossessionResult SimulatePossession(
            TeamMatchState attacking,
            TeamMatchState defending,
            int possessionNumber,
            bool isTransition = false)
        {
            // Réinitialise l'état pour cette nouvelle simulation
            _currentEvents = new List<PossessionEvent>();
            _lastScreenReaction = null;

            // Crée les matchups défensifs initiaux
            _currentMatchups = new DefensiveMatchup(
                attacking.CurrentLineup,
                defending.CurrentLineup
            );

            var possession = new Possession(attacking, defending)
            {
                PossessionNumber = possessionNumber,
                IsTransition = isTransition
            };

            var result = new PossessionResult(attacking, defending)
            {
                PossessionNumber = possessionNumber
            };

            bool possessionContinues = true;
            while (possessionContinues)
            {
                HandleInitiationPhase(possession);

                if (!possession.IsSecondChance)
                {
                    bool stolenInAdvancement = HandleAdvancementPhase(possession);
                    if (stolenInAdvancement)
                    {
                        FinalizeResult(result, possession, PossessionOutcome.Steal);
                        break;
                    }
                }

                var action = HandleOrganizationPhase(possession);

                bool turnoverDuringActions = HandleActionsPhase(possession, action);
                if (turnoverDuringActions)
                {
                    FinalizeResult(result, possession, PossessionOutcome.Turnover);
                    break;
                }

                var shotResult = HandleResolutionPhase(possession);

                if (shotResult.IsMade)
                {
                    FinalizeResult(result, possession, PossessionOutcome.MadeShot, shotResult);
                    possessionContinues = false;
                }
                else
                {
                    var reboundResult = HandleReboundPhase(possession, shotResult);

                    if (reboundResult.IsOffensiveRebound)
                    {
                        possession.IsSecondChance = true;
                        possession.BallHandler = reboundResult.Rebounder;
                        // Reset de la réaction défensive pour la seconde chance
                        _lastScreenReaction = null;
                    }
                    else
                    {
                        FinalizeResult(result, possession,
                            PossessionOutcome.MissedShotDefensiveRebound, shotResult);
                        possessionContinues = false;
                    }
                }
            }

            return result;
        }

        // ============================================================
        // PHASE 1 : INITIATION
        // ============================================================

        private void HandleInitiationPhase(Possession possession)
        {
            if (!possession.IsSecondChance)
            {
                Player receiver = _decisionMaker.ChooseInitialBallHandler(
                    possession.AttackingTeam.CurrentLineup,
                    possession.IsTransition
                );
                possession.BallHandler = receiver;
            }

            // Détermine le défenseur principal via les matchups
            possession.PrimaryDefender = _currentMatchups!.GetDefenderOf(possession.BallHandler!);

            CourtZone receptionZone = possession.IsSecondChance
                ? CourtZone.Paint
                : CourtZone.ThreeTop;

            var receivedEvent = new BallReceivedEvent(possession.BallHandler!, receptionZone)
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, receivedEvent);
        }

        // ============================================================
        // PHASE 2 : ADVANCEMENT
        // ============================================================

        private bool HandleAdvancementPhase(Possession possession)
        {
            // Le défenseur est déjà déterminé par les matchups
            var dribbleEvent = new DribbleEvent(
                possession.BallHandler!,
                CourtZone.ThreeTop,
                CourtZone.ThreeTop,
                isBallAdvance: true
            )
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, dribbleEvent);

            return CheckForSteal(possession, isOnAdvancement: true);
        }

        // ============================================================
        // PHASE 3 : ORGANIZATION
        // ============================================================

        private OffensiveAction HandleOrganizationPhase(Possession possession)
        {
            var context = BuildDecisionContext(possession);
            return _decisionMaker.ChooseOffensiveAction(context);
        }

        // ============================================================
        // PHASE 4 : ACTIONS
        // ============================================================

        private bool HandleActionsPhase(Possession possession, OffensiveAction action)
        {
            int actionCount = action switch
            {
                OffensiveAction.DirectShot => 0,
                OffensiveAction.Isolation => _random.Next(0, 2),
                OffensiveAction.PickAndRoll => _random.Next(1, 3),
                OffensiveAction.Motion => _random.Next(2, 4),
                OffensiveAction.PostUp => _random.Next(0, 2),
                _ => 1
            };

            // Si Pick and Roll, on commence par un écran ET la réaction défensive
            if (action == OffensiveAction.PickAndRoll)
            {
                CreateScreenWithDefensiveReaction(possession);
            }

            for (int i = 0; i < actionCount; i++)
            {
                if (possession.ShotClockRemaining < 5)
                    break;

                bool doPass = _random.NextDouble() < 0.65;

                if (doPass)
                {
                    bool stealOnPass = CreatePassEvent(possession);
                    if (stealOnPass)
                        return true;
                }
                else
                {
                    CreateDribbleEvent(possession);
                }

                if (CheckForUnforcedTurnover(possession))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Crée un screen event ET applique la réaction défensive.
        /// La défense réagit selon la couverture P&R choisie par le coach.
        /// </summary>
        private void CreateScreenWithDefensiveReaction(Possession possession)
        {
            var lineup = possession.AttackingTeam.CurrentLineup;
            Player screener = lineup.Center ?? lineup.PowerForward ?? lineup.SmallForward!;

            var screenEvent = new ScreenEvent(screener, possession.BallHandler!, ScreenType.OnBall)
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, screenEvent);

            // === RÉACTION DÉFENSIVE ===
            // La défense réagit à l'écran selon sa couverture tactique
            var defensiveTactics = possession.DefendingTeam.CurrentTactics.Defensive;
            _lastScreenReaction = _defenseStrategy.ReactToScreen(
                defensiveTactics,
                _currentMatchups!,
                possession.BallHandler!,
                screener
            );

            // Met à jour le défenseur principal (peut avoir changé via un switch)
            possession.PrimaryDefender = _currentMatchups!.GetDefenderOf(possession.BallHandler!);

            // Consomme du temps supplémentaire si la défense met du temps à réagir
            if (_lastScreenReaction.TimeUsedBonus > 0)
            {
                possession.SecondsElapsed += _lastScreenReaction.TimeUsedBonus;
            }
        }

        private bool CreatePassEvent(Possession possession)
        {
            var context = BuildDecisionContext(possession);
            Player receiver = _decisionMaker.ChoosePassReceiver(context);

            CourtZone targetZone = DetermineTargetZone(receiver);

            var passEvent = new PassEvent(
                possession.BallHandler!,
                receiver,
                targetZone,
                PassType.Chest
            )
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, passEvent);

            bool stolen = CheckForSteal(possession, isOnAdvancement: false);
            if (stolen)
                return true;

            // La passe réussit : changement de porteur et de défenseur principal
            possession.BallHandler = receiver;
            possession.PrimaryDefender = _currentMatchups!.GetDefenderOf(receiver);

            return false;
        }

        private void CreateDribbleEvent(Possession possession)
        {
            var dribbleEvent = new DribbleEvent(
                possession.BallHandler!,
                CourtZone.ThreeTop,
                CourtZone.ThreeLeftWing,
                isBallAdvance: false
            )
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, dribbleEvent);
        }

        // ============================================================
        // PHASE 5 : RESOLUTION
        // ============================================================

        private ShotEvent HandleResolutionPhase(Possession possession)
        {
            var shooter = possession.BallHandler!;
            var defender = possession.PrimaryDefender;

            CourtZone shotZone = ChooseShotZone(shooter);
            ShotContext shotContext = ChooseShotContext(possession, shotZone);

            // === CONTESTATION CALCULÉE AVEC LA DÉFENSE ACTIVE ===
            var defensiveTactics = possession.DefendingTeam.CurrentTactics.Defensive;
            int contestLevel = _defenseAdjuster.CalculateShotContest(
                shooter,
                defender,
                shotZone,
                defensiveTactics,
                _lastScreenReaction
            );

            var shotEvent = new ShotEvent(shooter, shotZone, shotContext, defender, contestLevel)
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            var shotAttemptContext = new ShotAttemptContext
            {
                Shooter = shooter,
                Defender = defender,
                ShooterState = possession.AttackingTeam.PlayerStates[shooter],
                Zone = shotZone,
                Context = shotContext,
                ContestLevel = contestLevel
            };

            shotEvent.IsMade = _shotResolver.Resolve(shotAttemptContext);
            shotEvent.IsSuccessful = shotEvent.IsMade;

            shotEvent.Description = shotEvent.GenerateDescription();

            AddEvent(possession, shotEvent);

            UpdateShooterStats(possession, shotEvent);

            return shotEvent;
        }

        // ============================================================
        // PHASE 6 : POSTRESOLUTION (REBOND)
        // ============================================================

        private ReboundResult HandleReboundPhase(Possession possession, ShotEvent shotEvent)
        {
            var reboundContext = new ReboundAttemptContext
            {
                OffensiveBoxers = possession.AttackingTeam.CurrentLineup.GetAllPlayers(),
                DefensiveBoxers = possession.DefendingTeam.CurrentLineup.GetAllPlayers(),
                ShotZone = shotEvent.ShotZone,
                OriginalShooter = shotEvent.PrimaryPlayer
            };

            var result = _reboundResolver.Resolve(reboundContext);

            var reboundEvent = new ReboundEvent(
                result.Rebounder,
                result.IsOffensiveRebound,
                shotEvent.ShotZone
            )
            {
                PossessionTimeStamp = possession.SecondsElapsed
            };

            AddEvent(possession, reboundEvent);

            UpdateReboundStats(possession, reboundEvent);

            return result;
        }

        // ============================================================
        // HELPERS — STEALS ET TURNOVERS
        // ============================================================

        private bool CheckForSteal(Possession possession, bool isOnAdvancement)
        {
            if (possession.PrimaryDefender == null)
                return false;

            double stealChance = possession.PrimaryDefender.Steal / 100.0 * 0.05;

            // Application du modificateur défensif global
            var defensiveTactics = possession.DefendingTeam.CurrentTactics.Defensive;
            double stealModifier = _defenseAdjuster.GetStealChanceModifier(
                defensiveTactics,
                isOnPass: !isOnAdvancement,
                isUnderTrap: _lastScreenReaction?.Type == ScreenReactionType.Trap
            );
            stealChance *= stealModifier;

            // Bonus si on est en full court press et c'est une remontée
            if (isOnAdvancement && defensiveTactics.Pressure.Zone == Models.Tactics.PressureZone.FullCourt)
            {
                stealChance *= 1.5;
            }

            if (_random.NextDouble() < stealChance)
            {
                var stealEvent = new StealEvent(
                    possession.PrimaryDefender,
                    possession.BallHandler!,
                    isOnAdvancement ? StealContext.OnDribble : StealContext.OnPass
                )
                {
                    PossessionTimeStamp = possession.SecondsElapsed
                };

                AddEvent(possession, stealEvent);

                possession.DefendingTeam.PlayerStats[possession.PrimaryDefender].Steals++;
                possession.AttackingTeam.PlayerStats[possession.BallHandler!].Turnovers++;

                return true;
            }

            return false;
        }

        private bool CheckForUnforcedTurnover(Possession possession)
        {
            var turnoverContext = new TurnoverAttemptContext
            {
                BallHandler = possession.BallHandler!,
                BallHandlerState = possession.AttackingTeam.PlayerStates[possession.BallHandler!],
                PrimaryDefender = possession.PrimaryDefender,
                IsUnderShotClockPressure = possession.ShotClockRemaining < 8,
                IsUnderDefensivePressure = _lastScreenReaction?.Type == ScreenReactionType.Trap
            };

            var result = _turnoverResolver.Resolve(turnoverContext);

            if (result.IsTurnover && !result.IsSteal)
            {
                var turnoverEvent = new TurnoverEvent(
                    possession.BallHandler!,
                    TurnoverCause.BadPass
                )
                {
                    PossessionTimeStamp = possession.SecondsElapsed
                };

                AddEvent(possession, turnoverEvent);

                possession.AttackingTeam.PlayerStats[possession.BallHandler!].Turnovers++;

                return true;
            }

            return false;
        }

        // ============================================================
        // HELPERS — CHOIX DU TIR
        // ============================================================

        private CourtZone ChooseShotZone(Player shooter)
        {
            double threeAffinity = shooter.ThreePointShooting / 100.0;
            double insideAffinity = shooter.InsideShooting / 100.0;

            double roll = _random.NextDouble();

            if (roll < threeAffinity * 0.5)
            {
                var threeZones = new[]
                {
                    CourtZone.ThreeCornerLeft, CourtZone.ThreeLeftWing,
                    CourtZone.ThreeTop, CourtZone.ThreeRightWing,
                    CourtZone.ThreeCornerRight
                };
                return threeZones[_random.Next(threeZones.Length)];
            }
            else if (roll < threeAffinity * 0.5 + insideAffinity * 0.4)
            {
                return _random.NextDouble() < 0.6
                    ? CourtZone.RestrictedArea
                    : CourtZone.Paint;
            }
            else
            {
                var midZones = new[]
                {
                    CourtZone.MidRangeLeft, CourtZone.MidRangeLeftCenter,
                    CourtZone.MidRangeCenter, CourtZone.MidRangeRightCenter,
                    CourtZone.MidRangeRight
                };
                return midZones[_random.Next(midZones.Length)];
            }
        }

        private ShotContext ChooseShotContext(Possession possession, CourtZone zone)
        {
            if (zone == CourtZone.RestrictedArea)
            {
                if (possession.BallHandler!.Vertical >= 80)
                {
                    return _random.NextDouble() < 0.4 ? ShotContext.DrivingDunk : ShotContext.Layup;
                }
                return ShotContext.Layup;
            }

            var contexts = new[]
            {
                ShotContext.CatchAndShoot,
                ShotContext.PullUp,
                ShotContext.SpotUp
            };

            return contexts[_random.Next(contexts.Length)];
        }

        // ============================================================
        // HELPERS — CALCULS DIVERS
        // ============================================================

        private CourtZone DetermineTargetZone(Player receiver)
        {
            return receiver.Position switch
            {
                Position.Center => CourtZone.Paint,
                Position.PowerForward => CourtZone.MidRangeCenter,
                Position.SmallForward => CourtZone.ThreeLeftWing,
                Position.ShootingGuard => CourtZone.ThreeRightWing,
                Position.PointGuard => CourtZone.ThreeTop,
                _ => CourtZone.ThreeTop
            };
        }

        // ============================================================
        // HELPERS — MISE À JOUR DES STATS
        // ============================================================

        private void UpdateShooterStats(Possession possession, ShotEvent shotEvent)
        {
            var shooter = shotEvent.PrimaryPlayer;
            var stats = possession.AttackingTeam.PlayerStats[shooter];

            stats.RecordShotAttempt(shotEvent.ShotZone, shotEvent.IsThreePointShot);

            if (shotEvent.IsMade)
            {
                stats.RecordShotMade(shotEvent.ShotZone, shotEvent.IsThreePointShot);

                if (shotEvent.IsThreePointShot)
                {
                    possession.AttackingTeam.Stats.ThreePointAttempted++;
                    possession.AttackingTeam.Stats.ThreePointMade++;
                }
                else
                {
                    possession.AttackingTeam.Stats.TwoPointAttempted++;
                    possession.AttackingTeam.Stats.TwoPointMade++;
                }
            }
            else
            {
                if (shotEvent.IsThreePointShot)
                    possession.AttackingTeam.Stats.ThreePointAttempted++;
                else
                    possession.AttackingTeam.Stats.TwoPointAttempted++;
            }
        }

        private void UpdateReboundStats(Possession possession, ReboundEvent reboundEvent)
        {
            var rebounder = reboundEvent.PrimaryPlayer;

            var team = possession.AttackingTeam.PlayerStats.ContainsKey(rebounder)
                ? possession.AttackingTeam
                : possession.DefendingTeam;

            var stats = team.PlayerStats[rebounder];

            if (reboundEvent.IsOffensiveRebound)
            {
                stats.OffensiveRebounds++;
                team.Stats.OffensiveRebounds++;
            }
            else
            {
                stats.DefensiveRebounds++;
                team.Stats.DefensiveRebounds++;
            }
        }

        // ============================================================
        // HELPERS — UTILITAIRES
        // ============================================================

        private void AddEvent(Possession possession, PossessionEvent ev)
        {
            _currentEvents.Add(ev);
            possession.SecondsElapsed += ev.SecondsUsed;
        }

        private DecisionContext BuildDecisionContext(Possession possession)
        {
            return new DecisionContext
            {
                Possession = possession,
                BallHandler = possession.BallHandler!,
                PrimaryDefender = possession.PrimaryDefender,
                CurrentBallZone = CourtZone.ThreeTop,
                ShotClockRemaining = possession.ShotClockRemaining,
                ActionsCount = _currentEvents.Count,
                IsSecondChance = possession.IsSecondChance
            };
        }

        private void FinalizeResult(
            PossessionResult result,
            Possession possession,
            PossessionOutcome outcome,
            ShotEvent? shotEvent = null)
        {
            result.Outcome = outcome;
            result.SecondsUsed = possession.SecondsElapsed;

            if (shotEvent != null)
            {
                result.PrimaryAttacker = shotEvent.PrimaryPlayer;
                result.ShotZone = shotEvent.ShotZone;
                result.ShotContext = shotEvent.ShotContextType;

                if (shotEvent.IsMade)
                {
                    result.PointsScored = shotEvent.PointValue;
                    possession.AttackingTeam.Stats.PointsByQuarter[0] += shotEvent.PointValue;
                }
            }
            else
            {
                result.PrimaryAttacker = possession.BallHandler;
            }
        }

        // ============================================================
        // ACCÈS À LA LISTE D'EVENTS
        // ============================================================

        public List<PossessionEvent> GetLastEvents()
        {
            return _currentEvents;
        }
    }
}