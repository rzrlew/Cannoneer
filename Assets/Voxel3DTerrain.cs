using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Voxel3DTerrain:MonoBehaviour{
	public GameObject cannon;
	public GameObject target;
	public GameObject cannonInstance;
	public string mapSeed = "Lewis";
	public int numTargets = 5;
	public Vector3 chunkSize = new Vector3(64,64,128);
	public Vector2 mapChunks = new Vector2(2,2);
	public List<GameObject>[,] levelList;
	// This first list contains every vertex of the mesh that we are going to render
	public List<Vector3> newVertices = new List<Vector3>();
	// The triangles tell Unity how to build each section of the mesh joining the vertices
	public List<int> newTriangles = new List<int>();
	// Tells Unity how the texture is aligned on each polygon
	public List<Vector2> newUV = new List<Vector2>();
	public byte[,][,,] blockMap;//Odd numbers "0" is even represent wall levels and even numbers represent floors and ceilings, the "Z" coorsinate meaning the 3rd index is the level.
	public List<Vector3> colVertices = new List<Vector3>();
	public List<int> colTriangles = new List<int>();
	public bool[,] update; //Indicate chunk update;
	public bool[,][] updateLevel; //Indicate level update on chunk[x,y][level array index];

	private int colCount = 0;
	private float tUnit = 0.25f;
	private Vector2 tStone = new Vector2(0,0);
	private Vector2 tDirt = new Vector2(0,1);
	private int squareCount = 0;
	private SimplexNoiseGenerator noiseGenerator;
	private Dictionary<string, byte> blockMapValues= new Dictionary<string, byte>();

	void InitBlockValues(){
		blockMapValues.Add("Front", 1);
		blockMapValues.Add("Back", 2);
		blockMapValues.Add("Left", 4);
		blockMapValues.Add("Right", 8);
		blockMapValues.Add("Filled", 16);
		blockMapValues.Add("Stone", 32);
		blockMapValues.Add("Dirt", 64);
	}

	// Use this for initialization
	public void Start () {
		//Initialization of terrain
		noiseGenerator = new SimplexNoiseGenerator(mapSeed);
		InitBlockValues();
		GenTerrain(Mathf.FloorToInt(chunkSize.x),Mathf.FloorToInt(chunkSize.y),Mathf.FloorToInt(chunkSize.z));
		//Initialize structures!
		levelList = new List<GameObject>[Mathf.FloorToInt(chunkSize.x),Mathf.FloorToInt(chunkSize.y)];
		updateLevel = new bool[Mathf.FloorToInt(mapChunks.x),Mathf.FloorToInt(mapChunks.y)][];
		update = new bool[Mathf.FloorToInt(chunkSize.x),Mathf.FloorToInt(chunkSize.y)];
		for (int chunkX = 0; chunkX<blockMap.GetLength(0); chunkX++){
			for(int chunkY = 0; chunkY<blockMap.GetLength(1); chunkY++){
				updateLevel[chunkX, chunkY] = new bool[blockMap[chunkX, chunkY].GetLength(2)];//Initiate update level structure with the number of levels in each chunk!
				levelList[chunkX, chunkY] = new List<GameObject>();
				for(int level=0; level<blockMap[chunkX,chunkY].GetLength(2);level++){
					levelList[chunkX, chunkY].Add(new GameObject());
					levelList[chunkX, chunkY][level].transform.SetParent(transform);
					levelList[chunkX, chunkY][level].name = "Chunk[" + chunkX.ToString() + "," + chunkY.ToString() +"]-Lvl[" + level.ToString() + "]";
					levelList[chunkX, chunkY][level].AddComponent<MeshFilter>();
					levelList[chunkX, chunkY][level].AddComponent<MeshCollider>();
					levelList[chunkX, chunkY][level].AddComponent<MeshRenderer>();
					levelList[chunkX, chunkY][level].GetComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;
					BuildLevelMesh(level, chunkX, chunkY);
					UpdateMesh(level, chunkX, chunkY);
				}
			}
		}
		PositionPlayerAndTargets();
	}

	void PositionPlayerAndTargets(){
		//Player
		cannonInstance = Instantiate(cannon, new Vector3((blockMap[0,0].GetLength(0) * mapChunks.x)/2, blockMap[0,0].GetLength(2),(blockMap[0,0].GetLength(1)* mapChunks.y)/2), new Quaternion(0,0,0,0)) as GameObject;
		cannonInstance.GetComponentInChildren<CannonBehaviour>().terrain = gameObject;
		cannonInstance.transform.SetParent(gameObject.transform);
		//Enemies
		//TODO: Position targets in a block area.
		for(int i=0; i<numTargets; i++){
			float xPos = Random.Range(0, chunkSize.x * mapChunks.x);
			float yPos = Random.Range(0, chunkSize.y * mapChunks.y);
			float zPos = blockMap[0,0].GetLength(2);
			for (int lvl=0; lvl<blockMap[Mathf.FloorToInt(xPos / chunkSize.x), Mathf.FloorToInt(yPos / chunkSize.y)].GetLength(2); lvl++){
				if(blockMap[Mathf.FloorToInt(xPos / chunkSize.x), Mathf.FloorToInt(yPos / chunkSize.y)][Mathf.FloorToInt(xPos % chunkSize.x), Mathf.FloorToInt(yPos % chunkSize.y), lvl] == 0){
					zPos = lvl;
					break;
				}
			}
			GameObject targetInstance = Instantiate(target, new Vector3(xPos, zPos, yPos), new Quaternion(0,0,0,0)) as GameObject;
			GetComponent<TargetManager>().AddTarget(targetInstance);
			
		}
	}

	int Noise (int x, int y, int level, float scale, float mag, float exp){
		return (int) (Mathf.Pow(noiseGenerator.noise(x/scale,y/scale,level/scale)*mag, exp));
	}

	void GenTerrain(int sizeX, int sizeZ, int sizeY){
		blockMap = new byte[Mathf.FloorToInt(mapChunks.x),Mathf.FloorToInt(mapChunks.y)][,,];
		for (int chunkX = 0; chunkX<blockMap.GetLength(0); chunkX++){
			for(int chunkY = 0; chunkY<blockMap.GetLength(1); chunkY++){
				blockMap[chunkX,chunkY] = new byte[sizeX,sizeZ,sizeY];
				//Generate Chunk
				for (int px=0; px<blockMap[chunkX,chunkY].GetLength(0); px++){
					for (int pz=0; pz<blockMap[chunkX,chunkY].GetLength(1); pz++){
						int height =7;
						height += Noise(px,pz,0,20,10,1);
						height += Noise(px,pz,1,20,10,1);
						height += Noise(px,pz,3,30,12,2);
						for (int py=0; py<blockMap[chunkX,chunkY].GetLength(2); py++){
							if(py>height){
								blockMap[chunkX,chunkY][px,pz,py]=0;
							}
//							else if(Noise(px,pz,py,20,17,2)>10 && py<height){ //Caves
//								blockMap[chunkX,chunkY][px,pz,py]=0;	
//							}
							else{blockMap[chunkX,chunkY][px,pz,py]=1;}
						}
					}
				}
			}
		}


	}
	void BuildLevelMesh(int level, int chunkX, int chunkY){
		for(int px=0;px<blockMap[chunkX,chunkY].GetLength(0);px++){
			for(int pz=0;pz<blockMap[chunkX,chunkY].GetLength(1);pz++){
				if(blockMap[chunkX,chunkY][px,pz,level]!=0){
					if(blockMap[chunkX,chunkY][px,pz,level]==1){
						GenCube(chunkX, chunkY, px, pz, level, tDirt);
					}
					else if(blockMap[0,0][px,pz,0]==2){
						GenCube(chunkX, chunkY, px, pz, level, tStone);
					}
				}
			}
		}
	}

	void UpdateMesh(int level, int chunkX, int chunkY) {
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.Clear();
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.vertices = newVertices.ToArray();
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.triangles = newTriangles.ToArray();
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.uv = newUV.ToArray();
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.Optimize();
		levelList[chunkX, chunkY][level].GetComponent<MeshFilter>().mesh.RecalculateNormals();
		
		newVertices.Clear();
		newTriangles.Clear();
		newUV.Clear();
		squareCount=0;
		
		Mesh collisionMesh = new Mesh();
		collisionMesh.vertices = colVertices.ToArray();
		collisionMesh.triangles= colTriangles.ToArray();
		levelList[chunkX, chunkY][level].GetComponent<MeshCollider>().sharedMesh = collisionMesh;
		
		colVertices.Clear();
		colTriangles.Clear();
		colCount=0;
		
	}

	byte Block (int chunkX, int chunkY, int x, int z, int level){
		
		if(x==-1 || x==blockMap[chunkX,chunkY].GetLength(0) || z==-1 || z==blockMap[chunkX,chunkY].GetLength(1) || level==-1 || level==blockMap[chunkX,chunkY].GetLength(2)){
			return (byte)0;
		}
		
		return blockMap[chunkX,chunkY][x,z,level];
	}

	void GenCube(int chunkX, int chunkY, int x, int z, int level, Vector2 texture){
		if(Block(chunkX, chunkY, x,z-1,level)==0){
			//Front of Cube
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);		
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
		if(Block(chunkX, chunkY,x,z+1,level)==0){
			//Back of Cube
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
		if(Block(chunkX, chunkY,x,z,level+1)==0){
			//Top of Cube
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);	
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
		if(Block(chunkX, chunkY,x,z,level-1)==0){
			//Bottom of Cube
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);		
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
		if(Block(chunkX, chunkY,x-1,z,level)==0){
			//Left of Cube
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
		if(Block(chunkX, chunkY,x+1,z,level)==0){
			//Right of Cube
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			newVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(newTriangles, squareCount);
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
			newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y - tUnit));
			newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y- tUnit));
			//Collider
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level+1, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			colVertices.Add(new Vector3(x+1 + chunkX * Mathf.FloorToInt(chunkSize.x), level, z+1 + chunkY * Mathf.FloorToInt(chunkSize.y)));
			GenTriangles(colTriangles, colCount);
			colCount++;
			squareCount++;
		}
	}

	void GenTriangles(List<int> trianglesList, int counter){
		trianglesList.Add(counter*4);
		trianglesList.Add(counter*4+1);
		trianglesList.Add(counter*4+3);
		trianglesList.Add(counter*4+1);
		trianglesList.Add(counter*4+2);
		trianglesList.Add(counter*4+3);
	}

	void Update(){
		for (int chunkX=0; chunkX<blockMap.GetLength(0); chunkX++){
			for (int chunkY=0; chunkY<blockMap.GetLength(1); chunkY++){
				//Check if chunk update is needed!
				if(update[chunkX, chunkY]){
					//Go through all chunk levels!!
					for(int lvl=0; lvl<blockMap[chunkX, chunkY].GetLength(2); lvl++){
						//If level update is indicated!
						if(updateLevel[chunkX,chunkY][lvl]){ //|| updateLevel[chunkX,chunkY][lvl]<blockMap[chunkX,chunkY].GetLength(2)){
							BuildLevelMesh(lvl, chunkX, chunkY);
							UpdateMesh(lvl, chunkX, chunkY);
							//Also update bottom lvl
							if(lvl-1>=0){
								BuildLevelMesh(lvl-1, chunkX, chunkY);
								UpdateMesh(lvl-1, chunkX, chunkY);
							}
							//And top level
							if(lvl+1<blockMap[chunkX,chunkY].GetLength(2)){
								BuildLevelMesh(lvl+1, chunkX, chunkY);
								UpdateMesh(lvl+1, chunkX, chunkY);
							}
						}
					}
				}
				update[chunkX, chunkY]=false;
			}
		}
	}
}

//Noise Generator (Do not mess with!)
public class SimplexNoiseGenerator {
	private int[] A = new int[3];
	private float s, u, v, w;
	private int i, j, k;
	private float onethird = 0.333333333f;
	private float onesixth = 0.166666667f;
	private int[] T;
	
	public SimplexNoiseGenerator() {
		if (T == null) {
			System.Random rand = new System.Random();
			T = new int[8];
			for (int q = 0; q < 8; q++)
				T[q] = rand.Next();
		}
	}
	
	public SimplexNoiseGenerator(string seed) {
		T = new int[8];
		string[] seed_parts = seed.Split(new char[] {' '});
		
		for(int q = 0; q < 8; q++) {
			int b;
			try {
				b = int.Parse(seed_parts[q]);
			} catch {
				b = 0x0;
			}
			T[q] = b;
		}
	}
	
	public SimplexNoiseGenerator(int[] seed) { // {0x16, 0x38, 0x32, 0x2c, 0x0d, 0x13, 0x07, 0x2a}
		T = seed;
	}
	
	public string GetSeed() {
		string seed = "";
		
		for(int q=0; q < 8; q++) {
			seed += T[q].ToString();
			if(q < 7)
				seed += " ";
		}
		
		return seed;
	}
	
	public float coherentNoise(float x, float y, float z, int octaves=1, int multiplier = 25, float amplitude = 0.5f, float lacunarity = 2, float persistence = 0.9f) {
		Vector3 v3 = new Vector3(x,y,z)/multiplier;
		float val = 0;
		for (int n = 0; n < octaves; n++) {
			val += noise(v3.x,v3.y,v3.z) * amplitude;
			v3 *= lacunarity;
			amplitude *= persistence;
		}
		return val;
	}
	
	public int getDensity(Vector3 loc) {
		float val = coherentNoise(loc.x, loc.y, loc.z);
		return (int)Mathf.Lerp(0,255,val);
	}
	
	// Simplex Noise Generator
	public float noise(float x, float y, float z) {
		s = (x + y + z) * onethird;
		i = fastfloor(x + s);
		j = fastfloor(y + s);
		k = fastfloor(z + s);
		
		s = (i + j + k) * onesixth;
		u = x - i + s;
		v = y - j + s;
		w = z - k + s;
		
		A[0] = 0; A[1] = 0; A[2] = 0;
		
		int hi = u >= w ? u >= v ? 0 : 1 : v >= w ? 1 : 2;
		int lo = u < w ? u < v ? 0 : 1 : v < w ? 1 : 2;
		
		return kay(hi) + kay(3 - hi - lo) + kay(lo) + kay(0);
	}
	
	float kay(int a) {
		s = (A[0] + A[1] + A[2]) * onesixth;
		float x = u - A[0] + s;
		float y = v - A[1] + s;
		float z = w - A[2] + s;
		float t = 0.6f - x * x - y * y - z * z;
		int h = shuffle(i + A[0], j + A[1], k + A[2]);
		A[a]++;
		if (t < 0) return 0;
		int b5 = h >> 5 & 1;
		int b4 = h >> 4 & 1;
		int b3 = h >> 3 & 1;
		int b2 = h >> 2 & 1;
		int b1 = h & 3;
		
		float p = b1 == 1 ? x : b1 == 2 ? y : z;
		float q = b1 == 1 ? y : b1 == 2 ? z : x;
		float r = b1 == 1 ? z : b1 == 2 ? x : y;
		
		p = b5 == b3 ? -p : p;
		q = b5 == b4 ? -q : q;
		r = b5 != (b4 ^ b3) ? -r : r;
		t *= t;
		return 8 * t * t * (p + (b1 == 0 ? q + r : b2 == 0 ? q : r));
	}
	
	int shuffle(int i, int j, int k) {
		return b(i, j, k, 0) + b(j, k, i, 1) + b(k, i, j, 2) + b(i, j, k, 3) + b(j, k, i, 4) + b(k, i, j, 5) + b(i, j, k, 6) + b(j, k, i, 7);
	}
	
	int b(int i, int j, int k, int B) {
		return T[b(i, B) << 2 | b(j, B) << 1 | b(k, B)];
	}
	
	int b(int N, int B) {
		return N >> B & 1;
	}
	
	int fastfloor(float n) {
		return n > 0 ? (int)n : (int)n - 1;
	}
}
