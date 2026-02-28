using System.Collections.Generic;
using fluXis.Map.Structures;

namespace fluXis.Screens.Edit.Actions.Notes.Shortcuts;

public class NoteSetGroupAction : EditorAction
{
    public override string Description => "Add notes to group";

    private IEnumerable<HitObject> notes { get; }
    private List<string> oldGroups { get; } = new();
    private string newGroup { get; }

    public NoteSetGroupAction(IEnumerable<HitObject> notes, string newGroup)
    {
        this.notes = notes;
        this.newGroup = newGroup;
    }

    public override void Run(EditorMap map)
    {
        foreach (var note in notes)
        {
            oldGroups.Add(note.Group);
            note.Group = newGroup;
            map.Update(note);
        }
    }

    public override void Undo(EditorMap map)
    {
        int i = 0;

        foreach (var note in notes)
        {
            note.Group = oldGroups[i];
            map.Update(note);
            ++i;
        }

        oldGroups.Clear();
    }
}
