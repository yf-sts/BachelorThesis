using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour {
	private float tileSize = 2.65f;

	Cell[,] grid;
	int gridDim = 100; //100 default 
	int startXPos = 100/2; //50 default
	int startYPos = 100/2; //50 default
	RandomWalker walker1;
	ArrayList walkerList;

	public int corridorCount = 30; //30 default
	public int minCorridorLength = 5; //5 by default
	public int maxCorridorLength = 10; //10 by default 
	public float minBranchingCoeff = 0.3f; //0.3 default
	public float maxBranchingCoeff = 0.8f; //0.8 default
	public int numBranches = 2; //2 default

	public GameObject straightTile;
	public GameObject cornerTile;
	public GameObject threeWayTile;
	public GameObject endTile;

	public int maxDistanceToNextTorch = 6;
	public int minDistanceToNextTorch = 3;
	public int torchDistanceCounter = 0;

	public GameObject pathPrefab;
	private GameObject path;

	[HideInInspector] 
	public int NORTH = 0;
	[HideInInspector] 
	public int EAST = 1;
	[HideInInspector] 
	public int SOUTH = 2;
	[HideInInspector] 
	public int WEST = 3;

	// Use this for initialization
	void Start () {
	}

	public void Generate(){
		path = Instantiate (pathPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
		path.name = "Path";
		setupGrid();
		walker = new RandomWalker(startXPos, startYPos, grid, minCorridorLength, maxCorridorLength);
		pathWithNBranches (numBranches);
		drawPath();
		//Assign path generated on 2d grid to TDPath which will be rendered in 3D.
		path.GetComponent<TDPath> ().grid = this.grid;
	}

	//Create grid for path generation
	void setupGrid(){
		grid = new Cell[100, 100];
		for(int r = 0; r < 100; r++){
			for(int c = 0; c < 100; c++){
				grid[r , c] = new Cell(c,r);
			}
		}
	}

	void pathWithNBranches(int n){
		//Create root path
		walker1.randomWalk(corridorCount); 
		walker1.generateIndices (0);

		walkerList = new ArrayList();
		walkerList.Add(walker1);

		//For every branch instantiate random walker and randomly choose a branch or root path to walk from 
		for(int i = 0; i < n; i ++){
			//Get random walker from walker list to branch off 
			RandomWalker walker = getRandomWalker();

			//Get random corridor to branch off from
			int rCorrIndex = (int)Random.Range(walker.corridors.Count*minBranchingCoeff, walker.corridors.Count*maxBranchingCoeff);

			Corridor rCorr = (Corridor) walker.corridors[rCorrIndex];

			//Get random cell to branch off from
			Cell rCell = (Cell)rCorr.cells[(int)Random.Range(0, rCorr.cells.Count)];

			//Create path starting from rCell with max and min corridor lengths. 
			RandomWalker branchWalker = new RandomWalker(rCell.x,rCell.y,grid, minCorridorLength, maxCorridorLength);

			bool successfulWalk = branchWalker.randomWalk(walker.corridors.Count-rCorrIndex);
			branchWalker.generateIndices (rCell.index);

			//If the branch walk produces no legal path then try again
			if(!successfulWalk) i--;
			else walkerList.Add(branchWalker);
		}

	}

	RandomWalker getRandomWalker(){
		int rIndex = (int)Random.Range(0, walkerList.Count);
		return (RandomWalker)walkerList[rIndex];
	}

	//Looks at neighbouring cells and determines what type of path tile to draw.
	void drawPath(){
		for(int r = 0; r < grid.GetLength(0); r++){
			for(int c = 0; c < grid.GetLength(0); c++){
				if (grid [r , c].isPath) {

					Vector2 gridPos = new Vector2 (c,r);
					if (getNumNeightbours (gridPos) == 1) {
						if (hasNorthNeighbour(gridPos)) {
							createBridgeTile(endTile, gridPos, 90);
						} 
						else if (hasEastNeighbour(gridPos)){
							createBridgeTile(endTile, gridPos, 0);
						} 
						else if (hasSouthNeighbour(gridPos)){
							createBridgeTile(endTile, gridPos, 270);
						} 
						else if (hasWestNeighbour(gridPos)){
							createBridgeTile(endTile, gridPos, 180);
						} 
					}
					else if (getNumNeightbours (gridPos) == 2) {
						//Check for corner tiles
						if (hasNorthNeighbour (gridPos) && hasEastNeighbour (gridPos)) {
							createBridgeTile (cornerTile, gridPos, 180);
						} 
						else if (hasEastNeighbour (gridPos) && hasSouthNeighbour (gridPos)) {
							createBridgeTile (cornerTile, gridPos, 90);
						} 
						else if (hasSouthNeighbour (gridPos) && hasWestNeighbour (gridPos)) {
							createBridgeTile (cornerTile, gridPos, 0);
						} 
						else if (hasWestNeighbour (gridPos) && hasNorthNeighbour (gridPos)) {
							createBridgeTile (cornerTile, gridPos, 270);
						}
						//Check for straight tiles
						else if (hasWestNeighbour (gridPos) && hasEastNeighbour (gridPos)) {
							createBridgeTile (straightTile, gridPos, 0);
						} 
						else if (hasSouthNeighbour (gridPos) && hasNorthNeighbour (gridPos)) {
							createBridgeTile (straightTile, gridPos, 90);
						}

					}
					else if (getNumNeightbours (gridPos) == 3) {
						if (!hasNorthNeighbour(gridPos)) {
							createBridgeTile(threeWayTile, gridPos, 0);
						} 
						else if (!hasEastNeighbour(gridPos)){
							createBridgeTile(threeWayTile, gridPos, 270);
						} 
						else if (!hasSouthNeighbour(gridPos)){
							createBridgeTile(threeWayTile, gridPos, 180);
						} 
						else if (!hasWestNeighbour(gridPos)){
							createBridgeTile(threeWayTile, gridPos, 90);
						} 
					}

				}
			}
		}
	}

	//Draws a path tile in the game world at a given coordinate
	void createBridgeTile(GameObject tilePrefab, Vector2 pos, int rotaAngle){
		GameObject tile = Instantiate(tilePrefab, new Vector3(pos.x*tileSize, 0, pos.y*tileSize), Quaternion.identity, path.transform);
		tile.name = "" + grid [(int)pos.y , (int)pos.x].index;
		tile.transform.rotation = Quaternion.Euler (0, rotaAngle, 0);

		//Randomly spawn torches between intervals
		int spawnTorchChance = (int) Random.Range(0,4);
		if (torchDistanceCounter >= minDistanceToNextTorch && (spawnTorchChance == 1 || torchDistanceCounter >= maxDistanceToNextTorch)) {
			torchDistanceCounter = 0;
	
			int randomTorchCorner = (int)Random.Range(0,tile.transform.childCount);
			tile.transform.GetChild (randomTorchCorner).gameObject.SetActive (true);
		} else {
			torchDistanceCounter++;
		}
	}

	bool hasNorthNeighbour(Vector2 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		if (y > 0 && grid [y-1, x].isPath) {
			return true;
		}
		return false;
	}

	bool hasEastNeighbour(Vector2 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		if (x < grid.GetLength(0) && grid [y, x+1].isPath) {
			return true;
		}
		return false;
	}

	bool hasSouthNeighbour(Vector2 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		if (y < grid.GetLength(0) && grid [y+1, x].isPath) {
			return true;
		}
		return false;
	}

	bool hasWestNeighbour(Vector2 pos){
		int x = (int)pos.x;
		int y = (int)pos.y;
		if (x > 0 && grid [y, x-1].isPath) {
			return true;
		}
		return false;
	}

	int getNumNeightbours(Vector2 pos){
		int numNeightbours = 0;
		if (hasNorthNeighbour (pos)) numNeightbours++;
		if (hasWestNeighbour (pos)) numNeightbours++;
		if (hasSouthNeighbour (pos)) numNeightbours++;
		if (hasEastNeighbour (pos)) numNeightbours++;
		return numNeightbours;
	}



	public void newScaleX(GameObject theGameObject, float newSize) {

		float size = theGameObject.GetComponent<Renderer> ().bounds.size.x;

		Vector3 rescale = theGameObject.transform.localScale;

		rescale.x = newSize * rescale.x / size;

		theGameObject.transform.localScale = rescale;

	}

	public void newScaleZ(GameObject theGameObject, float newSize) {

		float size = theGameObject.GetComponent<Renderer> ().bounds.size.z;

		Vector3 rescale = theGameObject.transform.localScale;

		rescale.z = newSize * rescale.z / size;

		theGameObject.transform.localScale = rescale;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
