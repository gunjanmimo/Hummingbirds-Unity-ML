using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

/// <summary>
/// Mangaes a single flower with nectar
/// </summary>
public class Flower : MonoBehaviour
{
    [Tooltip("The color when flower is full")]
    public Color fullFlowerColor = new Color(1f, 0f, 3f);

    [Tooltip("The color when flower is emepty")]
    public Color emptyFlowerColor = new Color(.5f, 0f, 1f);

    /// <summary>
    /// the tigger collider representing the nectar
    /// </summary>
    [HideInInspector]
    public Collider nectarCollider;

    // the solid collider representing the petals
    private Collider flowerCollider;

    // the flower's materials
    private Material flowerMaterial;

    /// a vector pointing straight out of the flower
    public Vector3 FlowerUpVector
    {
        get
        {
            return nectarCollider.transform.up;
        }
    }
    /// <summary>
    /// the center position of the nectar collider
    /// </summary>
    public Vector3 FlowerCenterPosition
    {
        get
        {
            return nectarCollider.transform.position;
        }
    }

    //necter amount remaining in the flower
    public float NectarAmount { get; private set; }

    /// <summary>
    /// wheather flower has any nectar remaining
    /// </summary>
    public bool HasNectar
    {
        get
        {
            return NectarAmount > 0f;
        }
    }

    /// <summary>
    /// attempt to remove from the flower
    /// </summary>
    /// <param name="amount">the amount of nectar to remove </param>
    /// <returns>the actual amount successfully removed</returns>
    public float Feed(float amount)
    {
        // track how much nectar was successfully taken
        float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);
        NectarAmount -= amount;
        if (NectarAmount <= 0)
        {
            NectarAmount = 0;
            flowerCollider.gameObject.SetActive(false);
            nectarCollider.gameObject.SetActive(false);

            //change the flower color to indicate that it is empty
            flowerMaterial.SetColor("_BaseColor", emptyFlowerColor);
        }
        return nectarTaken;
    }

    /// <summary>
    /// reset the flower
    /// </summary>
    public void ResetFlower()
    {
        // refill the nectar
        NectarAmount = 1f;
        flowerCollider.gameObject.SetActive(true);
        nectarCollider.gameObject.SetActive(true);

        //change the flower color to indicate that it is full
        flowerMaterial.SetColor("_BaseColor", fullFlowerColor);
    }
    /// <summary>
    /// called when the flower wakes up
    /// </summary>
    private void Awake()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        flowerMaterial = meshRenderer.material;

        flowerCollider = transform.Find("FlowerCollider").GetComponent<Collider>();
        nectarCollider = transform.Find("FlowerNectarCollider").GetComponent<Collider>();
    }
}
