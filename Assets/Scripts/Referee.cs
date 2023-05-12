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
        Vector3 directionToBall = scriptBall.transform.position - transform.position;
        if (distanceToBall > 9)
        {
            // move in direction of ball
            transform.LookAt(new Vector3(directionToBall.x, transform.position.y, directionToBall.z));
            Vector3 moveSpeed = new Vector3(directionToBall.normalized.x * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, directionToBall.normalized.z * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime);
            transform.position += moveSpeed;
            animator.SetFloat("Speed", Player.NORMAL_MOVEMENT_SPEED * 2);
        }
        else if (distanceToBall < 8)
        {
            // move away from ball
            transform.LookAt(new Vector3(-directionToBall.x, transform.position.y, -directionToBall.z));
            Vector3 moveSpeed = new Vector3(-directionToBall.normalized.x * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime, 0, -directionToBall.normalized.z * Player.NORMAL_MOVEMENT_SPEED * Time.deltaTime);
            transform.position += moveSpeed;
            animator.SetFloat("Speed", Player.NORMAL_MOVEMENT_SPEED * 2);
        }
        else
        {
            transform.LookAt(new Vector3(directionToBall.x, transform.position.y, directionToBall.z));
            animator.SetFloat("Speed", 0);
        } 
    }


}
