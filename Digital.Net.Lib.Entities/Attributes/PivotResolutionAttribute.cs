namespace Digital.Net.Lib.Entities.Attributes;

/// <summary>
///     Marks a <c>Pivot&lt;TParent,TChild&gt;</c> class as patchable through the CRUD pipeline.
///     The framework's assembly scanner reads this attribute to know:
///     <list type="bullet">
///         <item>
///             <see cref="VirtualPath" /> — the JSON Patch path that the dispatcher should match (e.g. <c>"/sheets"</c>
///             ).
///         </item>
///         <item><see cref="Mode" /> — what happens when an entry is removed from the array.</item>
///     </list>
///     A pivot without this attribute is invisible to the dispatcher.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PivotResolutionAttribute(string virtualPath, Ownership mode) : Attribute
{
    public string VirtualPath { get; } = virtualPath;
    public Ownership Mode { get; } = mode;
}

public enum Ownership
{
    Cascade,
    Dissociate
}