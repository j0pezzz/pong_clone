using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Platform Settings", menuName = "Scriptable Objects/Create New Platform Settings")]
public class PlatformSettings : ScriptableObject, IPlatform
{
    [field: SerializeField] public bool IsMobile { get; set; }
    [field: SerializeField] public string PlatformName { get; set; }
}
