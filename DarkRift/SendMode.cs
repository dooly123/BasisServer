/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift
{
    public enum SendMode : byte
    {
        Unreliable = 0,
        Reliable = 1,
        Unsequenced = 2,
        UnreliableFragmented = 3,
        Instant = 4,
        //NoAlloc,
        //Unthrottled,
    }
}
