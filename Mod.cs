using HarmonyLib;
using System;
using System.Collections;
using UnityEngine;

namespace SlotMachineNS
{
    public class SlotMachine : Mod
    {
        public override void Ready()
        {
            Logger.Log("Ready!");
            WorldManager.instance.GameDataLoader.AddCardToSetCardBag(SetCardBagType.AdvancedBuildingIdea, "slot_machine_card_idea", 1);
        }
    }

    public class SlotMachineCard : CardData
    {
        public CardBag LootBag;

        public CardBag ExplodeBag;

        public int ExplodeChance = 0;

        protected override bool CanHaveCard(CardData otherCard)
        {
            if (otherCard.Id == "gold" || otherCard.Id == Id)
                return true;
            return base.CanHaveCard(otherCard);
        }

        public override bool CanHaveCardsWhileHasStatus()
        {
            return true;
        }

        public override void UpdateCard()
        {
            if (ChildrenMatchingPredicateCount(card => card.Id == "gold") > 0)
            {
                MyGameCard.StartTimer(2f, Gamble, SokLoc.Translate("slot_machine_card_gamble_status"), "gamble");
            }
            else
            {
                MyGameCard.CancelTimer("gamble");
            }

            base.UpdateCard();
        }

        [TimedAction("gamble")]
        public void Gamble()
        {
            // destroy 1 gold
            MyGameCard.GetRootCard().CardData.DestroyChildrenMatchingPredicateAndRestack((CardData c) => c.Id == "gold", 1);

            if (new System.Random().Next(1, 100) <= ExplodeChance)
            {
                MyGameCard.DestroyCard();
            }
            else
            {
                // increase explode chance
                ExplodeChance += 10;

                // get random card from LootBag
                ICardId cardId = LootBag.GetCard(false);

                // base values
                int numOfCards = 1;
                bool checkAddToStack = false;

                // modify if card is gold
                if (cardId.Id == "gold")
                {
                    numOfCards = new System.Random().Next(0, 3);
                    checkAddToStack = true;
                }

                if (numOfCards > 0)
                {
                    // create card(s)
                    GameCard card = WorldManager.instance.CreateCardStack(Position, numOfCards, cardId.Id, checkAddToStack: checkAddToStack);

                    // spawn animation
                    WorldManager.instance.CreateSmoke(Position);

                    // spawn card(s)
                    WorldManager.instance.StackSend(card, MyGameCard);
                }

            }
        }

        public override void OnDestroyCard()
        {
            // get random card from LootBag
            ICardId cardId = ExplodeBag.GetCard(false);
            int numOfCards = 1;

            if (cardId.Id == "gold")
            {
                numOfCards = new System.Random().Next(0, 5);
            }

            // explode animation
            WorldManager.instance.CreateSmoke(Position);

            // spawn some gold as compensation
            WorldManager.instance.CreateCardStack(Position, numOfCards, cardId.Id, checkAddToStack: false);

            base.OnDestroyCard();
        }
    }
}