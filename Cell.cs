using System;
using System.Collections;
using System.Collections.Generic;


public class Cell : Object
{
	public int x;
	public int y;
	public int index;
	public bool isPath = false;
	public Corridor corridor;

	public Cell(int x, int y){
		this.x = x;
		this.y = y;
		index = int.MaxValue;
	}
}


