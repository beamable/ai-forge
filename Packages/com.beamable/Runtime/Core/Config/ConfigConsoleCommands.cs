using Beamable.Common.Dependencies;
using Beamable.ConsoleCommands;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.Config
{
	[BeamableConsoleCommandProvider]
	public class ConfigConsoleCommands
	{
		private readonly IDependencyProvider _provider;
		public BeamableConsole Console => _provider.GetService<BeamableConsole>();

		[Preserve]
		public ConfigConsoleCommands(IDependencyProvider provider)
		{
			_provider = provider;
		}

		[BeamableConsoleCommand(nameof(Config), "Manipulate config values.", "CONFIG [list | get <name> | set <name> <value> | reset [name] | usefile [filename]]")]
		private string Config(params string[] args)
		{
			if (args.Length < 1)
			{
				return Console.Help(nameof(Config));
			}

			string command = args[0];
			if (command == "list")
			{
				string retVal = "Config Variables:";
				IEnumerator<string> values = ConfigDatabase.GetAllValueNames().GetEnumerator();
				while (values.MoveNext())
				{
					retVal += "\n   " + values.Current + " = " + ConfigDatabase.GetString(values.Current);
				}

				return retVal;
			}
			else if (command == "get")
			{
				if (args.Length < 2)
				{
					return Console.Help(nameof(Config));
				}

				string name = args[1];
				try
				{
					string value = ConfigDatabase.GetString(name);
					return name + " = " + value;
				}
				catch (KeyNotFoundException knfe)
				{
					return "Invalid config variable: " + name + " " + knfe.ToString();
				}
			}
			else if (command == "set")
			{
				if (args.Length < 2)
				{
					return Console.Help(nameof(Config));
				}

				string name = args[1];
				string value = args[2];
				try
				{
					ConfigDatabase.SetString(name, value);
					value = ConfigDatabase.GetString(name);
					return name + " = " + value;
				}
				catch (KeyNotFoundException knfe)
				{
					return "Invalid config variable: " + name + " " + knfe.ToString();
				}
			}
			else if (command == "reset")
			{
				if (args.Length < 2)
				{
					IEnumerator<string> values = ConfigDatabase.GetAllValueNames().GetEnumerator();
					while (values.MoveNext())
					{
						ConfigDatabase.Reset(values.Current);
					}

					return "Resetting all config variables...";
				}
				else
				{
					string name = args[1];
					try
					{
						ConfigDatabase.Reset(name);
						string value = ConfigDatabase.GetString(name);
						return name + " = " + value;
					}
					catch (KeyNotFoundException knfe)
					{
						return "Invalid config variable: " + name + " " + knfe.ToString();
					}
				}
			}
			else if (command == "usefile")
			{
				if (args.Length < 2)
				{
					return "File Name Needed";
				}
				else
				{
					string fileName = args[1];

					//Try some possible shortcuts
					if (fileName == "demo")
					{
						fileName = "config-demo";
					}

					if (fileName == "ci")
					{
						fileName = "config-ci";
					}

					if (fileName == "daily")
					{
						fileName = "config-daily";
					}

					if (fileName == "defaults")
					{
						fileName = "config-defaults";
					}

					try
					{
						TextAsset asset = Resources.Load(fileName) as TextAsset;
						if (asset == null)
						{
							return "Cannot find config file " + fileName + ".txt in Resource directory";
						}

						ConfigDatabase.SetPreferredConfigFile(fileName);
						ConfigDatabase.SetConfigValuesFromFile(fileName);
						//reset the player account, so that data will load from the new location.

						return "Config Set to: " + fileName +
							   " and Auth Token has been reset.  Restart Game to reconnect.";
					}
					catch (Exception e)
					{
						return "Invalid config file : " + fileName + " " + e.ToString();
					}
				}
			}
			else
			{
				string retVal = "Unrecognized Config Command: " + command;
				retVal += "\n\n";
				retVal += Console.Help(nameof(Config));
				return retVal;
			}
		}
	}
}
