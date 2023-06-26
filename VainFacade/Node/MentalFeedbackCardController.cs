using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Node
{
    public class MentalFeedbackCardController : NodeUtilityCardController
    {
        public MentalFeedbackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show Connected Ongoing cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsOngoing(c) && IsConnected(c), "Connected Ongoing"));
        }

        public override IEnumerator Play()
        {
            // "Destroy 1 [i]Connected[/i] Ongoing."
            List<DestroyCardAction> chosen = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) && IsConnected(c), "Connected Ongoing"), false, chosen, base.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "{NodeCharacter} deals the character from that deck with the highest HP 0 psychic damage and 0 psychic damage."
            //Log.Debug("MentalFeedbackCardController.Play: creating destroyChoice");
            DestroyCardAction destroyChoice = chosen.Where((DestroyCardAction dca) => dca.CardToDestroy != null).FirstOrDefault();
            //Log.Debug("MentalFeedbackCardController.Play: checking whether destroyChoice == null");
            if (destroyChoice != null)
            {
                //Log.Debug("MentalFeedbackCardController.Play: destroyChoice != null, creating chosenOngoing");
                Card chosenOngoing = destroyChoice.CardToDestroy.Card;
                //Log.Debug("MentalFeedbackCardController.Play: checking whether chosenOngoing == null");
                if (chosenOngoing != null)
                {
                    //Log.Debug("MentalFeedbackCardController.Play: chosenOngoing != null, selecting target and dealing damage");
                    List<DealDamageAction> instances = new List<DealDamageAction>();
                    DealDamageAction zeroPsychic = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 0, DamageType.Psychic);
                    instances.Add(zeroPsychic);
                    instances.Add(zeroPsychic);
                    IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => c.ParentDeck == chosenOngoing.ParentDeck, HighestLowestHP.HighestHP);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
                else
                {
                    //Log.Debug("MentalFeedbackCardController.Play: chosenOngoing == null, sending message");
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("No Connected Ongoing card was selected, so " + base.CharacterCard.Title + " does not deal damage.", Priority.Medium, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                }
            }
            else
            {
                //Log.Debug("MentalFeedbackCardController.Play: destroyChoice == null, sending message");
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("No Connected Ongoing card was selected, so " + base.CharacterCard.Title + " does not deal damage.", Priority.Medium, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }
    }
}
