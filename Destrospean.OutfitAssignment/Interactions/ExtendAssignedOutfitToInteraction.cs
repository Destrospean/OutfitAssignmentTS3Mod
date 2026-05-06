using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class ExtendAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/ExtendAssignedOutfitToInteraction";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ExtendAssignedOutfitToInteraction>
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
                return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => targetSim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;

            }
        }

        public override bool Run()
        {
            Sim targetSim = Target as Sim;
            Type[] destinationInteractionInstanceTypes, sourceInteractionInstanceTypes;
            if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName)), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
            {
                return true;
            }
            string specialOutfitKey = "";
            foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                {
                    if (!string.IsNullOrEmpty(specialOutfitKey))
                    {
                        Common.Notify(Common.Localize(targetSim != null && targetSim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                        return true;
                    }
                    specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                }
            }
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, Actor, sourceInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, Actor, sourceInteractionInstanceTypes[0], out exitCallbackType))
            {
                foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                {
                    targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                }
            }
            return true;
        }
    }
}
