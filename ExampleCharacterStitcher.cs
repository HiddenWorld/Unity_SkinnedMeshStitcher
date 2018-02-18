using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YourProjectName
{
	public enum CharacterMeshType
	{
		Invalid_Type,

		Head,
		Torso,
		Legs,
		Feet,
		//OtherMeshTypeHere,

		Exceeded_Type
	};

	public class CharacterStitcher : SkinnedMeshStitcher<CharacterMeshType>
	{
		protected override void Awake()
		{
			base.Awake();

			// Remove the palceholder mesh on the avatar (so that only stitched meshes exist and are rendered):
			SkinnedMeshRenderer avatarSkinnedMeshRenderer = m_avatarMesh.GetComponent<SkinnedMeshRenderer>();
			avatarSkinnedMeshRenderer.sharedMesh = null;
		}
	}
}
