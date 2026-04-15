using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment.MasterController
{
    [MonoPatcherLib.Plugin]
    public class Main
    {
        public class AssignOutfitToInteraction : Interactions.AssignOutfitToInteraction
        {
            public class DefinitionModified : ImmediateInteractionDefinition<Sim, Sim, AssignOutfitToInteraction>
            {
                Definition mDefinitionBase = new Definition();

                public override string GetInteractionName(Sim actor, Sim target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
                {
                    return mDefinitionBase.GetInteractionName(actor, target, iop);
                }

                public override string[] GetPath(bool isFemale)
                {
                    return mDefinitionBase.GetPath(isFemale);
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return mDefinitionBase.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
                }
            }

            public override bool Run()
            {
                System.Type interactionInstanceType;
                InteractionInstanceCallbackTypes? entryCallbackType, exitCallbackType;
                if (TryGetInteractionInstanceType(Target, out interactionInstanceType) && TryGetApplyOutfitCallbackType(Target, out entryCallbackType) && TryGetRemoveOutfitCallbackType(Target, out exitCallbackType))
                {
                    bool outfitAlreadyExisted = Actor.SimDescription.HasSpecialOutfit(interactionInstanceType.FullName);
                    if (EditSpecialOutfit(Actor, interactionInstanceType.FullName))
                    {
                        OutfitAssignment.AssignOutfitToInteraction(Target.SimDescription, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                    else if (!outfitAlreadyExisted)
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(interactionInstanceType.FullName);
                    }
                }
                return true;
            }
        }

        static Main()
        {
            LoadSaveManager.ObjectGroupsPreLoad += () => Interactions.AssignOutfitToInteraction.Singleton = new AssignOutfitToInteraction.DefinitionModified();
        }

        static bool EditSpecialOutfit(Sim actor, string specialOutfitKey)
        {
            SimDescription simDescription = actor.SimDescription;
            if (!simDescription.HasSpecialOutfit(specialOutfitKey))
            {
                simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
            }
            OutfitCategories previousOutfitCategory = actor.CurrentOutfitCategory;
            int previousOutfitIndex = actor.CurrentOutfitIndex;
            simDescription.AddOutfit(simDescription.GetSpecialOutfit(specialOutfitKey), OutfitCategories.Everyday, 0);
            simDescription.RemoveSpecialOutfit(specialOutfitKey);
            actor.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
            CASLogic casLogic = CASLogic.GetSingleton();
            new NRaas.MasterControllerSpace.Sims.Stylist().Perform(new NRaas.CommonSpace.Options.GameHitParameters<Sims3.Gameplay.Abstracts.GameObject>(actor, actor, GameObjectHit.NoHit));
            casLogic.ShowUI += OutfitExtensions.OnShowUI;
            while (GameStates.NextInWorldStateId != 0)
            {
                NRaas.SpeedTrap.Sleep();
            }
            casLogic.ShowUI -= OutfitExtensions.OnShowUI;
            simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
            simDescription.RemoveOutfit(OutfitCategories.Everyday, 0, true);
            actor.SwitchToOutfitWithoutSpin(previousOutfitCategory, previousOutfitIndex);
            return !CASChangeReporter.Instance.CasCancelled;
        }
    }
}
