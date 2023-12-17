/*This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.

The Original Code is the LoginProtocol library.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s):
*/

namespace LoginProtocol.Database
{
    /// <summary>
    /// Represents a user in the DB with the following columns needed for SRP authentication.
    /// </summary>
    public class User
    {
        public string Username = string.Empty;
        public string Salt = string.Empty;
        public string Verifier = string.Empty;
    }
}
