using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalker : Object{

	public int NORTH = 0;
	public int EAST = 1;
	public int SOUTH = 2;
	public int WEST = 3;
     
	public ArrayList corridors;
	Cell[,] grid;

	int xPos;
	int yPos;

	int minCorridorLength = 5; //5 default
	int maxCorridorLength = 10; //10 default

	int pM = 4; //4 Default


	public RandomWalker(int xPos, int yPos, Cell[,] grid, int minCorridorLength,int maxCorridorLength )
	{
		this.grid = grid;
		corridors = new ArrayList();
		this.xPos = xPos;
		this.yPos = yPos;
		this.minCorridorLength = minCorridorLength;
		this.maxCorridorLength = maxCorridorLength;
	}

	//Generates Indeces for the cells in a corridor.
	public void generateIndices(int startIndex){
		int index = startIndex;
		for (int n = 0; n < corridors.Count; n++) {
			ArrayList cells = ((Corridor)corridors [n]).cells;
			for (int i = 0; i < cells.Count; i++) {
					index++;
					((Cell)cells[i]).index = index;
			}
		}
	}

	//Performs a recursive random walk that backtracks when stuck. corridorCounters specifies the length of the path in corridors. 
	public bool randomWalk(int corridorCounter){

		if(corridorCounter <= 0) return true;

		int tempY = yPos;
		int tempX = xPos;
		ArrayList dirs = new ArrayList();
		dirs.Add(NORTH); dirs.Add(EAST); dirs.Add(SOUTH); dirs.Add(WEST); 

		while(!(dirs.Count==0)){ 
			yPos = tempY; //Reset coordinates incase first random walk failed
			xPos = tempX;
			int rDirIndex = (int)  Random.Range(0,dirs.Count); //Pick a random direction
			int rDir = (int)dirs[rDirIndex];

			if(rDir==NORTH){
				if(walkNorth()){
					if(randomWalk(corridorCounter-1)){ 
						return true; //should happen at top of stack and will break recursion - if false is returned then recursion is continued with other random directions
					}
					else{ 
						((Corridor)corridors[corridors.Count - 1]).removeAllCells();
						corridors.RemoveAt (corridors.Count - 1);
					}
				}
			}
			if(rDir==EAST){
				if(walkEast()){
					if(randomWalk(corridorCounter-1)){ 
						return true; //should happen at top of stack and will break recursion - if false is returned then recursion is continued with other random directions
					}
					else{
						((Corridor)corridors[corridors.Count - 1]).removeAllCells();
						corridors.RemoveAt (corridors.Count - 1);
					}
				}
			}
			if(rDir==SOUTH){
				if(walkSouth()){
					if(randomWalk(corridorCounter-1)){ 
						return true; //should happen at top of stack and will break recursion - if false is returned then recursion is continued with other random directions
					}
					else{
						((Corridor)corridors[corridors.Count - 1]).removeAllCells();
						corridors.RemoveAt (corridors.Count - 1);
					}
				}
			}
			if(rDir==WEST){
				if(walkWest()){
					if(randomWalk(corridorCounter-1)){ 
						return true; //should happen at top of stack and will break recursion - if false is returned then recursion is continued with other random directions
					}
					else{
						((Corridor)corridors[corridors.Count - 1]).removeAllCells();
						corridors.RemoveAt (corridors.Count - 1);
					}
				}
			}

			dirs.RemoveAt(rDirIndex);
		}  
		//if none of the directions in the while loops managed to satisfy algo we return false
		return false;
	}

	//Returns true if corridor was made successfully or not
	bool walkNorth(){ 
		int rDist = (int) Random.Range(minCorridorLength,maxCorridorLength);

		Corridor corridor = new Corridor(NORTH);

		while(rDist > 0){   
			rDist--;
			yPos--;
			//First check is for frontal collision, second and third check is to prevent parallel walking, 4th collision check with border of grid
			if(grid[yPos-pM , xPos].isPath || pathExistsNCellsToEast(grid[yPos , xPos],pM) || pathExistsNCellsToWest(grid[yPos , xPos],pM) || yPos < (pM+2)){ //COULD IMPOSE 4th rule with yPos < 2... in dissertation explain what each rule does, how it affects shape of generation
				corridor.removeAllCells();
				return false;
			}

			grid[yPos , xPos].isPath = true; corridor.addCell(grid[yPos , xPos]); grid [yPos, xPos].corridor = corridor;

		}

		corridors.Add(corridor);
		return true;
	}

	bool walkEast(){ 
		int rDist = (int) Random.Range(minCorridorLength,maxCorridorLength);

		Corridor corridor = new Corridor(EAST);

		while(rDist > 0){
			rDist--;
			xPos++;
			//First check is for frontal collision, second and third check is to prevent parallel walking, 4th collision check with border border of grid
			if(grid[yPos , xPos+pM].isPath || pathExistsNCellsToNorth(grid[yPos , xPos],pM) || pathExistsNCellsToSouth(grid[yPos , xPos],pM) || xPos > grid.GetLength(0)-(pM+2)){
				corridor.removeAllCells();
				return false;
			}

			grid[yPos , xPos].isPath = true; corridor.addCell(grid[yPos , xPos]); grid [yPos, xPos].corridor = corridor;

		}

		corridors.Add(corridor);
		return true;
	}

	bool walkSouth(){ 
		int rDist = (int) Random.Range(minCorridorLength,maxCorridorLength);

		Corridor corridor = new Corridor(SOUTH); 

		while(rDist > 0){
			rDist--;
			yPos++;
			//First check is for frontal collision, second and third check is to prevent parallel walking, 4th collision check with border border of grid
			if(grid[yPos+pM , xPos].isPath || pathExistsNCellsToEast(grid[yPos , xPos],pM) || pathExistsNCellsToWest(grid[yPos , xPos],pM) || yPos > grid.GetLength(0)-(pM+2)){
				corridor.removeAllCells();
				return false;
			}

			grid[yPos , xPos].isPath = true; corridor.addCell(grid[yPos , xPos]); grid [yPos, xPos].corridor = corridor;

		}

		corridors.Add(corridor);
		return true;
	}

	bool walkWest(){ 
		int rDist = (int) Random.Range(minCorridorLength,maxCorridorLength);

		Corridor corridor = new Corridor(WEST);

		while(rDist > 0){
			rDist--;
			xPos--;
			//First check is for frontal collision, second and third check is to prevent parallel walking, 4th collision check with border border of grid
			if(grid[yPos , xPos-pM].isPath || pathExistsNCellsToNorth(grid[yPos , xPos],pM) || pathExistsNCellsToSouth(grid[yPos , xPos],pM) || xPos < (pM+2)){
				corridor.removeAllCells();
				return false;
			}

			grid[yPos , xPos].isPath = true; corridor.addCell(grid[yPos , xPos]); grid [yPos, xPos].corridor = corridor;

		}

		corridors.Add(corridor);
		return true;
	}

	bool pathExistsNCellsToEast(Cell c, int n){

		int x = c.x;
		int y = c.y;

		while(x <= c.x+n){
			if(grid[y , x].isPath) return true;
			x++;
		}

		return false;
	}

	bool pathExistsNCellsToWest(Cell c, int n){

		int x = c.x;
		int y = c.y;

		while(x >= c.x-n){
			if(grid[y , x].isPath) return true;
			x--;
		}

		return false;
	}

	bool pathExistsNCellsToNorth(Cell c, int n){

		int x = c.x;
		int y = c.y;

		while(y >= c.y-n){
			if(grid[y , x].isPath) return true;
			y--;
		}

		return false;
	}

	bool pathExistsNCellsToSouth(Cell c, int n){

		int x = c.x;
		int y = c.y;

		while(y <= c.y+n){
			if(grid[y , x].isPath) return true;
			y++;
		}

		return false;
	}

}
