using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class CopyAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/CopyAssignedOutfitToInteraction";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, CopyAssignedOutfitToInteraction>
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
            Sim targetSim = Target as Sim;
            Type[] destinationInteractionInstanceTypes, sourceInteractionInstanceTypes;
            string sourceSpecialOutfitKey = "";
            if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => (targetSim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType)), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
            {
                return true;
            }
            foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription) && sourceSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
                {
                    if (!string.IsNullOrEmpty(sourceSpecialOutfitKey))
                    {
                        Common.Notify(Common.Localize(targetSim != null && targetSim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                        return true;
                    }
                    sourceSpecialOutfitKey = outfitAssignment.SpecialOutfitKey;
                }
            }
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, Actor, sourceInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, Actor, sourceInteractionInstanceTypes[0], out exitCallbackType))
            {
                string destinationSpecialOutfitKey = (targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID();
                if (OutfitAssignmentUtils.AssignedOutfits.ContainsKey(sourceSpecialOutfitKey))
                {
                    Sims3.SimIFace.CAS.BodyTypes[] partOverrides;
                    if (OutfitAssignmentUtils.ShowPartOverrideListDialog(OutfitAssignmentUtils.AssignedOutfits[destinationSpecialOutfitKey] = new OutfitAssignmentUtils.AssignedOutfit(OutfitAssignmentUtils.AssignedOutfits[sourceSpecialOutfitKey]), out partOverrides))
                    {
                        OutfitAssignmentUtils.AssignedOutfits[destinationSpecialOutfitKey].PartOverrides = new System.Collections.Generic.List<Sims3.SimIFace.CAS.BodyTypes>(partOverrides);
                    }
                }
                else
                {
                    targetSim.SimDescription.AddSpecialOutfit(new Sims3.SimIFace.CAS.SimOutfit(targetSim.SimDescription.GetSpecialOutfit(sourceSpecialOutfitKey).Key), destinationSpecialOutfitKey);
                }
                foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                {
                    targetSim.GetSimDescription().AssignOutfitToInteraction(destinationSpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                }
            }
            return true;
        }
    }
}
