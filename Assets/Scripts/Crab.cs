using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this crab just wanders around on the see floor
public class Crab : MonoBehaviour {

    private enum State {
        Idle,
        Moving,
    }

    private State currently = State.Idle;
    private float stateTimer = 0f;
    
    private Vector2 destination;
    private float speed;

    private const float MAX_X_WANDER = 7f;
    private const float MIN_X_WANDER = -7f;
    private const float MAX_Y_WANDER = .08f;
    private const float MIN_Y_WANDER = .01f;
    private const float MIN_SPEED = 1f;
    private const float MAX_SPEED = 3f;
    private const float MIN_WAIT = 5f;
    private const float MAX_WAIT = 10f;
    private const float MIN_WOBBLE = 0.4f;
    private const float MAX_WOBBLE = 0.6f;
    private const float WOBBLE_SPEED_MODIFIER = 0.25f;

    private void Wobble() {
        float wobble = Random.Range(MIN_WOBBLE, MAX_WOBBLE);
        if (transform.rotation.z >= 0) {
            wobble *= -1;
        }
        float distance = Mathf.Abs(transform.rotation.z - wobble);
        float time = speed * WOBBLE_SPEED_MODIFIER * distance;
        Go.to(transform, time, new GoTweenConfig().rotation(new Vector3(0,0, wobble)).onComplete((AbstractGoTween agt) => {
            if (currently == State.Moving) {
                Wobble();
            }
        }));
    }
    
    // the crab will choose a destination to move to and go there
    private void Move() {
        // kill all pre-existing tweens on this transform
        Go.killAllTweensWithTarget(transform);
        // pick a destination
        destination = new Vector2(Random.Range(MIN_X_WANDER, MAX_X_WANDER), Random.Range(MIN_Y_WANDER, MAX_Y_WANDER));
        speed = Random.Range(MIN_SPEED, MAX_SPEED);
        float time = speed * Vector2.Distance(destination, transform.localPosition);
        // and go there
        Go.to(transform, time, new GoTweenConfig().localPosition(destination));
        // also wobble
        Wobble();

        currently = State.Moving;
        stateTimer = time; //set timer to end after movement
    }

    // crabs gonna hang here a while
    private void Idle() {
        // kill all pre-existing tweens on this transform
        Go.killAllTweensWithTarget(transform);

        // return to neutral wobble
        float distance = Mathf.Abs(transform.rotation.z);
        if (distance > 0) {
            float time = speed * WOBBLE_SPEED_MODIFIER * distance; // use whatever speed we were last wobbling with
            Go.to(transform, time, new GoTweenConfig().rotation(Vector3.zero));
        }
        
        // stay a while...
        stateTimer = Random.Range(MIN_WAIT, MAX_WAIT);
        currently = State.Idle;
    }

    void Update() {
        if (stateTimer > 0) {
            stateTimer -= Time.deltaTime;
        } else {
            switch (currently){
                case State.Idle:
                    Move();
                    break;
                case State.Moving:
                    Idle();
                    break;
                default:
                    Debug.LogWarningFormat("{0} unhandled state: {1}", this.name, currently);
                    break;
            }
        }
    }

    void OnDestroy() {
        // gotta remember to kill tweens if this is destroyed or it'll get kinda obnoxious
        Go.killAllTweensWithTarget(transform);
    }
}
