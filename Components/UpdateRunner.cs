using UnityEngine;

namespace Needleforge.Components;

internal class UpdateRunner : MonoBehaviour
{
    public event System.Action? OnUpdate;
    private void Update() => OnUpdate?.Invoke();
}
