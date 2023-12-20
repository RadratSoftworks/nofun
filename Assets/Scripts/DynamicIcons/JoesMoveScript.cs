using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Nofun.DynamicIcons
{
    public class JoesMoveScript : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform startPoint;
        [SerializeField] private float rotateDuration = 0.5f;

        [SerializeField]
        [Min(0.1f)]
        private float speed = 4.0f;

        private const string ShouldGangParameter = "shouldGang";
        private const string MoveBackParameter = "shouldGoBack";

        private bool readyToGang = false;
        private bool goingBack = false;

        private void Start()
        {
            transform.position = startPoint.transform.position;
        }

        private void Update()
        {
            if (!readyToGang)
            {
                if (!goingBack)
                {
                    if (math.distance(transform.position, endPoint.transform.position) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, endPoint.transform.position, speed * Time.deltaTime);
                        transform.rotation = Quaternion.LookRotation(endPoint.transform.position - transform.position);
                    }
                    else
                    {
                        readyToGang = true;
                        animator.SetBool(ShouldGangParameter, true);
                    }
                }
                else
                {
                    if (math.distance(transform.position, startPoint.transform.position) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, startPoint.transform.position, speed * Time.deltaTime);
                    }
                    else
                    {
                        goingBack = false;

                        animator.SetBool(MoveBackParameter, false);
                        transform.DOLookAt(endPoint.transform.position, rotateDuration);
                    }
                }
            }
        }

        public void OnGangDone()
        {
            readyToGang = false;
            goingBack = true;

            animator.SetBool(ShouldGangParameter, false);
            animator.SetBool(MoveBackParameter, true);

            transform.DOLookAt(startPoint.transform.position, rotateDuration);
        }
    }
}