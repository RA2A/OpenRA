#region Copyright & License Information
/*
 * CnP of FindResources.cs of OpenRA... erm... Not quite, anymore!
 * Modded by Boolbada of OP Mod
 * 
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

/*
This one itself doesn't need engine mod.
The slave harvester's docking however, needs engine mod.
*/

using System.Collections.Generic;
using System.Drawing;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Pathfinder;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Yupgi_alert.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Yupgi_alert.Activities
{
	public class SpawnerHarvesterHarvest : Activity
	{	
		readonly SpawnerHarvesterMaster harv;
		readonly SpawnerHarvesterMasterInfo harvInfo;
		readonly Mobile mobile;
		readonly MobileInfo mobileInfo;
		readonly ResourceClaimLayer claimLayer;
		readonly IPathFinder pathFinder;
		readonly DomainIndex domainIndex;
		readonly GrantConditionOnDeploy deploy;

		CPos? avoidCell;

		public SpawnerHarvesterHarvest(Actor self)
		{
			harv = self.Trait<SpawnerHarvesterMaster>();
			harvInfo = self.Info.TraitInfo<SpawnerHarvesterMasterInfo>();
			mobile = self.Trait<Mobile>();
			mobileInfo = self.Info.TraitInfo<MobileInfo>();
			deploy = self.Trait<GrantConditionOnDeploy>();
			claimLayer = self.World.WorldActor.TraitOrDefault<ResourceClaimLayer>();
			pathFinder = self.World.WorldActor.Trait<IPathFinder>();
			domainIndex = self.World.WorldActor.Trait<DomainIndex>();
		}

		public SpawnerHarvesterHarvest(Actor self, CPos avoidCell)
			: this(self)
		{
			this.avoidCell = avoidCell;
		}

		Activity UndeployAndGo(Actor self, out MiningState state)
		{
			state = MiningState.Scan;
			QueueChild(new UndeployForGrantedCondition(self, deploy));
			return this;
		}

		Activity ScanTick(Actor self, out MiningState state)
		{
			if (ChildActivity != null)
			{
				ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);
				state = MiningState.Scan;
				return this;
			}

			var closestHarvestablePosition = ClosestHarvestablePos(self, harvInfo.LongScanRadius);

			// No suitable resource field found.
			// We only have to wait for resource to regen.
			if (!closestHarvestablePosition.HasValue)
			{
				var randFrames = self.World.SharedRandom.Next(100, 175);

				// Avoid creating an activity cycle
				QueueChild(new Wait(randFrames));
				state = MiningState.Scan;
				return this;
			}

			//// ... Don't claim resource layer here. Slaves will claim by themselves.

			// If not given a direct order, assume ordered to the first resource location we find:
			if (!harv.LastOrderLocation.HasValue)
				harv.LastOrderLocation = closestHarvestablePosition;

			self.SetTargetLine(Target.FromCell(self.World, closestHarvestablePosition.Value), Color.Red, false);

			// Calculate best depoly position.
			var deployPosition = CalcDeployPosition(self, closestHarvestablePosition.Value);

			// Just sit there until we can. Won't happen unless the map is filled with units.
			if (deployPosition == null)
			{
				QueueChild(new Wait(harvInfo.KickDelay));
				state = MiningState.Scan;
				return this;
			}

			// I could be in deployed state and given this order.
			if (deploy.DeployState == DeployState.Deployed)
			{
				if ((deployPosition.Value - self.Location).LengthSquared <= harvInfo.KickScanRadius * harvInfo.KickScanRadius)
				{
					// New target near enough. Stay.
					state = MiningState.Mining;
					return this;
				}
				else
				{
					return UndeployAndGo(self, out state);
				}
			}

			// TODO: The harvest-deliver-return sequence is a horrible mess of duplicated code and edge-cases
			var notify = self.TraitsImplementing<INotifyHarvesterAction>();
			foreach (var n in notify)
				n.MovingToResources(self, deployPosition.Value, this);

			state = MiningState.TryDeploy;

			// This gives glitch. If you repeatedly on an ore target then
			// the child Move() will glitch out and the harvester will be positioned in illegal places.
			// QueueChild(mobile.MoveTo(deployPosition.Value, 2));
			// Instead of queing, we RETURN MOVE.
			// This doesn't break the graph and will work fine (as "bad" codes did in older ORA engine).
			var move = mobile.MoveTo(deployPosition.Value, 2);
			move.Queue(this);
			return move;
		}

		Activity TryDeployTick(Actor self, out MiningState state)
		{
			// Wait for child wait activity to be done.
			// Could be wait or could be move to.
			if (ChildActivity != null)
			{
				ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);
				state = MiningState.TryDeploy;
				return this;
			}

			if (!deploy.IsValidTerrain(self.Location))
			{
				// If we can't deploy, go back to scan state so that we scan try deploy again.
				state = MiningState.Scan;
				return this;
			}

			// Issue deploy order and enter deploying state.
			if (deploy.DeployState == DeployState.Undeployed)
			{
				IsInterruptible = false;
				QueueChild(new DeployForGrantedCondition(self, deploy));
			}

			state = MiningState.Deploying;
			return this;
		}

		Activity DeployingTick(Actor self, out MiningState state)
		{
			// Deploying in progress
			if (ChildActivity != null)
			{
				ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);
				state = MiningState.Deploying;
				return this;
			}

			// deploy failure.
			if (deploy.DeployState == DeployState.Undeployed)
			{
				QueueChild(new Wait(15));
				state = MiningState.TryDeploy;
				return this;
			}

			state = MiningState.Mining;
			return this;
		}

		Activity MiningTick(Actor self, out MiningState state)
		{
			// Let the harvester become idle so it can shoot enemies.
			// Tick in SpawnerHarvester trait will kick activity back to KickTick.
			state = MiningState.Mining;
			return NextActivity;
		}

		Activity KickTick(Actor self, out MiningState state)
		{
			var closestHarvestablePosition = ClosestHarvestablePos(self, harvInfo.KickScanRadius);
			if (closestHarvestablePosition.HasValue)
			{
				// I may stay mining.
				state = MiningState.Mining;
				return NextActivity;
			}

			// get going
			harv.LastOrderLocation = null;
			return UndeployAndGo(self, out state);
		}

		public override Activity Tick(Actor self)
		{
			if (IsCanceled)
				return NextActivity;

			if (NextInQueue != null)
				return NextInQueue;

			// Erm... looking at this, I could split these into separte activites...
			// I prefer finite state machine style though...
			// I can see what is going on at high level in this single place -_-
			// I think this is less horrible than OpenRA FindResources... stuff.
			// We are losing one tick, but so what?
			// If this loss isn't acceptable, call ATick() from BTick() or something.
			switch (harv.MiningState)
			{
				case MiningState.Scan:
					return ScanTick(self, out harv.MiningState);
				case MiningState.TryDeploy:
					return TryDeployTick(self, out harv.MiningState);
				case MiningState.Deploying:
					return DeployingTick(self, out harv.MiningState);
				case MiningState.Mining:
					return MiningTick(self, out harv.MiningState);
				case MiningState.Kick:
					return KickTick(self, out harv.MiningState);
				default:
					Game.Debug("SpawnHarvesterFindResources.cs in invalid state!");
					return null;
			}
		}

		// Find a nearest deployable position from harvestablePos
		CPos? CalcDeployPosition(Actor self, CPos harvestablePos)
		{
			// FindTilesInAnnulus gives sorted cells by distance :) Nice.
			foreach (var tile in self.World.Map.FindTilesInAnnulus(harvestablePos, 0, harvInfo.DeployScanRadius))
				if (deploy.IsValidTerrain(tile) && mobile.CanEnterCell(tile))
					return tile;

			// Try broader search if unable to find deploy location
			foreach (var tile in self.World.Map.FindTilesInAnnulus(harvestablePos, harvInfo.DeployScanRadius, harvInfo.LongScanRadius))
				if (deploy.IsValidTerrain(tile) && mobile.CanEnterCell(tile))
					return tile;

			return null;
		}

		/// <summary>
		/// Using LastOrderLocation and self.Location as starting points,
		/// perform A* search to find the nearest accessible and harvestable cell.
		/// </summary>
		CPos? ClosestHarvestablePos(Actor self, int searchRadius)
		{
			if (harv.CanHarvestCell(self, self.Location) && claimLayer.CanClaimCell(self, self.Location))
				return self.Location;

			// Determine where to search from and how far to search:
			var searchFromLoc = harv.LastOrderLocation ?? self.Location;
			var searchRadiusSquared = searchRadius * searchRadius;

			// Find any harvestable resources:
			var passable = (uint)mobileInfo.GetMovementClass(self.World.Map.Rules.TileSet);
			List<CPos> path;
			using (var search = PathSearch.Search(self.World, mobileInfo, self, true,
				loc => domainIndex.IsPassable(self.Location, loc, mobileInfo, passable)
					&& harv.CanHarvestCell(self, loc) && claimLayer.CanClaimCell(self, loc))
				.WithCustomCost(loc =>
				{
					if ((avoidCell.HasValue && loc == avoidCell.Value) ||
						(loc - self.Location).LengthSquared > searchRadiusSquared)
						return int.MaxValue;

					return 0;
				})
				.FromPoint(self.Location)
				.FromPoint(searchFromLoc))
				path = pathFinder.FindPath(search);

			if (path.Count > 0)
				return path[0];

			return null;
		}

		public override IEnumerable<Target> GetTargets(Actor self)
		{
			yield return Target.FromCell(self.World, self.Location);
		}
	}
}
