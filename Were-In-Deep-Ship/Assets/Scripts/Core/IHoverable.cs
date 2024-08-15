using UnityEngine;
public interface IHoverable
{
    void OnHover(Transform player);
    void Disable(Transform player = default);
}