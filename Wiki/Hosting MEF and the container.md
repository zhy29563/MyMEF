## Hosting MEF in an application
Hosting MEF in an application involves creating an instance of the **{{CompositionContainer}}**, adding Composable Parts to it, including the application host itself and then composing.

Below are the steps involved with hosting.

1. Create a host class. In the sample below we are using a console application, so the host is the Program class. 

2. Add a reference to the **{{System.ComponentModel.Composition}}** assembly. 

3. Add the following using statement: **{{using System.ComponentModel.Composition;}}**

4. Add a **{{Compose()}}** method which creates an instance of the container and composes the host.

5. Add a **{{Run()}}** method which calls **{{Compose()}}**;

6. In the **{{Main()}}** method instantiate the host class.

    ```c#
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Reflection;
    using System;
    
    public class Program
    {
        public static void Main(string[]() args)
        {
            Program p = new Program();
            p.Run();
        }
        
        public void Run()
        {
            Compose();
        }
        
        private void Compose()
        {
            var container = new CompositionContainer();
            container.ComposeParts(this);
        }
    }
    ```

    

7. Define one or more exports which the host will import. In the code below we've defined an **{{IMessageSender}}** interface. We've also defined an **{{EmailSender}}** Composable Part that exports an **{{IMessageSender}}** which it declares through the **{{[System.ComponentModel.Composition.Export](System.ComponentModel.Composition.Export)}}** attribute.

   ```c#
   public interface IMessageSender
   {
       void Send(string message);
   }
   
   [Export(typeof(IMessageSender))](Export(typeof(IMessageSender)))
   public class EmailSender : IMessageSender
   {
       public void Send(string message)
       {
           Console.WriteLine(message);
       }
   }
   ```

8. Add properties to the host class for each import which are decorated with the **{{[System.ComponentModel.Composition.Import](System.ComponentModel.Composition.Import)}}** attribute. For example below is an import for **{{IMessageSender}}** that is added to the **{{Program}}** class.

   ```c#
   [Import]
   public IMessageSender MessageSender { get; set; }
   ```

9. Add parts to the container. In MEF, there are several ways in which to do this. One way is by directly adding existing Composable Part instances, while a second, more common approach is through the use of catalogs, which we will mention after the section below.

**Adding parts directly to the container** 

In the **{{Compose()}}** method manually add each Composable Part by using the **{{ComposeParts()}}** extension method. In the example below, an instance of the **{{EmailSender}}** added to the container along with the current instance of the Program class which imports it.

```c#
private void Compose()
{
    var container = new CompositionContainer();
    container.ComposeParts(this, new EmailSender());
}
```

**Adding to the container using an AssemblyCatalog**

By using the catalog, the container handles creating parts automatically rather than them having to be added explicitly. To do this, create a catalog in the **{{Compose()}}** method. Next create a resolver off of the catalog and pass it to the container's constructor. 

In the example below an **{{AssemblyCatalog}}** is created with the executing assembly passed into the constructor. We're not adding an instance of **{{EmailSender}}** as it it will be discovered in the catalog that was passed for the current assembly.

```c#
private void Compose()
{
    var catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
    var container = new CompositionContainer(catalog);
    container.ComposeParts(this);
}
```

After following each of the above steps, the code should look as shown below.

```c#
using System.ComponentModel.Composition;
  using System.ComponentModel.Composition.Hosting;
  using System.Reflection;
  using System;

  public class Program
  {
    [Import](Import)
    public IMessageSender MessageSender { get; set; }

    public static void Main(string[]() args)
    {
      Program p = new Program();
      p.Run();
    }
    
    public void Run()
    {
      Compose();
      MessageSender.Send("Message Sent");
    }
    
    private void Compose()
    {
      AssemblyCatalog catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
      var container = new CompositionContainer(catalog);
      container.ComposeParts(this);
    }
  }

  public interface IMessageSender
  {
    void Send(string message);
  }

  [Export(typeof(IMessageSender))](Export(typeof(IMessageSender)))
  public class EmailSender : IMessageSender
  {
    public void Send(string message)
    {
      Console.WriteLine(message);
    }
  }
```




When the above code is compiled and executed, the application will be composed with its **{{IMessageSender}}** import. The **{{Send()}}** method will then be called which will output "Message Sent" on the console.

**Note:** For more advanced scenarios around hosting, see this post: [uri:http://codebetter.com/blogs/glenn.block/archive/2010/01/15/hosting-mef-within-your-applications.aspx](uri_http___codebetter.com_blogs_glenn.block_archive_2010_01_15_hosting-mef-within-your-applications.aspx)
