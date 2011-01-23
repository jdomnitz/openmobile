using OpenMobile.Controls;

namespace OpenMobile.Controls
{
    public interface IContainer
    {
        OMControl this[int i] { get; }
        int controlCount { get; }
        bool Contains(OMControl control);
        int ofsetX { get; }
        int ofsetY { get; }
    }
}
