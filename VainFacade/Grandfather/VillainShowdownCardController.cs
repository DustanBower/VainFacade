using Boomlagoon.JSON;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace VainFacadePlaytest.Grandfather
{
    public class VillainShowdownCardController : CardController
    {
        public VillainShowdownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible while it has 1 or more HP."
            if (card == base.Card)
            {
                return base.Card.HitPoints >= 1;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals the hero target with the highest HP {H + 1} melee damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, H + 1, DamageType.Melee);
            // After leaving play, reset nemesis identifiers
            AddAfterLeavesPlayAction (() => base.GameController.UpdateNemesisIdentifiers(this, new string[] { }, GetCardSource()));
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Select a hero to be " + base.Card.Title + "'s nemesis", "choosing a hero to be " + base.Card.Title + "'s nemesis", "Vote for a hero to be " + base.Card.Title + "'s nemesis", "hero to be " + base.Card.Title + "'s nemesis");
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, choose an active hero."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectHeroCharacterCard(DecisionMaker, SelectionType.Custom, choices, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidSelectCard(choices))
            {
                Card chosenHero = GetSelectedCard(choices);
                if (chosenHero != null && chosenHero.NemesisIdentifiers != null && chosenHero.NemesisIdentifiers.Count() > 0)
                {
                    // "This card counts as that hero's nemesis until it leaves play."
                    IEnumerator updateCoroutine = base.GameController.UpdateNemesisIdentifiers(this, chosenHero.NemesisIdentifiers, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(updateCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(updateCoroutine);
                    }
                }
            }
        }
    }
}
