using System;

namespace Architecture
{
  public interface IPooedObject 
  {
    public Action<IPooedObject> OnDisabled { get; set; }
  }
}
