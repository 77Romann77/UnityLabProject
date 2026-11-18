using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private Rigidbody _rb;
    public float speedCar = 5.0f, speedRotation = 400f;
    private bool isClicked;
    private AudioSource _audio;
    public AudioClip _audioSart, _audioTurn;
    [NonSerialized] public Vector3 finalPosition;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _audio = GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (isClicked) return;
        
        isClicked = true;
        _audio.Stop();
        _audio.clip = _audioSart;
        _audio.loop = true;
        _audio.Play();
    }

    private void Update()
    {
        if (finalPosition.x != 0)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                finalPosition, speedCar * 3 * Time.deltaTime);
            
            Vector3 lookPos = finalPosition - transform.position;
            lookPos.y = 0;
            
            if (lookPos != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(lookPos), Time.deltaTime * speedRotation);
            }
        }

        if (transform.position == finalPosition)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        if (isClicked && finalPosition.x == 0)
        {
            _rb.MovePosition(_rb.transform.position + _rb.rotation * Vector3.forward * speedCar * Time.fixedDeltaTime);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RoadTrigger"))
        {
            _audio.PlayOneShot(_audioTurn);
        }
    }
}