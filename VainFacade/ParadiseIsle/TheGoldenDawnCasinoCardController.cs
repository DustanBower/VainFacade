using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.ParadiseIsle
{
    public class TheGoldenDawnCasinoCardController : AreaCardController
    {
        public TheGoldenDawnCasinoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, 1 player chooses a number between 1 and 4, selects a keyword, and discards the top card of their deck. If that card has the selected keyword, that hero draws X cards, otherwise they deal themselves X psychic damage, where X = the chosen number."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(base.GameController, base.DecisionMaker, base.GameController.AllHeroControllers.Select((HeroTurnTakerController httc) => httc.TurnTaker).Where((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame), SelectionType.TurnTaker, cardSource: GetCardSource()), GambleResponse), new TriggerType[] { TriggerType.DiscardCard, TriggerType.DrawCard, TriggerType.DealDamage });
        }

        private IEnumerator GambleResponse(TurnTaker tt)
        {
            if (IsHero(tt))
            {
                // "... chooses a number between 1 and 4, ..."
                List<SelectNumberDecision> numberChoices = new List<SelectNumberDecision>();
                IEnumerator numberCoroutine = base.GameController.SelectNumber(base.GameController.FindHeroTurnTakerController(tt.ToHero()), SelectionType.SelectNumeral, 1, 4, storedResults: numberChoices, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(numberCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(numberCoroutine);
                }
                SelectNumberDecision numberDecision = numberChoices.FirstOrDefault();
                if (numberDecision != null && numberDecision.SelectedNumber.HasValue)
                {
                    int x = numberDecision.SelectedNumber.Value;
                    // "... selects a keyword, ..."
                    List<SelectWordDecision> wordChoices = new List<SelectWordDecision>();
                    IOrderedEnumerable<string> keywords = from s in tt.Deck.Cards.SelectMany((Card c) => base.GameController.GetAllKeywords(c)).Distinct().Where((string s) => s.ToLower() != "one-shot") orderby s select s;
                    keywords = keywords.Concat("Another keyword - always fails to match".ToEnumerable()).OrderBy((string s) => s);
                    IEnumerator wordCoroutine = base.GameController.SelectWord(base.GameController.FindHeroTurnTakerController(tt.ToHero()), keywords, SelectionType.SelectKeyword, wordChoices, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(wordCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(wordCoroutine);
                    }
                    if (DidSelectWord(wordChoices))
                    {
                        string chosenWord = GetSelectedWord(wordChoices);
                        // "... and discards the top card of their deck."
                        List<MoveCardAction> discardResults = new List<MoveCardAction>();
                        IEnumerator discardCoroutine = DiscardCardsFromTopOfDeck(base.GameController.FindTurnTakerController(tt), 1, storedResults: discardResults, showMessage: true);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(discardCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(discardCoroutine);
                        }
                        if (DidMoveCard(discardResults))
                        {
                            Card discarded = discardResults.FirstOrDefault().CardToMove;
                            if (discarded.DoKeywordsContain(chosenWord))
                            {
                                // "If that card has the selected keyword, that hero draws X cards, ..."
                                IEnumerator drawCoroutine = DrawCards(base.GameController.FindHeroTurnTakerController(tt.ToHero()), x);
                                if (base.UseUnityCoroutines)
                                {
                                    yield return base.GameController.StartCoroutine(drawCoroutine);
                                }
                                else
                                {
                                    base.GameController.ExhaustCoroutine(drawCoroutine);
                                }
                            }
                            else
                            {
                                // "... otherwise they deal themselves X psychic damage, where X = the chosen number."
                                IEnumerator damageCoroutine = base.GameController.SelectTargetsToDealDamageToSelf(base.GameController.FindHeroTurnTakerController(tt.ToHero()), x, DamageType.Psychic, 1, false, 1, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Owner == tt, selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
                                if (base.UseUnityCoroutines)
                                {
                                    yield return base.GameController.StartCoroutine(damageCoroutine);
                                }
                                else
                                {
                                    base.GameController.ExhaustCoroutine(damageCoroutine);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
