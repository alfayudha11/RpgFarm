using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject item;
    private Transform player;
    public string itemName;
    public int itemPrice;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnDroppedItem()
    {
        Vector3 playerpos = new Vector3(player.position.x, player.position.y, player.position.z + 3);
        Instantiate(item, playerpos, Quaternion.identity);
    }
}
