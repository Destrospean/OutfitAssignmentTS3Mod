using System;
using System.Collections.Generic;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;

namespace Destrospean.OutfitAssignment.Interactions
{
    public class AssignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
    {
        public static InteractionDefinition PartialOutfitSingleton = new Definition(true),
        Singleton = new Definition();

        public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitToInteraction>
        {
            public bool IsPartial;

            public Definition(bool isPartial = false)
            {
                IsPartial = isPartial;
            }

            public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
            {
                Sim targetSim = target as Sim;
                return Common.Localize(targetSim != null && targetSim.IsFemale, sLocalizationKey + "/Names:" + (IsPartial ? "Partial" : "Full"));
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
                return targetSim == null || targetSim.IsHuman;
            }
        }

        public class DummyComparer : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                return 1;
            }
        }

        public override bool Run()
        {
            Sim targetSim = Target as Sim,
            targetOrActor = targetSim ?? Actor;
            Type[] selectedInteractionInstanceTypes;
            InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
            if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => !OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && TryGetEntryCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out entryCallbackType) && TryGetExitCallbackType(targetSim, Actor, selectedInteractionInstanceTypes[0], out exitCallbackType))
            {
                string specialOutfitKey = "";
                foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, Actor.SimDescription) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(specialOutfitKey))
                        {
                            Common.Notify(Common.Localize(targetSim == null || targetSim.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetOrActor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                    }
                }
                specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? (targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                bool isPartial = ((Definition)InteractionDefinition).IsPartial,
                isPreexisting = targetSim != null && !isPartial && targetSim.SimDescription.HasSpecialOutfit(specialOutfitKey);
                if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey) || isPartial && targetSim.SimDescription.HasSpecialOutfit(specialOutfitKey))
                {
                    targetOrActor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                }
                if ((targetSim == null || isPartial) && OutfitAssignmentUtils.AssignedOutfits.ContainsKey(specialOutfitKey))
                {
                    targetOrActor.AddAssignedOutfit(specialOutfitKey);
                }
                if (!isPreexisting && targetSim != null)
                {
                    string globalSpecialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (targetSim.SimDescription.TryGetGlobalOutfitAssignment(interactionInstanceType, out outfitAssignment) && globalSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(globalSpecialOutfitKey))
                            {
                                globalSpecialOutfitKey = "";
                                break;
                            }
                            globalSpecialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    if (!string.IsNullOrEmpty(globalSpecialOutfitKey))
                    {
                        targetSim.AddAssignedOutfit(globalSpecialOutfitKey, specialOutfitKey);
                    }
                }
                if (OutfitExtensions.EditSpecialOutfit(targetOrActor, specialOutfitKey))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                    if (targetSim == null || isPartial)
                    {
                        Sims3.SimIFace.CAS.BodyTypes[] partOverrides;
                        if (OutfitAssignmentUtils.ShowPartOverrideListDialog(OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey] = new OutfitAssignmentUtils.AssignedOutfit(targetOrActor.SimDescription.GetSpecialOutfit(specialOutfitKey)), out partOverrides, isPartial ? new Sims3.SimIFace.CAS.BodyTypes[0] : null))
                        {
                            OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey].PartOverrides = new List<Sims3.SimIFace.CAS.BodyTypes>(partOverrides);
                            if (targetOrActor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                            {
                                targetOrActor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                            }
                        }
                    }
                }
                else if (!isPreexisting)
                {
                    targetOrActor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                }
            }
            return true;
        }

        public static bool TryGetEntryCallbackType(Sim sim, Sim fallbackSim, Type interactionInstanceType, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
        {
            OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
            string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
            text = UI.Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                {
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:InteractionStarted"),
                        InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:StandardEntry"),
                        InteractionInstanceTypeUtils.CallbackTypes.StandardEntry.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:SyncLevelRouted"),
                        InteractionInstanceTypeUtils.CallbackTypes.SyncLevelRouted.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:SyncLevelCommitted"),
                        InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCommitted.ToString()
                    }
                }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, fallbackSim.SimDescription) ? (outfitAssignment.EntryCallbackType == InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged ? InteractionInstanceTypeUtils.CallbackTypes.StandardEntry : outfitAssignment.EntryCallbackType).ToString() : InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()) as string;
            if (text == null)
            {
                callbackType = null;
                return false;
            }
            callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
            return true;
        }

        public static bool TryGetExitCallbackType(Sim sim, Sim fallbackSim, Type interactionInstanceType, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
        {
            OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
            string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
            text = UI.Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                {
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:InteractionEnded"),
                        InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:StandardExit"),
                        InteractionInstanceTypeUtils.CallbackTypes.StandardExit.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:SyncLevelCompleted"),
                        InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted.ToString()
                    },
                    {
                        Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:Never"),
                        InteractionInstanceTypeUtils.CallbackTypes.Never.ToString()
                    }
                }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment, fallbackSim.SimDescription) ? outfitAssignment.ExitCallbackType.ToString() : InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()) as string;
            if (text == null)
            {
                callbackType = null;
                return false;
            }
            callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
            return true;
        }
    }
}
