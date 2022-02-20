using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class Unit_Move : MonoBehaviour
{
    private NavMeshAgent myAgent;
    public Color lineWhite, lineRed;

    private bool isDrawing = false, isMoving = false;
    private LineRenderer lineRend;
    public SpriteRenderer arrowRend;
    public SpriteRenderer glowRend;

    private List<Vector3> linePoints = new List<Vector3>();
    private Queue<Vector3> lineQueue = new Queue<Vector3>();
    private Unit_Base scrAI;
    private float pathLength;

    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        lineRend = GetComponentInChildren<LineRenderer>();
        scrAI = GetComponent<Unit_Base>();
    }

    void Update()
    {
        if (isDrawing)
        {
            Vector3 mousePosRaw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 mousePos = new Vector3(mousePosRaw.x, 1f, mousePosRaw.z);

            if (DistancePoint(mousePos) > 0.3f)
            {
                linePoints.Add(mousePos);
                lineRend.positionCount = linePoints.Count();
                lineRend.SetPositions(linePoints.ToArray());

                pathLength += 0.3f;
            }
        }

        if(isMoving && MoveOn())
        {
            myAgent.SetDestination(lineQueue.Dequeue());
        }
    }

    private float DistancePoint(Vector3 point)
    {
        PathLimit();

        if(linePoints.Count == 0)
        {
            float dist = Vector3.Distance(transform.position, point);
            if(dist < 1.5f)
            {
                return 0f;
            }
        }

        if (linePoints.Any())
        {
            return Vector3.Distance(point, linePoints.Last());
        }
        else
        {
            return 5f;
        }
    }

    private bool MoveOn()
    {
        if (lineQueue.Count == 0)
        {
            isMoving = false;
            DrawOff();
            return false;
        }
        else if(myAgent.hasPath == false || myAgent.remainingDistance < 0.4f)
        {
            lineRend.SetPositions(lineQueue.ToArray());
            return true;
        }

        return false;
    }

    public void StartDraw()
    {
        DrawOff();
        pathLength = 0f;
        lineRend.endColor = lineWhite;
        lineRend.enabled = true;
        isDrawing = true;
        isMoving = false;

        myAgent.SetDestination(transform.position);
        myAgent.isStopped = true;
    }

    public void EndDraw()
    {
        isDrawing = false;
        NavMeshPath path = new NavMeshPath();

        if (linePoints.Count > 1)
        {
            myAgent.CalculatePath(linePoints.Last(), path);
            lineRend.Simplify(0.5f);
            linePoints.Clear();

            for (int i = 0; i < lineRend.positionCount; i++)
            {
                linePoints.Add(lineRend.GetPosition(i));
            }

            SetArrow();
            lineQueue = new Queue<Vector3>(linePoints);
            isMoving = true;
            myAgent.isStopped = false;

            scrAI.UnitSound(1);
        }
        else
        {
            DrawOff();
        }

    }

    private void DrawOff()
    {
        lineRend.positionCount = 1;
        linePoints.Clear();
        lineQueue.Clear();
        lineRend.enabled = false;
        arrowRend.enabled = false;
    }

    private void SetArrow()
    {
        int lastPoint = linePoints.Count;
        Vector3 a1 = linePoints[lastPoint - 1];
        Vector3 a2 = linePoints[lastPoint - 2];
        Vector3 a3 = a1 - a2;

        Quaternion arrowRot = Quaternion.identity;
        arrowRot = Quaternion.LookRotation(a3);
        arrowRot.eulerAngles = new Vector3(90, arrowRot.eulerAngles.y, arrowRot.eulerAngles.z + 90);

        arrowRend.transform.rotation = arrowRot;
        arrowRend.transform.position = linePoints[lastPoint - 1];
        arrowRend.enabled = true;
    }

    private void PathLimit()
    {
        int reWater = scrAI.PathCalc();
        float pathL = myAgent.speed * (reWater/3);

        if(pathLength > pathL)
        {
            lineRend.endColor = lineRed;
            arrowRend.color = lineRed;
        }
        else
        {
            lineRend.endColor = lineWhite;
            arrowRend.color = lineWhite;
        }
    }

    public void DirectMove(Vector3 destination)
    {
        DrawOff();
        isMoving = false;
        myAgent.isStopped = false;

        myAgent.SetDestination(destination);
    }

    public void Die()
    {
        DrawOff();
        Destroy(gameObject);
    }
}
