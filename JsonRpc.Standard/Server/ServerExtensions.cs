﻿using System.Threading;
using System.Threading.Tasks;

namespace JsonRpc.Standard.Server
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ServerExtensions
    {
        /// <summary>
        /// Asynchronously starts the JSON RPC service host.
        /// </summary>
        /// <returns>A task that finishes when the server has stopped.</returns>
        public static Task RunAsync(this IJsonRpcServiceHost host)
        {
            return host.RunAsync(CancellationToken.None);
        }
    }
}