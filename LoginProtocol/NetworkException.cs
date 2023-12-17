/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the LoginProtocol library.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s):
*/

using System;

namespace LoginProtocol
{
    /// <summary>
    /// Thrown when something network-related goes awry!
    /// </summary>
    internal class NetworkException : Exception
    {
        /// <summary>
        /// Creates a NetworkException.
        /// </summary>
        /// <param name="Message">The message that goes with the exception.</param>
        public NetworkException(string Message) : base(Message)
        {

        }
    }
}
