using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	public enum State
	{
		Input, // ждем ввода
		Projectile, // ждем результатов полета мяча
		AnimFinish, // ждем конца анимации прохода игрока к цели
		GameOver, // конец игры
	}

	public GameObject mainScreen, gameScreen;
	public Text resultText, lifeText;
	public Transform playerTransform, targetTransform;
	public GameObject obstaclePrefab, projectilePrefab;
	public Material deadObstacleMaterial;
	public LayerMask obstacleLayerMask;
	//
	private List<GameObject> obstacleList = new List<GameObject>();
	private List<GameObject> deadObstacleList = new List<GameObject>();
	private State state = State.GameOver;
	private Projectile projectile;
	private bool tap = false, prevTap = false;
	private float playerLife = 0, shootPower = 0, obstacleRemoveTimer = 0, finishTimer = 0;
	private Vector3 shootDir, playerInitialPos;

	private void Start()
    {
		Application.targetFrameRate = 60;
		// показываем стартовый экран
		resultText.gameObject.SetActive(false);
		mainScreen.SetActive(true);
		gameScreen.SetActive(false);

		playerInitialPos = playerTransform.position;
	}

	public void PlayButtonClick()
	{
		// показываем игровой экран
		mainScreen.SetActive(false);
		gameScreen.SetActive(true);

		StartGame();
	}


	private void StartGame()
	{
		// старт игры
		// направление от игрока к цели
		playerTransform.position = playerInitialPos;
		shootDir = (targetTransform.position - playerTransform.position).normalized;
		playerTransform.localScale = Vector3.one;

		GenerateObstacles();

		// сброс переменных
		playerLife = 1;
		finishTimer = 0;
		tap = false;
		prevTap = false;

		state = State.Input;
	}

	private void GenerateObstacles()
	{
		// генерация уровня

		// удаляем старые препятствия
		foreach(GameObject go in obstacleList)
		{
			GameObject.Destroy(go);
		}
		obstacleList.Clear();
		deadObstacleList.Clear();

		//
		int w = 20, h = 25;
		for(int z = 0; z < h; ++z)
		{
			for(int x = 0; x < w; ++x)
			{
				if(Random.Range(0, 1000) % 2 == 0)
				{
					Vector2 rpos2d = Random.insideUnitCircle;
					Vector3 rpos3d = new Vector3(rpos2d.x, 0, rpos2d.y);
					Vector3 pos = new Vector3(-4 + 8 * x / (float)w, 0, -4 + 10 * z / (float)h) + rpos3d * 0.2f;
					GameObject go = GameObject.Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);
					obstacleList.Add(go);
				}
			}
		}
	}


	private void Update()
    {
		switch(state)
		{
		case State.Input:
			UpdateInput();
			break;

		case State.Projectile:
			UpdateProjectile();
			break;

		case State.AnimFinish:
			UpdateFinishAnim();
			break;
		}


		if(deadObstacleList.Count > 0)
		{
			obstacleRemoveTimer -= Time.deltaTime;
			if(obstacleRemoveTimer <= 0)
			{ 
				foreach(GameObject go in deadObstacleList)
				{
					go.SetActive(false);
				}
				deadObstacleList.Clear();
			}
		}
	}

	public void AddDeadObstacles(Collider[] cldrs)
	{
		// добавляем препятствия в список на уничтожение
		foreach(Collider cldr in cldrs)
		{
			deadObstacleList.Add(cldr.gameObject);
		}
		obstacleRemoveTimer = 1.0f;
	}

	public void ProjectileFinished()
	{
		state = State.Input;
		// проверяем что можно долететь до цели
		RaycastHit rch;
		if(!Physics.SphereCast(playerTransform.position, playerLife, shootDir, out rch, 500, obstacleLayerMask.value))
		{
			state = State.AnimFinish;
		}
	}

	private void UpdateInput()
	{
		// реагируем на тач пробел или мышь
		tap = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0) || Input.touchCount > 0;

		if(tap != prevTap)
		{
			if(tap)
			{
				// нажали
				shootPower = 0.05f; // минимальный размер
				GameObject go = GameObject.Instantiate(projectilePrefab, playerTransform.position + shootDir * (0.5f * playerLife + 0.5f * shootPower + 0.1f), Quaternion.identity, transform);
				projectile = go.GetComponent<Projectile>();
				projectile.game = this;
			}
			else
			{
				// отжали
				projectile.rigidBody.velocity = 7 * shootDir;
				projectile.power = shootPower;
				state = State.Projectile;
			}
		}

		if(tap)
		{
			// нажато
			shootPower += 0.5f * Time.deltaTime;
			playerLife -= 0.25f * Time.deltaTime;

			if(playerLife < 0.05f)
			{
				state = State.GameOver;
				resultText.text = "Конец игры: кончился шарик";
				resultText.color = Color.red;

				resultText.gameObject.SetActive(true);
				mainScreen.SetActive(true);
				gameScreen.SetActive(false);
			}

			playerTransform.localScale = Vector3.one * playerLife;
			projectile.transform.position = playerTransform.position + shootDir * (0.5f * playerLife + 0.5f * shootPower + 0.1f);
			projectile.transform.localScale = Vector3.one * shootPower;

			lifeText.text = string.Format("{0:0.00} %", playerLife * 100.0f);
		}

		// сохраняем текущее значение
		prevTap = tap;
	}

	private void UpdateProjectile()
	{

	}

	private void UpdateFinishAnim()
	{
		finishTimer += Time.deltaTime;
		float t = finishTimer * 0.5f;
		if(t < 1)
		{
			playerTransform.position = Vector3.Lerp(playerInitialPos, targetTransform.position, t);
		}
		else
		{
			state = State.GameOver;
			resultText.text = "Конец игры: Вы достигли цели";
			resultText.color = Color.green;

			resultText.gameObject.SetActive(true);
			mainScreen.SetActive(true);
			gameScreen.SetActive(false);
		}
	}
}
