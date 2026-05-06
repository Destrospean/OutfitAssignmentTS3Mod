using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class EditAssignedOutfit : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/EditAssignedOutfit";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, EditAssignedOutfit>
        {
            public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
            {
                Sim targetSim = target as Sim;
                return Common.Localize(targetSim != null && targetSim.IsFemale, sLocalizationKey + ":Name");
            }

            public override string[] GetPath(bool isFemale)
            {
                return new[]
                {
                    Common.Localize(isFemale, sLocalizationKey + ":Path")
                };
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Sim targetSim = target as Sim;
                return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) && (targetSim != null || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix()))).Length > 0;
            }
        }

        public override bool Run()
        {
            Sim targetSim = Target as Sim,
            targetOrActor = targetSim ?? Actor;
            Type[] selectedInteractionInstanceTypes;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => (targetSim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment = null;
                string specialOutfitKey = "";
                foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment tempOutfitAssignment;
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out tempOutfitAssignment, Actor.SimDescription) && specialOutfitKey != tempOutfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(specialOutfitKey))
                        {
                            Common.Notify(Common.Localize(targetSim != null && targetSim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetOrActor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        outfitAssignment = tempOutfitAssignment;
                        specialOutfitKey = tempOutfitAssignment.SpecialOutfitKey;
                    }
                }
                bool isPartial = targetSim != null && OutfitAssignmentUtils.AssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey);
                if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey) || isPartial && targetSim.SimDescription.HasSpecialOutfit(specialOutfitKey))
                {
                    targetOrActor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                }
                if ((targetSim == null || isPartial) && OutfitAssignmentUtils.AssignedOutfits.ContainsKey(specialOutfitKey))
                {
                    targetOrActor.AddAssignedOutfit(specialOutfitKey);
                }
                if (OutfitExtensions.EditSpecialOutfit(targetOrActor, specialOutfitKey))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, outfitAssignment.EntryCallbackType, outfitAssignment.ExitCallbackType);
                    }
                    if (targetSim == null || isPartial)
                    {
                        OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey] = new OutfitAssignmentUtils.AssignedOutfit(targetOrActor.SimDescription.GetSpecialOutfit(specialOutfitKey))
                            {
                                PartOverrides = OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey].PartOverrides
                            };
                    }
                }
                if (targetSim == null || isPartial)
                {
                    if (targetOrActor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        targetOrActor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                }
            }
            return true;
        }
    }
}
