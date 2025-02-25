﻿using System.Collections;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Map
{
	public class Asteroid : MonoBehaviour
	{
		private MatrixMove mm;
		private OreGenerator oreGenerator;

		// TODO Find a use for these variables or delete them.
		/*
	private float asteroidDistance = 550; //How far can asteroids be spawned

	private float distanceFromStation = 175; //Offset from station so it doesnt spawn into station
	*/

		void OnEnable()
		{
			if (mm == null) mm = GetComponent<MatrixMove>();

			if(oreGenerator == null) oreGenerator = GetComponent<OreGenerator>();
		}

		private void Start()
		{
			if (CustomNetworkManager.IsServer)
			{
				StartCoroutine(Init());
			}
		}

		[Server]
		public void SpawnNearStation()
		{
			//Request a position from GameManager and cache the object in SpaceBodies List
			GameManager.Instance.ServerSetSpaceBody(mm);
		}

		[Server] //Asigns random rotation to each asteroid at startup for variety.
		public void RandomRotation()
		{
			int rand = Random.Range(0, 4);

			 switch (rand)
			 {
			 	case 0:
				    mm.NetworkedMatrixMove.TargetOrientation = OrientationEnum.Up_By0;
			 		break;
			 	case 1:
				    mm.NetworkedMatrixMove.TargetOrientation = OrientationEnum.Down_By180;
			 		break;
			 	case 2:
				    mm.NetworkedMatrixMove.TargetOrientation = OrientationEnum.Right_By270;
			 		break;
			 	case 3:
				    mm.NetworkedMatrixMove.TargetOrientation = OrientationEnum.Left_By90;
			 		break;
			 }
		}

		//Wait for MatrixMove init on the server:
		IEnumerator Init()
		{
			yield return WaitFor.EndOfFrame;
			SpawnNearStation();
			RandomRotation();
		}

	}
}