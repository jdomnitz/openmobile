
namespace OpenMobile.Controls
{
    /// <summary>
    /// Any control implementing this interface will not be clickable
    /// </summary>
    public interface INotClickable
    {
        /// <summary>
        /// Return true if the point should be clickable
        /// Return false if the click should fall through
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IsPointClickable(int x, int y);
    }
}
