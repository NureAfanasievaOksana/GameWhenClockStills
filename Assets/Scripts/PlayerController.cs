using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Vector2 _targetPosition;
    private bool _isMoving;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private InteractableObject _currentInteractable;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Floor"))
                {
                    _targetPosition = hit.point;
                    _isMoving = true;
                    _animator.SetBool("isMoving", true);
                    _currentInteractable = null;
                }
                else if (hit.collider.TryGetComponent<InteractableObject>(out var interactable))
                {
                    List<ItemData> allItems = interactable.GetAllItemData();
                    bool isTimeDevice = allItems.Any(item => item.type == "time_device");
                    if (isTimeDevice)
                    {
                        interactable.Interact(); // одразу викликаємо без руху
                        return;
                    }

                    _targetPosition = interactable.GetInteractionPoint();
                    _isMoving = true;
                    _animator.SetBool("isMoving", true);
                    _currentInteractable = interactable;
                }
            }
        }

        if (_isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            if (_targetPosition.x < transform.position.x)
            {
                _spriteRenderer.flipX = true;
            }
            else if (_targetPosition.x > transform.position.x)
            {
                _spriteRenderer.flipX = false;
            }

            if ((Vector2)transform.position == _targetPosition)
            {
                _isMoving = false;
                _animator.SetBool("isMoving", false);

                if (_currentInteractable != null)
                {
                    _currentInteractable.Interact();
                    _currentInteractable = null;
                }
            }
        }
    }

    public void MoveToAndInteract(Vector2 position, InteractableObject interactable)
    {
        _targetPosition = position;
        _isMoving = true;
        _animator.SetBool("isMoving", true);
        _currentInteractable = interactable;
    }
}