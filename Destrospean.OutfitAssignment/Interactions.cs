using System;
using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;

namespace Destrospean.OutfitAssignment
{
    public class Interactions
    {
        public class AssignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

            class DummyComparer : IComparer<string>
            {
                public int Compare(string a, string b)
                {
                    return 1;
                }
            }

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, AssignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.IsHuman;
                }
            }

            public static bool TryGetEntryCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionStarted"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardEntry"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardEntry.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelRouted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelRouted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelCommitted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCommitted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:OutfitChanged"),
                            InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            public static bool TryGetExitCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionEnded"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardExit"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardExit.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelCompleted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Never"),
                            InteractionInstanceTypeUtils.CallbackTypes.Never.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            public override bool Run()
            {
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes) && TryGetEntryCallbackType(Target, out entryCallbackType) && TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(Target.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? "OutfitAssignment_" + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    bool outfitIsPreexisting = Target.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (Target.EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            Target.SimDescription.AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        Target.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                }
                return true;
            }
        }

        public class ConfigureOutfitAssignment : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ConfigureOutfitAssignment";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ConfigureOutfitAssignment>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.SimDescription.GetAllOutfitAssignments().Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Target.SimDescription.GetAllOutfitAssignments(), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))) && AssignOutfitToInteraction.TryGetEntryCallbackType(Target, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment))
                        {
                            Target.SimDescription.AssignOutfitToInteraction(outfitAssignment.SpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                }
                return true;
            }
        }

        public class CopyAssignedOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/CopyAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, CopyAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.SimDescription.GetAllOutfitAssignments().Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] sourceInteractionInstanceTypes, destinationInteractionInstanceTypes;
                if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.ConvertAll(Target.SimDescription.GetAllOutfitAssignments(), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType)), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
                {
                    return true;
                }
                string sourceSpecialOutfitKey = "";
                foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && sourceSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(sourceSpecialOutfitKey))
                        {
                            Common.Notify(Common.Localize(Target.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        sourceSpecialOutfitKey = outfitAssignment.SpecialOutfitKey;
                    }
                }
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(Target, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    string destinationSpecialOutfitKey = "OutfitAssignment_" + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID();
                    Target.SimDescription.AddSpecialOutfit(new Sims3.SimIFace.CAS.SimOutfit(Target.SimDescription.GetSpecialOutfit(sourceSpecialOutfitKey).Key), destinationSpecialOutfitKey);
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        Target.SimDescription.AssignOutfitToInteraction(destinationSpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class EditAssignedOutfit : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/EditAssignedOutfit";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, EditAssignedOutfit>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.SimDescription.GetAllOutfitAssignments().Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Target.SimDescription.GetAllOutfitAssignments(), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment = null;
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment tempOutfitAssignment;
                        if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out tempOutfitAssignment) && specialOutfitKey != tempOutfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(Target.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            outfitAssignment = tempOutfitAssignment;
                            specialOutfitKey = tempOutfitAssignment.SpecialOutfitKey;
                        }
                    }
                    if (Target.EditSpecialOutfit(specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            Target.SimDescription.AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, outfitAssignment.EntryCallbackType, outfitAssignment.ExitCallbackType);
                        }
                    }
                }
                return true;
            }
        }

        public class ExtendAssignedOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ExtendAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ExtendAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.SimDescription.GetAllOutfitAssignments().Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] sourceInteractionInstanceTypes, destinationInteractionInstanceTypes;
                if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.ConvertAll(Target.SimDescription.GetAllOutfitAssignments(), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType)), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
                {
                    return true;
                }
                string specialOutfitKey = "";
                foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(specialOutfitKey))
                        {
                            Common.Notify(Common.Localize(Target.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", Target), Target.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                    }
                }
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(Target, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(Target, out exitCallbackType))
                {
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        Target.SimDescription.AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Common.Localize(target.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return target.SimDescription.GetAllOutfitAssignments().Length > 0;
                }
            }

            public override bool Run()
            {
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Target.SimDescription.GetAllOutfitAssignments(), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (Target.SimDescription.TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && Array.FindAll(Target.SimDescription.GetAllOutfitAssignments(), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                        {
                            Target.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                        }
                        Target.SimDescription.UnassignOutfitToInteraction(interactionInstanceType);
                    }
                }
                return true;
            }
        }
    }
}
