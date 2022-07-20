using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VainFacadePlaytest.Sphere
{
    public class ConversionFieldCardController : EmanationCardController
    {
        public ConversionFieldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => "No damage type has been chosen for " + base.Card.Title + ".").Condition = () => base.Card.IsInPlayAndHasGameText && base.GetCardPropertyJournalEntryInteger(LastChosenType).HasValue && (base.GetCardPropertyJournalEntryInteger(LastChosenType).Value < 0 || base.GetCardPropertyJournalEntryInteger(LastChosenType).Value >= typeOptions.Length);
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + "'s damage type is " + MostRecentChosen().Value.ToString() + ".").Condition = () => base.Card.IsInPlayAndHasGameText && base.GetCardPropertyJournalEntryInteger(LastChosenType).HasValue && base.GetCardPropertyJournalEntryInteger(LastChosenType).Value >= 0 && base.GetCardPropertyJournalEntryInteger(LastChosenType).Value < typeOptions.Length;
        }

        private readonly string LastChosenType = "LastChosenType";
        private readonly DamageType[] typeOptions = { DamageType.Cold, DamageType.Energy, DamageType.Fire, DamageType.Infernal, DamageType.Lightning, DamageType.Melee, DamageType.Projectile, DamageType.Psychic, DamageType.Radiant, DamageType.Sonic, DamageType.Toxic };

        public DamageType? MostRecentChosen()
        {
            int? lastChosenIndex = base.GetCardPropertyJournalEntryInteger(LastChosenType);
            if (lastChosenIndex.HasValue && lastChosenIndex.Value >= 0 && lastChosenIndex.Value < typeOptions.Length)
            {
                return typeOptions[lastChosenIndex.Value];
            }
            return null;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of your turn, ... select a damage type."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SelectTypeResponse, TriggerType.Other);
            // "When {Sphere} is dealt damage of that type, he regains 2HP."
            AddTrigger((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.CharacterCard && MostRecentChosen().HasValue && dda.DamageType == MostRecentChosen(), (DealDamageAction dda) => base.GameController.GainHP(base.CharacterCard, 2, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, select a damage type."
            yield return SelectTypeResponse(null);
        }

        private IEnumerator SelectTypeResponse(PhaseChangeAction pca)
        {
            // "... select a damage type."
            List<SelectDamageTypeDecision> typeChoice = new List<SelectDamageTypeDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectDamageType(base.HeroTurnTakerController, storedResults: typeChoice, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            DamageType chosenType = typeChoice.FirstOrDefault((SelectDamageTypeDecision sdtd) => sdtd.Completed).SelectedDamageType.Value;
            base.SetCardProperty(LastChosenType, typeOptions.IndexOf(chosenType).Value);
            yield break;
        }
    }
}
