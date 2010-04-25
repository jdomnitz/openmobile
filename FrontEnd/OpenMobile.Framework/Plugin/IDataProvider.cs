
using System;
namespace OpenMobile.Plugin
{
    /// <summary>
    /// Provides access to a data source or dumps data into the main database
    /// </summary>
    public interface IDataProvider:IBasePlugin
    {
        /// <summary>
        /// Forces the plugin to update the database with new data. Return false if not capable (no signal, no internet connection, etc.). Return true even on failed.
        /// </summary>
        /// <returns></returns>
        bool refreshData();
        /// <summary>
        /// Forces the plugin to update the database with new data. Return false if not capable (no signal, no internet connection, etc.). Return true even on failed.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        bool refreshData(string arg);
        /// <summary>
        /// Forces the plugin to update the database with new data. Return false if not capable (no signal, no internet connection, etc.). Return true even on failed.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        bool refreshData(string arg1, string arg2);
        /// <summary>
        /// Return 0 if no update occurred, return -1 if update failed, return 1 if update successful, return 2 if update in progress
        /// </summary>
        /// <returns></returns>
        int updaterStatus();
        /// <summary>
        /// When the data provider last updated its data
        /// </summary>
        DateTime lastUpdated{get;}
        /// <summary>
        /// Returns a string representing the type of data the plugin provides or the data source.
        /// </summary>
        /// <returns></returns>
        string pluginType();
    }
}
