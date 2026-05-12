using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class ConfigureOutfitAssignment : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition GlobalOutfitSingleton = new Definition
            {
                IsGlobal = true
            },
        SimSingleton = new Definition
            {
                TargetIsSim = true
            },
        Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/ConfigureOutfitAssignment";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ConfigureOutfitAssignment>
        {
            public bool IsGlobal = false,
            TargetIsSim = false;

            public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
            {
                Sim targetSim = IsGlobal ? null : target as Sim ?? actor;
                return Common.Localize(targetSim != null && targetSim.IsFemale, sLocalizationKey + ":Name");
            }

            public override string[] GetPath(bool isFemale)
            {
                string basePath = Common.Localize(isFemale, sLocalizationKey + "/Paths:Base");
                return TargetIsSim ? new[]
                {
                    basePath
                } : new[]
                {
                    basePath,
                    Common.Localize(isFemale, sLocalizationKey + "/Paths:" + (IsGlobal ? "Global" : "Individual"))
                };
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Sim targetSim = target as Sim;
                return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => targetSim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;
            }
        }

        public override bool Run()
        {
            Sim targetSim = ((Definition)InteractionDefinition).IsGlobal ? null : Target as Sim ?? Actor;
            Type[] selectedInteractionInstanceTypes;
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out exitCallbackType))
            {
                Sims3.SimIFace.CAS.BodyTypes[] partOverrides = null;
                bool changePartOverrides = false;
                OutfitAssignmentUtils.OutfitAssignment firstOutfitAssignment;
                if (targetSim.GetSimDescription().TryGetOutfitAssignment(selectedInteractionInstanceTypes[0], out firstOutfitAssignment, Actor.SimDescription))
                {
                    OutfitAssignmentUtils.AssignedOutfit assignedOutfit; 
                    if (OutfitAssignmentUtils.AssignedOutfits.TryGetValue(firstOutfitAssignment.SpecialOutfitKey, out assignedOutfit))
                    {
                        changePartOverrides = OutfitAssignmentUtils.ShowPartOverrideListDialog(OutfitAssignmentUtils.AssignedOutfits[firstOutfitAssignment.SpecialOutfitKey], out partOverrides);
                    }
                }
                foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription))
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(outfitAssignment.SpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value, Actor.SimDescription);
                    }
                    OutfitAssignmentUtils.AssignedOutfit assignedOutfit; 
                    if (changePartOverrides && partOverrides != null && OutfitAssignmentUtils.AssignedOutfits.TryGetValue(outfitAssignment.SpecialOutfitKey, out assignedOutfit))
                    {
                        assignedOutfit.PartOverrides = new System.Collections.Generic.List<Sims3.SimIFace.CAS.BodyTypes>(partOverrides);
                    }
                }
            }
            return true;
        }
    }
}
