using System.Collections.Generic;
using UnityEngine;

namespace YourProjectName
{
	public class SkinnedMeshStitcher<MeshType> : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The mesh object containing the skeleton that is animated by the Animator.")]
		protected GameObject m_avatarMesh;                            // The base mesh object, whose skeleton is animated by the Animator, to share materials and the animated skeleton to stitched meshes.
		protected Transform m_rootAnimatedBone;                       // The root animated bone of the Avatar's skeleton, for applying to stitched meshes.
		protected Transform[] m_animatedBones;                        // The animated bones of the Avatar's skeleton, for applying to stitched meshes.
		protected Dictionary<string, Material> m_avatarMeshMaterials; // The avatar mesh's materials, for sharing common material instances with stitched meshes.
		protected Dictionary<MeshType, GameObject> m_stitchedMeshes;  // Holds currently stitched meshes for swapping and removal.

		protected virtual void Awake()
		{
			SkinnedMeshRenderer avatarSkinnedMeshRenderer = m_avatarMesh.GetComponent<SkinnedMeshRenderer>();
			m_rootAnimatedBone = avatarSkinnedMeshRenderer.rootBone;
			m_animatedBones = avatarSkinnedMeshRenderer.bones;
			m_avatarMeshMaterials = new Dictionary<string, Material>();
			m_stitchedMeshes = new Dictionary<MeshType, GameObject>();

			Material[] avatarMeshMaterials = avatarSkinnedMeshRenderer.materials;
			foreach (Material avatarMeshMaterial in avatarMeshMaterials)
			{
				string materialName = avatarMeshMaterial.name.Replace(" (Instance)", string.Empty);
				m_avatarMeshMaterials.Add(materialName, avatarMeshMaterial);
			}
		}

		/// <summary> Stitches the skinned mesh to the animated skeleton, replacing the currently stitched mesh type (if one exists). <para/>
		///           Returns a reference to the stitched mesh. </summary>
		public virtual GameObject StitchMesh(MeshType _meshType, GameObject _skinnedMeshPrefab, bool _isShareMaterials = true)
		{
			GameObject stitchedMesh = null;
			try
			{
				RemoveStitchedMesh(_meshType); // Clean up the stitched mesh to be replaced.
				GameObject skinnedMesh = Instantiate(_skinnedMeshPrefab) as GameObject;
				stitchedMesh = StitchMeshToSkeleton(skinnedMesh, _isShareMaterials);
				m_stitchedMeshes[_meshType] = stitchedMesh;
			}
			catch (System.Exception _e)
			{
				Debug.LogWarningFormat("Attempted to stitch Skinned Mesh Prefab {0}: {1}", (_skinnedMeshPrefab != null) ? _skinnedMeshPrefab.name : "", _e);
			}

			return (stitchedMesh);
		}

		/// <summary> Removes the mesh currently stitched for the type, if one exists. <para/>
		///           NOTE: Does not need to be called before the 'StitchMesh' function when swapping/replacing a mesh as it does this internally. </summary>
		public void RemoveStitchedMesh(MeshType _meshType)
		{
			GameObject stitchedMeshToRemove;
			if (m_stitchedMeshes.TryGetValue(_meshType, out stitchedMeshToRemove))
			{
				Destroy(stitchedMeshToRemove);
			}
		}

		/// <summary> Returns the avatar mesh's material instance based on the provided name. </summary>
		public Material GetAvatarMeshMaterial(string _materialName)
		{
			string materialName = _materialName.Replace(" (Instance)", string.Empty); // Ensure the name doesn't contain instance.
			Material avatarMeshMaterial;
			m_avatarMeshMaterials.TryGetValue(materialName, out avatarMeshMaterial);

			return (avatarMeshMaterial);
		}

		/// <summary> Creates a new skinned mesh object and stitches it to the avatar mesh's skeleton, cleaning up the original mesh to stitch. <para/>
		///           Returns a reference to the stitched mesh. </summary>
		private GameObject StitchMeshToSkeleton(GameObject _skinnedMeshToStitch, bool _isShareMaterials)
		{
			GameObject stitchedMesh = null;

			try
			{
				// Create the stitched mesh based on the skinned mesh (name, tag, layer):
				stitchedMesh = new GameObject(_skinnedMeshToStitch.name);
				stitchedMesh.tag = _skinnedMeshToStitch.tag;
				stitchedMesh.layer = _skinnedMeshToStitch.layer;
				stitchedMesh.transform.SetParent(m_avatarMesh.transform.parent, false); // Parent to the same parent of the avatar mesh.

				// Apply the shared bones and the mesh data:
				SkinnedMeshRenderer skinnedMeshRenderer = _skinnedMeshToStitch.GetComponentInChildren<SkinnedMeshRenderer>();
				SkinnedMeshRenderer stitchedMeshRenderer = stitchedMesh.AddComponent<SkinnedMeshRenderer>();
				stitchedMeshRenderer.bones = m_animatedBones;
				stitchedMeshRenderer.rootBone = m_rootAnimatedBone;
				stitchedMeshRenderer.sharedMesh = skinnedMeshRenderer.sharedMesh;

				// Apply the materials:
				if (_isShareMaterials)
				{
					Material[] stitchedMeshMaterials = skinnedMeshRenderer.materials;

					// Replace the corresponding skinned materials with the avatar mesh's material instances:
					for (int i = 0; i < stitchedMeshMaterials.Length; ++i)
					{
						Material avatarMeshMaterial = GetAvatarMeshMaterial(stitchedMeshMaterials[i].name);
						if (avatarMeshMaterial != null)
						{
							stitchedMeshMaterials[i] = avatarMeshMaterial;
						}
					}

					stitchedMeshRenderer.materials = stitchedMeshMaterials;
				}
				else
				{
					stitchedMeshRenderer.materials = skinnedMeshRenderer.materials;
				}

				Destroy(_skinnedMeshToStitch); // Destroying the skinned mesh as it has been replaced by the stitched mesh.
			}
			catch (System.Exception _e)
			{
				Debug.LogWarningFormat("Attempted to stitch mesh {0}: {1}", (_skinnedMeshToStitch != null) ? _skinnedMeshToStitch.name : "", _e);
			}

			return (stitchedMesh);
		}
	}
}
