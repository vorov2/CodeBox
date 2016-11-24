﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeBox.Core.CommandModel
{
    public sealed class CommandCatalog
    {
        private readonly Dictionary<Identifier, CommandMetadata> commands = new Dictionary<Identifier, CommandMetadata>();
        private readonly Dictionary<string, CommandMetadata> commandsAlias = new Dictionary<string, CommandMetadata>();

        private CommandCatalog()
        {

        }

        public void RegisterCommand(CommandMetadata cmd)
        {
            commands.Remove(cmd.Key);
            commands.Add(cmd.Key, cmd);
            commandsAlias.Remove(cmd.Alias);
            commandsAlias.Add(cmd.Alias, cmd);
        }

        public void RegisterCommands(IEnumerable<CommandMetadata> cmds)
        {
            foreach (var c in cmds)
                RegisterCommand(c);
        }

        public IEnumerable<CommandMetadata> EnumerateCommands()
        {
            return commands.Select(p => p.Value);
        }
        
        public CommandMetadata GetCommandByAlias(string alias)
        {
            CommandMetadata ret;
            commandsAlias.TryGetValue(alias, out ret);
            return ret;
        }

        public CommandMetadata GetCommandByKey(Identifier key)
        {
            CommandMetadata ret;
            commands.TryGetValue(key, out ret);
            return ret;
        }

        public static CommandCatalog Instance { get; } = new CommandCatalog();
    }
}
