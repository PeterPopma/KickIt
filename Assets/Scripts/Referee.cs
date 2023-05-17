using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Referee : MonoBehaviour
{
    [SerializeField] GameObject cameraReferee;
    [SerializeField] GameObject yellowCard;
    protected Ball scriptBall;
    protected Animator animator;
    float timeleftShowYellowCard = 0;
    float speed, targetSpeed;
    Quaternion targetRotation;

    private void Awake()
    {
        scriptBall = GameObject.Find("Ball").transform.GetComponent<Ball>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraReferee.SetActive(false);
        yellowCard.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (timeleftShowYellowCard>0)
        {
            timeleftShowYellowCard -= Time.deltaTime;
            if (timeleftShowYellowCard <= 0)
            {
                cameraReferee.SetActive(false);
                yellowCard.SetActive(false);
                animator.SetLayerWeight(1, 0);
            }
        }
        else
        {
            MoveAlongWithBall();
        }
    }

    public void DrawYellowCard()
    {
        yellowCard.SetActive(true);
        cameraReferee.SetActive(true);
        timeleftShowYellowCard = 5;
        animator.Play("DrawCard", 1, 0);
        animator.SetLayerWeight(1, 100);
    }

    void MoveAlongWithBall()
    {
        float distanceToBall = (scriptBall.transform.position - transform.position).magnitude;
        Vector2 directionToBall = new Vector2(scriptBall.transform.position.x - transform.position.x, scriptBall.transform.position.z - transform.position.z).normalized;

        if (distanceToBall > 9)
        {   
            // move in direction of ball
            Vector3 moveDirection = new Vector3(directionToBall.x * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, directionToBall.y * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime);
            targetRotation = Quaternion.LookRotation(moveDirection);
            transform.position += moveDirection;
            targetSpeed = Player.NORMAL_MOVEMENT_SPEED * 2;
        }
        else if (distanceToBall < 8)
        {
            // move away from ball
            Vector3 moveDirection = new Vector3(-directionToBall.x * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, -directionToBall.y * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime);
            targetRotation = Quaternion.LookRotation(moveDirection);
            transform.position += moveDirection;
            targetSpeed = Player.NORMAL_MOVEMENT_SPEED * 2;
        }
        else
        {
            // look at ball
            targetRotation = Quaternion.LookRotation(new Vector3(directionToBall.x, 0, directionToBall.y));
            targetSpeed = 0;
        }

        if (speed > targetSpeed)
        {
            speed -= Time.deltaTime * 40;
        }
        if (speed < targetSpeed)
        {
            speed += Time.deltaTime * 40;
        }

        animator.SetFloat("Speed", speed);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 300f);
    }


}
