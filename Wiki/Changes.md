# MEF for Windows 8 Windows Store apps

In .NET 4.5 Beta, Windows 8 Windows Store apps could use a subset of MEF functionality from the full .NET Framework. To better align with the goals and scenarios of Windows Store apps, in .NET 4.5 RC MEF for Windows Store apps is consumed by installing the` _Microsoft.Composition_ `package.

The MEF implementation in `_Microsoft.Composition_` is not code-compatible with the MEF functionality in .NET 4.5 Beta.

For most applications, a few simple steps will be required to move to the new MEF version. These are outlined step-by-step below. A full list of changes follows.

# Project-level changes

## Referencing the new assemblies

The new assemblies are referenced using the Visual Studio Package Manager dialog.

- Right-click on your Windows Store application project

- Select **Manage NuGet Packages**
- Select **Include Prerelease Packages**
- Search for **Microsoft.Composition**
- Click **Install**

Alternatively, the Package Manager Console can be used:

> Install-Package Microsoft.Composition –Pre

## Namespace change

Use Visuals Studio's Find and Replace in Files dialog to replace the text:

`System.ComponentModel.Composition` with: `System.Composition`

**Rationale:** MEF for Windows Store apps is optimized for this model and therefore makes changes to some fundamental MEF types like `Import` and `Export`. Changing the namespace ensures that types can be uniquely identified and correct information can be found more easily.

# Part-level changes

## Import/Export visibility

Imported and exported members must be publicly visible in order for them to be composed. For example:

```c#
[Import]
private ILogger Logger { get; set; }
```

Must be updated to:

```c#
[Import]
public ILogger Logger { get; set; }
```

The same is true of importing constructors and property exports.

**Rationale:** Requiring that imports and exports are public ensures that parts behave consistently on all platforms, and that code generation techniques can be used to improve throughput.

### Unsupported features

Field exports and imports are not supported – use property imports and exports instead.

## Part creation policy

`[PartCreationPolicy(CreationPolicy.NonShared)]`is no longer required, as parts are non-shared by **default**. Parts that need to be shared are marked with the `[Shared]` attribute.

## IPartImportsSatisfiedNotification

This interface has been replaced with the `[OnImportsSatisfied]` attribute. For example, the following part:

```c#
[Export](Export)
public class APart : IPartImportsSatisfiedNotification
{
    public void OnImportsSatisfied() { }
}
```

Can be rewritten as:

```c#
[Export]
public class APart 
{
    [OnImportsSatisfied](OnImportsSatisfied)
    public void OnImportsSatisfied()
    { 
    }
}
```

**Rationale:** MEF's convention support is based on attributes. By marking the OnImportsSatisfied() method with an attribute, conventions can select this method. This was not possible with the interface-based approach.

## Format of Metadata Views

Metadata views must be concrete types rather than interfaces. For example, the metadata view:

```c#
public interface INamed
{
    string Name { get; set; }
}
```

Can be rewritten as:

```c#
public class Named
{
    public string Name { get; set; }
}
```

**Rationale:** This requirement arises from the absence of the `System.Reflection.Emit` namespace in .NET for Windows Store apps. Without the functionality in this namespace, metadata views based on interfaces cannot be generated.

# Hosting changes

## Container configuration

On the hosting side, a smaller API with fewer configuration points is presented. Assemblies and part types are added to a ContainerConfiguration (rather than a catalog) and from there a container can be created and exports requested.

{code:c#}
    var configuration = new ContainerConfiguration()
                        .WithAssembly(typeof(App).GetTypeInfo().Assembly);

    using (var container = configuration.CreateContainer())
    {
        var handler = container.GetExport<IMessageHandler>();
        handler.Handle(message);
    }
{code:c#}

The {{ContainerConfiguration}} class provides a method-chaining API to add types and assemblies to the container. There is no catalog concept, nor composition batches, nor any “container hierarchy” or composition scoping APIs.

_**Rationale:** The hosting APIs used in MEF in the full .NET Framework are designed to support open, extensible applications. This functionality is not required in Windows Store apps, and so the developer experience is simplified._

## Conventions and RegistrationBuilder

In .NET 4.5 MEF introduces the {{RegistrationBuilder}} class. On Windows Store apps the same functionality is available via {{ConventionBuilder}}.

_**Rationale:** ConventionBuilder is expected to evolve significantly in conjunction with the MEF implementation for Windows Store apps. Retaining the {{RegistrationBuilder}} name would make these differences confusing._

# MEF in Windows Store apps vs. .NET Framework

The table below summarizes high-level feature compatibility across MEF for Windows Store apps and the full framework.

||Functionality||Windows Store apps||Full||
|Catalogs|No|Yes|
|Recomposition|No|Yes|
|Rejection|No|Yes|
|Instance Exports|Yes|Yes|
|Static Exports|No|Yes|
|Property Exports|Yes|Yes|
|Field Exports|No|Yes|
|Importing Constructors|Yes|Yes|
|Property Imports|Yes|Yes|
|Field Imports|No|Yes|
|Method Exports|No|Yes|
|Internal Part Types|Yes|Yes|
|Non Public Imports/Exports|No|Yes|
|Export Metadata|Yes|Yes|
|Import Metadata Views|Yes|Yes|
|Part Metadata|Yes|Yes|
|Nested Containers|No|Yes|
|Composition Scope Definition|No|Yes|
|Lazy Imports|Yes|Yes|
|ExportFactory Imports|Yes|Yes|
|Import Many|Yes|Yes|
|Import Custom Collections|No|Yes|
|Contract Names|Yes|Yes|
|Import Sources|No|Yes|
|Cyclic Dependencies|Yes|Yes|
|Open Generics|Yes|Yes|
|IPartImportsSatisfiedNotification|No|Yes|
|Import Required Creation Policy|No|Yes|
|Interface Metadata Views|No|Yes|
|Concrete Metadata View s|Yes|Yes|
|Missing Metadata Ignored|No|Yes|
|Inherited Exports|No|Yes|
|PartCreationPolicyAttribute|No|Yes|
|CatalogReflectionContextAttribute|No|Yes|
|MetadataAttributeAttribute|Yes|Yes|
|PartNotDiscoverableAttribute|Yes|Yes|
|ICompositionService|No|Yes|
|CompositionOptions|No|Yes|