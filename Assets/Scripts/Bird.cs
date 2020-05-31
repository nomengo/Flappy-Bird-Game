using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey;

public class Bird : MonoBehaviour
{
    private const float JUMP_AMOUNT = 100f;

    public event EventHandler OnDied;
    public event EventHandler OnStartedPlaying;

    private static Bird instance;
    private State state;

    public static Bird GetInstance()
    {
        return instance;
    }

    private enum State
    {
        WaitingToStart,
        Playing,
        Dead
    }

    private Rigidbody2D birdrigidbody2D;
    private void Awake()
    {
        instance = this;
        birdrigidbody2D = GetComponent<Rigidbody2D>();
        birdrigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.WaitingToStart;
    }
    private void Update()
    {
        switch (state)
        {
            default:
            case State.WaitingToStart:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    state = State.Playing;
                    birdrigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                    Jump();
                    if(OnStartedPlaying != null)
                    {
                        OnStartedPlaying(this, EventArgs.Empty);
                    }
                }
                break;
            case State.Playing:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    Jump();
                }
                break;
            case State.Dead:
                break;
        }

    }
    private void Jump()
    {
        birdrigidbody2D.velocity = Vector2.up * JUMP_AMOUNT;
        SoundManager.PlaySound(SoundManager.Sound.BirdJump);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        
        birdrigidbody2D.bodyType = RigidbodyType2D.Static;
        SoundManager.PlaySound(SoundManager.Sound.Lose);
        if (OnDied != null)
        {
            OnDied(this, EventArgs.Empty);
        }
    }
}
