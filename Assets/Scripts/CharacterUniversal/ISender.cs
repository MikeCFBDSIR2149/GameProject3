using UnityEngine;

namespace CharacterUniversal
{
    public interface ISender
    {
        Vector3 GetWorldPosition();
        bool IsAlive { get; }
    }
}


