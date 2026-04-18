using System;
using System.Collections.Generic;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Utilities;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public static class Common
    {
        internal const string kLocalizationPath = "Destrospean/OutfitAssignment";

        static Type[] sInteractionInstanceTypes;

        public static Type[] InteractionInstanceTypes
        {
            get
            {
                if (sInteractionInstanceTypes == null)
                {
                    InitInteractionInstanceTypes();
                }
                return sInteractionInstanceTypes;
            }
        }

        class InteractionInstanceTypeColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>
        {
            const string sLocalizationKey = kLocalizationPath + "/Dialogs/InteractionListDialog";

            public InteractionInstanceTypeColumn() : base(sLocalizationKey + "/Header:Text", sLocalizationKey + "/Header:Tooltip", 40)
            {
            }

            public override ObjectPicker.ColumnInfo GetValue(Type interactionInstanceType)
            {
                return new ObjectPicker.TextColumn(interactionInstanceType == null ? "" : interactionInstanceType.FullName);
            }
        }

        public static void CopyTuning(Type baseType, Type oldType, Type newType)
        {
            if (AutonomyTuning.GetTuning(newType.FullName, baseType.FullName) == null)
            {
                InteractionTuning tuning = AutonomyTuning.GetTuning(oldType, oldType.FullName, baseType);
                if (tuning != null)
                {
                    AutonomyTuning.AddTuning(newType.FullName, baseType.FullName, tuning);
                }
            }
            InteractionObjectPair.sTuningCache.Remove(new Pair<Type, Type>(newType, baseType));
        }

        public static void InitInteractionInstanceTypes()
        {
            List<Type> interactionInstanceTypes = new List<Type>();
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                interactionInstanceTypes.AddRange(Array.FindAll(assembly.GetTypes(), x => typeof(Sims3.Gameplay.Interactions.InteractionInstance).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract));
            }
            sInteractionInstanceTypes = interactionInstanceTypes.ToArray();
        }

        public static string Localize(string entryKey)
        {
            return Localization.LocalizeString(kLocalizationPath + entryKey);
        }

        public static string Localize(string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(kLocalizationPath + entryKey, parameters);
        }

        public static string Localize(bool isFemale, string entryKey, params object[] parameters)
        {
            return Localization.LocalizeString(isFemale, kLocalizationPath + entryKey, parameters);
        }

        public static void Notify(string message, SimDescription simDescription, StyledNotification.NotificationStyle style)
        {
            Notify(message, simDescription, style, true);
        }

        public static void Notify(string message, SimDescription fakeSimDescription, StyledNotification.NotificationStyle style, bool checkForFake)
        {
            SimDescription simDescription = fakeSimDescription;
            if (simDescription == null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
                return;
            }
            if (checkForFake)
            {
                simDescription = SimDescription.Find(fakeSimDescription.SimDescriptionId);
                if (simDescription == null)
                {
                    StyledNotification.Show(new StyledNotification.Format(message, style));
                    return;
                }
            }
            if (simDescription.CreatedSim != null)
            {
                StyledNotification.Show(new StyledNotification.Format(message, Sims3.SimIFace.ObjectGuid.InvalidObjectGuid, simDescription.CreatedSim.ObjectId, style));
            }
            else
            {
                StyledNotification.Show(new StyledNotification.Format(message, style));
            }
        }

        public static bool TryGetInteractionInstanceTypes(out Type[] interactionInstanceTypes, Type[] allInteractionInstanceTypes = null)
        {
            try
            {
                string localizationKey = "/Dialogs/InteractionListDialog";
                bool okayed;
                List<Type> selected = Dialogs.ObjectPickerDialog.Show(Localize(localizationKey + ":Title"), new List<ObjectPicker.TabInfo>
                    {
                        new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), new List<Type>(allInteractionInstanceTypes ?? InteractionInstanceTypes).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                    }, new List<Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>>
                    {
                        new InteractionInstanceTypeColumn()
                    }, int.MaxValue, out okayed);
                if (okayed)
                {
                    interactionInstanceTypes = selected.ToArray();
                    return true;
                }
                interactionInstanceTypes = null;
                return false;
            }
            catch (Exception ex)
            {
                ((Sims3.SimIFace.IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                interactionInstanceTypes = null;
                return false;
            }
        }
    }
}
