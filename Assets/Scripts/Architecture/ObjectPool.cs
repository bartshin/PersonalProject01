using System;
using UnityEngine;
using UnityPool = UnityEngine.Pool;

namespace Architecture
{
  public abstract class ObjectPool<T> where T: class, IPooedObject
  {
    UnityPool.ObjectPool<T> pool;
    protected bool collectionCheck;
    const int DEFAULT_MAX_POOL_SIZE = 10000;

    public ObjectPool(int defaultPoolSize, int? maxPoolSize = DEFAULT_MAX_POOL_SIZE, bool collectionCheck = true)
    {
      if (defaultPoolSize <= 0) {
        throw new ArgumentException($"{nameof(defaultPoolSize)} must be greater than 0");
      }
      if (defaultPoolSize > maxPoolSize) {
        throw new ArgumentException($"{nameof(defaultPoolSize)} must be less than ${nameof(maxPoolSize)}");
      }

      this.pool = new UnityPool.ObjectPool<T>(
          this.createPooledObject,
          this.OnTakeFromPool,
          this.OnReturnedToPool,
          this.OnDestoryPoolObject,
          true,
          defaultPoolSize,
          maxPoolSize ?? DEFAULT_MAX_POOL_SIZE
          );
    }

    public T Get() {
      return (this.pool.Get());
    }

    abstract protected T CreatePooledObject();

    T createPooledObject() {
      T newObject = this.CreatePooledObject();
      newObject.OnDisabled += (pooledObject) => {
        this.pool.Release((pooledObject as T));
      };
      return (newObject);
    }

    protected virtual void OnReturnedToPool(T obj) {}

    protected virtual void OnTakeFromPool(T obj) {}

    protected virtual void OnDestoryPoolObject(T obj) { }

    protected void ReturnToPool(T obj) 
    {
      this.pool.Release(obj);
    }
  }
}
