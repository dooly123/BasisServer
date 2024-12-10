/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkRift.Server.Plugins.Commands;

namespace DarkRift
{
    /// <summary>
    ///     Message class for all messages sent through DarkRift.
    /// </summary>
    /// <remarks>
    ///     Since each message is handled by single, separate threads this class is not thread safe.
    /// </remarks>
    public sealed class Message : IDisposable
    {

        /// <summary>
        ///     The buffer behind the message.
        /// </summary>
        private IMessageBuffer buffer;

        /// <summary>
        ///     The number of bytes of data in this message.
        /// </summary>
        public int DataLength => buffer.Count;

        /// <summary>
        ///     Are setters on this object disabled?
        /// </summary>
        // TODO Readonly isn't really needed now that we return copied instances from event args
        public bool IsReadOnly { get; private set; }

        /// <summary>
        ///     The tag of the message.
        /// </summary>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public ushort Tag
        {
            get => tag;

            set
            {
                if (IsReadOnly)
                    throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");
                else
                    tag = value;
            }
        }

        private ushort tag;

        /// <summary>
        ///     Random number generator for each thread.
        /// </summary>
        [ThreadStatic]
        private static Random random;

        /// <summary>
        ///     Whether this message is currently in an object pool waiting or not.
        /// </summary>
        private volatile bool isCurrentlyLoungingInAPool;

        /// <summary>
        ///     Creates a new message with the given tag and an empty payload.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        public static Message CreateEmpty(ushort tag)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;
            message.buffer = MessageBuffer.Create(0);
            message.tag = tag;
            return message;
        }

        /// <summary>
        ///     Creates a new message with the given tag and writer.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        /// <param name="writer">The initial data in the message.</param>
        public static Message Create(ushort tag, DarkRiftWriter writer)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;
            message.buffer = writer.ToBuffer();
            message.tag = tag;
            return message;
        }

        /// <summary>
        ///     Creates a new message with the given tag and serializable object.
        /// </summary>
        /// <param name="tag">The tag the message has.</param>
        /// <param name="obj">The initial object in the message data.</param>
        public static Message Create<T>(ushort tag, T obj) where T : IDarkRiftSerializable
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            message.IsReadOnly = false;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                message.buffer = writer.ToBuffer();
            }

            message.tag = tag;
            return message;
        }

        /// <summary>
        ///     Creates a new message from the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the message.</param>
        /// <param name="isReadOnly">Whether the message should be created read-only or not.</param>
        internal static Message Create(IMessageBuffer buffer, bool isReadOnly)
        {
            Message message = ObjectCache.GetMessage();

            message.isCurrentlyLoungingInAPool = false;

            // We clone the message buffer so we can modify its properties safely
            message.buffer = buffer.Clone();

            // Adjust buffer offset and count
            int headerLength = 3; // Previously depended on ping messages, now fixed
            message.buffer.Offset = buffer.Offset + headerLength;
            message.buffer.Count = buffer.Count - headerLength;

            message.IsReadOnly = isReadOnly;

            message.tag = BigEndianHelper.ReadUInt16(buffer.Buffer, buffer.Offset + 1);

            return message;
        }

        /// <summary>
        ///     Creates a new Message. For use from the ObjectCache.
        /// </summary>
        internal Message()
        {
            
        }

        /// <summary>
        ///     Clears the data in this message.
        /// </summary>
        public void Empty()
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            // To avoid corrupting the shared memory just get rid of the buffer and create a new one
            buffer.Dispose();
            buffer = MessageBuffer.Create(0);
        }

        /// <summary>
        ///     Creates a DarkRiftReader to read the data in the message.
        /// </summary>
        /// <returns>A DarkRiftReader for the message.</returns>
        public DarkRiftReader GetReader()
        {
            // Clone the buffer so the reader has it's own lifecycle for the underlying memory
            return DarkRiftReader.Create(buffer.Clone());
        }

        /// <summary>
        ///     Serializes a <see cref="DarkRiftWriter"/> into the data of this message.
        /// </summary>
        /// <param name="writer">The writer to serialize.</param>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public void Serialize(DarkRiftWriter writer)
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            buffer.Dispose();
            buffer = writer.ToBuffer();
        }

        /// <summary>
        ///     Deserializes the data to the given object type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>() where T : IDarkRiftSerializable, new()
        {
            using (DarkRiftReader reader = GetReader())
                return reader.ReadSerializable<T>();
        }

        /// <summary>
        ///     Deserializes the data to the given object.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to.</typeparam>
        /// <param name="t">The object to deserialize the data into.</param>
        public void DeserializeInto<T>(ref T t) where T : IDarkRiftSerializable
        {
            using (DarkRiftReader reader = GetReader())
                reader.ReadSerializableInto<T>(ref t);
        }

        /// <summary>
        ///     Serializes an object into the data of this message.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <exception cref="AccessViolationException">If the message is readonly.</exception>
        public void Serialize<T>(T obj) where T : IDarkRiftSerializable
        {
            if (IsReadOnly)
                throw new AccessViolationException("Message is read-only. This property can only be set when IsReadOnly is false. You may want to create a writable instance of this Message using Message.Clone().");

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(obj);

                Serialize(writer);
            }
        }
        public bool IsCommandMessage => Tag == BasisTags.Configure || Tag == BasisTags.Identify;

        /// <summary>
        ///     Converts this message into a buffer.
        /// </summary>
        /// <returns>The buffer.</returns>
        //TODO DR3 Make this return an IMessageBuffer
        internal MessageBuffer ToBuffer()
        {
            int headerLength = 3; // Fixed header length, no conditional logic needed
            int totalLength = headerLength + DataLength;

            MessageBuffer buffer = MessageBuffer.Create(totalLength);
            buffer.Count = totalLength;
            BigEndianHelper.WriteBytes(buffer.Buffer, buffer.Offset + 1, tag);

            // Due to poor design, here's an unavoidable memory copy! Hooray!
            Buffer.BlockCopy(this.buffer.Buffer, this.buffer.Offset, buffer.Buffer, buffer.Offset + headerLength, this.buffer.Count);

            return buffer;
        }

        /// <summary>
        ///     Performs a shallow copy of the message.
        /// </summary>
        /// <returns>A new instance of the message.</returns>
        public Message Clone()
        {
            Message message = ObjectCache.GetMessage();

            //We don't want to give a reference to our buffer so we need to clone it
            message.buffer = buffer.Clone();
            message.tag = tag;
            return message;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Message with tag '{Tag}' and {DataLength} bytes of data.";
        }

        /// <summary>
        ///     Recycles this object back into the pool.
        /// </summary>
        public void Dispose()
        {
            buffer.Dispose();

            ObjectCache.ReturnMessage(this);
            isCurrentlyLoungingInAPool = true;
        }

        /// <summary>
        ///     Finalizer so we can inform the cache system we were not recycled correctly.
        /// </summary>
        ~Message()
        {
            if (!isCurrentlyLoungingInAPool)
                ObjectCacheHelper.MessageWasFinalized();
        }
    }
}
