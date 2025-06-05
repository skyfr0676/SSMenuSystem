// -----------------------------------------------------------------------
// <copyright file="Log.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable MemberCanBePrivate.Global
namespace SSMenuSystem.Features;

using System;

using Discord;

/// <summary>
/// An internal logging class.
/// </summary>
internal static class Log
{
    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Info" /> level messages to the game console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Info(object message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Info, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Info" /> level messages to the game console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Info(string message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Info, ConsoleColor.Cyan);
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Debug" /> level messages to the game console.
    /// Server must have exiled_debug config enabled.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Debug(object message)
    {
        if (Plugin.Instance?.Config?.Debug ?? false)
        {
            Send($"[SSMenuSystem] {message}", LogLevel.Debug, ConsoleColor.Green);
        }
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Debug" /> level messages to the game console.
    /// Server must have exiled_debug config enabled.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Debug(string message)
    {
        if (Plugin.Instance?.Config?.Debug ?? false)
        {
            Send($"[SSMenuSystem] {message}", LogLevel.Debug, ConsoleColor.Green);
        }
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Warn" /> level messages to the game console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Warn(object message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Warn, ConsoleColor.Magenta);
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Warn" /> level messages to the game console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Warn(string message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Warn, ConsoleColor.Magenta);
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Error" /> level messages to the game console.
    /// This should be used to send errors only.
    /// It's recommended to send any messages in the catch block of a try/catch as errors with the exception string.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Error(object message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Error, ConsoleColor.DarkRed);
    }

    /// <summary>
    /// Sends a <see cref="F:Discord.LogLevel.Error" /> level messages to the game console.
    /// This should be used to send errors only.
    /// It's recommended to send any messages in the catch block of a try/catch as errors with the exception string.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Error(string message)
    {
        Send($"[SSMenuSystem] {message}", LogLevel.Error, ConsoleColor.DarkRed);
    }

    /// <summary>Sends a log message to the game console.</summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="level">The message level of importance.</param>
    /// <param name="color">The message color.</param>
    internal static void Send(object message, LogLevel level, ConsoleColor color = ConsoleColor.Gray)
    {
        SendRaw($"[{level.ToString().ToUpper()}] {message}", color);
    }

    /// <summary>Sends a log message to the game console.</summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="level">The message level of importance.</param>
    /// <param name="color">The message color.</param>
    internal static void Send(string message, LogLevel level, ConsoleColor color = ConsoleColor.Gray)
    {
        SendRaw("[" + level.ToString().ToUpper() + "] " + message, color);
    }

    /// <summary>Sends a raw log message to the game console.</summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="color">The <see cref="T:System.ConsoleColor" /> of the message.</param>
    internal static void SendRaw(object message, ConsoleColor color)
    {
        ServerConsole.AddLog(message.ToString(), color);
    }

    /// <summary>Sends a raw log message to the game console.</summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="color">The <see cref="T:System.ConsoleColor" /> of the message.</param>
    internal static void SendRaw(string message, ConsoleColor color)
    {
        ServerConsole.AddLog(message, color);
    }

    /// <summary>
    /// Sends an <see cref="M:Exiled.API.Features.Log.Error(System.Object)" /> with the provided message if the condition is false and stops the execution.
    /// <example> For example:
    /// <code>
    /// Player ply = Player.Get(2);
    /// Log.Assert(ply is not null, "The player with the id 2 is null");
    /// </code>
    /// results in it logging an error if the player is null and not continuing.
    /// </example>
    /// </summary>
    /// <param name="condition">The conditional expression to evaluate. If the condition is true it will continue.</param>
    /// <param name="message">The information message. The error and exception will show this message.</param>
    /// <exception cref="T:System.Exception">If the condition is false. It throws an exception stopping the execution.</exception>
    internal static void Assert(bool condition, object message)
    {
        if (condition)
        {
            return;
        }

        Error(message);
        throw new Exception(message.ToString());
    }
}