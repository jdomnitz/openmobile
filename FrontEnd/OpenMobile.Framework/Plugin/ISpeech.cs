/*********************************************************************************
    This file is part of Open Mobile.

    Open Mobile is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Open Mobile is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Open Mobile.  If not, see <http://www.gnu.org/licenses/>.
 
    There is one additional restriction when using this framework regardless of modifications to it.
    The About Panel or its contents must be easily accessible by the end users.
    This is to ensure all project contributors are given due credit not only in the source code.
*********************************************************************************/

namespace OpenMobile.Plugin
{
    /// <summary>
    /// Speech Synthesis and/or Recognition
    /// </summary>
    public interface ISpeech:IBasePlugin
    {
        /// <summary>
        /// Listen for a speech command
        /// </summary>
        void listen();
        /// <summary>
        /// Stop listening for a speech command.  Note: Listen timeout should be handled internally by the plugin
        /// </summary>
        void stopListening();
        /// <summary>
        /// Load the specified speech recognition context
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool loadContext(string name);
        /// <summary>
        /// Unload the specified speech recognition context
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool unloadContext(string name);
        /// <summary>
        /// Adds an item to a speech context
        /// </summary>
        /// <param name="contextName"></param>
        /// <param name="recoText"></param>
        void addToContext(string contextName, string recoText);
        /// <summary>
        /// Speak the specified text.  Return true if successful
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">System.NotImplementedException</exception>
        bool speak(string text);
        /// <summary>
        /// Stop Speaking and purge the speech buffer
        /// </summary>
        void stopSpeaking();
        /// <summary>
        /// Is speech recognition supported
        /// </summary>
        bool RecognitionSupported { get; }
        /// <summary>
        /// Is speech synthesis supported
        /// </summary>
        bool SpeechSupported { get; }
        /// <summary>
        /// Gets a list of all available input sources on the system.
        /// </summary>
        string[] getMicrophones{ get; }
    }
}
