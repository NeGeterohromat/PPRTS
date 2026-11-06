using Leopotam.EcsProto;
using Leopotam.EcsProto.QoL;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class HighlightSystem : IProtoRunSystem
{
    [DI] private UnityUpdateAspect aspect;
    public void Run()
    {
        foreach (var entity in aspect.UnhighlightEventIterator)
        {
            ref var goComp = ref aspect.GameObjectPool.Get(entity);
            goComp.Obj.GetComponent<HighlightableObject>().Unhighlight();
            aspect.HighlightedTagPool.Del(entity);
            aspect.UnhighlightEventPool.Del(entity);
        }

        foreach (var e in aspect.HighlightedTagIterator)
        {
            ref var highlightTag = ref aspect.HighlightedTagPool.Get(e);
            if (!highlightTag.IsGOHighlighted)
            {
                ref var goComp = ref aspect.GameObjectPool.Get(e);
                goComp.Obj.GetComponent<HighlightableObject>().Highlight();
                highlightTag.IsGOHighlighted = true;
            }
        }
    }
}
