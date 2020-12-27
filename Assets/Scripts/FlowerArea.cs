using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// manages a collection of flowrs
/// </summary>
public class FlowerArea : MonoBehaviour
{
    // the diameter of the area where the agent and flower can be used
    public const float AreaDiameter = 20f;
    //the list of all flower plants in this flower area
    private List<GameObject> flowerPlants;

    // a lookup dictionary for looking up flower from a nectar collider
    private Dictionary<Collider, Flower> nectarFlowerDictionary;

    /// <summary>
    /// the list of all flowers in the area
    /// </summary>
    public List<Flower> Flowers { get; private set; }

    public void ResetFlowers()
    {
        // rotate each flower plant around the y axis and subtly around x and z
        foreach (GameObject flowerPlant in flowerPlants)
        {
            float xRotation = Random.Range(-5f, 5f);
            float yRotation = Random.Range(-180f, 180f);
            float zRotation = Random.Range(-5f, 5f);
            flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, xRotation);
        }
        foreach (Flower flower in Flowers)
        {
            flower.ResetFlower();
        }
    }

    /// <summary>
    /// get the flower that a nectar collider belongs to 
    /// </summary>
    /// <param name="collider">the centar collider</param>
    /// <returns>the matching flower</returns>
    public Flower GetFlowerFromNectar(Collider collider)
    {
        return nectarFlowerDictionary[collider];
    }

    /// <summary>
    /// called when the area wakes up
    /// </summary>
    private void Awake()
    {
        flowerPlants = new List<GameObject>();
        nectarFlowerDictionary = new Dictionary<Collider, Flower>();
        Flowers = new List<Flower>();
    }
    /// <summary>
    /// called when the game starts
    /// </summary>
    private void Start()
    {
        // find all flowers that are children of this gameobject
        FindChildFlowers(transform);

    }
    /// <summary>
    /// find all the flowers and flower plants 
    /// </summary>
    /// <param name="transform"></param>
    private void FindChildFlowers(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.CompareTag("flower_plant"))
            {
                //found a flower plant, add it to the flowerplant list
                flowerPlants.Add(child.gameObject);
                FindChildFlowers(child);
            }
            else
            {
                //not a flower plant, look for a flowr plant
                Flower flower = child.GetComponent<Flower>();
                if (flower != null)
                {
                    Flowers.Add(flower);
                    // add nectar collider to the lookup dictionary
                    nectarFlowerDictionary.Add(flower.nectarCollider, flower);
                }
                else
                {
                    // flower component not found, so check childern
                    FindChildFlowers(child);
                }
            }
        }
    }
}
