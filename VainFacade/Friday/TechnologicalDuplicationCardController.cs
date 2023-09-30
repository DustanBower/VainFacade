using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Reflection;

namespace VainFacadePlaytest.Friday
{
	public class TechnologicalDuplicationCardController:CardController
	{
        //Known issues with this card:
        //--This card will not work properly with equipment cards that go next to a hero and grant them a power.
        //  A special exception is included for Micro Assembler, but any other card like that will also grant its power to Friday when copied

        public TechnologicalDuplicationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.AllowAddingTriggersBeforeEnteringPlay = false;
            _powerCardSources = new Dictionary<Power, CardSource>();
            _abilityCardSources = new Dictionary<ActivatableAbility, CardSource>();
            _triggersForCard = new Dictionary<string, List<ITrigger>>();
            AddThisCardControllerToList(CardControllerListType.ActivatesEffects);
        }

        private Dictionary<string, List<ITrigger>> _triggersForCard;

        private SelfDestructTrigger _removeCardSourceWhenDestroyedTrigger;

        private Dictionary<Power, CardSource> _powerCardSources;

        private Dictionary<ActivatableAbility, CardSource> _abilityCardSources;

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            IEnumerator coroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsRealCard && c.IsInPlayAndHasGameText && IsEquipment(c) && c.Owner != this.Card.Owner, "equipment"), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override IEnumerator Play()
        {
            //Play this card next to an equipment card from another deck without a copy of this card beside it. This card gains the text of that card. Treat the name of the hero on that card as {Friday}, and “you” on that card as {Friday}'s player.
            if (GetCardThisCardIsNextTo(allowReplacements: false) == null)
            {
                yield break;
            }

            //_powerCardSources = new Dictionary<Power, CardSource>();
            //_abilityCardSources = new Dictionary<ActivatableAbility, CardSource>();
            //_triggersForCard = new Dictionary<string, List<ITrigger>>();
            AddLastTriggers();
            CardController cardController = FindCardController(GetCardThisCardIsNextTo(allowReplacements: false));
            if (!cardController.DoesHaveActivePlayMethod)
            {
                IEnumerator coroutine = cardController.Play();
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

        public override void AddLastTriggers()
        {
            //Increase lightning damage dealt to {Friday} by 1.
            AddIncreaseDamageTrigger((DealDamageAction dd) => dd.Target == this.CharacterCard && dd.DamageType == DamageType.Lightning, (DealDamageAction dd) => 1);

            //When that card leaves play, destroy this card.
            AddTrigger((BulkMoveCardsAction m) => m.CardsToMove.Any((Card c) => IsThisCardNextToCard(c)) && !m.Destination.IsInPlayAndNotUnderCard, (BulkMoveCardsAction bmc) => DestroyThisCardResponse(bmc), TriggerType.DestroySelf, TriggerTiming.After);
            AddTrigger((MoveCardAction d) => IsThisCardNextToCard(d.CardToMove) && !d.CardToMove.Location.IsInPlayAndNotUnderCard, (MoveCardAction m) => DestroyThisCardResponse(m), TriggerType.DestroySelf, TriggerTiming.After);

            if (GetCardThisCardIsNextTo(false) != null)
            {
                AddThisCardControllerToList(CardControllerListType.ReplacesCards);
                AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
                AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);

                //Log.Debug("Adding to AddsPowers list");
                AddThisCardControllerToList(CardControllerListType.AddsPowers);
                AddEquipmentCard(GetCardThisCardIsNextTo(false));
                AddWhenDestroyedTrigger(ReplaceCardSourceWhenDestroyed, TriggerType.Hidden);
                _removeCardSourceWhenDestroyedTrigger = AddWhenDestroyedTrigger(RemoveCardSourceWhenDestroyed, TriggerType.HiddenLast);
                RemoveAfterDestroyedAction(AfterDestroyCleanup);
                AddAfterDestroyedAction(AfterDestroyCleanup);
                AddBeforeLeavesPlayActions(ReturnCardsToOwnersTrashResponse);
            }
        }

        private IEnumerator ReplaceCardSourceWhenDestroyed(DestroyCardAction dc)
        {
            Card equipment = GetCardThisCardIsNextTo(false);
            if (equipment != null)
            {
                CardController cardController = FindCardController(equipment);
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
            Card equipment = GetCardThisCardIsNextTo(false);
            if (equipment != null)
            {
                CardController cardController = FindCardController(equipment);
                if (cardController.HasWhenDestroyedTriggers)
                {
                    CardSource cardSource = GetCardSource();
                    cardSource.SourceLimitation = CardSource.Limitation.WhenDestroyed;
                    cardController.RemoveAssociatedCardSource(cardSource);
                }
            }
            yield return null;
        }

        private void AddEquipmentCard(Card card)
        {
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

        //private void RemoveEquipmentCard(Card card)
        //{
        //    IEnumerable<ITrigger> enumerable = FindTriggersWhere((ITrigger t) => t.CardSource.Card == card && t.CardSource.AssociatedCardSources.Any((CardSource cs) => cs != null && cs.CardController == this));
        //    if (!_triggersForCard.ContainsKey(card.Identifier))
        //    {
        //        return;
        //    }
        //    enumerable.ForEach(delegate (ITrigger t)
        //    {
        //        RemoveTrigger(t);
        //    });
        //    _triggersForCard[card.Identifier].Clear();
            
        //    _triggersForCard.Remove(card.Identifier);
                
        //}

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
            CardSource source = GetCardSource();
            cc.AddAssociatedCardSource(source);
            cc.SetCardSourceLimitation(this, CardSource.Limitation.AfterDestroyed);
            SetCardSourceLimitation(this, CardSource.Limitation.AfterDestroyed);
            DestroyCardAction destroy = ((action != null && action is DestroyCardAction) ? ((DestroyCardAction)action) : null);
            IEnumerator coroutine = cc.PerformAfterDestroyedActions(destroy);
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
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCards);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCardSource);
            RemoveThisCardControllerFromList(CardControllerListType.ReplacesTurnTakerController);
            RemoveThisCardControllerFromList(CardControllerListType.AddsPowers);
            yield return null;
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

            //Hopefully this change allow this card to copy power contributing equipments correctly 
            if ((power.CardController == FindCardController(this.CharacterCard) || power.IsContributionFromCardSource) && power.CardSource != null && power.CardSource.CardController == this)
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
                    if (GetCardThisCardIsNextTo(false) != null)
                    {
                        bool? flag = FindCardController(GetCardThisCardIsNextTo(false)).AskIfActivatesEffect(FindTurnTakerController(GetCardThisCardIsNextTo(false).Owner), effectKey);
                        if (flag.HasValue)
                        {
                            return flag.Value;
                        }
                        return false;
                    }
                    return false;
            }
            return null;
        }

        public override Card AskIfCardIsReplaced(Card card, CardSource cardSource)
        {
            if (cardSource != null && cardSource.AllowReplacements && GetCardThisCardIsNextTo(false) != null && cardSource.CardController == this && card != base.CardWithoutReplacements)
            {
                CardController cardController = (from cs in cardSource.AssociatedCardSources
                                                 select cs.CardController into cc
                                                 where GetCardThisCardIsNextTo(false) == cc.CardWithoutReplacements
                                                 select cc).FirstOrDefault();
                if (cardController != null)
                {
                    Card result = null;
                    if (cardController.CharacterCardsWithoutReplacements.Contains(card))
                    {
                        result = base.CharacterCard;
                    }
                    else if (cardController.CardWithoutReplacements == card)
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
            if (cardSource != null && cardSource.AllowReplacements && GetCardThisCardIsNextTo(false) != null && cardSource.CardController == this)
            {
                Card card = (from cs in cardSource.AssociatedCardSources
                             select cs.Card into a
                             where GetCardThisCardIsNextTo(false) == a
                             select a).FirstOrDefault();
                if (card != null && ttc.TurnTaker == card.Owner)
                {
                    return base.TurnTakerController;
                }
            }
            return null;
        }

        public override CardSource AskIfCardSourceIsReplaced(CardSource cardSource, GameAction gameAction = null, ITrigger trigger = null)
        {
            if (cardSource != null && cardSource.AllowReplacements && GetCardThisCardIsNextTo(false) != null && ShouldSwapCardSources(cardSource, trigger))
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
                bool num = GetCardThisCardIsNextTo(false) == cardSource.CardController.CardWithoutReplacements;
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
            List<Power> list = new List<Power>();
            if (cardController == FindCardController(this.CharacterCard) && GetCardThisCardIsNextTo(false) != null)
            {
                //Log.Debug("Granting powers from " + GetCardThisCardIsNextTo(false).Title);
                int num = 0;
                if (GetCardThisCardIsNextTo(false).HasPowers)
                {
                    Card card = GetCardThisCardIsNextTo(false);
                    for (int j = 0; j < card.NumberOfPowers; j++)
                    {
                        if (card.IsInPlayAndHasGameText)
                        {
                            CardController powerCC = FindCardController(card);
                            CardSource cardSource = GetCardSource();
                            cardSource.AddAssociatedCardSource(powerCC.GetCardSource());
                            string powerDescription = card.GetPowerDescription(j);
                            string oldValue = "{" + card.Owner.Identifier + "}";
                            powerDescription = powerDescription.Replace(oldValue, "{" + this.TurnTaker.Name + "}");
                            powerDescription = powerDescription.Replace(card.Title, this.Card.Title);
                            int powerIndex = j;
                            Power item = new Power(DecisionMaker, cardController, powerDescription, () => powerCC.UsePower(powerIndex), num, powerCC, cardSource);
                            list.Add(item);
                        }
                        num++;
                    }
                }
                //Log.Debug($"Contributing powers to {cardController.Card.Identifier}: {list.ToCommaList()}");
                //return list;
            }

            if (cardController.Card.IsHeroCharacterCard && GetCardThisCardIsNextTo(false) != null)
            {
                int num = 0;
                Card card = GetCardThisCardIsNextTo(false);

                if (base.GameController.IsInCardControllerList(card, CardControllerListType.AddsPowers))
                {
                    //Does not work for Micro Assembler, since it will return true for whichever hero MA is next to and grant the power to Friday when it should not
                    if (card.Identifier != "MicroAssembler")
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
                                Power item = new Power(cardController.HeroTurnTakerController, cardController, powerDescription, p.MethodCall, num, powerCC, cardSource);
                                list.Add(item);
                            }
                            num++;
                        }
                    }
                }
                
                //return list;
            }

            if (list.Count() > 0)
            {
                return list;
            }
            return null;
        }

        public override IEnumerable<ActivatableAbility> GetActivatableAbilities(string abilityKey = null, TurnTakerController activatingTurnTaker = null)
        {
            List<ActivatableAbility> list = new List<ActivatableAbility>();
            int num = 0;
            if (GetCardThisCardIsNextTo(false).HasActivatableAbility(abilityKey))
            {
                Card card = GetCardThisCardIsNextTo(false);
                CardController abilityCC = FindCardController(card);
                foreach (ActivatableAbility ability in abilityCC.GetActivatableAbilities(abilityKey))
                {
                    if (card.IsInPlayAndHasGameText)
                    {
                        CardSource cardSource = GetCardSource();
                        cardSource.AddAssociatedCardSource(abilityCC.GetCardSource());
                        string description = ability.Description;
                        string oldValue = "{" + card.Owner.Identifier + "}";
                        description = description.Replace(oldValue, "{" + this.TurnTaker.Name +"}");
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
    }
}

