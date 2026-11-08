using UnityEngine;

// Движение от первого лица: ходьба (WASD), бег (Shift), приседание (C или Ctrl),
// прыжок (Пробел), обзор мышью.
// Вешается на объект Player, у которого есть CharacterController.
// Камера должна быть ДОЧЕРНИМ объектом Player.
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащи сюда камеру (дочерний объект игрока)")]
    public Transform cameraHolder;

    [Header("Скорость")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float jumpHeight = 1f;
    public float gravity = -20f;

    [Header("Мышь")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Приседание")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 8f;

    // Другие скрипты могут читать эти значения (например, монстр "слышит" бег)
    public bool IsRunning { get; private set; }
    public bool IsCrouching { get; private set; }

    // Можно выключить управление (смерть, меню, пауза)
    [HideInInspector] public bool canMove = true;

    CharacterController controller;
    float verticalVelocity;   // скорость по вертикали (гравитация/прыжок)
    float xRotation;          // наклон камеры вверх-вниз
    float cameraStandY;       // высота камеры стоя

    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standHeight;
        controller.center = Vector3.zero;
        cameraStandY = cameraHolder.localPosition.y;

        // Прячем и фиксируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!canMove) return;

        Look();
        HandleCrouch();
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Камера наклоняется вверх-вниз, но не дальше maxLookAngle
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Всё тело поворачивается влево-вправо
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!IsCrouching) IsCrouching = true;
            else if (CanStandUp()) IsCrouching = false; // не встаём, если над головой потолок
        }

        // Плавно меняем рост капсулы, ноги остаются на месте
        float targetHeight = IsCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0f, (controller.height - standHeight) / 2f, 0f);

        // Камера опускается вместе с ростом
        Vector3 camPos = cameraHolder.localPosition;
        camPos.y = cameraStandY - (standHeight - controller.height);
        cameraHolder.localPosition = camPos;
    }

    bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * (-standHeight / 2f + crouchHeight);
        float distance = standHeight - crouchHeight;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.up, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != controller) return false;
        }
        return true;
    }

    void Move()
    {
        bool grounded = controller.isGrounded;
        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // прижимаем к земле

        float x = Input.GetAxis("Horizontal"); // A / D
        float z = Input.GetAxis("Vertical");   // W / S

        Vector3 direction = transform.right * x + transform.forward * z;
        if (direction.magnitude > 1f) direction.Normalize(); // чтобы по диагонали не было быстрее

        // Бежать можно только вперёд и не в приседе
        IsRunning = Input.GetKey(KeyCode.LeftShift) && !IsCrouching && z > 0.1f;
        float speed = IsCrouching ? crouchSpeed : (IsRunning ? runSpeed : walkSpeed);

        if (Input.GetButtonDown("Jump") && grounded && !IsCrouching)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
