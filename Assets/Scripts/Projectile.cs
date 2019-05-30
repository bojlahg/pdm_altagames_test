using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public Rigidbody rigidBody;
	public Game game;
	public float power;

	private void OnTriggerEnter(Collider cldr)
	{
		// Вылет за пределы экрана
		if(cldr.name == "DestroyVolume")
		{
			game.ProjectileFinished();
			GameObject.Destroy(gameObject);
		}
	}

	private void OnCollisionEnter(Collision clsn)
	{
		// столкнулись с препятствием
		Collider[] cldrs = Physics.OverlapSphere(transform.position, power * 5, game.obstacleLayerMask.value);
		
		// подсвечиваем препятствия
		foreach(Collider cldr in cldrs)
		{
			cldr.enabled = false;
			Renderer r = cldr.gameObject.GetComponent<Renderer>();
			r.sharedMaterial = game.deadObstacleMaterial;
		}
		game.AddDeadObstacles(cldrs);

		// удаляем пулю
		GameObject.Destroy(gameObject);
		game.ProjectileFinished();
	}
}
