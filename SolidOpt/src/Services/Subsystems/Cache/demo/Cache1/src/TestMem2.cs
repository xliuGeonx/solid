/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */

using System;

namespace SolidOpt.Services.Subsystems.Cache.Demo.Cache1
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Runtime.Serialization.Formatters.Binary;
  using SolidOpt.Services.Subsystems.Cache;
  
  /// <summary>
  /// Description of TestMem2.
  /// </summary>
  public class TestMem2: TestBase
  {
    public TestMem2(): base()
    {
      this.cache = new CacheManager<int,ResultClass>(
        new CacheManager<int,ResultClass>.InitDelegate(Init),
        new CacheManager<int,ResultClass>.TouchDelegate(Touch),
        new CacheManager<int,ResultClass>.ValidateDelegate(Validate),
        new CacheManager<int,ResultClass>.CalculateDelegate(Calculate),
        new CacheManager<int,ResultClass>.UpdateDelegate(Update),
        new CacheManager<int,ResultClass>.DeleteDelegate(Delete)
      );
    }
    
    public virtual CacheManager<int,ResultClass>.CacheItem Init(int key, ResultClass value)
    {
      return new CacheManager<int,ResultClass>.MemoryCacheItem(value);
    }
    
    public virtual CacheManager<int,ResultClass>.CacheItem Touch(int key, CacheManager<int,ResultClass>.CacheItem item)
    {
      return item;
    }
    
    public virtual bool Validate(int key, CacheManager<int,ResultClass>.CacheItem item)
    {
      return key != 1;
    }
    
    public virtual CacheManager<int,ResultClass>.CacheItem Calculate(int key)
    {
      return new CacheManager<int,ResultClass>.MemoryCacheItem(ClassBuilder.BuildResult(0)); //key
    }
    
    public virtual void Update(int key,  CacheManager<int,ResultClass>.CacheItem oldItem)
    {
      oldItem.Value.Text += "!";
    }
    
    public virtual void Delete(int key, CacheManager<int,ResultClass>.CacheItem item)
    {
      //
    }
  }
}
