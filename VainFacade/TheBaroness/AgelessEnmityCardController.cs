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
    public class AgelessEnmityCardController : BaronessUtilityCardController
    {
        public AgelessEnmityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1);
            // Show list of Scheme cards in play
            SpecialStringMaker.ShowListOfCardsInPlay(SchemeCard());
            // Show number of villain Scheme cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(SchemeCard(), owners: base.GameController.FindTurnTakersWhere((TurnTaker tt) => IsVillain(tt)));
        }

        protected LinqCardCriteria SchemeCard()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain(SchemeKeyword), "Scheme");
        }

        public override IEnumerator Play()
        {
            // "{TheBaroness} deals the hero target with the highest HP 5 melee damage."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => IsHeroTarget(c), (Card c) => 5, DamageType.Melee, numberOfTargets: () => 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Play the top card of the villain deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "Destroy a Scheme."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, SchemeCard(), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If there are {H - 2} or fewer villain Schemes in play, discard the top 5 cards of the villain deck. Put any Schemes discarded this way into play."
            if (base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsVillain(c) && c.DoKeywordsContain(SchemeKeyword), "villain Scheme"), visibleToCard: GetCardSource()).Count() <= H - 2)
            {
                List<MoveCardAction> results = new List<MoveCardAction>();
                IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.TurnTakerController, 5, storedResults: results, responsibleTurnTaker: base.TurnTaker);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                foreach (MoveCardAction mca in results)
                {
                    if (mca.CardToMove.DoKeywordsContain(SchemeKeyword))
                    {
                        IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, mca.CardToMove, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(putCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(putCoroutine);
                        }
                    }
                }
            }
        }
    }
}
