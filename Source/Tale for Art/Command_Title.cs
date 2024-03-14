using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tale_for_Art
{
    public class Command_Title : Command_Action
    {
        public Command_Title() { }

        public void Rename(CompArt compArt) => Find.WindowStack.Add(new Dialog_RenameTitle(compArt));
    }

    public class Dialog_RenameTitle : Dialog_Rename
    {
        public CompArt compArt;

        public Dialog_RenameTitle(CompArt compArt) => this.compArt = compArt;

        protected override void SetName(string name) => typeof(CompArt).GetField("titleInt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(compArt, (TaggedString)name);
    }

    public class Dialog_Rename : Window
    {
#if v1_4
        protected Dialog_Rename()
            : base()
#else
        protected Dialog_Rename()
            : base(null)
#endif
        {
            doCloseX = true;
            forcePause = true;
            closeOnAccept = false;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        protected virtual void SetName(string name) { }

        public override Vector2 InitialSize => new Vector2(280f, 175f);

        protected virtual AcceptanceReport NameIsValid(string name) => true;

        public virtual string Label => "Rename".Translate();

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                flag = true;
                Event.current.Use();
            }
            Rect rect = new Rect(inRect);
            Text.Font = GameFont.Medium;
            rect.height = Text.LineHeight + 10f;
            Widgets.Label(rect, Label);
            Text.Font = GameFont.Small;
            GUI.SetNextControlName("RenameField");
            string text = Widgets.TextField(new Rect(0f, rect.height, inRect.width, 35f), curName);
            if (text.Length < 100)
                curName = text;
            else
                ((TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl)).SelectAll();

            if (!this.focusedRenameField)
            {
                UI.FocusControl("RenameField", this);
                this.focusedRenameField = true;
            }
            if (Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 10f, inRect.width - 15f - 15f, 35f), "OK", true, true, true, null) || flag)
            {
                AcceptanceReport acceptanceReport = NameIsValid(curName);
                if (!acceptanceReport.Accepted)
                {
                    if (acceptanceReport.Reason.NullOrEmpty())
                    {
                        Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                    Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, false);
                    return;
                }
                else
                {
                    SetName(curName);
                    Find.WindowStack.TryRemove(this, true);
                }
            }
        }

        public string curName;
        public bool focusedRenameField;
    }
}
