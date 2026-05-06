using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
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

        public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, UnassignOutfitToInteraction>
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
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))))
            {
                foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription) && Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                    {
                        if (targetSim == null)
                        {
                            foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                            {
                                if (sim.SimDescription != null && sim.SimDescription.HasSpecialOutfit(outfitAssignment.SpecialOutfitKey))
                                {
                                    sim.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                                }
                            }
                            if (OutfitAssignmentUtils.AssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey))
                            {
                                OutfitAssignmentUtils.AssignedOutfits.Remove(outfitAssignment.SpecialOutfitKey);
                            }
                        }
                        else if (OutfitAssignmentUtils.AssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey))
                        {
                            if (Array.FindAll(targetSim.SimDescription.GetAllOutfitAssignments(), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                            {
                                OutfitAssignmentUtils.AssignedOutfits.Remove(outfitAssignment.SpecialOutfitKey);
                            }
                        }
                        else
                        {
                            targetSim.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                        }
                    }
                    OutfitAssignmentUtils.UnassignOutfitToInteraction(targetSim.GetSimDescription(), interactionInstanceType);
                }
            }
            return true;
        }
    }
}
