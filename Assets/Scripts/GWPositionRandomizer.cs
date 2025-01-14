using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LatticeElement
{
    public Vector2      latticeCoord;
    public Vector2      worldCoord;
    public GWBuilding   building;

    public LatticeElement(Vector2 iLatticeCoord, Vector2 iWorldCoord)
    {
        latticeCoord = iLatticeCoord;
        worldCoord = iWorldCoord;
        building = null;
    }

    public bool IsOccupied()
    {
        return building != null;
    }
}

public class GWPositionRandomizer : MonoBehaviour
{
    public float YCoordinate = 0f;
    public Mesh boundaries;
    private float xmin;
    private float xmax;
    private float zmin;
    private float zmax;
    public int xDef, yDef;
    public float square_w, square_h;

    public List<GWBuilding> RandomizedTransforms;
    public LatticeElement[,] lattice;

    void Start()
    {
        //init();
    }

    public void init()
    {
        xmin = transform.localPosition.x + boundaries.bounds.min.x * transform.lossyScale.x;
        xmax = transform.localPosition.x + boundaries.bounds.max.x * transform.lossyScale.x;

        zmin = transform.localPosition.z + boundaries.bounds.min.z * transform.lossyScale.z;
        zmax = transform.localPosition.z + boundaries.bounds.max.z * transform.lossyScale.z;

        // world lattice
        square_w = (xmax - xmin)/xDef;
        square_h = (zmax - zmin)/yDef;
        lattice = new LatticeElement[xDef,yDef];

        for(int i=0; i<xDef;i++)
        {
            for (int j=0; j<yDef;j++)
            {
                lattice[i,j] = new LatticeElement
                                    (
                                        new Vector2(i,j),
                                        new Vector2( xmin + (i * square_w), zmin + (j * square_h) )
                                    );
            }
        }
    }

    public Vector3 GetRandPosition()
    {
        return new Vector3(Random.Range(xmin,xmax), YCoordinate, Random.Range(zmin,zmax));
    }

    public void RandomizeAll()
    {
        foreach(GWBuilding b in RandomizedTransforms)
        {
            // TODO : Prevent spawning in same spot
            Transform t = b.transform;
            LatticeElement latticeEl = lattice[Random.Range(0,xDef), Random.Range(0, yDef)];
            while(latticeEl.IsOccupied())
            {   // TODO : Ensure the lattice is not full
                latticeEl = lattice[Random.Range(0,xDef), Random.Range(0, yDef)];
            }
            latticeEl.building = b;
            t.localPosition = new Vector3(latticeEl.worldCoord.x, YCoordinate, latticeEl.worldCoord.y);
        }
    }
}
