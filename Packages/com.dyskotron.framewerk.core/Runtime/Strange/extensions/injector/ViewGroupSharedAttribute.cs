/*
 * ViewGroupShared attribute for Framewerk
 *
 * Marks a property for view-group-scoped injection. When used alongside [Inject],
 * properties with this attribute receive a shared instance scoped to the view group,
 * rather than the global singleton.
 *
 * Example:
 *
 *     [Inject, ViewGroupShared]
 *     public SelectionChangedSignal SelectionSignal { get; set; }
 *
 * Behavior:
 * 1. When instantiating a ViewGroup, the system scans all mediators for [ViewGroupShared] properties
 * 2. For each unique type found, a new instance is created (per view group instantiation)
 * 3. That instance is SupplyTo'd to all mediators in the group
 * 4. Solo views (outside view groups) still get the global binding if one exists
 *
 * This is useful for dependencies that should be shared within a view group but isolated
 * from other view groups and the global context.
 */

using System;

[AttributeUsage(AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public class ViewGroupShared : Attribute
{
    public ViewGroupShared() { }
}
