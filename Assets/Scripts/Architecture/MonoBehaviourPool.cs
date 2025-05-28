using UnityEngine;

namespace Architecture
{
  public class MonoBehaviourPool<T>: ObjectPool<T> where T: MonoBehaviour, IPooedObject
  {

    GameObject prefab;

    public MonoBehaviourPool(int poolSize, int? maxPoolSize = null, GameObject prefab = null): base(poolSize, maxPoolSize)
    { 
      this.prefab = prefab;
    }

    protected override T CreatePooledObject()
    {
      var gameObject = this.prefab != null ?
        Object.Instantiate(this.prefab):
        new GameObject(nameof(T));
      T monoBehaviour = this.prefab != null ?
        gameObject.GetComponent<T>():
        gameObject.AddComponent<T>();
      return (monoBehaviour);
    }

    protected override void OnTakeFromPool(T obj) 
    {
      obj.gameObject.SetActive(true);
    }

    protected override void OnReturnedToPool(T obj)
    {
      if (obj.gameObject.activeSelf) {
        obj.gameObject.SetActive(false);
      }
    }
  }
}
