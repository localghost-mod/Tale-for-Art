using System.Reflection;
using RimWorld;
using Verse;

namespace Tale_for_Art
{
    public class Command_Title : Command_Action
    {
        public Command_Title() { }

        public void Rename(CompArt compArt) =>
            Find.WindowStack.Add(new Dialog_RenameTitle(compArt));
    }

    public class Dialog_RenameTitle : Dialog_Rename
    {
        public CompArt compArt;

        public Dialog_RenameTitle(CompArt compArt) => this.compArt = compArt;

        protected override void SetName(string name) =>
            typeof(CompArt)
                .GetField("titleInt", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(compArt, (TaggedString)name);
    }
}
