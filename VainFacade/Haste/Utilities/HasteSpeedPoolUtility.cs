using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra;

namespace VainFacadePlaytest.Haste
{
    public static class HasteSpeedPoolUtility
    {
        //This class is loosely based on the TerminusWrathPoolUtility class  
        public static TokenPool GetSpeedPool(CardController cardController)
        {
            //Check Character Card first
            if (cardController.CharacterCard != null)
            {
                var prospect1 = cardController.CharacterCard.FindTokenPool("SpeedPool");
                if (prospect1 != null)
                {
                    return prospect1;
                }
            }

            //If not, look for a "Haste" TurnTaker and get their character card
            var haste = cardController.GameController.Game.HeroTurnTakers.Where(htt => htt.Identifier == "Haste").FirstOrDefault();
            if (haste != null)
            {
                return haste.CharacterCard.FindTokenPool("SpeedPool");
            }

            //If not there, look for a card with the identifier "HasteCharacter" (for Representative of Earth purposes)
            var hasteChar = cardController.GameController.FindCardsWhere((Card c) => c.IsCharacter && !c.IsIncapacitatedOrOutOfGame && c.IsInPlayAndHasGameText && c.Identifier == "HasteCharacter", visibleToCard: new CardSource(cardController)).FirstOrDefault();
            Console.WriteLine($"hasteChar is {(hasteChar == null ? "null" : hasteChar.Title)}");
            if (hasteChar != null)
            {
                return hasteChar.FindTokenPool("SpeedPool");
            }

            //If not there, try the card itself (for Representative of Earth purposes)
            var prospect2 = cardController.CardWithoutReplacements.FindTokenPool("SpeedPool");
            if (prospect2 != null)
            {
                return prospect2;
            }

            Console.WriteLine($"{(cardController == null ? "null CardController" : cardController.Card.Title)} failed to find speed pool");
            //If not, we have failed to find it - error handle!
            return null;
        }

        public static string KillingTimeKey = "KillingTimeKey";
        public static string UnmatchedAlacrityKey = "UnmatchedAlacrityKey";

        public static IEnumerator AddSpeedTokens(CardController cardController, int amountToAdd, CardSource cardSource = null)
        {
            IEnumerator coroutine;

            if (GetSpeedPool(cardController) == null)
            {
                coroutine = SpeedPoolErrorMessage(cardController);
                if (cardController.UseUnityCoroutines)
                {
                    yield return cardController.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    cardController.GameController.ExhaustCoroutine(coroutine);
                }
                yield break;
            }

            amountToAdd += GetNumberOfCardPropertiesTrue(cardController,KillingTimeKey);
            amountToAdd -= 2*GetNumberOfCardPropertiesTrue(cardController, UnmatchedAlacrityKey);


            if (amountToAdd > 0)
            {
                if (cardSource == null)
                {
                    cardSource = cardController.GetCardSource();
                }
                coroutine = cardController.GameController.AddTokensToPool(GetSpeedPool(cardController), amountToAdd, cardSource);
                if (cardController.UseUnityCoroutines)
                {
                    yield return cardController.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    cardController.GameController.ExhaustCoroutine(coroutine);
                }
            }
            yield break;
        }

        public static int GetNumberOfCardPropertiesTrue(CardController cardController,string key)
        {
            IEnumerable<CardPropertiesJournalEntry> source = from e in cardController.GameController.Game.Journal.CardPropertiesEntries((CardPropertiesJournalEntry e) => e.Key == key) where e.Key == key select e;
            IEnumerable<Card> sourceCards = source.Where((CardPropertiesJournalEntry e) => e.BoolValue.HasValue).Select((CardPropertiesJournalEntry e) => e.Card).Distinct();
            int num = 0;
            foreach (Card card in sourceCards)
            {
                if (cardController.GameController.IsCardVisibleToCardSource(card, cardController.GetCardSource()) && !(IsAlone(card) ^ IsAlone(cardController.Card)))
                {
                    CardPropertiesJournalEntry entry = source.Where((CardPropertiesJournalEntry e) => e.Card == card).LastOrDefault();
                    if (entry != null && entry.BoolValue.Value)
                    {
                        num++;
                        Console.WriteLine($"{entry.Card.Title} {(key == KillingTimeKey ? "in" : "de")}creases token addition by {(key == KillingTimeKey ? 1 : 2)}");
                    }
                }
            }
            return num;
        }

        public static bool IsAlone(Card card)
        {
            if (card.Owner.CharacterCard != null)
            {
                return card.Owner.CharacterCard.NextToLocation.Cards.Any((Card c) => c.Identifier == "YouAreAlone");
            }
            else
            {
                return card.Owner.CharacterCards.Any((Card cc) => cc.NextToLocation.Cards.Any((Card c) => c.Identifier == "YouAreAlone"));
            }
        }

        public static IEnumerator RemoveSpeedTokens(CardController cardController, int amount, GameAction gameAction, bool optional = false, List<RemoveTokensFromPoolAction> storedResults = null, IEnumerable<Card> associatedCards = null, CardSource cardSource = null)
        {
            IEnumerator coroutine;
            if (GetSpeedPool(cardController) == null)
            {
                coroutine = SpeedPoolErrorMessage(cardController);
                if (cardController.UseUnityCoroutines)
                {
                    yield return cardController.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    cardController.GameController.ExhaustCoroutine(coroutine);
                }
                yield break;
            }

            if (cardSource == null)
            {
                cardSource = cardController.GetCardSource();
            }
            coroutine = RemoveTokensFromPoolCustom(cardController, GetSpeedPool(cardController), amount, storedResults, optional, gameAction, cardSource, associatedCards);
            if (cardController.UseUnityCoroutines)
            {
                yield return cardController.GameController.StartCoroutine(coroutine);
            }
            else
            {
                cardController.GameController.ExhaustCoroutine(coroutine);
            }
        }

        public static IEnumerator SpeedPoolErrorMessage(CardController cardController)
        {
            return cardController.GameController.SendMessageAction("No appropriate Speed Pool could be found.", Priority.High, cardController.GetCardSource());
        }

        private static IEnumerator RemoveTokensFromPoolCustom(CardController cardController, TokenPool pool, int numberOfTokens, List<RemoveTokensFromPoolAction> storedResults = null, bool optional = false, GameAction gameAction = null, CardSource cardSource = null, IEnumerable<Card> associatedCards = null)
        {
            bool proceed = true;
            GameController gc = cardController.GameController;
            if (optional)
            {
                proceed = false;
                SelectionType type = SelectionType.RemoveTokens;
                if (cardController.DecisionMaker != null && (cardController.DecisionMaker.Name.Contains("Friday") || cardController.DecisionMaker.Name.Contains("Haste")))
                {
                    //Haste and Friday should have the custom decision text handling for Haste, but Guise does not.
                    type = SelectionType.Custom;
                }
                if (pool.CurrentValue < numberOfTokens)
                {
                    type = SelectionType.RemoveTokensToNoEffect;
                }
                YesNoAmountDecision yesNo = new YesNoAmountDecision(gc, cardController.DecisionMaker, type, numberOfTokens, false, false,  gameAction, associatedCards, cardSource);
                IEnumerator coroutine = gc.MakeDecisionAction(yesNo);
                if (gc.UseUnityCoroutines)
                {
                    yield return gc.StartCoroutine(coroutine);
                }
                else
                {
                    gc.ExhaustCoroutine(coroutine);
                }
                if (yesNo.Answer.HasValue)
                {
                    proceed = yesNo.Answer.Value;
                }
            }
            if (proceed)
            {
                RemoveTokensFromPoolAction removeTokensFromPoolAction = ((cardSource == null) ? new RemoveTokensFromPoolAction(gc, pool, numberOfTokens) : new RemoveTokensFromPoolAction(cardSource, pool, numberOfTokens));
                storedResults?.Add(removeTokensFromPoolAction);
                IEnumerator coroutine2 = gc.DoAction(removeTokensFromPoolAction);
                if (gc.UseUnityCoroutines)
                {
                    yield return gc.StartCoroutine(coroutine2);
                }
                else
                {
                    gc.ExhaustCoroutine(coroutine2);
                }
            }
        }

        //private static IEnumerator RemoveOneTokenFromPool(CardController cardController, TokenPool pool, List<RemoveTokensFromPoolAction> storedResults = null, GameAction gameAction = null, CardSource cardSource = null)
        //{
        //    GameController gc = cardController.GameController;
        //    bool proceed = true;
        //    proceed = false;
        //    SelectionType type = cardController.DecisionMaker == gc.FindCardController(pool.CardWithTokenPool).DecisionMaker ? SelectionType.Custom : SelectionType.RemoveTokens;
        //    if (pool.CurrentValue > 0)
        //    {
        //        YesNoAmountDecision yesNo = new YesNoAmountDecision(gc, cardController.DecisionMaker, type, 1, upTo: false, requireUnanimous: false, gameAction, null, cardSource);
        //        IEnumerator coroutine = cardController.GameController.MakeDecisionAction(yesNo);
        //        if (gc.UseUnityCoroutines)
        //        {
        //            yield return gc.StartCoroutine(coroutine);
        //        }
        //        else
        //        {
        //            gc.ExhaustCoroutine(coroutine);
        //        }
        //        if (yesNo.Answer.HasValue)
        //        {
        //            proceed = yesNo.Answer.Value;
        //        }

        //        if (proceed)
        //        {
        //            RemoveTokensFromPoolAction removeTokensFromPoolAction = ((cardSource == null) ? new RemoveTokensFromPoolAction(gc, pool, 1) : new RemoveTokensFromPoolAction(cardSource, pool, 1));
        //            storedResults?.Add(removeTokensFromPoolAction);
        //            IEnumerator coroutine2 = gc.DoAction(removeTokensFromPoolAction);
        //            if (gc.UseUnityCoroutines)
        //            {
        //                yield return gc.StartCoroutine(coroutine2);
        //            }
        //            else
        //            {
        //                gc.ExhaustCoroutine(coroutine2);
        //            }
        //        }
        //    }
        //}
    }
}