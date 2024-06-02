using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VainFacadePlaytest.Friday
{
	public class PrototypeCombatMimicCardController:CardController
	{
		public PrototypeCombatMimicCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisRound(OneTimePerRoundKey), () => $"{this.Card.Title} has been used this round", () => $"{this.Card.Title} has not been used this round").Condition = () => this.Card.IsInPlayAndHasGameText;
        }

        private Power _powerChosen;

        private ITrigger _makeDecisionTrigger;

        private ITrigger _replaceGuiseMessageTrigger;

        private Dictionary<Power, CardSource> _cardSources;

        private string OneTimePerRoundKey = "OneTimePerRoundKey";

        public override void AddTriggers()
		{
            //Once per round, at the end of another hero's turn, {Friday} may deal that hero 2 lightning damage.
            //When a hero other than {Friday} is dealt damage this way, {Friday} may use a power in that hero's play area, replacing the name of that hero on that card with {Friday}. “You” on that card refers to that hero's player.
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsPlayer && tt != this.TurnTaker && !HasBeenSetToTrueThisRound(OneTimePerRoundKey) && !tt.IsIncapacitatedOrOutOfGame, EndOfTurnResponse, new TriggerType[2] { TriggerType.DealDamage, TriggerType.UsePower });
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction pca)
        {
            TurnTaker tt = pca.ToPhase.TurnTaker;
            HeroTurnTaker hero = tt.ToHero();
            HeroTurnTakerController httc = FindHeroTurnTakerController(hero);

            List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
            IEnumerator coroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.DealDamage, this.Card, new DealDamageAction(base.GameController, new DamageSource(base.GameController,this.CharacterCard), tt.CharacterCard, 2, DamageType.Lightning), storedResults, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            if (DidPlayerAnswerYes(storedResults))
            {
                SetCardPropertyToTrueIfRealAction(OneTimePerRoundKey);

                List<Card> characterResults = new List<Card>();
                coroutine = FindCharacterCardToTakeDamage(tt, characterResults, this.CharacterCard, 2, DamageType.Lightning);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                Card target = characterResults.FirstOrDefault();
                if (target != null)
                {
                    List<DealDamageAction> results = new List<DealDamageAction>();
                    coroutine = DealDamage(this.CharacterCard, target, 2, DamageType.Lightning, false, false, storedResults: results, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }

                    if (DidDealDamage(results))
                    {
                        Card damagedCard = results.FirstOrDefault().Target;

                        if (IsHeroCharacterCard(damagedCard) && damagedCard != this.CharacterCard)
                        {
                            TurnTaker damagedTT = damagedCard.Owner;

                            //This code copied from I Can Do That Too
                            AddThisCardControllerToList(CardControllerListType.ReplacesCards);
                            AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);
                            AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
                            _cardSources = new Dictionary<Power, CardSource>();
                            _makeDecisionTrigger = AddTrigger((MakeDecisionAction d) => d.Decision is UsePowerDecision && d.Decision.CardSource.CardController == this, PowerChosenResponse, new TriggerType[1]
                            {
                            TriggerType.Hidden
                            }, TriggerTiming.After);

                            if (damagedCard.PromoIdentifierOrIdentifier == "CompletionistGuiseCharacter")
                            {
                                _replaceGuiseMessageTrigger = AddTrigger<MessageAction>((MessageAction m) => m.CardSource != null && m.CardSource.Card.PromoIdentifierOrIdentifier == "CompletionistGuiseCharacter", ReplaceGuiseMessage, new TriggerType[2] { TriggerType.CancelAction, TriggerType.Hidden }, TriggerTiming.Before);
                            }

                            coroutine = base.GameController.SelectAndUsePower(DecisionMaker, true, (Power p) => p.CardSource != null && p.CardSource.Card.Location.HighestRecursiveLocation == damagedTT.PlayArea && p.CardController.Card.Location.HighestRecursiveLocation == damagedTT.PlayArea, allowAnyHeroPower: true, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(coroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(coroutine);
                            }

                            RemoveTrigger(_makeDecisionTrigger);
                            if (damagedCard.Identifier == "CompletionistGuiseCharacter")
                            {
                                RemoveTrigger(_replaceGuiseMessageTrigger);
                            }

                            if (_powerChosen != null && _powerChosen.CardSource != null)
                            {
                                _powerChosen.CardSource.CardController.OverrideAllowReplacements = null;
                            }
                            _powerChosen = null;
                            _cardSources = null;
                            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCards);
                            RemoveThisCardControllerFromList(CardControllerListType.ReplacesTurnTakerController);
                            RemoveThisCardControllerFromList(CardControllerListType.ReplacesCardSource);
                        }
                    }
                }
            }
        }

        private IEnumerator PowerChosenResponse(MakeDecisionAction d)
        {
            if (d != null)
            {
                _powerChosen = (d.Decision as UsePowerDecision).SelectedPower;
                if (_powerChosen != null)
                {
                    _powerChosen.TurnTakerController = base.TurnTakerController;
                    if (_powerChosen.CardSource != null)
                    {
                        _powerChosen.CardSource.AddAssociatedCardSource(GetCardSource());
                        _powerChosen.CardSource.CardController.OverrideAllowReplacements = true;
                    }
                }
            }
            yield return null;
        }

        public override Card AskIfCardIsReplaced(Card card, CardSource cardSource)
        {
            if (_powerChosen != null && card.IsHeroCharacterCard && cardSource.AllowReplacements)
            {
                CardController cardController = _powerChosen.CardController;
                IEnumerable<CardController> source = cardSource.CardSourceChain.Select((CardSource cs) => cs.CardController);
                bool flag = _powerChosen == cardSource.PowerSource || (cardSource.PowerSource == null && _powerChosen.CardController.CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements);
                if (source.Contains(cardController) && source.Contains(this) && flag && card == cardSource.CardController.CardWithoutReplacements)
                {
                    return base.CharacterCard;
                }
            }
            return null;
        }

        public override TurnTakerController AskIfTurnTakerControllerIsReplaced(TurnTakerController ttc, CardSource cardSource)
        {
            if (_powerChosen != null && cardSource.AllowReplacements)
            {
                Card cardWithoutReplacements = _powerChosen.CardController.CardWithoutReplacements;
                TurnTakerController turnTakerControllerWithoutReplacements = _powerChosen.CardController.TurnTakerControllerWithoutReplacements;
                if (ttc == turnTakerControllerWithoutReplacements && (cardSource.CardController.CardWithoutReplacements == cardWithoutReplacements || cardSource.CardSourceChain.Any((CardSource cs) => cs.CardController == this)))
                {
                    return base.TurnTakerControllerWithoutReplacements;
                }
            }
            return null;
        }

        public override CardSource AskIfCardSourceIsReplaced(CardSource cardSource, GameAction gameAction = null, ITrigger trigger = null)
        {
            if (_powerChosen != null && cardSource.AllowReplacements && _powerChosen.CardSource.CardController.CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements)
            {
                cardSource.AddAssociatedCardSource(GetCardSource());
                return cardSource;
            }
            return null;
        }

        public override void PrepareToUsePower(Power power)
        {
            base.PrepareToUsePower(power);
            if (ShouldAssociateThisCard(power))
            {
                _cardSources.Add(power, GetCardSource());
                power.CardController.AddAssociatedCardSource(_cardSources[power]);
            }
        }

        public override void FinishUsingPower(Power power)
        {
            base.FinishUsingPower(power);
            if (ShouldAssociateThisCard(power))
            {
                power.CardController.RemoveAssociatedCardSource(_cardSources[power]);
                _cardSources.Remove(power);
            }
        }

        private bool ShouldAssociateThisCard(Power power)
        {
            if (_powerChosen.CardController == power.CardController)
            {
                return power.CardSource != null;
            }
            return false;
        }

        private IEnumerator ReplaceGuiseMessage(MessageAction m)
        {
            string message = m.Message;
            Log.Debug("Replacing Guise message");
            IEnumerator coroutine = CancelAction(m, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            coroutine = base.GameController.SendMessageAction("[i][Compiling joke]... [/i]" + m.Message, Priority.Low, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (decision.CardSource != null)
            {
                CardSource source = decision.CardSource.AssociatedCardSources.FirstOrDefault();
                if (source != null)
                {
                    Console.WriteLine("Prototype Combat Mimic requesting custom decision text from " + source.Card.Title);
                    return source.CardController.GetCustomDecisionText(decision);
                }
            }
            return null;
        }
    }
}