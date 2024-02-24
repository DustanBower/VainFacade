using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Doomsayer
{
	public class YouHaveNoChoiceCardController:ProclamationCardController
	{
		public YouHaveNoChoiceCardController(Card card, TurnTakerController turnTakerController)
        : base(card, turnTakerController)
        {
		}

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //Play this card in a hero play area.
            List<SelectTurnTakerDecision> results = new List<SelectTurnTakerDecision>();
            IEnumerator coroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.MoveCardToPlayArea, results, additionalCriteria: (TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (DidSelectTurnTaker(results))
            {
                TurnTaker hero = GetSelectedTurnTaker(results);
                storedResults.Add(new MoveCardDestination(hero.PlayArea));
            }
        }

        public override void AddTriggers()
        {
            //When that hero’s player would play a card or that hero would use a power, that hero’s player may discard a card. If they do not, play the bottom card of that hero’s deck instead.
            AddTrigger<PlayCardAction>((PlayCardAction pca) => this.Card.Location.IsHeroPlayAreaRecursive && pca.ResponsibleTurnTaker == this.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && !pca.IsPutIntoPlay, DiscardOrPlayBottom, new TriggerType[2] { TriggerType.CancelAction, TriggerType.PlayCard }, TriggerTiming.Before);
            AddTrigger<UsePowerAction>((UsePowerAction up) => this.Card.Location.IsHeroPlayAreaRecursive && up.HeroUsingPower.TurnTaker == this.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, DiscardOrPlayBottom, new TriggerType[2] { TriggerType.CancelAction, TriggerType.PlayCard }, TriggerTiming.Before);
        }

        private IEnumerator DiscardOrPlayBottom(GameAction g)
        {
            HeroTurnTakerController httc = FindHeroTurnTakerController(this.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero());
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator coroutine = base.GameController.SelectAndDiscardCard(httc, true, additionalCriteria: (Card c) => g is PlayCardAction ? ((PlayCardAction)g).CardToPlay != c : true, storedResults: results, cardSource: GetCardSource(), responsibleTurnTaker: httc.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            if (!DidDiscardCards(results))
            {
                coroutine = CancelAction(g);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                if (g is PlayCardAction)
                {
                    PlayCardAction pca = (PlayCardAction)g;
                    Location origin = pca.Origin;
                    if (origin.IsRevealed)
                    {
                        //If the card was revealed and played, put it back where it was revealed from so it doesn't get stuck in the Revealed area
                        //based on PrimordialSeedCardController
                        MoveCardJournalEntry entry = (from mc in base.Journal.MoveCardEntriesThisTurn()
                                                      where mc.Card == pca.CardToPlay && mc.ToLocation.IsRevealed
                                                      select mc).LastOrDefault();
                        if (entry != null)
                        {
                            origin = entry.FromLocation;
                        }
                    }
                    IEnumerator moveCoroutine = GameController.MoveCard(null, pca.CardToPlay, origin, pca.FromBottom, false, evenIfIndestructible: true, doesNotEnterPlay: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
                

                coroutine = base.GameController.PlayCard(this.TurnTakerController, httc.HeroTurnTaker.Deck.BottomCard, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
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

