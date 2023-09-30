using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VainFacadePlaytest.Friday
{
	public class GrimReflectionCardController:CardController
	{
        //Known issues with this card:
        //--Uh Yeah crashes the game in several scenarios, so this card cannot copy Uh Yeah and Uh Yeah cannot use this card's text to copy this card
        //--I think this will not make cards indestructible if the condition is based on the TurnTaker instead of the card. I also think that no ongoing cards exist that do that.
        //--When this card self-destructs due to copying Impending Doom, cards that react to destruction may react twice - once to Impending Doom, and once to Grim Reflection
        //  This is because the copied self-destruct effect actually destroys Impending Doom, so this card just cancels Impending Doom's self-destruct when appropriate and destroys this card instead
        //  The reason this card doesn't copy Impending Doom's destruction effect correctly is that it needs to not replace references to the card Impending Doom. If it did, the copied trigger would go off on Friday's turn instead of the scion's turn.
        
		public GrimReflectionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.AllowAddingTriggersBeforeEnteringPlay = false;
            _ongoingsGR = new List<Card>();
            _powerCardSources = new Dictionary<Power, CardSource>();
            _abilityCardSources = new Dictionary<ActivatableAbility, CardSource>();
            _triggersForCard = new Dictionary<string, List<ITrigger>>();
            base.SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => ShouldCardBeCopied(c), "copied ongoing", useCardsSuffix: true)).Condition = () => base.Card.IsInPlayAndHasGameText;
            AddThisCardControllerToList(CardControllerListType.ActivatesEffects);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            //Not sure if this would discriminate based on battle zone 
            if (_ongoingsGR != null)
            {
                bool result = false;
                foreach (Card c in _ongoingsGR)
                {
                    CardController cardController = FindCardController(c);
                    if (this.CharacterCard == card || this.CharacterCards.Contains(card))
                    {
                        result |= cardController.AskIfCardIsIndestructible(cardController.CharacterCardWithoutReplacements);
                    }
                    else if (this.Card == card)
                    {
                        result |= cardController.AskIfCardIsIndestructible(cardController.CardWithoutReplacements);
                    }
                }
                //Log.Debug("Asking " + this.Card.Title + " if " + card.Title + " is indestructible. Returning " + result.ToString());
                return result;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        private List<Card> _ongoingsGR;

        private Dictionary<string, List<ITrigger>> _triggersForCard;

        private SelfDestructTrigger _removeCardSourceWhenDestroyedTrigger;

        private Dictionary<Power, CardSource> _powerCardSources;

        private Dictionary<ActivatableAbility, CardSource> _abilityCardSources;

        //The game crashes if Grim Reflection and Uh Yeah are ever copying each other at the same time.
        //To avoid this, this card will not copy Uh Yeah even if Friday has a card next to it.
        private string[] ForbiddenCards = {"UhYeahImThatGuy"};

        private bool ShouldCardBeCopied(Card c)
        {
            //Do not allow this card to copy itself, like if Uh Yeah puts one of Guise's cards under this card
            return IsOngoing(c) && c.IsInPlayAndHasGameText && c.UnderLocation.Cards.Any((Card cc) => cc.Owner == this.TurnTaker) && !ForbiddenCards.Contains(c.Identifier) && c != this.CardWithoutReplacements;
        }

        private string WarningString(Card c)
        {
            return c.Identifier + "Warning";
        }

        public override IEnumerator UsePower(int index = 0)
        {
            //Put any number of cards in your hand under ongoing cards from another deck. Draw a card or use a power.
            IEnumerable<MoveCardDestination> destinations = FindCardsWhere((Card c) => IsOngoing(c) && c.Owner != this.TurnTaker && c.IsInPlayAndHasGameText).Select((Card c) => new MoveCardDestination(c.UnderLocation));
            IEnumerator coroutine = base.GameController.SelectCardsFromLocationAndMoveThem(DecisionMaker, this.HeroTurnTaker.Hand, 0, 100, new LinqCardCriteria((Card c) => true), destinations, playIfMovingToPlayArea: false, flipFaceDown: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            IEnumerable<Function> functionChoices = new Function[2]
                {
                new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => DrawCard()),
                new Function(base.HeroTurnTakerController, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(DecisionMaker, false,cardSource:GetCardSource()))
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, functionChoices, false, null, null, null, GetCardSource());
            IEnumerator choose = base.GameController.SelectAndPerformFunction(selectFunction);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(choose);
            }
            else
            {
                base.GameController.ExhaustCoroutine(choose);
            }
        }

        public override IEnumerator Play()
        {
            //Treat this card as having the game-text of each ongoing card with one of your cards under it, replacing the name of any other characters with {Friday} and treating 'you' as {Friday}'s player.
            _ongoingsGR = new List<Card>();
            _powerCardSources = new Dictionary<Power, CardSource>();
            _abilityCardSources = new Dictionary<ActivatableAbility, CardSource>();
            _triggersForCard = new Dictionary<string, List<ITrigger>>();
            AddLastTriggers();
            IEnumerable<Card> play = _ongoingsGR.Where((Card c) => FindCardController(c).DoesHaveActivePlayMethod);
            IEnumerable<string> enumerable = play.Select((Card c) => c.Identifier).Distinct();
            List<Card> ongoingsToRunPlay = new List<Card>();
            foreach (string distinctPlayIdentifier in enumerable)
            {
                Card item = play.Where((Card c) => c.Identifier == distinctPlayIdentifier).First();
                ongoingsToRunPlay.Add(item);
            }
            GameController gameController = base.GameController;
            HeroTurnTakerController decisionMaker = DecisionMaker;
            Func<Card, bool> criteria = (Card c) => ongoingsToRunPlay.Contains(c) && base.Card.IsInPlayAndHasGameText;
            CardSource cardSource = GetCardSource();
            SelectCardsDecision selectCardsDecision = new SelectCardsDecision(gameController, decisionMaker, criteria, SelectionType.AmbiguousDecision, null, isOptional: false, null, eliminateOptions: true, allowAutoDecide: true, allAtOnce: false, null, null, null, null, cardSource);
            IEnumerator coroutine = base.GameController.SelectCardsAndDoAction(selectCardsDecision, (SelectCardDecision d) => PlayOngoing(d.SelectedCard));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (!base.Card.IsInPlayAndHasGameText)
            {
                yield break;
            }
            foreach (Card item2 in _ongoingsGR.Where((Card c) => !play.Contains(c)))
            {
                coroutine = PlayOngoing(item2);
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

        private IEnumerator PlayOngoing(Card card)
        {
            CardController cc = FindCardController(card);
            CardSource source = GetCardSource();
            source.SourceLimitation = CardSource.Limitation.Play;
            cc.AddAssociatedCardSource(source);
            IEnumerator coroutine = cc.Play();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            cc.RemoveAssociatedCardSource(source);
        }

        private void DebugResponse(MoveCardAction mc)
        {
            bool condition = mc.CardToMove.Owner == this.TurnTaker && mc.Destination.IsUnderCard && IsOngoing(mc.Destination.OwnerCard) && mc.Destination.OwnerCard.IsInPlayAndHasGameText && !ForbiddenCards.Contains(mc.Destination.OwnerCard.Identifier) && mc.Destination.OwnerCard != this.CardWithoutReplacements && !_ongoingsGR.Contains(mc.Destination.OwnerCard);
            Console.WriteLine($"Trigger condition returns {condition} for moving {mc.CardToMove.Title} under {mc.Destination.OwnerCard.Title}\n");
            Console.WriteLine($"IsOngoing: {IsOngoing(mc.Destination.OwnerCard)}\n");
            Console.WriteLine($"IsInPlay: {mc.Destination.OwnerCard.IsInPlayAndHasGameText}\n");
            Console.WriteLine($"Is Forbidden: {ForbiddenCards.Contains(mc.Destination.OwnerCard.Identifier)}\n");
            Console.WriteLine($"Destination not this card: {mc.Destination.OwnerCard != this.CardWithoutReplacements}\n");
            Console.WriteLine($"Already copied: {_ongoingsGR.Contains(mc.Destination.OwnerCard)}");
            Console.WriteLine($"MoveCardAction battle zone: {mc.BattleZone.Identifier}");
        }

        public override void AddLastTriggers()
        {
            //At the start of your turn, destroy one of your ongoing cards.
            AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == this.TurnTaker, "ongoing"), false, cardSource: GetCardSource()), TriggerType.DestroyCard);

            AddThisCardControllerToList(CardControllerListType.ReplacesCards);
            AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
            AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);
            AddThisCardControllerToList(CardControllerListType.AddsPowers);
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);

            //Cards in the other battle zone need to be added to _ongoingsGR so that when Friday moves into that battle zone, it copies the card
            //Triggers from cards in the other battle zone do not activate anyway, so this does act correctly
            FindCardsWhere((Card c) => ShouldCardBeCopied(c), ignoreBattleZone: true).ForEach(delegate (Card c)
            {
                AddOngoingCard(c);
            });

            //The MoveCardAction when moving a card next to Impending Doom or Focus of Power is in OblivAeon's battle zone, not Friday's.
            //To account for this, ignoreBattleZone is set to true, and the battle zone is checked in the condition
            AddTrigger((MoveCardAction mc) => mc.CardToMove.Owner == this.TurnTaker && mc.Destination.IsUnderCard && IsOngoing(mc.Destination.OwnerCard) && mc.Destination.OwnerCard.IsInPlayAndHasGameText && !ForbiddenCards.Contains(mc.Destination.OwnerCard.Identifier) && mc.Destination.OwnerCard != this.CardWithoutReplacements && !_ongoingsGR.Contains(mc.Destination.OwnerCard) && this.BattleZone == mc.Destination.BattleZone, (MoveCardAction mc) => AddOngoingCardEnumerator(mc,mc.Destination.OwnerCard), TriggerType.Hidden, TriggerTiming.After, ignoreBattleZone: true);

            AddTrigger((MoveCardAction mc) => mc.CardToMove.Owner == this.TurnTaker && mc.Origin.IsUnderCard && IsOngoing(mc.Origin.OwnerCard) && !ForbiddenCards.Contains(mc.Origin.OwnerCard.Identifier) && mc.Origin.OwnerCard != this.CardWithoutReplacements && _ongoingsGR.Contains(mc.Origin.OwnerCard) && !ShouldCardBeCopied(mc.Origin.OwnerCard), (MoveCardAction mc) => RemoveOngoingTriggersForCard(mc.Origin.OwnerCard), TriggerType.RemoveTrigger, TriggerTiming.After);

            //Show a funny message the first time a card is moved under Uh Yeah
            AddTrigger((MoveCardAction mc) => mc.CardToMove.Owner == this.TurnTaker && mc.Destination.IsUnderCard && ForbiddenCards.Contains(mc.Destination.OwnerCard.Identifier) && !HasBeenSetToTrueThisGame(WarningString(mc.Destination.OwnerCard)), (MoveCardAction mc) => SendUhYeahMessage(mc.Destination.OwnerCard), TriggerType.ShowMessage, TriggerTiming.After);

            AddWhenDestroyedTrigger(ReplaceCardSourceWhenDestroyed, TriggerType.Hidden);
            _removeCardSourceWhenDestroyedTrigger = AddWhenDestroyedTrigger(RemoveCardSourceWhenDestroyed, TriggerType.HiddenLast);

            //Originally, these triggers add and remove ongoing triggers when the target turntaker moves battle zone
            //For this card, they are changed slightly to only add/remove cards with Friday's cards under them
            AddTrigger((SwitchBattleZoneAction sb) => TurntakerHasCopiedCard(sb.TurnTaker) && sb.Origin == base.BattleZone && sb.Destination != base.BattleZone, (SwitchBattleZoneAction sb) => RemoveOngoingTriggersForTurnTaker(sb.TurnTaker), TriggerType.RemoveTrigger, TriggerTiming.After);
            AddTrigger((SwitchBattleZoneAction sb) => TurntakerHasCopiedCard(sb.TurnTaker) && sb.Origin != base.BattleZone && sb.Destination == base.BattleZone, (SwitchBattleZoneAction sb) => AddOngoingTriggersForTurnTaker(sb.TurnTaker), TriggerType.AddTrigger, TriggerTiming.After);
            RemoveAfterDestroyedAction(AfterDestroyCleanup);
            AddAfterDestroyedAction(AfterDestroyCleanup);
            AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);

            //Some villain ongoing cards do not use a dynamic reference for their character card's name in messages, so the name must be replaced by triggers on this card
            AddReplaceNameTrigger("AdaptivePlatingSubroutine", "Omnitron");
            AddReplaceNameTrigger("DisintegrationRay", "Omnitron");
            AddReplaceNameTrigger("ElectroMagneticRailgun", "Omnitron");
            AddReplaceNameTrigger("InterpolationBeam", "Omnitron");
            AddReplaceNameTrigger("ReturnWithTheDawn", "Citizen Dawn");

            //Impending Doom will destroy itself instead of Grim Reflection, so this trigger corrects that
            AddTrigger((DestroyCardAction dc) => dc.CardToDestroy.Card.Identifier == "ImpendingDoom" && dc.CardSource != null && _ongoingsGR.Any((Card c) => c.Identifier == "ImpendingDoom") && dc.CardSource.AssociatedCardSources.Select((CardSource cs) => cs.Card).Any((Card cc) => cc.Identifier == "ImpendingDoom"), HandleImpendingDoom, new TriggerType[2] { TriggerType.CancelAction, TriggerType.DestroySelf }, TriggerTiming.Before, priority: TriggerPriority.High);
        }

        private IEnumerator SendUhYeahMessage(Card problemCard)
        {
            SetCardProperty(WarningString(problemCard), true);
            string message = "To prevent the multiverse from imploding, " + this.Card.Title + " cannot copy " + problemCard.Title + ".";
            string extra = "";

            if (problemCard.Identifier == "UhYeahImThatGuy")
            {
                string[] extraMessages = new string[4]
                {
                    "{BR}[i]Oh sure, take all the fun out of it.[/i]",
                    "{BR}[i]You telling me the modders can't figure out how to make that work?![/i]",
                    "{BR}[i]Come on, who wouldn't want to be me?[/i]",
                    "{BR}[i]Can't I crash the game just once? Pleeeeease?[/i]"
                };
                Random rNG = base.GameController.Game.RNG;
                extra = extraMessages.TakeRandom(1, rNG).FirstOrDefault();
            }

            string finalMessage = message + extra;
            IEnumerator coroutine = base.GameController.SendMessageAction(finalMessage, Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private void AddReplaceNameTrigger(string cardName, string replace)
        {
            AddTrigger<MessageAction>((MessageAction m) => m.CardSource != null && _ongoingsGR.Any((Card c) => c.Identifier == cardName) && m.CardSource.AssociatedCardSources.Select((CardSource cs) => cs.Card).Any((Card cc) => cc.Identifier == cardName) && m.Message.Contains(replace), (MessageAction m) => ReplaceMessage(m, replace), new TriggerType[2] { TriggerType.CancelAction, TriggerType.Hidden }, TriggerTiming.Before);
        }

        private IEnumerator HandleImpendingDoom(DestroyCardAction dc)
        {
            CardSource source = dc.CardSource;
            GameAction actionsource = dc.ActionSource;
            IEnumerator coroutine = CancelAction(dc, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.DestroyCard(DecisionMaker, this.Card, actionSource: actionsource, cardSource: source);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        private bool TurntakerHasCopiedCard(TurnTaker tt)
        {
            return tt.PlayArea.Cards.Any((Card c) => ShouldCardBeCopied(c));
        }

        private IEnumerator ReplaceCardSourceWhenDestroyed(DestroyCardAction dc)
        {
            foreach (Card ongoing in _ongoingsGR)
            {
                CardController cardController = FindCardController(ongoing);
                if (cardController.HasWhenDestroyedTriggers)
                {
                    CardSource cardSource = GetCardSource();
                    cardSource.SourceLimitation = CardSource.Limitation.WhenDestroyed;
                    cardController.AddAssociatedCardSource(cardSource);
                }
            }
            yield return null;
        }

        private IEnumerator RemoveCardSourceWhenDestroyed(DestroyCardAction dc)
        {
            foreach (Card ongoing in _ongoingsGR)
            {
                CardController cardController = FindCardController(ongoing);
                if (cardController.HasWhenDestroyedTriggers)
                {
                    CardSource cardSource = GetCardSource();
                    cardSource.SourceLimitation = CardSource.Limitation.WhenDestroyed;
                    cardController.RemoveAssociatedCardSource(cardSource);
                }
            }
            yield return null;
        }

        private void CopyWhenDestroyedTriggers(CardController cc)
        {
            RemoveWhenDestroyedTrigger(_removeCardSourceWhenDestroyedTrigger);
            foreach (ITrigger whenDestroyedTrigger in cc.GetWhenDestroyedTriggers())
            {
                SelfDestructTrigger destroyTrigger = whenDestroyedTrigger as SelfDestructTrigger;
                AddWhenDestroyedTrigger((DestroyCardAction dc) => SetCardSourceLimitationsWhenDestroy(dc, destroyTrigger), destroyTrigger.Types.ToArray()).CardSource.AddAssociatedCardSource(cc.GetCardSource());
            }
            AddWhenDestroyedTrigger(_removeCardSourceWhenDestroyedTrigger);
        }

        private IEnumerator SetCardSourceLimitationsWhenDestroy(DestroyCardAction dc, SelfDestructTrigger destroyTrigger)
        {
            if (destroyTrigger.CardSource != null && destroyTrigger.CardSource.CardController != null)
            {
                destroyTrigger.CardSource.CardController.SetCardSourceLimitation(this, CardSource.Limitation.WhenDestroyed);
            }
            IEnumerator coroutine = destroyTrigger.Response(dc);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (destroyTrigger.CardSource != null && destroyTrigger.CardSource.CardController != null)
            {
                destroyTrigger.CardSource.CardController.RemoveCardSourceLimitation(this);
            }
        }

        private IEnumerator CopyBeforeDestroyedActions(GameAction action, CardController cc)
        {
            CardSource source = GetCardSource();
            cc.SetCardSourceLimitation(this, CardSource.Limitation.BeforeDestroyed);
            SetCardSourceLimitation(this, CardSource.Limitation.BeforeDestroyed);
            cc.AddAssociatedCardSource(source);
            IEnumerator coroutine = cc.PerformBeforeDestroyActions(action);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            cc.RemoveCardSourceLimitation(this);
            cc.RemoveAssociatedCardSource(source);
            RemoveCardSourceLimitation(this);
        }

        private IEnumerator CopyAfterDestroyedActions(GameAction action, CardController cc)
        {
            Console.Write("Grim Reflection copying AfterDestroyAction for " + cc.CardWithoutReplacements.Title + "\n");
            CardSource source = GetCardSource();
            cc.AddAssociatedCardSource(source);
            cc.SetCardSourceLimitation(this, CardSource.Limitation.AfterDestroyed);
            SetCardSourceLimitation(this, CardSource.Limitation.AfterDestroyed);
            DestroyCardAction destroy = ((action != null && action is DestroyCardAction) ? ((DestroyCardAction)action) : null);
            IEnumerator coroutine = cc.PerformAfterDestroyedActions(destroy);

            //Stitch in Time softlocks if its destroy action is copied, so instead just do what it should do manually
            //Turns out it caused a softlock anyway when they both triggered at the end of the environment turn.
            //if (cc.CardWithoutReplacements.Identifier == "StitchInTime")
            //{
            //    coroutine = TakeAFullTurnNow(this.TurnTakerController);
            //}
            if (cc.CardWithoutReplacements.Identifier == "StitchInTime")
            {
                coroutine = SendUhYeahMessage(cc.CardWithoutReplacements);
            }

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            cc.RemoveCardSourceLimitation(this);
            cc.RemoveAssociatedCardSource(source);
            RemoveCardSourceLimitation(this);
        }

        private IEnumerator AfterDestroyCleanup(GameAction action)
        {
            Console.WriteLine("Grim Reflection running AfterDestroyCleanup\n");
            //Console.WriteLine($"Triggers: {base.GameController.FindTriggersWhere((ITrigger t) => t.CardSource != null && t.CardSource.Card == this.Card).FirstOrDefault().ToString()}\n");
            //AddTrigger<PhaseChangeAction>((PhaseChangeAction pca) => pca.FromPhase.IsEnd && pca.FromPhase.EphemeralSource == Card, (PhaseChangeAction pca) => DebugStitchInTime(pca), TriggerType.Hidden, TriggerTiming.Before);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCards);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCardSource);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesTurnTakerController);
            RemoveThisCardControllerFromList(CardControllerListType.AddsPowers);
            RemoveThisCardControllerFromList(CardControllerListType.MakesIndestructible);
            yield return null;
        }

        //private IEnumerator DebugStitchInTime(PhaseChangeAction pca)
        //{
        //    Console.WriteLine($"FromPhase Ephemeral source: {(pca.FromPhase.EphemeralSource == null ? "null" : pca.FromPhase.EphemeralSource.Identifier)}");
        //    Console.WriteLine($"FromPhase TurnTaker: {pca.FromPhase.TurnTaker.Name}");
        //    Console.WriteLine($"ToPhase Ephemeral source: {(pca.ToPhase.EphemeralSource == null ? "null" : pca.ToPhase.EphemeralSource.Identifier)}");
        //    Console.WriteLine($"Triggers: {base.GameController.FindTriggersWhere((ITrigger t) => t.CardSource != null && t.CardSource.Card == this.Card).ToCommaList()}\n");
        //    Console.WriteLine($"Does meet criteria: {base.GameController.FindTriggersWhere((ITrigger t) => t.CardSource != null && t.CardSource.Card == this.Card).FirstOrDefault().DoesMatchTypeAndCriteria(pca)}\n");
        //    yield return null;
        //}

        private void AddOngoingCard(Card card)
        {
            Console.Write($"Adding {card.Title} to _ongoingsGR\n");
            //Log.Debug($"Adding {card.Title} to _ongoingsGR");
            _ongoingsGR.Add(card);
            if (_triggersForCard.ContainsKey(card.Identifier))
            {
                return;
            }
            IEnumerable<ITrigger> enumerable = FindTriggersWhere((ITrigger t) => t.CardSource.CardController.CardWithoutReplacements == card);
            _triggersForCard[card.Identifier] = new List<ITrigger>();
            foreach (ITrigger item in enumerable)
            {
                if (!item.IsStatusEffect)
                {
                    Console.Write($"Adding trigger to Grim Reflection: {item.ToString()}\n");
                    //Log.Debug($"Adding trigger: {item.ToString()}");
                    ITrigger trigger = (ITrigger)item.Clone();
                    trigger.CardSource = FindCardController(card).GetCardSource();
                    trigger.CardSource.AddAssociatedCardSource(GetCardSource());
                    trigger.SetCopyingCardController(this);
                    AddTrigger(trigger);
                    _triggersForCard[card.Identifier].Add(trigger);
                }
            }
            CardController cc = FindCardController(card);
            AddBeforeLeavesPlayActions((GameAction ga) => CopyBeforeDestroyedActions(ga, cc));
            CopyWhenDestroyedTriggers(cc);

            RemoveAfterDestroyedAction(AfterDestroyCleanup);
            AddAfterDestroyedAction((GameAction ga) => CopyAfterDestroyedActions(ga, cc));
            AddAfterDestroyedAction(AfterDestroyCleanup);
        }

        private void RemoveOngoingCard(Card card)
        {
            Console.Write($"Removing {card.Title} from _ongoingsGR\n");
            IEnumerable<ITrigger> enumerable = FindTriggersWhere((ITrigger t) => t.CardSource.Card == card && t.CardSource.AssociatedCardSources.Any((CardSource cs) => cs != null && cs.CardController == this));
            if (!_triggersForCard.ContainsKey(card.Identifier))
            {
                return;
            }
            if (_ongoingsGR.Where((Card c) => c.Identifier == card.Identifier && c != card).Count() >= 1)
            {
                CardController cardController = FindCardController(_ongoingsGR.Where((Card c) => c.Identifier == card.Identifier && c != card).First());
                foreach (ITrigger item in enumerable)
                {
                    item.CardSource = cardController.GetCardSource();
                }
            }
            else
            {
                enumerable.ForEach(delegate (ITrigger t)
                {
                    Console.Write($"Removing trigger from Grim Reflection: {t.ToString()}\n");
                    RemoveTrigger(t);
                });
                _triggersForCard[card.Identifier].Clear();
            }
            if (_ongoingsGR.Contains(card))
            {
                _ongoingsGR.Remove(card);
                if (!_ongoingsGR.Any((Card c) => c.Identifier == card.Identifier))
                {
                    _triggersForCard.Remove(card.Identifier);
                }
            }
        }

        private IEnumerator RemoveOngoingTriggersForTurnTaker(TurnTaker turnTaker)
        {
            foreach (Card item in from c in turnTaker.GetCardsAtLocation(turnTaker.PlayArea)
                                  where ShouldCardBeCopied(c)
                                  select c)
            {
                RemoveOngoingCard(item);
            }
            yield return null;
        }

        private IEnumerator AddOngoingTriggersForTurnTaker(TurnTaker turnTaker)
        {
            foreach (Card item in from c in turnTaker.GetCardsAtLocation(turnTaker.PlayArea)
                                  where ShouldCardBeCopied(c) && !_ongoingsGR.Contains(c)
                                  select c)
            {
                AddOngoingCard(item);
            }
            yield return null;
        }

        private IEnumerator RemoveOngoingTriggersForCard(Card card)
        {
            RemoveOngoingCard(card);
            yield return null;
        }

        private IEnumerator AddOngoingCardEnumerator(MoveCardAction mc, Card card)
        {
            //DebugResponse(mc);
            AddOngoingCard(card);
            yield return null;
        }

        public override Card AskIfCardIsReplaced(Card card, CardSource cardSource)
        {
            if (cardSource != null && cardSource.AllowReplacements && _ongoingsGR != null && cardSource.CardController == this && card != base.CardWithoutReplacements)
            {
                CardController cardController = (from cs in cardSource.AssociatedCardSources
                                                 select cs.CardController into cc
                                                 where _ongoingsGR.Contains(cc.CardWithoutReplacements)
                                                 select cc).FirstOrDefault();
                //Do not replace Focus of Power or Impending Doom with Grim Reflection, since that will cause Grim Reflection to trigger at the end of Friday's turn.
                //The rest of the cards' effects should work fine, replacing "OblivAeon" with "Friday"
                if (cardController != null)
                {
                    Card result = null;
                    if (cardController.CharacterCardsWithoutReplacements.Contains(card))
                    {
                        result = base.CharacterCard;
                    }
                    else if (cardController.CardWithoutReplacements == card && cardController.CardWithoutReplacements.Identifier != "FocusOfPower" && cardController.CardWithoutReplacements.Identifier != "ImpendingDoom")
                    {
                        result = base.CardWithoutReplacements;
                    }
                    return result;
                }
            }
            return null;
        }

        public override TurnTakerController AskIfTurnTakerControllerIsReplaced(TurnTakerController ttc, CardSource cardSource)
        {
            if (cardSource != null && cardSource.AllowReplacements && _ongoingsGR != null && cardSource.CardController == this)
            {
                Card card = (from cs in cardSource.AssociatedCardSources
                             select cs.Card into a
                             where _ongoingsGR.Contains(a)
                             select a).FirstOrDefault();
                //Exclude non-team non-OblivAeon villain cards from having their TurnTaker replaced so that things like "the villain trash" do not refer to Friday's trash
                if (card != null && ttc.TurnTaker == card.Owner && (!card.Owner.IsVillain || card.Owner.IsVillainTeam || card.Owner.Name == "OblivAeon"))
                {
                    return base.TurnTakerController;
                }
            }
            return null;
        }

        public override CardSource AskIfCardSourceIsReplaced(CardSource cardSource, GameAction gameAction = null, ITrigger trigger = null)
        {
            if (cardSource != null && cardSource.AllowReplacements && _ongoingsGR != null && ShouldSwapCardSources(cardSource, trigger))
            {
                CardSource cardSource2 = cardSource.AssociatedCardSources.Where((CardSource cs) => cs.CardController == this).LastOrDefault();
                if (!cardSource2.AssociatedCardSources.Any((CardSource cs) => cs.CardController == cardSource.CardController))
                {
                    cardSource2.AddAssociatedCardSource(cardSource);
                }
                cardSource.RemoveAssociatedCardSourcesWhere((CardSource cs) => cs.CardController == this);
                return cardSource2;
            }
            return null;
        }

        private bool ShouldSwapCardSources(CardSource cardSource, ITrigger trigger = null)
        {
            IEnumerable<CardSource> associatedCardSources = cardSource.AssociatedCardSources;
            CardSource cardSource2 = associatedCardSources.Where((CardSource cs) => cs != null && cs.CardController == this).LastOrDefault();
            if (cardSource2 != null)
            {
                bool num = _ongoingsGR.Contains(cardSource.CardController.CardWithoutReplacements);
                bool flag = associatedCardSources.Select((CardSource cs) => cs.CardController).Contains(this);
                if (num && flag)
                {
                    bool hasValue = cardSource.SourceLimitation.HasValue;
                    if (base.CardSourceLimitation == CardSource.Limitation.BeforeDestroyed)
                    {
                        cardSource2.SourceLimitation = CardSource.Limitation.BeforeDestroyed;
                    }
                    else if (base.CardSourceLimitation == CardSource.Limitation.AfterDestroyed)
                    {
                        cardSource2.SourceLimitation = CardSource.Limitation.AfterDestroyed;
                    }
                    bool hasValue2 = cardSource2.SourceLimitation.HasValue;
                    if (hasValue && hasValue2)
                    {
                        return cardSource.SourceLimitation.Value == cardSource2.SourceLimitation.Value;
                    }
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            if (cardController == this)
            {
                List<Power> list = new List<Power>();
                IEnumerable<Card> source = _ongoingsGR.Where((Card o) => o.HasPowers);
                int num = 0;
                for (int i = 0; i < source.Count(); i++)
                {
                    Card card = source.ElementAt(i);
                    for (int j = 0; j < card.NumberOfPowers; j++)
                    {
                        if (card.IsInPlayAndHasGameText && card.BattleZone == this.BattleZone)
                        {
                            CardController powerCC = FindCardController(card);
                            CardSource cardSource = GetCardSource();
                            cardSource.AddAssociatedCardSource(powerCC.GetCardSource());
                            string powerDescription = card.GetPowerDescription(j);
                            string oldValue = "{" + card.Owner.Identifier + "}";
                            powerDescription = powerDescription.Replace(oldValue, "{" + this.Card.Owner.Name + "}");
                            powerDescription = powerDescription.Replace(card.Title, this.Card.Title);
                            int powerIndex = j;
                            Power item = new Power(DecisionMaker, this, powerDescription, () => powerCC.UsePower(powerIndex), num, powerCC, cardSource);
                            list.Add(item);
                        }
                        num++;
                    }
                }
                return list;
            }
            else if (cardController.Card.IsHeroCharacterCard)
            {
                List<Power> list = new List<Power>();
                IEnumerable<Card> source = _ongoingsGR.Where((Card o) => base.GameController.IsInCardControllerList(o,CardControllerListType.AddsPowers));
                int num = 0;
                for (int i = 0; i < source.Count(); i++)
                {
                    Card card = source.ElementAt(i);
                    if (card.IsInPlayAndHasGameText && card.BattleZone == this.BattleZone)
                    {
                        CardController powerCC = FindCardController(card);
                        IEnumerable<Power> tempList = new Power[] { };
                        if (cardController.Card != this.CharacterCard && cardController.Card != powerCC.CharacterCardWithoutReplacements)
                        {
                            //If not asking which powers this contributes to Friday or the original character card, ask for the powers normally
                            tempList = powerCC.AskIfContributesPowersToCardController(cardController);
                        }
                        else if (cardController.Card == this.CharacterCard)
                        {
                            //If asking which powers this contributes to Friday, ask the copied card which powers it contributes to its character card
                            tempList = powerCC.AskIfContributesPowersToCardController(FindCardController(powerCC.CharacterCardWithoutReplacements));
                        }
                        else if (cardController.Card == powerCC.CharacterCardWithoutReplacements)
                        {
                            //If asking which powers this contributes to its own character, instead ask what it would contribute to Friday
                            tempList = powerCC.AskIfContributesPowersToCardController(FindCardController(this.CharacterCard));
                        }
                        if (tempList != null)
                        {
                            for (int j = 0; j < tempList.Count(); j++)
                            {
                                Power p = tempList.ElementAt(j);
                                CardSource cardSource = GetCardSource();
                                cardSource.AddAssociatedCardSource(powerCC.GetCardSource());
                                string powerDescription = p.Description;
                                string oldValue = "{" + card.Owner.Identifier + "}";
                                powerDescription = powerDescription.Replace(oldValue, "{" + this.Card.Owner.Name + "}");
                                powerDescription = powerDescription.Replace(card.Title, this.Card.Title);
                                powerDescription = powerDescription.Replace(card.Location.GetFriendlyName(), this.Card.Location.GetFriendlyName()); //This is only here so that Psychic Link's power displays correctly
                                Power item = new Power(cardController.HeroTurnTakerController, cardController, powerDescription, p.MethodCall, num, powerCC, cardSource);
                                list.Add(item);
                            }
                            num++;
                        }
                    }
                }
                return list;
            }
            return null;
        }

        public override IEnumerable<ActivatableAbility> GetActivatableAbilities(string abilityKey = null, TurnTakerController activatingTurnTaker = null)
        {
            List<ActivatableAbility> list = new List<ActivatableAbility>();
            IEnumerable<Card> source = _ongoingsGR.Where((Card o) => o.HasActivatableAbility(abilityKey));
            int num = 0;
            for (int i = 0; i < source.Count(); i++)
            {
                Card card = source.ElementAt(i);
                CardController abilityCC = FindCardController(card);
                foreach (ActivatableAbility ability in abilityCC.GetActivatableAbilities(abilityKey))
                {
                    if (card.IsInPlayAndHasGameText && card.BattleZone == this.BattleZone)
                    {
                        CardSource cardSource = GetCardSource();
                        cardSource.AddAssociatedCardSource(abilityCC.GetCardSource());
                        string description = ability.Description;
                        string oldValue = "{" + card.Owner.Identifier + "}";
                        description = description.Replace(oldValue, "{" + this.Card.Owner.Name + "}");
                        ActivatableAbility item = new ActivatableAbility(DecisionMaker, this, ability.Definition, () => abilityCC.ActivateAbilityEx(ability.Definition), num, abilityCC, activatingTurnTaker, cardSource, description);
                        list.Add(item);
                    }
                    num++;
                }
            }
            return list;
        }

        public override bool AskIfActionCanBePerformed(GameAction gameAction)
        {
            if (gameAction.CardSource != null && gameAction.CardSource.CardController == this && gameAction.CardSource.AssociatedCardSources != null && !gameAction.CardSource.IsForStatusEffect && !gameAction.CardSource.AssociatedCardSources.All((CardSource cs) => cs.CardController.CardWithoutReplacements.IsInPlayAndHasGameText))
            {
                return false;
            }
            return true;
        }

        public override void PrepareToUsePower(Power power)
        {
            base.PrepareToUsePower(power);
            if (ShouldAssociateThisCard(power))
            {
                _powerCardSources.Add(power, GetCardSource());
                power.CopiedFromCardController.AddAssociatedCardSource(_powerCardSources[power]);
            }
        }

        public override void FinishUsingPower(Power power)
        {
            base.FinishUsingPower(power);
            if (ShouldAssociateThisCard(power))
            {
                power.CopiedFromCardController.RemoveAssociatedCardSource(_powerCardSources[power]);
                _powerCardSources.Remove(power);
            }
        }

        public override void PrepareToUseAbility(ActivatableAbility ability)
        {
            base.PrepareToUseAbility(ability);
            if (ShouldAssociateThisCard(ability))
            {
                ability.CardController.OverrideAllowReplacements = true;
                ability.CopiedFromCardController.OverrideAllowReplacements = true;
                _abilityCardSources.Add(ability, GetCardSource());
                ability.CopiedFromCardController.AddAssociatedCardSource(_abilityCardSources[ability]);
            }
        }

        public override void FinishUsingAbility(ActivatableAbility ability)
        {
            base.FinishUsingAbility(ability);
            if (ShouldAssociateThisCard(ability))
            {
                ability.CopiedFromCardController.RemoveAssociatedCardSource(_abilityCardSources[ability]);
                _abilityCardSources.Remove(ability);
                ability.CardController.OverrideAllowReplacements = null;
                ability.CopiedFromCardController.OverrideAllowReplacements = null;
            }
        }

        private bool ShouldAssociateThisCard(Power power)
        {
            //Console.WriteLine("CardController: " + power.CardController.Card.Identifier);
            //Console.WriteLine("CardSource: " + power.CardSource.CardController.Card.Identifier);
            //Console.WriteLine("Power is contribution: " + power.IsContributionFromCardSource);
            //if (power.CopiedFromCardController == null)
            //{
            //    Console.WriteLine("Copied from CardController: null");
            //}
            //else
            //{
            //    Console.WriteLine("Copied from CardController: " + power.CopiedFromCardController.Card.Title);
            //}

            //Hopefully this change allow this card to copy power contributing ongoings correctly 
            //if (power.CardController == this && power.CardSource != null && power.CardSource.CardController == this)
            if ((power.CardController == this || power.IsContributionFromCardSource) && power.CardSource != null && power.CardSource.CardController == this)
            {
                return power.CopiedFromCardController != null;
            }
            return false;
        }

        private bool ShouldAssociateThisCard(ActivatableAbility ability)
        {
            if (ability.CardController == this && ability.CardSource != null && ability.CardSource.CardController == this)
            {
                return ability.CopiedFromCardController != null;
            }
            return false;
        }

        public override bool? AskIfActivatesEffect(TurnTakerController turnTakerController, string effectKey)
        {
            if (turnTakerController == base.TurnTakerController)
            {
                return _ongoingsGR.Any(delegate (Card c)
                {
                    if (c.IsInPlayAndHasGameText)
                    {
                        bool? flag = FindCardController(c).AskIfActivatesEffect(FindTurnTakerController(c.Owner), effectKey);
                        if (flag.HasValue)
                        {
                            return flag.Value;
                        }
                        return false;
                    }
                    return false;
                });
            }
            return null;
        }

        private IEnumerator ReturnCardsToOwnersTrashResponse(GameAction gameAction)
        {
            while (base.Card.UnderLocation.Cards.Count() > 0)
            {
                Card topCard = base.Card.UnderLocation.TopCard;
                MoveCardDestination trashDestination = FindCardController(topCard).GetTrashDestination();
                GameController gameController = base.GameController;
                TurnTakerController turnTakerController = base.TurnTakerController;
                Location location = trashDestination.Location;
                bool toBottom = trashDestination.ToBottom;
                CardSource cardSource = GetCardSource();
                IEnumerator coroutine = gameController.MoveCard(turnTakerController, topCard, location, toBottom, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, cardSource);
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

        private IEnumerator ReplaceMessage(MessageAction m, string replace)
        {
            String message = m.Message;
            Priority priority = m.Priority;
            CardSource cardsource = m.CardSource;

            IEnumerator coroutine = CancelAction(m);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SendMessageAction(message.Replace(replace, this.CharacterCard.Title), priority, cardsource );
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