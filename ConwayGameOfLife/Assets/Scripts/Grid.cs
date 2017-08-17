using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//	TODO: Adjust camera based on user-specified information, right now camera is hard-coded
public class Grid : MonoBehaviour
{
	[Header("Prefabs for the cubes")]
	public GameObject liveCube;
	public GameObject deadCube;
	private string liveCubeIdentifier, deadCubeIdentifier;

	[Header("Grid settings")]
	public uint gridWidth;
	public uint gridHeight;

	[Header("Simulation settings")]
	public bool autoProgress;
	[Range(0.01f,5f)]
	public float autoProgressionTime = 1;
	private float deltaTime;
	public bool useRandomStart;
	public GridItem[] initialLivingGridItems;

	private bool[,] grid;
	private List<GridItem> livingGridItemIndexes;//TODO: Can be expanded upon to reduce computational complexity
	private List<GridItem> gridItemsToKill, gridItemsToLive;
	private GameObject[,] cubes;

	//	Check if a given point is within the bounds of the grid
	private bool IsPointInBounds(uint x, uint y){
		return x < gridWidth && y < gridHeight;
	}

	private bool IsPointInBounds(GridItem gridItem){
		return IsPointInBounds (gridItem.x, gridItem.y);
	}

	//	Get number of neighbors that are alive
	private uint GetLivingNeighborsCount(uint x, uint y){
		List<GridItem> neighbors = GetNeighborsAt (x, y);
		uint livingCount = 0;
		foreach (GridItem neighbor in neighbors) {
			if (grid [neighbor.y, neighbor.x])
				livingCount++;
		}
		return livingCount;
	}

	//	Get neighbors at a specific (x,y) point
	private List<GridItem> GetNeighborsAt(uint x, uint y){
		List<GridItem> neighbors = new List<GridItem> ();

		//	Sanity bounds check
		if (!IsPointInBounds (x, y)) {
			return neighbors;
		}

		//	Check if all neighbors are in bounds
		if (IsPointInBounds(x, y - 1)) neighbors.Add(new GridItem(x, y - 1));
		if (IsPointInBounds(x, y + 1)) neighbors.Add(new GridItem(x, y + 1));
		if (IsPointInBounds(x - 1, y)) neighbors.Add(new GridItem(x - 1, y));
		if (IsPointInBounds(x + 1, y)) neighbors.Add(new GridItem(x + 1, y));

		return neighbors;
	}

	//	Instantiate the given prefab at the point, settings it as a child of the Grid
	private GameObject CreateCubeAt(GameObject prefab, uint x, uint y)
	{
		GameObject cube = Instantiate (prefab, transform);
		cube.transform.position = new Vector3 (x, -y, 0);
		return cube;
	}

	//	Setup the visual grid
	private void SetupGrid()
	{
		cubes = new GameObject[gridHeight, gridWidth];
		liveCubeIdentifier = liveCube.name;
		deadCubeIdentifier = deadCube.name;
		for (uint y = 0; y < gridHeight; y++) {
			for (uint x = 0; x < gridWidth; x++) {
				cubes [y, x] = CreateCubeAt (deadCube, x, y);
			}
		}
	}

	//	Check if the given prefab object is a Dead Cube
	//	TODO: This is really hacky and can be better than string comparison
	private bool IsDeadCube(GameObject cube)
	{
		return cube.name.Contains (deadCubeIdentifier);
	}

	//	Check if the given prefab object if a Live Cube
	//	TODO: This is really hacky and can be better than string comparison
	private bool IsLiveCube(GameObject cube)
	{
		return cube.name.Contains (liveCubeIdentifier);
	}

	//	Render updates to the visual grid
	private void RenderGrid()
	{
		for (uint y = 0; y < gridHeight; y++) {
			for (uint x = 0; x < gridWidth; x++) {
				GameObject cube = cubes [y, x];
				bool gridStatus = grid [y, x];
				if (gridStatus && !IsLiveCube (cube)) {
					Destroy (cube);
					cubes [y, x] = CreateCubeAt (liveCube, x, y);
				} else if (!gridStatus && !IsDeadCube (cube)) {
					Destroy (cube);
					cubes [y, x] = CreateCubeAt (deadCube, x, y);
				}
			}
		}
	}

	//	Perform neighbor calculations and render the grid
	private void StepForward()
	{
		//	Start with a blank grid
		//	TODO: If we save this information we can reduce computation from the current O(n)
		gridItemsToKill = new List<GridItem> ();
		gridItemsToLive = new List<GridItem> ();

		for (uint y = 0; y < gridHeight; y++) {
			for (uint x = 0; x < gridWidth; x++) {
				bool isAlive = grid [y, x];
				uint neighborsAlive = GetLivingNeighborsCount(x, y);
				if (isAlive) {
					if (neighborsAlive == 2 || neighborsAlive == 3)
						gridItemsToLive.Add (new GridItem (x, y));
					else
						gridItemsToKill.Add (new GridItem (x, y));
				} else {
					if (neighborsAlive == 3)
						gridItemsToLive.Add (new GridItem (x, y));
					else
						gridItemsToKill.Add (new GridItem (x, y));
				}
			}
		}

		//	TODO: This likely is unnecessary if we set the values in the above loop
		foreach( GridItem killItem in gridItemsToKill )
		{
			grid [killItem.y, killItem.x] = false;
		}
		foreach (GridItem liveItem in gridItemsToLive) {
			grid [liveItem.y, liveItem.x] = true;
		}

		//	Render the changes from this step
		RenderGrid ();
	}

	void Awake()
	{
		//	Initialize index for living grid items
		livingGridItemIndexes = new List<GridItem>();
		grid = new bool[gridHeight,gridWidth];

		//	Set the initial state of the grid, using randomness if selected by the user
		if (useRandomStart) {
			for (uint y = 0; y < gridHeight; y++) {
				for (uint x = 0; x < gridWidth; x++) {
					grid [y, x] = Random.value > 0.5f;
				}
			}
		} else {
			foreach (GridItem gridItem in initialLivingGridItems) {
				if ( IsPointInBounds(gridItem) ) {
					livingGridItemIndexes.Add (gridItem);
					grid [gridItem.y, gridItem.x] = true;
				} else {
					Debug.LogWarning ("Index (" + gridItem.y + "," + gridItem.x + ") is out of bounds");
				}
			}
		}

		//	Render
		SetupGrid();
		RenderGrid();
	}

	void Update ()
	{
		//	If not auto-progressing, advance when user hits Return
		if ( !autoProgress && Input.GetKeyDown (KeyCode.Return)) {
			StepForward ();
		}

		//	If auto-probgressing, advance when time has elapsed
		//	TODO: Since this is not fixed time, a severe lag spike might lead to abnormal printing
		if (autoProgress) {
			deltaTime += Time.deltaTime;
			if (deltaTime >= autoProgressionTime) {
				StepForward ();
				deltaTime -= autoProgressionTime;
			}
		}

	}
}
