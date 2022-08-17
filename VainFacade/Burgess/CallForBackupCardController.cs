using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Burgess
{
    public class CallForBackupCardController : BurgessUtilityCardController
    {
        public CallForBackupCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Backup cards in Burgess's hand
            SpecialStringMaker.ShowListOfCardsAtLocation(base.HeroTurnTaker.Hand, BackupCard);
            // Show number of Backup cards in Burgess's deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, BackupCard);
            // Show list of Backup cards in Burgess's trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, BackupCard);
            // Show number of Backup cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(BackupCard);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may destroy this card. Otherwise, you may put a Backup from your trash into play. Then discard X minus 2 cards, where X = the number of Backups in play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyOrPlayAndDiscardResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PutIntoPlay, TriggerType.DiscardCard });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, discard any number of cards and draw a card for each card discarded this way."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, null, requiredDecisions: 0, storedResults: discards, allowAutoDecide: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            int numDiscarded = GetNumberOfCardsDiscarded(discards);
            IEnumerator nextCoroutine = null;
            if (numDiscarded > 0)
            {
                nextCoroutine = DrawCards(base.HeroTurnTakerController, numDiscarded);
            }
            else
            {
                nextCoroutine = base.GameController.SendMessageAction(base.TurnTaker.Name + " did not discard any cards, so no cards will be drawn.", Priority.High, GetCardSource(), showCardSource: true);
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(nextCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(nextCoroutine);
            }
            // "Then put any number of Backups from your hand into play."
            IEnumerable<Card> backupsInHand = base.HeroTurnTaker.Hand.Cards.Where((Card c) => BackupCard.Criteria(c));
            IEnumerator putCoroutine = SelectAndPlayCardsFromHand(base.HeroTurnTakerController, backupsInHand.Count(), cardCriteria: BackupCard, isPutIntoPlay: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
        }

        private IEnumerator DestroyOrPlayAndDiscardResponse(PhaseChangeAction pca)
        {
            // "... you may destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, optional: true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            // "Otherwise, you may put a Backup from your trash into play."
            IEnumerator putCoroutine = SearchForCards(base.HeroTurnTakerController, false, true, 0, 1, BackupCard, true, false, false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "Then discard X minus 2 cards, where X = the number of Backups in play."
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, FindCardsWhere(BackupInPlay, visibleToCard: GetCardSource()).Count() - 2, requiredDecisions: FindCardsWhere(BackupInPlay, visibleToCard: GetCardSource()).Count() - 2, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
        }
    }
}
