using System;
using System.Collections;
using System.Collections.Generic;

public class Corridor : Object{

	public int direction = -1;

	public ArrayList cells;
	Cell lastCellOfPreviousCorridor; 

	public Corridor(int direction){
		this.direction = direction;
		this.cells = new ArrayList();
	}

	public Corridor(ArrayList cells){
		this.cells = cells;
	}

	public void addCell(Cell c){
		cells.Add(c);
	}

	public void removeAllCells(){
		foreach(Cell c in cells){
			c.isPath = false;
			c.index = int.MaxValue;
		}
	}



}