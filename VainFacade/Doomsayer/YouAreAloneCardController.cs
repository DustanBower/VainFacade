using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra;

namespace VainFacadePlaytest.Doomsayer
{
	public class YouAreAloneCardController:ProclamationCardController
	{
		public YouAreAloneCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => GetCardThisCardIsNextTo().Owner.Name + " is alone, and cannot affect or be affected by other heroes except for dealing damage.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText;
            base.SpecialStringMaker.ShowHeroCharacterCardWithLowestHP().Condition = () => !this.Card.IsInPlayAndHasGameText;
		}

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card next to the hero with the lowest hp.
            List<Card> lowest = new List<Card>();
            IEnumerator coroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => IsHeroCharacterCard(c), lowest, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (lowest.FirstOrDefault() != null)
            {
                storedResults.Add(new MoveCardDestination(lowest.FirstOrDefault().NextToLocation));
            }
        }

        private TurnTaker GetIsolatedHero()
        {
            if (base.Card.Location.OwnerCard != null)
            {
                return base.Card.Location.OwnerCard.Owner;
            }
            return null;
        }

        public override bool AskIfActionCanBePerformed(GameAction g)
        {
            //That hero and that hero’s cards cannot affect or be affected by any other hero card or effect from another hero deck except for dealing damage.
            if (GetIsolatedHero() != null && !(g is DealDamageAction) && !(g is DestroyCardAction && ((DestroyCardAction)g).DealDamageAction != null))
            {
                //flags 1-4 based on Isolated Hero
                bool? flag = g.DoesFirstCardAffectSecondCard((Card c) => c.Owner == GetIsolatedHero(), (Card c) => c.Owner != GetIsolatedHero() && IsHero(c.Owner));
                bool? flag2 = g.DoesFirstCardAffectSecondCard((Card c) => c.Owner != GetIsolatedHero() && IsHero(c.Owner), (Card c) => c.Owner == GetIsolatedHero());
                bool? flag3 = g.DoesFirstTurnTakerAffectSecondTurnTaker((TurnTaker tt) => tt == GetIsolatedHero(), (TurnTaker tt) => tt != GetIsolatedHero() && IsHero(tt));
                bool? flag4 = g.DoesFirstTurnTakerAffectSecondTurnTaker((TurnTaker tt) => tt != GetIsolatedHero() && IsHero(tt), (TurnTaker tt) => tt == GetIsolatedHero());

                //flags 5 and 6 make it work with Uh Yeah
                bool? flag5 = g.CardSource != null && g.CardSource.AssociatedCardSources.Count() > 0 && g.CardSource.Card.Owner == GetIsolatedHero() && g.CardSource.AssociatedCardSources.Any((CardSource c) => c.Card.Owner != GetIsolatedHero() && IsHero(c.Card.Owner));
                bool? flag6 = g.CardSource != null && g.CardSource.AssociatedCardSources.Count() > 0 && g.CardSource.Card.Owner != GetIsolatedHero() && IsHero(g.CardSource.Card) && g.CardSource.AssociatedCardSources.Any((CardSource c) => c.Card.Owner == GetIsolatedHero());

                //flag 7 prevents power actions
                bool? flag7 = g is UsePowerAction && PowerConditions((UsePowerAction)g);

                //flag 8 prevents other hero cards from preventing phase actions of the isolated hero, and vice versa.
                //The reason this is included is because of Radiance's Radiant Duplicate
                bool? flag8 = g is PreventPhaseAction && PreventPhaseConditions((PreventPhaseAction)g);

                if ((flag.HasValue && flag.Value) || (flag2.HasValue && flag2.Value) || (flag3.HasValue && flag3.Value) || (flag4.HasValue && flag4.Value) || (flag5.HasValue && flag5.Value) || (flag6.HasValue && flag6.Value) || (flag7.HasValue && flag7.Value) || (flag8.HasValue && flag8.Value))
                {
                    Log.Debug($"{this.Card.Title} prevents action from occurring: {g.ToString()}");
                    return false;
                }
            }
            return true;
        }

        public override void AddTriggers()
        {
            //If Ra is isolated and you hit Ra with Stun Bolt, the status effect is still created, and it reduces the damage Ra deals by 1 (I think because the reduction is treated as affecting the target, not affecting Ra).
            //Except if Ra hits himself - in that case, the reduction does not apply because Wraith cannot reduce damage dealt to Ra.

            //Can't block Leyline Shift or Gimmicky Character from selecting the isolated hero to discard their top card, but it does prevent the discard itself

            AddTrigger((MakeDecisionsAction md) => md.CardSource != null && IsHero(md.CardSource.Card.Owner), RemoveDecisionsFromMakeDecisionsResponse, TriggerType.RemoveDecision, TriggerTiming.Before);
            //AddTrigger<UsePowerAction>((UsePowerAction up) => GetIsolatedHero() != null && PowerConditions(up), (UsePowerAction up) => base.GameController.CancelAction(up,cardSource:GetCardSource()), TriggerType.CancelAction, TriggerTiming.Before);
            AddTrigger<MakeDecisionAction>((MakeDecisionAction md) =>  md.CardSource != null && IsHero(md.CardSource.Card), DecisionResponse, TriggerType.Hidden, TriggerTiming.Before);

            base.AddTriggers();
        }

        private bool PowerConditions(UsePowerAction up)
        {
            //If the isolated hero is using a power due to another hero's card
            bool flag1 = up.HeroUsingPower.TurnTaker == GetIsolatedHero() && up.CardSource != null && IsHero(up.CardSource.Card) && up.CardSource.Card.Owner != GetIsolatedHero();

            //If another hero is using a power due to an isolated card
            bool flag2 = up.HeroUsingPower.TurnTaker != GetIsolatedHero() && up.CardSource != null && up.CardSource.Card.Owner == GetIsolatedHero();

            //If the isolated hero is using a power on another hero's card (I Can Do That Too)
            bool flag3 = up.HeroUsingPower.TurnTaker == GetIsolatedHero() && up.Power.CardSource != null && IsHero(up.Power.CardSource.Card.Owner) && up.Power.CardSource.Card.Owner != GetIsolatedHero();// && !(up.Power.IsContributionFromCardSource && !IsHero(up.Power.CopiedFromCardController.Card));

            //If another hero is using a power on an isolated card (I Can Do That Too)
            bool flag4 = up.HeroUsingPower.TurnTaker != GetIsolatedHero() && up.Power.CardSource != null && up.Power.CardSource.Card.Owner == GetIsolatedHero();

            //If the isolated hero is using a power granted by another hero's card
            bool flag5 = up.HeroUsingPower.TurnTaker == GetIsolatedHero() && up.Power.IsContributionFromCardSource && IsHero(up.Power.CopiedFromCardController.Card) && up.Power.CopiedFromCardController.Card.Owner != GetIsolatedHero();

            //If another hero is using a power granted by the isolated hero's card
            bool flag6 = up.HeroUsingPower.TurnTaker != GetIsolatedHero() && up.Power.IsContributionFromCardSource && up.Power.CopiedFromCardController.Card.Owner == GetIsolatedHero();

            return flag1 || flag2 || flag3 || flag4 || flag5 || flag6;
        }

        private bool PreventPhaseConditions(PreventPhaseAction g)
        {
            //If another hero's card is preventing a turn phase of the isolated hero 
            bool flag1 = g.TurnPhase.TurnTaker == GetIsolatedHero() && g.CardSource != null && IsHero(g.CardSource.Card) && g.CardSource.Card.Owner != GetIsolatedHero();

            //If a card belonging to the isolated hero is preventing a turn phase of another hero
            bool flag2 = g.CardSource != null && g.CardSource.Card.Owner == GetIsolatedHero() && g.TurnPhase.TurnTaker != GetIsolatedHero();

            return flag1 || flag2;
        }

        private IEnumerator DecisionResponse(MakeDecisionAction md)
        {
            IEnumerator coroutine;
            if (md.Decision is SelectCardDecision && ((SelectCardDecision)md.Decision).SelectionType != SelectionType.SelectTarget)
            {
                if (md.CardSource.Card.Owner != GetIsolatedHero())
                {
                    IEnumerable<Card> list = ((SelectCardDecision)md.Decision).Choices;
                    IEnumerable<Card> exclude = list.Where((Card c) => c.Owner == GetIsolatedHero());
                    ((SelectCardDecision)md.Decision).Choices = list.Except(exclude);
                    if (((SelectCardDecision)md.Decision).Choices.Count() == 0)
                    {
                        coroutine = CancelAction(md);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    if (exclude.Any())
                    {
                        Log.Debug($"{this.Card.Title} excludes {exclude.Select((Card c) => c.Title).ToCommaList()} from decision {md.Decision.ToString()}");
                    }
                }
                else
                {
                    IEnumerable<Card> list = ((SelectCardDecision)md.Decision).Choices;
                    IEnumerable<Card> exclude = list.Where((Card c) => IsHero(c.Owner) && c.Owner != GetIsolatedHero());
                    ((SelectCardDecision)md.Decision).Choices = list.Except(exclude);
                    if (((SelectCardDecision)md.Decision).Choices.Count() == 0)
                    {
                        coroutine = CancelAction(md);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    if (exclude.Any())
                    {
                        Log.Debug($"{this.Card.Title} excludes {exclude.Select((Card c) => c.Title).ToCommaList()} from decision {md.Decision.ToString()}");
                    }
                }
            }
            else if (md.Decision is SelectTurnTakerDecision && ((SelectTurnTakerDecision)md.Decision).SelectionType != SelectionType.SelectTarget)
            {
                if (md.CardSource.Card.Owner != GetIsolatedHero())
                {
                    IEnumerable<TurnTaker> list = ((SelectTurnTakerDecision)md.Decision).Choices;
                    List<TurnTaker> exclude = list.Where((TurnTaker tt) => tt == GetIsolatedHero()).ToList();

                    if (exclude.Count() > 0)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[0]);
                    }

                    if (exclude.Count() > 1)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[1]);
                    }

                    if (exclude.Count() > 2)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[2]);
                    }

                    if (exclude.Count() > 3)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[3]);
                    }

                    if (exclude.Count() > 4)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[4]);
                    }

                    if (((SelectTurnTakerDecision)md.Decision).Choices.Count() == 0)
                    {
                        coroutine = CancelAction(md);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    if (exclude.Any())
                    {
                        Log.Debug($"{this.Card.Title} excludes {exclude.Select((TurnTaker tt) => tt.Name).ToCommaList()} from decision {md.Decision.ToString()}");
                    }
                }
                else
                {
                    IEnumerable<TurnTaker> list = ((SelectTurnTakerDecision)md.Decision).Choices;
                    List<TurnTaker> exclude = list.Where((TurnTaker tt) => IsHero(tt) && tt != GetIsolatedHero()).ToList();
                    //Log.Debug($"possible turntakers: {list.Select((TurnTaker tt) => tt.Name).ToCommaList()}");
                    //Log.Debug($"excluded turntakers: {exclude.Select((TurnTaker tt) => tt.Name).ToCommaList()}");
                    //Log.Debug($"excluded element 0: {exclude[0].Name}");
                    //Log.Debug($"excluded element 1: {exclude[1].Name}");
                    //Log.Debug($"excluded element 2: {exclude[2].Name}");
                    if (exclude.Count() > 0)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[0]);
                    }

                    if (exclude.Count() > 1)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[1]);
                    }

                    if (exclude.Count() > 2)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[2]);
                    }

                    if (exclude.Count() > 3)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[3]);
                    }

                    if (exclude.Count() > 4)
                    {
                        ((SelectTurnTakerDecision)md.Decision).RemoveChoice(exclude[4]);
                    }

                    if (((SelectTurnTakerDecision)md.Decision).Choices.Count() == 0)
                    {
                        coroutine = CancelAction(md);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                    }

                    if (exclude.Any())
                    {
                        Log.Debug($"{this.Card.Title} excludes {exclude.Select((TurnTaker tt) => tt.Name).ToCommaList()} from decision {md.Decision.ToString()}");
                    }
                }
            }
        }

        private IEnumerator RemoveDecisionsFromMakeDecisionsResponse(MakeDecisionsAction md)
        {
            Log.Debug($"{this.Card.Title} is removing decisions from {md.ToString()}");
            md.RemoveDecisions((IDecision d) => d.CardSource.Card.Owner != GetIsolatedHero() && d.HeroTurnTakerController.TurnTaker == GetIsolatedHero());
            md.RemoveDecisions((IDecision d) => d.CardSource.Card.Owner == GetIsolatedHero() && d.HeroTurnTakerController.TurnTaker != GetIsolatedHero());
            Log.Debug($"Decision is now {md.ToString()}");
            yield return DoNothing();
        }

        public override IEnumerator Play()
        {
            if (base.Card.Location.IsNextToCard)
            {
                IEnumerator coroutine = base.GameController.SendMessageAction(base.Card.Title + " will prevent " + GetIsolatedHero().Name + " from affecting other heroes, and other heroes cannot affect " + GetIsolatedHero().Name + ", except for dealing damage.", Priority.High, GetCardSource(), null, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
        }
    }
}

