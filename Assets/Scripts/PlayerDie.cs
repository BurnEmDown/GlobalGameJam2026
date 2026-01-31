using UnityEngine;
using System;
using System.Collections;

public class PlayerDie : MonoBehaviour
{
	public Player player;
	public TrackManager trackManager;
	public GameObject frostCam;

	public float stopTime = 2,
		flySpeed = 10,
		camResetTime = 2;
	public GameObject[] accessories;
	
    void OnEnable()
    {
        player.enabled = false;
		trackManager.enabled = false;
		frostCam.SetActive(false);
		player.cam.parent = null;
		player.GetComponent<Collider>().enabled = false;
		player.GetComponentInChildren<Animator>().SetTrigger("Die");
		player.transform.GetChild(0).parent = transform;
		GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, flySpeed);
		foreach (GameObject obj in accessories)
		{
			obj.transform.parent = null;
			obj.GetComponent<Collider>().enabled = true;
			Rigidbody rb2 = obj.GetComponent<Rigidbody>();
			rb2.isKinematic = false;
			rb2.linearVelocity = new Vector3(0, 0, flySpeed);
		}
    }
	
	void Update()
	{
		Vector3 forward = Vector3.Lerp(player.cam.forward, Vector3.forward, Time.deltaTime),
			up = Vector3.Lerp(player.cam.up, Vector3.up, Time.deltaTime);
		player.cam.rotation = Quaternion.LookRotation(forward, up);
		transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, Vector3.zero, Time.deltaTime / camResetTime);
	}
}
