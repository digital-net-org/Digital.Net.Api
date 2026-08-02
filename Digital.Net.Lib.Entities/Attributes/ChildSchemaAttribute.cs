namespace Digital.Net.Lib.Entities.Attributes;

/// <summary>
///     Marks an entity collection navigation as an inline-edited child collection: the property is
///     exposed in the entity schema as a <c>Collection</c> embedding the child entity's own schema,
///     so clients can validate child rows generically. Cascade pivots
///     (<see cref="PivotResolutionAttribute" />) get the same treatment automatically; collections
///     that are server-managed or reference existing entities must not carry this attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ChildSchemaAttribute : Attribute;
