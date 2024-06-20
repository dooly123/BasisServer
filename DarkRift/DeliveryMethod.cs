/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace DarkRift
{
    public enum DeliveryMethod : byte
    {
        //
        // Summary:
        //     Unreliable. Packets can be dropped, can be duplicated, can arrive without order.
        Unreliable = 4,
        //
        // Summary:
        //     Reliable. Packets won't be dropped, won't be duplicated, can arrive without order.
        ReliableUnordered = 0,
        //
        // Summary:
        //     Unreliable. Packets can be dropped, won't be duplicated, will arrive in order.
        Sequenced = 1,
        //
        // Summary:
        //     Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive
        //     in order.
        ReliableOrdered = 2,
        //
        // Summary:
        //     Reliable only last packet. Packets can be dropped (except the last one), won't
        //     be duplicated, will arrive in order. Cannot be fragmented
        ReliableSequenced = 3
    }
}
