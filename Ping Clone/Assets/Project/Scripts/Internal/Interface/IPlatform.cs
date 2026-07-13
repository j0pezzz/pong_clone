using UnityEngine;

public interface IPlatform
{
    bool IsMobile { get; set; }
    string PlatformName { get; set; }
}
