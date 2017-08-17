using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConwayCube : MonoBehaviour
{
	public Material livingMaterial, deadMaterial;
	private MeshRenderer meshRenderer;

	public bool IsLiving { get { return meshRenderer.material == livingMaterial; } }
	public bool IsDead { get { return meshRenderer.material == deadMaterial; } }

	public void UseLivingMaterial()
	{
		if (!meshRenderer) SetupMeshRenderer();
		meshRenderer.material = livingMaterial;
	}

	public void UseDeadMaterial()
	{
		if (!meshRenderer) SetupMeshRenderer();
		meshRenderer.material = deadMaterial;
	}

	private void SetupMeshRenderer()
	{
		meshRenderer = GetComponent<MeshRenderer>();
	}

	void Awake()
	{
		if (!meshRenderer) SetupMeshRenderer();
		UseDeadMaterial();
	}
}
