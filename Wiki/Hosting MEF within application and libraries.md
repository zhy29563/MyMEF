# Hosting MEF within application and libraries

As you start to work with MEF one of the fundamental decisions you have to make is how MEF will be hosted. There are several factors which will impact your decision including whether you are building on Silverlight or the Desktop and whether you are building an application or a library.

The first concern you hit around MEF is how and where composition starts. MEF parts at the end of the day are simply classes (in our attributed model). Simply adding an Export attribute to a class does not cause it to get composed as something has to start the composition. That responsibility falls on the `CompositionContainer`. It uses one or more catalogs to discover the available parts in the system. Once the container is created, composition can begin. But it doesn’t begin until the container is asked to do work. This normally happens in the entry point of the application or application root, which in a WPF/Silverlight application would be the Application class.

There are several approaches to composition in MEF.

# Composing an existing part

The first approach is to pass an existing instance to MEF and ask it to compose it. The advantage of this approach is that it lets you declare imports on the existing application instance.

```c#
[Import]
public Window MainWindow { get; set; } // 在组合时导入

void App_Startup(object sender, StartupEventArgs e)
{
    var catalog = new AssemblyCatalog(typeof(App).Assembly);
    var container = new CompositionContainer(catalog);
    container.ComposeParts(this);
    
    // 用组合导入的实例进行赋值
    base.MainWindow = MainWindow;
    MainWindow.Show();
}
```

# Pulling on the container for an export

In this approach you pull on the container and ask MEF to create a part.

```c#
void App_Startup(object sender, StartupEventArgs e)
{
    var catalog = new AssemblyCatalog(typeof (App).Assembly);
    var container = new CompositionContainer(catalog);
    
    // 通过容器获取对象实例
    this.MainWindow = container.GetExportedValue<Window>();
    MainWindow.Show();
}
```

In the startup method above, I am creating a catalog with the current assembly, adding it to a container and then querying for the `MainWindow`. During the querying, MEF discovers the `MainWindow` (assuming it was exported) 

# Creating a standardized class for starting composition

In the previous examples, bootstrapping code is introduced into the startup of the application. This kind of approach works well for C/S applications that have a `centralized initialization point`. In the previous case the initialization code is very specific. With some refactoring I can create a standardized initialization mechanism such as `CompositionStarter` below which can be reused across different UI mediums including on the server.

```c#
public class CompositionStarter<TParams>
{
    private CompositionContainer _container;
    public CompositionStarter(params ComposablePartCatalog[] catalogs)
    {
        var aggregate = new AggregateCatalog(catalogs);
        _container = new CompositionContainer(aggregate);
    }

    public CompositionStarter(CompositionContainer container)
    {
        if (container == null)
            throw new ArgumentNullException("container");

        _container = container;
    }

    public CompositionStarter()
    {
        _container = new CompositionContainer(
            new AssemblyCatalog(Assembly.GetCallingAssembly()));
    }

    public void Start(TParams parameters)
    {
        var startables = _container.GetExportedValues<IStartable<TParams>>();
        foreach(var startable in startables)
        {
            startable.Start(parameters);
        }
    }

    public void Start()
    {
        Start(default(TParams));
    }

    public void Stop()
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Stop can only be called once");
        }

        _container.Dispose();
        _container = null;    

    }
}

[InheritedExport]
public interface IStartable<TParams>
{
    void Start(TParams parameters);
}
```

`CompositionStarter<TParam>` creates a container (or uses the one that you pass in). On the Start method it pulls all `IStartable<TParam>` exports from the container and invokes their Start methods passing in the parameters. With `CompositionStarter` in place, I can now introduce a WpfStartable and move the code for importing and displaying the window there.

```c#
public class WpfStartable : IStartable<object>
{
    [Import]
    public Window Window { get; set; }

    public void Start(object parameters)
    {
        Application.Current.MainWindow = Window;
        Window.Show();
    }
}
```

The app class can then be updated to use the following starter code:

```c#
void App_Startup(object sender, StartupEventArgs e)
{
    var starter = new CompositionStarter<object>();
    starter.Start();
}
```

The code is no longer app-specific, nor is it specific to a particular UI platform. The same code pattern also works for a Silverlight application, a WinForms application or a console app.

Notice that `CompositionStarter` pulls multiple `IStartable` implementations. This allows me to introduce other pluggable startup operations. 

### A note about Silverlight and PartInitializer

In Silverlight we have introduced PartInitializer which automatically bootstraps a shared container and composes an existing instance. This is primarily for usage in places such as XAML where objects are created by the XAML parser, but need to get composed. PartInitializer may be sufficient for your startup needs in Silverlight so if it works, don’t feel compelled that must use CompositionStarter, you don’t.

You may want to combine the two approaches. The benefits of doing this are you could have CompositionStarter bootstrap your app, while still using PartInitializer within XAML elements that need composition.

PartInitializer allows you to override its default container through the CompositionHost.InitializeContainer method. This means you can create a container that is shared by both PartInitializer and the CompositionStarter class.

```c#
void App_Startup(object sender, StartupEventArgs e)
{
    var catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetCallingAssembly()));
    var container = new CompositionContainer(catalog);
    CompositionHost.InitializeContainer(container);
    var starter = new CompositionStarter<object>();
    starter.Start();
}
```

# Using MEF within XAML in WPF

In the previous section I spoke about the PartInitializer API which we baked into Silverlight for composition in XAML, it lets you write code such as the following:

```c#
public partial class Dashboard : UserControl
{
    public MainPage()
    {
        InitializeComponent();
        PartInitializer.SatisfyImports(this);
        foreach (var widget in Widgets)
            TopWidgets.Items.Add(widget);
    }

    [ImportMany]
    public UserControl[] Widgets { get; set; }
}
```

Above Dashboard is a user control that is created within XAML. It imports a collection of widgets. It forces itself to be composed by calling PartInitializer.SatisfyImports(). 

This is great for Silverlight, but what about WPF? Although we don’t yet have it in the box, I’ve ported our PartInitializer implementation to WPF. The functionality is almost identical though instead of looking in the main application XAP, it looks in the bin folder of the application and an optional extensions folder. You can get it on my skydrive [here](http://cid-f8b2fd72406fb218.skydrive.live.com/self.aspx/blog/Composition.Initialization.Desktop.zip).

Alternatively you can have the control which hosts Dashboard compose it by calling ComposeParts on the container and pass it in. Another option is to pass a container through the visual tree as an attached property and then have a helper method / another attached property which allows Dashboard to get composed without depending on a static container. More on this in another post.

# Hosting MEF within a library

We've discussed an approach for hosting within an application, but what about a library? Should you use `CompositionStarter` then? I would err against that as it's not ideal. For example take the case of a rules engine that uses MEF to discover rules. The rules engine itself needs the rules, so delegating off to `IStartables` to get the rules adds extra complexity with little gain. Having the rules engine host a container which it uses to get the rules is not at all a problem. Here is one way to implement it.

```c#
public class RulesEngine
{
    private readonly CompositionContainer _container;
    private RulesEngineImports _rulesEngineImports;

    public RulesEngine()
    {
        if (_container == null)
            _container = new CompositionContainer(new AssemblyCatalog(Assembly.GetCallingAssembly()));

        var imports = new RulesEngineImports();
        _container.ComposeParts(imports);
    }

    public RulesEngine(CompositionContainer container)
    {
        _container = container;
    }

    public RulesEngine(params ComposablePartCatalog[] catalogs)
    {
        var aggregate = new AggregateCatalog(catalogs);
        _container = new CompositionContainer(aggregate);
    }

    private class RulesEngineImports
    {
        [ImportMany]
        public IRule[] Rules { get; set; }
    }
}
```

RulesEngine’s default ctor creates a default container though it accepts overrides for passing in a container and catalogs, this allows containers to be shared across other libraries that might use MEF. Once RulesEngine is created, the default ctor then creates a new RulesEngineImports class which it then composes. This class is basically a buddy class to get the imports that RulesEngine needs. 

**Note:** I could have put the imports directly on RulesEngine, but in Silverlight these have to be public members as private reflection is not supported. I don’t want to expose the engines internal implementation to the outside and I want the engine to be reused in SL and WPF. Using this approach allows that reuse by keeping the imports as an internal implementation detail.

## Why not use CommonServiceLocator?

I know there are going to be a bunch of folks reading this saying why should you couple the RulesEngine to MEF? Can’t you just use [CommonServiceLocator](http://commonservicelocator.codeplex.com/) to get the rules the engine needs? Yes you absolutely could do that, though it will limit the capabilities you can leverage from a specific container. For example MEF currently supports metadata views on imports, something that is currently specific to MEF, though Autofac is catching up quickly ![:-)](http://codebetter.com/glennblock/wp-includes/images/smilies/simple-smile.png) Additionally using CSL often results in icky service locatorish code all over the place, something we (the founders of CSL) never intended but which has happened.

An alternative to using CSL that supports any container is to have an app-specific IRulesEngineServiceAdapter export which returns all the dependencies / imports that the engine needs. The service adapter can use whichever IoC it chooses, and it can leverage the full capabilities of that container. The engine could still use MEF to discover the adapter and then use the adapter from there on. 

```c#
public class RulesEngine
{
    private readonly CompositionContainer _container;
    private IRulesEngineServiceAdapter _adapter;

    public RulesEngine()
    {
        if (_container == null)
            _container = new CompositionContainer(new AssemblyCatalog(Assembly.GetCallingAssembly()));

        _adapter = _container.GetExportedValue<IRulesEngineServiceAdapter>();
    }

    public RulesEngine(CompositionContainer container)
    {
        _container = container;
    }

    public RulesEngine(params ComposablePartCatalog[] catalogs)
    {
        var aggregate = new AggregateCatalog(catalogs);
        _container = new CompositionContainer(aggregate);
    }
}

```

In the MEF case, the adapter would simply be a part which imports the rules. For other containers the implementation would differ.

```c#
[InheritedExport]
public interface IRulesEngineServiceAdapter
{
   IRule[] Rules { get; set; }
}

public class MefRulesEngineServiceAdapter : IRulesEngineServiceAdapter
{
    [ImportMany]
    public IRule[] Rules { get; set;}
}
```

 

# Hosting MEF within a web application or a web service

Using MEF within a multi-request server application such as a web app or a WCF service is a whole other ball of wax. You can use CompositionStarter in limited scenarios as long as you are dealing with singletons shared across all requests. For more sophisticated scenarios you will need to create scoped per-request containers. That is outside of the scope of this post, but we will likely have future posts on this topic. 

**Update:** 

- We’ve posted a sample using MEF with web forms here: http://is.gd/cjysJ
- Scott Hanselman has a post on porting MVC’s Nerd Dinner to use MEF here: http://www.hanselman.com/blog/ExtendingNerdDinnerAddingMEFAndPluginsToASPNETMVC.aspx

 

# Recap

We’ve just dug in to what it means to host MEF within an application / library, and various approaches for doing so. My goal for this post was simply to illustrate a range of options, use them or leave them depending on whether or not they work for your scenario. Don’t try to force a square peg into a round hole!



