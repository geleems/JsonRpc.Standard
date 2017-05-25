﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using JsonRpc.Standard;

namespace JsonRpc.Streams
{
    /// <summary>
    /// Represents a message reader that parses the message line-by-line from <see cref="TextReader"/>.
    /// </summary>
    public class ByLineTextMessageReader : QueuedMessageReader
    {
        private readonly SemaphoreSlim readerSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initialize a line-by-line message reader from <see cref="TextReader" />.
        /// </summary>
        /// <param name="reader">The underlying text reader.</param>
        public ByLineTextMessageReader(TextReader reader) : this(reader, null)
        {
        }

        /// <summary>
        /// Initialize a message reader from <see cref="TextReader" />.
        /// </summary>
        /// <param name="reader">The underlying text reader.</param>
        /// <param name="delimiter">
        /// The indicator for the end of a message.
        /// If the reader reads a line that is the same as this parameter, the current message is finished.
        /// Use <c>null</c> to indicate that each line, as long as it is not empty,
        /// should be treated a message.
        /// </param>
        public ByLineTextMessageReader(TextReader reader, string delimiter)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            Reader = reader;
            Delimiter = delimiter;
        }

        /// <summary>
        /// Initialize a line-by-line message reader from <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">The underlying stream. A <see cref="TextReader"/> will be created based on it.</param>
        public ByLineTextMessageReader(Stream stream) : this(stream, null)
        {
        }

        /// <summary>
        /// Initialize a message reader from <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">The underlying stream. A <see cref="TextReader"/> with UTF-8 encoding will be created based on it.</param>
        /// <param name="delimiter">
        /// The indicator for the end of a message.
        /// If the reader reads a line that is the same as this parameter, the current message is finished.
        /// Use <c>null</c> to indicate that each line, as long as it is not empty,
        /// should be treated a message.
        /// </param>
        public ByLineTextMessageReader(Stream stream, string delimiter)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            Reader = new StreamReader(stream);
            Delimiter = delimiter;
        }

        /// <summary>
        /// The underlying text reader.
        /// </summary>
        public TextReader Reader { get; }

        /// <summary>
        /// The indicator for the end of a message.
        /// </summary>
        /// <remarks>
        /// If the reader reads a line that is the same as this parameter, the current message is finished.
        /// Use <c>null</c> to indicate that each line, as long as it is not empty,
        /// should be treated a message.
        /// </remarks>
        public string Delimiter { get; }

        /// <inheritdoc />
        protected override async Task<Message> ReadDirectAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await readerSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (Delimiter == null)
                {
                    string line;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        line = await Reader.ReadLineAsync();
                        if (line == null) return null;
                    } while (string.IsNullOrWhiteSpace(line));
                    return Message.LoadJson(line);
                }
                else
                {
                    string line;
                    var builder = new StringBuilder();
                    do
                    {
                        if (builder.Length == 0) cancellationToken.ThrowIfCancellationRequested();
                        line = await Reader.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(line)) builder.AppendLine(line);
                    } while (line != null);
                    if (builder.Length == 0) return null;
                    return Message.LoadJson(builder.ToString());
                }
            }
            catch (ObjectDisposedException)
            {
                // Throws OperationCanceledException if the cancellation has already been requested.
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }
            finally
            {
                readerSemaphore.Release();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Reader.Dispose();
        }
    }
}