using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using SFS.World.Drag;
using SFS.World;

using UnityEngine;

namespace TestModSFS
{
    [HarmonyPatch(typeof(AeroModule), "ApplyForce")]
    class Patch_AeroModule
    {
		// This method is called right after AeroModule.ApplyForce() - That method calculates and applies drag and heat effects
		static void Postfix(AeroModule __instance, List<Surface> exposedSurfaces, Location location, Matrix2x2 localToWorld)
        {
			(float lift, Vector2 centerOfLift) tuple2 = CalculateLiftForce(exposedSurfaces);

			float Pdyn = (float)location.planet.GetAtmosphericDensity(location.Height) * (float)location.velocity.sqrMagnitude * 1.5f;
			
			float liftForce = tuple2.lift * Pdyn;
			Vector2 centerOfLift_World = localToWorld * tuple2.centerOfLift;

			if (__instance is Aero_Rocket aero_Rocket)
			{
				centerOfLift_World = Vector2.Lerp(aero_Rocket.rocket.rb2d.worldCenterOfMass, centerOfLift_World, 0.2f);

				if (!float.IsNaN(liftForce))
				{
					Vector2 liftAxis = -location.velocity.ToVector2.normalized;
					liftAxis = liftAxis.Rotate_90();
					aero_Rocket.rocket.rb2d.AddForceAtPosition(liftAxis * liftForce, centerOfLift_World, ForceMode2D.Force);
				}
			}

			
		}

		private static (float lift, Vector2 centerOfLift) CalculateLiftForce(List<Surface> surfaces)
		{
			float totalLift = 0f;
			Vector2 centerOfLift = Vector2.zero;

			foreach (Surface surface in surfaces)
			{
				if (surface.owner.IsHeatShield == true)
				{
					Vector2 vector = surface.line.end - surface.line.start;
					if (!(vector.x < 0.01f))
					{
						// Calculation of drag in accordance with CalculateDragForce method
						float drag_coeff = vector.x / (vector.x + Mathf.Abs(vector.y));
						float drag = vector.x * drag_coeff;

						// calculate lift based on angle
						float angle = Mathf.Atan(vector.y / vector.x);
						float liftToDragRatio = getLiftToDragRatio(angle);
						float lift = drag * liftToDragRatio;

						totalLift += lift;
						centerOfLift += (surface.line.start + surface.line.end) * lift / 2.0f;
					}
				}
			}

			if (totalLift != 0.0f)
			{
				centerOfLift /= totalLift;
			}

			return (totalLift, centerOfLift);
		}

		static float getLiftToDragRatio(float angle)
		{
			const float C_MAX_LIFT_TO_DRAG = 0.8f;
			const float C_BEST_ANGLE = (float)(Math.PI / 15f); // 12º - The angle at which lift-to-drag ratio is maximal
			const float C_MAX_ANGLE  = (float)(Math.PI / 3f);  // 60º - Above this value, lift-to-drag is 0
			float liftToDragRatio = 0.0f;

			// the formula only works for positive angles, so we do it with absolute value
			float alpha = Math.Abs(angle);

			if (alpha <= C_BEST_ANGLE)
			{
				// This formula is valid between 0 and C_BEST_ANGLE
				// It ensures lift-to-drag is 0 for angle = 0, and 1 for angle = C_BEST_ANGLE
				// Higher "coeff" means steeper profile (lift-to-drag decreases faster when getting away from the optimal angle)
				float coeff = 1f;
				float a = 1.0f / (coeff * (C_BEST_ANGLE * C_BEST_ANGLE));
				liftToDragRatio = (1 + a) / (1 + coeff * (C_BEST_ANGLE - alpha) * (C_BEST_ANGLE - alpha)) - a;
			}
			else if (alpha <= C_MAX_ANGLE)
			{
				// This formula is valid between C_BEST_ANGLE and C_MAX_ANGLE
				// It ensures lift-to-drag is 0 for angle = C_MAX_ANGLE, and 1 for angle = C_BEST_ANGLE (to ensure continuity with previous formula)
				// Higher coeff means steeper profile (lift-to-drag decreases faster when getting away from the optimal angle)
				float coeff = 5f;
				float a = 1.0f / (coeff * (C_MAX_ANGLE - C_BEST_ANGLE) * (C_MAX_ANGLE - C_BEST_ANGLE));
				liftToDragRatio = (1 + a) / (1 + coeff * (alpha - C_BEST_ANGLE) * (alpha - C_BEST_ANGLE)) - a;
			}
			else // alpha > C_MAX_ANGLE
			{
				liftToDragRatio = 0.0f;
			}

			// At that stage, liftToDragRatio is a value between 0 and 1 --> convert it to the real value
			liftToDragRatio *= C_MAX_LIFT_TO_DRAG;

			if (angle < 0.0f)
			{
				// negate the value if the angle was negative
				liftToDragRatio = -liftToDragRatio;
			}

			return liftToDragRatio;
		}
	}
}
