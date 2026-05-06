using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class CopyOutfitAssignmentToSim : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition SimSingleton = new Definition
            {
                TargetIsSim = true
            },
        Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/CopyOutfitAssignmentToSim";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, CopyOutfitAssignmentToSim>
        {
            public bool TargetIsSim = false;

            public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
            {
                Sim targetSim = target as Sim ?? actor;
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
                    Common.Localize(isFemale, sLocalizationKey + "/Paths:Individual")
                };
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref Sims3.SimIFace.GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Sim targetSim = target as Sim ?? actor;
                return targetSim != null && Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)).Length > 0;
            }
        }

        public override bool Run()
        {
            Sim sourceSim = Target as Sim ?? Actor;
            SimDescription[] destinationSimDescriptions;
            Type[] selectedInteractionInstanceTypes;
            string sourceSpecialOutfitKey = "";
            if (!Common.ShowSimListDialog(out destinationSimDescriptions, x => x != sourceSim.SimDescription && OutfitUtils.GetAgePrefix(x.Age, true) == OutfitUtils.GetAgePrefix(sourceSim.SimDescription.Age, true) && x.Gender == sourceSim.SimDescription.Gender && x.Species == sourceSim.SimDescription.Species) || !InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(sourceSim.SimDescription.GetAllOutfitAssignments(), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
            {
                return true;
            }
            foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                if (sourceSim.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription) && sourceSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
                {
                    if (!string.IsNullOrEmpty(sourceSpecialOutfitKey))
                    {
                        return AssignOutfitToInteraction.NotifyMultipleOutfitsFound(sourceSim, sourceSim, false);
                    }
                    sourceSpecialOutfitKey = outfitAssignment.SpecialOutfitKey;
                }
            }
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            if (AssignOutfitToInteraction.TryGetEntryCallbackType(sourceSim, Actor, selectedInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(sourceSim, Actor, selectedInteractionInstanceTypes[0], out exitCallbackType))
            {
                foreach (SimDescription destinationSimDescription in destinationSimDescriptions)
                {
                    string destinationSpecialOutfitKey = "OutfitAssignment_" + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID();
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
                        destinationSimDescription.AddAssignedOutfit(new OutfitAssignmentUtils.AssignedOutfit(sourceSim.SimDescription.GetSpecialOutfit(sourceSpecialOutfitKey)), destinationSpecialOutfitKey);
                    }
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        destinationSimDescription.AssignOutfitToInteraction(destinationSpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
            }
            return true;
        }
    }
}
