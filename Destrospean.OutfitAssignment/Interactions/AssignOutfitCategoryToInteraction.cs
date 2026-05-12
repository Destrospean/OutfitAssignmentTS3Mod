using System;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class AssignOutfitCategoryToInteraction : ImmediateInteraction<Sim, GameObject>
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

        public const string sLocalizationKey = "/Interactions/AssignOutfitCategoryToInteraction";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitCategoryToInteraction>
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
                return targetSim == null || targetSim.IsHuman;
            }
        }

        public override bool Run()
        {
            Sim targetSim = ((Definition)InteractionDefinition).IsGlobal ? null : Target as Sim ?? Actor;
            Type[] selectedInteractionInstanceTypes;
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            OutfitCategories outfitCategory;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out exitCallbackType) && TryGetOutfitCategory(targetSim, Actor, selectedInteractionInstanceTypes[0], out outfitCategory))
            {
                foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                {
                    targetSim.GetSimDescription().AssignOutfitToInteraction(targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix(true) + outfitCategory : OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix + outfitCategory, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value, Actor.SimDescription);
                }
            }
            return true;
        }

        public static bool TryGetOutfitCategory(Sim sim, Sim fallbackSim, Type interactionInstanceType, out OutfitCategories outfitCategory)
        {
            OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
            string localizationKey = "/Dialogs/OutfitCategoryDialog",
            text = UI.Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new System.Collections.Generic.SortedDictionary<string, object>(new AssignOutfitToInteraction.DummyComparer())
                {
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:None"),
                        OutfitCategories.None.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Everyday"),
                        OutfitCategories.Everyday.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Formalwear"),
                        OutfitCategories.Formalwear.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Sleepwear"),
                        OutfitCategories.Sleepwear.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Swimwear"),
                        OutfitCategories.Swimwear.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Athletic"),
                        OutfitCategories.Athletic.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Career"),
                        OutfitCategories.Career.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Outerwear"),
                        OutfitCategories.Outerwear.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:MartialArts"),
                        OutfitCategories.MartialArts.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Naked"),
                        OutfitCategories.Naked.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Singed"),
                        OutfitCategories.Singed.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:SkinnyDippingTowel"),
                        OutfitCategories.SkinnyDippingTowel.ToString()
                    }
                }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, fallbackSim.SimDescription) && outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix.Length) : OutfitCategories.Everyday.ToString()) as string;
            if (text == null)
            {
                outfitCategory = OutfitCategories.None;
                return false;
            }
            outfitCategory = (OutfitCategories)Enum.Parse(typeof(OutfitCategories), text);
            return true;
        }
    }
}
