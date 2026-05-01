using System;
using System.Collections.Generic;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment
{
    public static class Interactions
    {
        public class AssignOutfitCategoryToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitCategoryToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitCategoryToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim targetSim = target as Sim;
                    return targetSim == null || targetSim.IsHuman;
                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                OutfitCategories outfitCategory;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, selectedInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, selectedInteractionInstanceTypes[0], out exitCallbackType) && TryGetOutfitCategory(targetSim, selectedInteractionInstanceTypes[0], out outfitCategory))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix + outfitCategory, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }

            public static bool TryGetOutfitCategory(Sim sim, Type interactionInstanceType, out OutfitCategories outfitCategory)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                string localizationKey = "/Dialogs/OutfitCategoryDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new AssignOutfitToInteraction.DummyComparer())
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
                    }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) ? outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix.Length) : OutfitCategories.Everyday.ToString()) as string;
                if (text == null)
                {
                    outfitCategory = OutfitCategories.None;
                    return false;
                }
                outfitCategory = (OutfitCategories)Enum.Parse(typeof(OutfitCategories), text);
                return true;
            }
        }

        public class AssignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
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
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => !OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && TryGetEntryCallbackType(targetSim, selectedInteractionInstanceTypes[0], out entryCallbackType) && TryGetExitCallbackType(targetSim, selectedInteractionInstanceTypes[0], out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(targetSim == null || targetSim.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? (targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    if (targetSim == null)
                    {
                        ShowIncludeHairDialog(specialOutfitKey);
                    }
                    bool outfitIsPreexisting = targetSim != null && targetSim.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (targetSim == null && OutfitAssignmentUtils.GlobalAssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        Actor.AddGlobalAssignedOutfit(specialOutfitKey);
                    }
                    if ((targetSim ?? Actor).EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        (targetSim ?? Actor).SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (targetSim == null)
                    {
                        OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey] = new SimOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey).Key);
                        if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                        }
                    }
                }
                return true;
            }

            public static bool TryGetEntryCallbackType(Sim sim, Type interactionInstanceType, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
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
                        },
                        {
                            Common.Localize(sim != null && sim.IsFemale, localizationKey + "/Options:OutfitChanged"),
                            InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged.ToString()
                        }
                    }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) ? outfitAssignment.EntryCallbackType.ToString() : InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            public static bool TryGetExitCallbackType(Sim sim, Type interactionInstanceType, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(sim != null && sim.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
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
                    }, sim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) ? outfitAssignment.ExitCallbackType.ToString() : InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }
        }

        public class ConfigureOutfitAssignment : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ConfigureOutfitAssignment";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ConfigureOutfitAssignment>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim targetSim = target as Sim;
                    return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => targetSim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;
                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, selectedInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, selectedInteractionInstanceTypes[0], out exitCallbackType))
                {
                    bool includeHair = targetSim == null && Sims3.UI.AcceptCancelDialog.Show(Common.Localize("/Dialogs/IncludeHairDialog:Title"));
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment))
                        {
                            targetSim.GetSimDescription().AssignOutfitToInteraction(outfitAssignment.SpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                        if (includeHair)
                        {
                            if (!OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.Contains(outfitAssignment.SpecialOutfitKey))
                            {
                                OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.Add(outfitAssignment.SpecialOutfitKey);
                            }
                        }
                        else
                        {
                            OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.RemoveAll(x => x == outfitAssignment.SpecialOutfitKey);
                        }
                    }
                }
                return true;
            }
        }

        public class CopyAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/CopyAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, CopyAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim targetSim = target as Sim;
                    return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)).Length > 0;
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
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && sourceSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
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
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, sourceInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, sourceInteractionInstanceTypes[0], out exitCallbackType))
                {
                    string destinationSpecialOutfitKey = (targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID();
                    if (targetSim == null)
                    {
                        OutfitAssignmentUtils.GlobalAssignedOutfits[destinationSpecialOutfitKey] = new SimOutfit(OutfitAssignmentUtils.GlobalAssignedOutfits[sourceSpecialOutfitKey].Key);
                        if (targetSim == null)
                        {
                            ShowIncludeHairDialog(destinationSpecialOutfitKey);
                        }
                    }
                    else
                    {
                        targetSim.SimDescription.AddSpecialOutfit(new SimOutfit(targetSim.SimDescription.GetSpecialOutfit(sourceSpecialOutfitKey).Key), destinationSpecialOutfitKey);
                    }
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(destinationSpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class EditAssignedOutfit : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/EditAssignedOutfit";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, EditAssignedOutfit>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim targetSim = target as Sim;
                    return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)).Length > 0;
                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => (targetSim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment = null;
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment tempOutfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out tempOutfitAssignment) && specialOutfitKey != tempOutfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(targetSim != null && targetSim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            outfitAssignment = tempOutfitAssignment;
                            specialOutfitKey = tempOutfitAssignment.SpecialOutfitKey;
                        }
                    }
                    if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (targetSim == null && OutfitAssignmentUtils.GlobalAssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        Actor.AddGlobalAssignedOutfit(specialOutfitKey);
                    }
                    if ((targetSim ?? Actor).EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, outfitAssignment.EntryCallbackType, outfitAssignment.ExitCallbackType);
                        }
                    }
                    if (targetSim == null)
                    {
                        OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey] = new SimOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey).Key);
                        if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                        }
                    }
                }
                return true;
            }
        }

        public class ExtendAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ExtendAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ExtendAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
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
                    if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
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
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(targetSim, sourceInteractionInstanceTypes[0], out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(targetSim, sourceInteractionInstanceTypes[0], out exitCallbackType))
                {
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
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

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim targetSim = target as Sim;
                    return Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => targetSim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;

                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
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
                                if (OutfitAssignmentUtils.GlobalAssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey))
                                {
                                    OutfitAssignmentUtils.GlobalAssignedOutfits.Remove(outfitAssignment.SpecialOutfitKey);
                                }
                                OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.RemoveAll(x => x == outfitAssignment.SpecialOutfitKey);
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

        public static bool ShowIncludeHairDialog(string globalAssignedSpecialOutfitKey)
        {
            if (Sims3.UI.AcceptCancelDialog.Show(Common.Localize("/Dialogs/IncludeHairDialog:Title")))
            {
                if (!OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.Contains(globalAssignedSpecialOutfitKey))
                {
                    OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.Add(globalAssignedSpecialOutfitKey);
                }
                return true;
            }
            OutfitAssignmentUtils.GlobalAssignedOutfitsIncludingHair.RemoveAll(x => x == globalAssignedSpecialOutfitKey);
            return false;
        }

        public static Sims3.Gameplay.CAS.SimDescription GetSimDescription(this Sim sim)
        {
            return sim == null ? null : sim.SimDescription;
        }
    }
}
