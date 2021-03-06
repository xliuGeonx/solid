/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;
using System.IO;

using SolidOpt.Services;

namespace SolidOpt.Services.Loader
{
  public class Loader
  {
    private PluginServiceContainer servicesContainer = new PluginServiceContainer();
    public PluginServiceContainer ServicesContainer {
      get { return servicesContainer; }
      set { servicesContainer = value; }
    }
    
    public Loader()
    {
    }

    public int Run(string[] args)
    {
      if (args.Length < 1) return -1;
      
      LoadServices(args);
      Transform(args);
      
      //TODO: Cache invocation
      
      return 0;
    }
    
    public virtual void LoadServices(string[] args)
    {
      string defaultDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
      if (Directory.Exists(defaultDir))
        ServicesContainer.AddPlugins(defaultDir);
      
      ServicesContainer.LoadPlugins();
    }
    
    public virtual void Transform(string[] args)
    {
      
    }
  }
}
