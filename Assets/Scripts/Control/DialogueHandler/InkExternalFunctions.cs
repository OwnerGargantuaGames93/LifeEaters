using System.Collections;
using Control.Player;
using Control.Quests;
using Control.Shop;
using Data.Entities.Item;
using Infra.EventBus;
using Ink.Runtime;
using UnityEngine;
using Utils;

namespace Control.DialogueHandler
{
    public class InkExternalFunctions
    {
        private readonly IEventBus _eventBus;
        
        public InkExternalFunctions(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Bind(Story story)
        {
            story.BindExternalFunction("AdvanceQuest", (string npcName) => AdvanceQuest(npcName));
            story.BindExternalFunction("OpenShop", (string shopName) => OpenShop(shopName));
            story.BindExternalFunction("NewTalent", (string talentName) => NewTalent(talentName));
        }
        
        public void Unbind(Story story)
        {
            story.UnbindExternalFunction("AdvanceQuest");
            story.UnbindExternalFunction("OpenShop");
            story.UnbindExternalFunction("NewTalent");
        }

        private void AdvanceQuest(string npcName)
        {
            _eventBus.Publish(new EAdvanceNpcQuest(npcName));
        }
        
        private void OpenShop(string shopName)
        {
            var e = new EOpenShop(shopName);
            CoroutineRunner.Instance.StartCoroutine(SendOpenShopEvent(e));
        }

        private void NewTalent(string talentName)
        {
            TalentId talent;
            switch (talentName)
            {
                case "dash":
                    talent = TalentId.Dash;
                    break;
                default:
                    Debug.LogWarning("[InkExternalFunctions] Unknown talent name: " + talentName);
                    return;
            }
            
            _eventBus.Publish(new ETalentAcquired(talent));
        }
        
        private IEnumerator SendOpenShopEvent(EOpenShop e)
        {
            yield return null; // Wait for one frame to ensure dialogue UI is closed
            _eventBus.Publish(e);
        }
        
    }
}