using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.TheBaroness
{
    public class CloudOfBatsCardController : BaronessUtilityCardController
    {
        public CloudOfBatsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show if Winged Terror is in play
            SpecialStringMaker.ShowIfSpecificCardIsInPlay(TerrorIdentifier);
        }

        private LinqCardCriteria HasBat()
        {
            return new LinqCardCriteria((Card c) => c.NextToLocation.HasCards && c.NextToLocation.Cards.Any((Card n) => n.Owner == base.TurnTaker && n.Identifier == BatIdentifier));
        }

        private LinqCardCriteria BatToRemove()
        {
            return new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier && c.Location != base.TurnTaker.OffToTheSide, "Bat");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a Bat is dealt damage, reduce damage dealt to Bats by 1 this turn."
            AddTrigger((DealDamageAction dda) => dda.Target.DoKeywordsContain(BatKeyword) && dda.DidDealDamage, BatsScatterResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            // "Blood cards have a maximum HP of 4 and are villain Bats."
            // When a card becomes Blood, assign it a Bat placeholder card from OffToTheSide
            AddTrigger((MoveCardAction mca) => mca.CardToMove.IsFaceDownNonCharacter && mca.CardToMove.IsHero && mca.WasCardMoved && mca.Destination.IsPlayAreaOf(base.TurnTaker) && !mca.Origin.IsPlayAreaOf(base.TurnTaker), (MoveCardAction mca) => MakeBat(mca.CardToMove), TriggerType.Hidden, TriggerTiming.After);
            // When a Blood card is destroyed, destroy its Bat and move that Bat to OffToTheSide
            AddTrigger((DestroyCardAction dca) => dca.WasCardDestroyed && HasBat().Criteria(dca.CardToDestroy.Card), DestroyBatRemoveBatResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.MoveCard }, TriggerTiming.After);
            // When a Blood card leaves play another way, move its Bat to OffToTheSide
            AddTrigger((BulkMoveCardsAction bmca) => bmca.CardsToMove.Any((Card c) => HasBat().Criteria(c)) && !bmca.Destination.IsInPlayAndNotUnderCard, RemoveBatResponse, TriggerType.MoveCard, TriggerTiming.After);
            AddTrigger((MoveCardAction mca) => BloodCard().Criteria(mca.CardToMove) && !mca.Destination.IsInPlayAndNotUnderCard, RemoveBatResponse, TriggerType.MoveCard, TriggerTiming.After);
            // When a Bat is destroyed, destroy its Blood card and move it to OffToTheSide
            AddTrigger((DestroyCardAction dca) => dca.WasCardDestroyed && dca.CardToDestroy.Card.Owner == base.TurnTaker && dca.CardToDestroy.Card.Identifier == BatIdentifier, DestroyBloodRemoveBatResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.MoveCard }, TriggerTiming.After);
            // When a Bat leaves play another way, ???
            AddTrigger((MoveCardAction mca) => mca.CardToMove.Owner == base.TurnTaker && mca.CardToMove.Identifier == BatIdentifier && !mca.Destination.IsInPlay && !mca.Destination.IsOffToTheSide, (MoveCardAction mca) => CancelAction(mca), TriggerType.CancelAction, TriggerTiming.Before);
            // ...
            // When this card leaves play, move all Bat placeholder cards back to OffToTheSide
            AddAfterLeavesPlayAction(() => base.GameController.BulkMoveCards(base.TurnTakerController, base.GameController.FindCardsWhere(BatToRemove()), base.TurnTaker.OffToTheSide, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()));
            // "At the start of the villain turn, each Bat deals each hero target 1 projectile damage. If less than 4 damage is dealt this way, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, BatsSwarmResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });

            /*AddTrigger((MoveCardAction mca) => mca.Origin.IsPlayAreaOf(base.TurnTaker), LogMoveCardAction, TriggerType.Hidden, TriggerTiming.Before);
            AddTrigger((BulkMoveCardsAction bmca) => true, LogBulkMoveAction, TriggerType.Hidden, TriggerTiming.Before);
            AddTrigger((BulkMoveCardsAction bmca) => true, LogBulkMoveAction, TriggerType.Hidden, TriggerTiming.After);*/
        }

        /*private IEnumerator LogBulkMoveAction(BulkMoveCardsAction bmca)
        {
            Log.Debug("CloudOfBatsCardController.LogBulkMoveAction activated");
            Log.Debug("CloudOfBatsCardController.LogBulkMoveAction: mca.Destination: " + bmca.Destination.GetFriendlyName());
            foreach(Card c in bmca.CardsToMove)
            {
                Log.Debug("CloudOfBatsCardController.LogBulkMoveAction: card to move: " + c.Title);
                Log.Debug("CloudOfBatsCardController.LogBulkMoveAction:  current location: " + c.Location.GetFriendlyName());
                Log.Debug("CloudOfBatsCardController.LogBulkMoveAction:  BloodCard().Criteria(" + c.Title + "): " + BloodCard().Criteria(c).ToString());
                Log.Debug("CloudOfBatsCardController.LogBulkMoveAction:  HasBat().Criteria(" + c.Title + "): " + HasBat().Criteria(c).ToString());
            }
            yield break;
        }

        private IEnumerator LogMoveCardAction(MoveCardAction mca)
        {
            Log.Debug("CloudOfBatsCardController.LogMoveCardAction activated");
            Log.Debug("CloudOfBatsCardController.LogMoveCardAction: mca.Destination: " + mca.Destination.GetFriendlyName());
            Log.Debug("CloudOfBatsCardController.LogMoveCardAction: mca.CardToMove: " + mca.CardToMove.Title);
            Log.Debug("CloudOfBatsCardController.LogMoveCardAction: mca.CardToMove.IsFaceDownNonCharacter: " + mca.CardToMove.IsFaceDownNonCharacter.ToString());
            Log.Debug("CloudOfBatsCardController.LogMoveCardAction: mca.CardToMove.NextToLocation.HasCards: " + mca.CardToMove.NextToLocation.HasCards.ToString());
            foreach(Card c in mca.CardToMove.NextToLocation.Cards)
            {
                Log.Debug("CloudOfBatsCardController.LogMoveCardAction: card next to mca.CardToMove: " + c.Title);
            }

            // Log.Debug("CloudOfBatsCardController.LogMoveCardAction");
            yield break;
        }*/

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy Winged Terror."
            Card terror = base.TurnTaker.FindCard(TerrorIdentifier);
            if (terror.IsInPlayAndHasGameText)
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, terror, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            // "Blood cards have a maximum HP of 4 and are villain Bats."
            // Assign a Bat to each Blood card in play
            IEnumerable<Card> bloodCards = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => BloodCard().Criteria(c) && !HasBat().Criteria(c)), visibleToCard: GetCardSource());
            foreach (Card c in bloodCards)
            {
                IEnumerator makeCoroutine = MakeBat(c);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(makeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(makeCoroutine);
                }
            }
        }

        private IEnumerator MakeBat(Card blood)
        {
            // Get a Bat from OffToTheSide and assign it to blood
            Card newBat = TurnTakerControllerWithoutReplacements.TurnTaker.GetAllCards().Where((Card c) => c.Location.IsOffToTheSide).FirstOrDefault();
            if (newBat != null && blood.Location.IsPlayAreaOf(base.TurnTaker) && blood.IsFaceDownNonCharacter)
            {
                IEnumerator moveBatCoroutine = base.GameController.MoveCard(base.TurnTakerController, newBat, blood.NextToLocation, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveBatCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveBatCoroutine);
                }
            }
        }

        private IEnumerator DestroyBatRemoveBatResponse(DestroyCardAction dca)
        {
            // When a Blood card is destroyed, destroy its Bat and move that Bat to OffToTheSide
            Card bloodDestroyed = dca.CardToDestroy.Card;
            if (bloodDestroyed != null && bloodDestroyed.NextToLocation.HasCards && bloodDestroyed.NextToLocation.Cards.Any((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier))
            {
                List<Card> associatedBats = bloodDestroyed.NextToLocation.Cards.Where((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier).ToList();
                foreach (Card c in associatedBats)
                {
                    IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, c, actionSource: dca, responsibleCard: dca.ResponsibleCard, overrideDestroyLocation: base.TurnTaker.OffToTheSide, cardSource: dca.CardSource);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                }
            }
        }

        private IEnumerator DestroyBloodRemoveBatResponse(DestroyCardAction dca)
        {
            // When a Bat is destroyed, destroy its Blood card...
            //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: ");
            Card batDestroyed = dca.CardToDestroy.Card;
            if (batDestroyed != null && batDestroyed.Location.IsNextToCard)
            {
                Card associatedBlood = batDestroyed.Location.OwnerCard;
                if (associatedBlood != null)
                {
                    // If the Bat would end up somewhere unusual on destruction, the Blood ends up there instead
                    //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: determining bloodDest");
                    Location bloodDest = associatedBlood.NativeTrash;
                    //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: default bloodDest: " + bloodDest.GetFriendlyName());
                    Location batIntendedDest = dca.Destination;
                    //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: batIntendedDest: " + batIntendedDest.GetFriendlyName());
                    if (batIntendedDest.IsInPlay || batIntendedDest.IsHand)
                    {
                        //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: batIntendedDest is in play or a player's hand => bloodDest is the same");
                        bloodDest = batIntendedDest;
                    }
                    else if (batIntendedDest.IsDeck)
                    {
                        if (batIntendedDest.OwnerTurnTaker == base.TurnTaker)
                        {
                            //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: batIntendedDest is The Baroness's deck => bloodDest is the Blood card's deck");
                            bloodDest = associatedBlood.NativeDeck;
                        }
                        else
                        {
                            //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: batIntendedDest is a deck other than The Baroness's => bloodDest is the same");
                            bloodDest = batIntendedDest;
                        }
                    }
                    else if (batIntendedDest.IsTrash && batIntendedDest.OwnerTurnTaker != base.TurnTaker)
                    {
                        //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: batIntendedDest is a trash other than The Baroness's => bloodDest is the same");
                        bloodDest = batIntendedDest;
                    }
                    //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: bloodDest result: " + bloodDest.GetFriendlyName());
                    IEnumerator destroyCoroutine = null;
                    if (bloodDest == associatedBlood.NativeTrash)
                    {
                        //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: destroyCoroutine is DestroyCard with no override");
                        destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, associatedBlood, actionSource: dca, responsibleCard: dca.ResponsibleCard, cardSource: dca.CardSource);
                    }
                    else
                    {
                        //Log.Debug("CloudOfBatsCardController.DestroyBloodRemoveBatResponse: destroyCoroutine is DestroyCard with overrideDestroyLocation");
                        destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, associatedBlood, actionSource: dca, responsibleCard: dca.ResponsibleCard, overrideDestroyLocation: bloodDest, cardSource: dca.CardSource);
                    }
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                }
            }
            // ... and move it to OffToTheSide
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, batDestroyed, base.TurnTaker.OffToTheSide, actionSource: dca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }

        private IEnumerator RemoveBatResponse(GameAction ga)
        {
            // When a Blood card leaves play another way, move its Bat to OffToTheSide
            List<Card> bloodMoved = new List<Card>();
            TurnTaker responsible = null;
            CardSource source = null;
            if (ga is MoveCardAction mca)
            {
                bloodMoved.Add(mca.CardToMove);
                responsible = mca.ResponsibleTurnTaker;
                source = mca.CardSource;
            }
            else if (ga is BulkMoveCardsAction bmca)
            {
                bloodMoved = bmca.CardsToMove.ToList();
                responsible = bmca.ResponsibleTurnTaker;
                source = bmca.CardSource;
            }
            foreach (Card bloodDrip in bloodMoved)
            {
                if (bloodDrip != null && bloodDrip.Location.HasCards && bloodDrip.NextToLocation.Cards.Any((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier))
                {
                    List<Card> associatedBats = bloodDrip.NextToLocation.Cards.Where((Card c) => c.Owner == base.TurnTaker && c.Identifier == BatIdentifier).ToList();
                    IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, associatedBats, base.TurnTaker.OffToTheSide, responsibleTurnTaker: responsible, cardSource: source);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
            }
        }

        private IEnumerator BatsScatterResponse(DealDamageAction dda)
        {
            // "... reduce damage dealt to Bats by 1 this turn."
            ReduceDamageStatusEffect shield = new ReduceDamageStatusEffect(1);
            List<string> keywords = new List<string>();
            keywords.Add(BatKeyword);
            shield.TargetCriteria.HasAnyOfTheseKeywords = keywords;
            shield.UntilThisTurnIsOver(base.GameController.Game);
            IEnumerator protectCoroutine = AddStatusEffect(shield);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(protectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(protectCoroutine);
            }
        }

        private IEnumerator BatsSwarmResponse(PhaseChangeAction pca)
        {
            // "...each Bat deals each hero target 1 projectile damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator damageCoroutine = MultipleDamageSourcesDealDamage(new LinqCardCriteria((Card c) => c.DoKeywordsContain(BatKeyword), "Bat"), TargetType.All, null, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget), 1, DamageType.Projectile, damageResults: results);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If less than 4 damage is dealt this way, destroy this card."
            int totalDamage = 0;
            foreach (DealDamageAction dda in results)
            {
                if (dda.DidDealDamage)
                {
                    totalDamage += dda.Amount;
                }
            }
            if (totalDamage < 4)
            {
                IEnumerator destroyCoroutine = DestroyThisCardResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }
    }
}
