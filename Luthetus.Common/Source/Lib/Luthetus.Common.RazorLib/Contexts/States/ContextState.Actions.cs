using Luthetus.Common.RazorLib.Contexts.Models;
using Luthetus.Common.RazorLib.Keymaps.Models;
using Luthetus.Common.RazorLib.Keys.Models;

namespace Luthetus.Common.RazorLib.Contexts.States;

public partial record ContextState
{
    public record SetActiveContextRecordsAction(ContextRecordKeyHeirarchy ContextRecordKeyHeirarchy);
    public record ToggleSelectInspectionTargetAction;
    public record SetSelectInspectionTargetTrueAction;
    public record SetSelectInspectionTargetFalseAction;
    public record SetInspectionTargetAction(ContextRecordKeyHeirarchy? ContextRecordKeyHeirarchy);
    public record AddInspectContextRecordEntryAction(InspectContextRecordEntry InspectContextRecordEntry);
    public record SetContextKeymapAction(Key<ContextRecord> ContextRecordKey, Keymap Keymap);
}
