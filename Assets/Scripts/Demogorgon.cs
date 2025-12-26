using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demogorgon : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] float detectionRadius = 5f; // Радиус поиска игрока
    [SerializeField] float attackRange = 1.5f;   // Дистанция атаки
    [SerializeField] float attackDistance = 2f;  // Расстояние, на котором враг останавливается при атаке
    [SerializeField] LayerMask whatIsPlayer;     // Слой игрока
    [SerializeField] LayerMask whatIsWall;       // Слой стен

    [Header("Patrol Points")]
    [SerializeField] List<Transform> patrolPoints = new List<Transform>(); // Точки патрулирования
    [SerializeField] float timeToLostPlayer; // Время, в течение которого враг продолжает преследование после потери игрока

    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float chaseSpeed = 6f;

    [Header("Player Scripts To Disable")]
    [SerializeField] List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>(); // Скрипты игрока для отключения

    [Header("Game Over")]
    [SerializeField] GameObject losePanel; // Экран проигрыша

    [Header("Camera Settings")]
    [SerializeField] Camera playerCamera; // Камера игрока
    [SerializeField] float attackFOV = 90f; // FOV камеры при атаке
    [SerializeField] float normalFOV = 60f; // Нормальный FOV камеры
    [SerializeField] float eyeHeight = 1.5f; // Высота глаз врага (корректировка для поворота камеры)

    private Animator anim;
    private Transform target; // Игрок
    private int currentPatrolIndex = 0;
    private bool isPatrolling = true;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool hasAttacked = false;
    private Vector3 startPosition;
    private Vector3 lastKnownPlayerPosition; // Последняя известная позиция игрока
    private bool playerWasVisible = false; // Был ли игрок виден в предыдущем кадре
    private float currentLostTime = 0f; // Текущее время с момента потери игрока
    private Rigidbody rb; // Используем Rigidbody для 3D движения

    void Start()
    {
        anim = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player")?.transform;

        startPosition = transform.position;

        if (GetComponent<Rigidbody>() != null)
            rb = GetComponent<Rigidbody>();

        if (patrolPoints.Count == 0 && transform.childCount > 0)
        {
            // Если список пуст, но есть дочерние объекты, предполагаем, что это точки патрулирования
            foreach (Transform child in transform)
            {
                patrolPoints.Add(child);
            }
        }

        SetAnim_Walk(-1); // Начинаем с ходьбы по точкам
    }

    void Update()
    {
        if (hasAttacked) return; // Если уже атаковал, больше ничего не делаем

        // Проверяем, виден ли игрок
        bool playerVisible = IsPlayerInSight();

        if (playerVisible)
        {
            // Игрок виден - преследуем
            if (!isAttacking)
            {
                isChasing = true;
                isPatrolling = false;
                SetAnim_Walk(1); // Бежим за игроком

                // Сохраняем последнюю известную позицию игрока
                lastKnownPlayerPosition = target.position;
                playerWasVisible = true;
                currentLostTime = 0f; // Сбрасываем таймер потери
            }

            // Если враг атакует, не преследуем
            if (!isAttacking)
            {
                ChasePlayer();
            }
        }
        else
        {
            // Игрок не виден
            if (playerWasVisible && !isAttacking)
            {
                // Игрок только что исчез из виду - начинаем отсчет времени
                currentLostTime += Time.deltaTime;

                if (currentLostTime <= timeToLostPlayer)
                {
                    // Продолжаем преследование до последней известной позиции
                    ContinueChase();
                }
                else
                {
                    // Время истекло - возвращаемся к патрулированию
                    isChasing = false;
                    isPatrolling = true;
                    playerWasVisible = false;
                    SetAnim_Walk(-1); // Возвращаемся к патрулированию
                }
            }
            else if (!isAttacking && !isChasing)
            {
                // Если игрок не виден и время потери истекло
                isPatrolling = true;
                SetAnim_Walk(-1); // Возвращаемся к патрулированию
            }

            if (isPatrolling && !isAttacking)
            {
                Patrol();
            }
        }

        // Проверяем дистанцию до игрока для атаки
        if (CanSeePlayerForAttack() && !isAttacking)
        {
            AttackPlayer();
        }
    }

    bool IsPlayerInSight()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Если слишком далеко, сразу возвращаем false
        if (distanceToTarget > detectionRadius)
            return false;

        // Проверяем, есть ли препятствие между врагом и игроком
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToTarget, out hit, distanceToTarget, whatIsWall))
        {
            // Если луч попал в стену, значит игрок за стеной
            return false;
        }

        return true; // Игрок виден
    }

    void Patrol()
    {
        if (patrolPoints.Count == 0) return;

        Transform currentPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (currentPoint.position - transform.position).normalized;

        // Поворачиваем врага в сторону движения
        if (direction.x != 0 || direction.z != 0) // Проверяем, что направление не равно нулю
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Плавный поворот

            // Обнуляем X-компонент вращения
            Vector3 eulers = transform.eulerAngles;
            eulers.x = 0f;
            transform.eulerAngles = eulers;
        }

        if (rb != null)
        {
            rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // Проверяем, достигли ли мы текущей точки
        if (Vector3.Distance(transform.position, currentPoint.position) < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
    }

    void ChasePlayer()
    {
        if (target == null) return;

        // Вычисляем направление к игроку
        Vector3 direction = (target.position - transform.position).normalized;

        // Проверяем расстояние до игрока - если уже близко к атаке, останавливаемся
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= attackDistance)
        {
            // Останавливаем движение
            if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        // Поворачиваем врага в сторону игрока
        if (direction.x != 0 || direction.z != 0) // Проверяем, что направление не равно нулю
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f); // Более быстрый поворот при преследовании

            // Обнуляем X-компонент вращения
            Vector3 eulers = transform.eulerAngles;
            eulers.x = 0f;
            transform.eulerAngles = eulers;
        }

        if (rb != null)
        {
            rb.velocity = new Vector3(direction.x * chaseSpeed, rb.velocity.y, direction.z * chaseSpeed);
        }
        else
        {
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }
    }

    void ContinueChase()
    {
        // Продолжаем преследование до последней известной позиции игрока
        Vector3 direction = (lastKnownPlayerPosition - transform.position).normalized;

        // Проверяем расстояние до последней известной позиции - если уже близко, останавливаемся
        float distanceToTarget = Vector3.Distance(transform.position, lastKnownPlayerPosition);
        if (distanceToTarget <= attackDistance)
        {
            // Останавливаем движение
            if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        // Поворачиваем врага в сторону последней известной позиции игрока
        if (direction.x != 0 || direction.z != 0) // Проверяем, что направление не равно нулю
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f); // Более быстрый поворот при преследовании

            // Обнуляем X-компонент вращения
            Vector3 eulers = transform.eulerAngles;
            eulers.x = 0f;
            transform.eulerAngles = eulers;
        }

        if (rb != null)
        {
            rb.velocity = new Vector3(direction.x * chaseSpeed, rb.velocity.y, direction.z * chaseSpeed);
        }
        else
        {
            transform.position += direction * chaseSpeed * Time.deltaTime;
        }
    }

    void AttackPlayer()
    {
        if (isAttacking || hasAttacked) return;

        isAttacking = true;
        isChasing = false;
        isPatrolling = false;

        // Останавливаем движение
        if (rb != null) rb.velocity = new Vector3(0, rb.velocity.y, 0);

        target.GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Поворачиваем врага к игроку
        if (target != null)
        {
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            if (directionToPlayer.x != 0 || directionToPlayer.z != 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
                transform.rotation = targetRotation;

                // Обнуляем X-компонент вращения
                Vector3 eulers = transform.eulerAngles;
                eulers.x = 0f;
                transform.eulerAngles = eulers;
            }
        }

        // Блокируем вращение игрока при атаке
        var targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.freezeRotation = true;
        }

        // Поворачиваем камеру игрока к врагу
        if (playerCamera != null)
        {
            // Вычисляем позицию глаз врага (корректировка для более естественного поворота)
            Vector3 enemyEyePosition = transform.position + Vector3.up * eyeHeight;

            // Направление от камеры игрока к глазам врага
            Vector3 lookDirection = (enemyEyePosition - playerCamera.transform.position).normalized;

            // Поворачиваем камеру к глазам врага
            playerCamera.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            // Изменяем FOV
            playerCamera.fieldOfView = attackFOV;
        }

        // Отключаем скрипты игрока
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Проигрываем анимацию атаки
        SetAnim_Attack(1);

        StartCoroutine(GameOver());
    }

    IEnumerator GameOver()
    {
        // Ждем немного перед завершением игры
        yield return new WaitForSeconds(2.5f);  // Даем время для проигрывания анимации

        hasAttacked = true;

        // Активируем экран проигрыша
        losePanel.SetActive(true);

        // Останавливаем все действия
        enabled = false;
    }

    bool CanSeePlayerForAttack()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > attackRange)
            return false;

        // Сдвигаем луч вверх, чтобы избежать пересечения с коллайдерами ног
        Vector3 rayOrigin = transform.position + Vector3.up * 0.7f;
        Vector3 targetPos = target.position + Vector3.up * 0.7f;
        Vector3 direction = (targetPos - rayOrigin).normalized;
        float distance = Vector3.Distance(rayOrigin, targetPos);

        // Проверяем, не блокирует ли стену путь
        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, distance, whatIsWall))
        {
            return false; // Стена мешает — нельзя атаковать
        }

        return true; // Игрок в зоне атаки и ничто не мешает
    }

    // Методы из вашего скрипта
    void SetAnim_Walk(int walk) => anim.SetInteger("Walk", walk);
    void SetAnim_Attack(int attack) => anim.SetInteger("Attack", attack != -1 ? Random.Range(0, 4) : -1);

    // Для отладки - показываем радиус обзора в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // Показываем точки патрулирования
        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                    if (i > 0)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                    }
                }
            }
        }
    }
}