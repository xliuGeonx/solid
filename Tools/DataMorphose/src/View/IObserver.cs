/*
 * $Id$
 * It is part of the SolidOpt Copyright Policy (see Copyright.txt)
 * For further details see the nearest License.txt
 */
using System;

namespace DataMorphose.View
{
  public interface IObserver 
  {
    void Update(Model.DataModel model);
  }
}
