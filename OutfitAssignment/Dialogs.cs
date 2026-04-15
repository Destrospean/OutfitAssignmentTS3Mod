using System;
using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.UI;

namespace Destrospean.OutfitAssignment
{
    public class Dialogs
    {
        public class ComboSelectionDialog : ModalDialog
        {
            public const string kLayoutName = "ComboSelectionDialog";

            public const int kWinExportID = 4096;

            public Button mCancelButton, mOkButton;

            public ComboBox mCombo;

            public object mResult;

            public object Result
            {
                get
                {
                    return mResult;
                }
            }

            public enum ControlID : uint
            {
                kComboBoxId = 2,
                kOKButtonID,
                kCancelButtonID,
                kTitleTextID
            }

            public ComboSelectionDialog(string titleText, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode) : base(kLayoutName, kWinExportID, true, pauseMode, null)
            {
                if (mModalDialogWindow == null)
                {
                    return;
                }
                Text text = mModalDialogWindow.GetChildByID(5, true) as Text;
                if (text != null)
                {
                    text.Caption = titleText;
                }
                mCombo = mModalDialogWindow.GetChildByID(2, true) as ComboBox;
                foreach (KeyValuePair<string, object> entry in entries)
                {
                    mCombo.ValueList.Add(entry.Key, entry.Value);
                    if (entry.Value as string == defaultEntry as string)
                    {
                        mCombo.CurrentSelection = (uint)(mCombo.ValueList.Count - 1);
                    }
                }
                Rect area = mModalDialogWindow.Area;
                float height = area.BottomRight.y - area.TopLeft.y,
                width = area.BottomRight.x - area.TopLeft.x,
                x = position.x,
                y = position.y;
                if (x < 0 && y < 0)
                {
                    Rect parentArea = mModalDialogWindow.Parent.Area;
                    x = (float)Math.Round((parentArea.BottomRight.x - parentArea.TopLeft.x - width) / 2);
                    y = (float)Math.Round((parentArea.BottomRight.y - parentArea.TopLeft.y - height) / 2);
                }
                area.Set(x, y, x + width, y + height);
                mModalDialogWindow.Area = area;
                mOkButton = mModalDialogWindow.GetChildByID(3, false) as Button;
                if (mOkButton != null)
                {
                    mOkButton.Click += OnButtonClick;
                }
                mCancelButton = mModalDialogWindow.GetChildByID(4, false) as Button;
                if (mCancelButton != null)
                {
                    mCancelButton.Click += OnButtonClick;
                }
                OkayID = 3;
                SelectedID = OkayID;
                CancelID = 4;
            }

            public void OnButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
            {
                eventArgs.Handled = true;
                EndDialog(sender.ID);
            }

            public override bool OnEnd(uint buttonID)
            {
                if (buttonID == 3)
                {
                    mResult = mCombo.EntryTags[(int)mCombo.CurrentSelection];
                }
                else
                {
                    mResult = null;
                }
                return true;
            }

            public static object Show(string titleText, IDictionary<string, object> entries, object defaultEntry)
            {
                return Show(titleText, entries, defaultEntry, new Vector2(-1, -1), PauseMode.PauseSimulator);
            }

            public static object Show(string titleText, IDictionary<string, object> entries, object defaultEntry, Vector2 position, PauseMode pauseMode)
            {
                if (EnableModalDialogs)
                {
                    using (ComboSelectionDialog comboSelectionDialog = new ComboSelectionDialog(titleText, entries, defaultEntry, position, pauseMode))
                    {
                        comboSelectionDialog.StartModal();
                        return comboSelectionDialog.Result;
                    }
                }
                return defaultEntry;
            }
        }
    }
}