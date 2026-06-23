using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public static class Common
    {
        internal const string kLocalizationPath = "Destrospean/OutfitAssignment";

        public delegate bool SimDescriptionFilterFunc(SimDescription simDescription);

        public class SimColumn : UI.Dialogs.ObjectPickerDialog.CommonHeaderInfo<SimDescription>
        {
            public SimColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
            {
            }

            public override ObjectPicker.ColumnInfo GetValue(SimDescription simDescription)
            {
                return new ObjectPicker.ThumbAndTextColumn(simDescription.GetEverydayThumbnail(Sims3.SimIFace.ThumbnailSize.ExtraLarge), simDescription.FullName);
            }
        }

        public static SimDescription GetSimDescription(this Sims3.Gameplay.Actors.Sim sim)
        {
            return sim == null ? null : sim.SimDescription;
        }

        public static string Localize(string entryKey)
        {
            return Sims3.Gameplay.Utilities.Localization.LocalizeString(kLocalizationPath + entryKey);
        }

        public static string Localize(string entryKey, params object[] parameters)
        {
            return Sims3.Gameplay.Utilities.Localization.LocalizeString(kLocalizationPath + entryKey, parameters);
        }

        public static string Localize(bool isFemale, string entryKey, params object[] parameters)
        {
            return Sims3.Gameplay.Utilities.Localization.LocalizeString(isFemale, kLocalizationPath + entryKey, parameters);
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
            StyledNotification.Show(simDescription.CreatedSim == null ? new StyledNotification.Format(message, style) : new StyledNotification.Format(message, Sims3.SimIFace.ObjectGuid.InvalidObjectGuid, simDescription.CreatedSim.ObjectId, style));
        }

        public static bool ShowSimListDialog(out SimDescription[] selectedSims, SimDescriptionFilterFunc filterBy = null)
        {
            try
            {
                const string localizationPath = kLocalizationPath + "/Dialogs/SimListDialog";
                bool cancelled, confirmed;
                List<SimDescription> selectedSimList = UI.Dialogs.ObjectPickerDialog.Show(Responder.Instance.LocalizationModel.LocalizeString(localizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                    {
                        new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), Household.EverySimDescription().FindAll(x => filterBy == null ? true : filterBy(x)).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                    }, new List<UI.Dialogs.ObjectPickerDialog.CommonHeaderInfo<SimDescription>>
                    {
                        new SimColumn(localizationPath),
                    }, int.MaxValue, out confirmed, out cancelled);
                if (confirmed)
                {
                    selectedSims = selectedSimList.ToArray();
                    return true;
                }
                selectedSims = null;
                return false;
            }
            catch (System.Exception ex)
            {
                ((Sims3.SimIFace.IScriptErrorWindow)System.AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                selectedSims = null;
                return false;
            }
        }
    }
}
