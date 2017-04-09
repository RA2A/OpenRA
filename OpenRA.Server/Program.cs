#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenRA.Primitives;
using OpenRA.Support;

namespace OpenRA.Server
{
	class Program
	{
		static TcpClient orahostClient;
		static StreamReader orahostReader;
		static StreamWriter orahostWriter;
		static Server server;

		static void Main(string[] args)
		{
			var arguments = new Arguments(args);
			Log.AddChannel("debug", "dedicated-debug.log");
			Log.AddChannel("perf", "dedicated-perf.log");
			Log.AddChannel("server", "dedicated-server.log");
			Log.AddChannel("nat", "dedicated-nat.log");

			// Special case handling of Game.Mod argument: if it matches a real filesystem path
			// then we use this to override the mod search path, and replace it with the mod id
			var modArgument = arguments.GetValue("Game.Mod", null);
			var explicitModPaths = new string[0];
			if (modArgument != null && (File.Exists(modArgument) || Directory.Exists(modArgument)))
			{
				explicitModPaths = new[] { modArgument };
				arguments.ReplaceValue("Game.Mod", Path.GetFileNameWithoutExtension(modArgument));
			}

			// HACK: The engine code assumes that Game.Settings is set.
			// This isn't nearly as bad as ModData, but is still not very nice.
			Game.InitializeSettings(arguments);
			var settings = Game.Settings.Server;

			var mod = Game.Settings.Game.Mod;
			var modSearchArg = arguments.GetValue("Engine.ModSearchPaths", null);
			var modSearchPaths = modSearchArg != null ?
				FieldLoader.GetValue<string[]>("Engine.ModsPath", modSearchArg) :
				new[] { Path.Combine(".", "mods"), Path.Combine("^", "mods") };
			var mods = new InstalledMods(modSearchPaths, explicitModPaths);

			// HACK: The engine code *still* assumes that Game.ModData is set
			var modData = Game.ModData = new ModData(mods[mod], mods);
			modData.MapCache.LoadMaps();

			settings.Map = modData.MapCache.ChooseInitialMap(settings.Map, new MersenneTwister());

			// orahost
			TypeDictionary serverTraits = new TypeDictionary();
			string orahostAddress = arguments.GetValue("orahost.address", null);
			int orahostPort;
			if (orahostAddress != null && int.TryParse(arguments.GetValue("orahost.port", null), out orahostPort))
			{
				Console.WriteLine("[{0}] Connecting to orahost: {1}:{2}", DateTime.Now.ToString(settings.TimestampFormat), orahostAddress, orahostPort);
				orahostClient = new TcpClient(orahostAddress, orahostPort);
				var networkStream = orahostClient.GetStream();
				orahostReader = new StreamReader(networkStream);
				orahostWriter = new StreamWriter(networkStream);

				WriteOrahost("connect " + settings.ListenPort);
				Console.WriteLine("[{0}] Orahost connection established", DateTime.Now.ToString(settings.TimestampFormat));

				new Thread(() => {
					while (true) ReadOrahost();
				}).Start();

				serverTraits.Add(new OrahostListener());
			}

			Console.WriteLine("[{0}] Starting dedicated server for mod: {1}", DateTime.Now.ToString(settings.TimestampFormat), mod);
			while (true)
			{
				server = new Server(new IPEndPoint(IPAddress.IPv6Any, settings.ListenPort), settings, modData, true, serverTraits);
				WriteOrahost("up");

				while (true)
				{
					Thread.Sleep(1000);
					if (server.State != ServerState.ShuttingDown) WriteOrahost("info " + server.State + " " + server.Conns.Count);
					if (server.State == ServerState.GameStarted && server.Conns.Count < 1)
					{
						Console.WriteLine("[{0}] No one is playing, shutting down...", DateTime.Now.ToString(settings.TimestampFormat));
						WriteOrahost("down");
						server.Shutdown();
						break;
					}
				}

				Console.WriteLine("[{0}] Starting a new server instance...", DateTime.Now.ToString(settings.TimestampFormat));
			}
		}

		public static void WriteOrahost(string message)
		{
			if (orahostClient != null)
			{
				orahostWriter.WriteLine(message);
				orahostWriter.Flush();
			}
		}

		public static void ReadOrahost()
		{
			if (orahostClient == null) return;

			switch (orahostReader.ReadLine())
			{
			case "exit":
				server.Shutdown();

				if (orahostClient != null)
				{
					WriteOrahost("exit");
					orahostReader.Close();
					orahostWriter.Close();
					orahostClient.Close();
					orahostClient = null;
				}

				Environment.Exit(0);
				break;
			}
		}
	}
}
