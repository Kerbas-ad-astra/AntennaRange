﻿// AntennaRange
//
// ARFlightController.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KSP;
using System;
using ToadicusTools;
using UnityEngine;

namespace AntennaRange
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class ARFlightController : MonoBehaviour
	{
		#region Static Members
		public static bool requireConnectionForControl;
		#endregion

		#region Properties
		public ControlTypes currentControlLock
		{
			get
			{
				if (this.lockID == string.Empty)
				{
					return ControlTypes.None;
				}

				return InputLockManager.GetControlLock(this.lockID);
			}
		}

		public string lockID
		{
			get;
			protected set;
		}

		public ControlTypes lockSet
		{
			get
			{
				return ControlTypes.ALL_SHIP_CONTROLS;
			}
		}

		public Vessel vessel
		{
			get
			{
				if (FlightGlobals.ready && FlightGlobals.ActiveVessel != null)
				{
					return FlightGlobals.ActiveVessel;
				}

				return null;
			}
		}
		#endregion

		#region MonoBehaviour LifeCycle
		protected void Awake()
		{
			this.lockID = "ARConnectionRequired";

			GameEvents.onGameSceneLoadRequested.Add(this.onSceneChangeRequested);
			GameEvents.onVesselChange.Add(this.onVesselChange);
		}

		protected void FixedUpdate()
		{
			// Do nothing if the vessel reference is invalid.
			if (this.vessel == null)
			{
				return;
			}

			// If we are requiring a connection for control, the vessel does not have any adequately staffed pods,
			// and the vessel does not have any connected relays...
			if (
				HighLogic.LoadedSceneIsFlight &&
				requireConnectionForControl &&
				!this.vessel.hasCrewCommand() &&
				!this.vessel.HasConnectedRelay())
			{
				// ...and if the controls are not currently locked...
				if (currentControlLock == ControlTypes.None)
				{
					// ...lock the controls.
					InputLockManager.SetControlLock(this.lockSet, this.lockID);
				}
			}
			// ...otherwise, if the controls are locked...
			else if (currentControlLock != ControlTypes.None)
			{
				// ...unlock the controls.
				InputLockManager.RemoveControlLock(this.lockID);
			}
		}

		protected void Destroy()
		{
			InputLockManager.RemoveControlLock(this.lockID);

			GameEvents.onGameSceneLoadRequested.Remove(this.onSceneChangeRequested);
			GameEvents.onVesselChange.Remove(this.onVesselChange);
		}
		#endregion

		#region Event Handlers
		protected void onSceneChangeRequested(GameScenes scene)
		{
			if (scene != GameScenes.FLIGHT)
			{
				InputLockManager.RemoveControlLock(this.lockID);
			}
		}

		protected void onVesselChange(Vessel vessel)
		{
			InputLockManager.RemoveControlLock(this.lockID);
		}
		#endregion
	}
}

