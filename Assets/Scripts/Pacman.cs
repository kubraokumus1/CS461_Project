using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    public AnimatedSprite deathSequence;
    public SpriteRenderer spriteRenderer { get; private set; }
    public new Collider2D collider { get; private set; }
    public Movement movement { get; private set; }

    private List<Vector3> actions;
    Vector3 start = new Vector3(0.5f, -9.5f, -5.0f);
    public static Vector3 goal = new Vector3(12.5f, 12.5f, -5.0f);

    public static int i;

    private void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        movement = GetComponent<Movement>();

        switch (i)
        {
            case 0:
                actions = bfs();
                break;
            case 1:
                actions = dfs();
                break;
            case 2:
                actions = UCS();
                break;
            case 3:
                actions = Astar();
                break;

        }

        print("cost : " + actions.Count);
        actions.RemoveAt(0);
    }

    private void Update()
    {
        // Set the new direction based on the current input
        // if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
        //     movement.SetDirection(Vector2.up);
        // }
        // else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
        //     movement.SetDirection(Vector2.down);
        // }
        // else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
        //     movement.SetDirection(Vector2.left);
        // }
        // else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
        //     movement.SetDirection(Vector2.right);
        // }

        if (actions.Count == 0)
        {
            FindObjectOfType<GameManager>().GoalReached();
        }
        else{
            movement.SetDirection(normalize(actions[0], transform.position));
            if (Vector3.Distance(actions[0], transform.position) < 0.2f)
            {
                actions.RemoveAt(0);
            }
        }

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);

    }

    private Vector3 normalize(Vector3 action, Vector3 pos)
    {
        if (Math.Abs(action.x - pos.x) > Math.Abs(action.y - pos.y))
        {
            return new Vector3((action.x - pos.x) / Math.Abs(action.x - pos.x), 0f, 0f);
        }
        else
        {
            return new Vector3(0f, (action.y - pos.y) / Math.Abs(action.y - pos.y), 0f);
        }
    }


    private List<Vector3> getSuccessor(Vector3 position)
    {
        List<Vector3> successors = new List<Vector3>();
        if (movement.isAvailable(Vector2.right, position))
        {
            successors.Add(position + Vector3.right);
        }
        if (movement.isAvailable(Vector2.left, position))
        {
            successors.Add(position + Vector3.left);
        }
        if (movement.isAvailable(Vector2.up, position))
        {
            successors.Add(position + Vector3.up);
        }
        if (movement.isAvailable(Vector2.down, position))
        {
            successors.Add(position + Vector3.down);
        }
        return successors;
    }


    private List<Vector3> dfs()
    {
        print("dfs");
        Stack<Vector3> position = new Stack<Vector3>();
        Stack<List<Vector3>> path = new Stack<List<Vector3>>();
        // Vector3 start = new Vector3(0.5f, -9.5f, -5.0f);
        // Vector3 goal = new Vector3(12.5f, 12.5f, -5.0f);
        List<Vector3> visited = new List<Vector3>();

        position.Push(start);
        List<Vector3> path1 = new List<Vector3>();
        path1.Add(start);

        while (position.Count != 0)
        {
            Vector3 vertex = position.Pop();
            List<Vector3> actions = new List<Vector3>();
            if (path.Count != 0)
            {
                actions = path.Pop();
            }

            //return path when pacman is close enough to goal
            if (Vector3.Distance(vertex, goal) < 0.5f)
            {
                return actions;
            }


            foreach (Vector3 successor in getSuccessor(vertex))
            {
                List<Vector3> tmp = new List<Vector3>(actions);
                //    print(successor);
                if (!visited.Contains(successor))
                {
                    tmp.Add(successor);
                    path.Push(tmp);
                    visited.Add(vertex);
                    position.Push(successor);
                }
            }
        }
        return new List<Vector3>();
    }


    private List<Vector3> bfs()
    {
        print("bfs");
        Queue<Vector3> position = new Queue<Vector3>();
        Queue<List<Vector3>> path = new Queue<List<Vector3>>();
        //Vector3 start = new Vector3(0.5f, -9.5f, -5.0f);
        //Vector3 goal = new Vector3(12.5f, 12.5f, -5.0f);
        List<Vector3> visited = new List<Vector3>();

        position.Enqueue(start);
        List<Vector3> path1 = new List<Vector3>();
        path1.Add(start);

        while (position.Count != 0)
        {
            Vector3 vertex = position.Dequeue();
            List<Vector3> actions = new List<Vector3>();
            if (path.Count != 0)
            {
                actions = path.Dequeue();
            }

            //return path when pacman is close enough to goal
            if (Vector3.Distance(vertex, goal) < 0.5f)
            {
                return actions;
            }


            foreach (Vector3 successor in getSuccessor(vertex))
            {
                List<Vector3> tmp = new List<Vector3>(actions);
                // print(successor);
                if (!visited.Contains(successor))
                {
                    tmp.Add(successor);
                    path.Enqueue(tmp);
                    visited.Add(vertex);
                    position.Enqueue(successor);
                }
            }
        }
        return new List<Vector3>();
    }

    private List<Vector3> UCS()
    {
        print("ucs");
        int totalCost = 0;
        PriorityQueue<int, State> queue = new PriorityQueue<int, State>();
        //Vector3 start = new Vector3(0.5f, -9.5f, -5.0f);
        //Vector3 goal = new Vector3(12.5f, 12.5f, -5.0f);

        List<Vector3> actions = new List<Vector3>();
        actions.Add(start);

        State startState = new State(start, actions, 0);
        queue.Enqueue(0, startState);
        List<Vector3> visited = new List<Vector3>();


        while (queue.Count != 0)
        {
            State currState = queue.Dequeue();

            if (!visited.Contains(currState.Position))
            {
                visited.Add(currState.Position);
                if (Vector3.Distance(currState.Position, goal) < 0.5f)
                {
                    return currState.Actions;
                }

                foreach (Vector3 successor in getSuccessor(currState.Position))
                {
                    List<Vector3> tmpActions = new List<Vector3>(currState.Actions);
                    tmpActions.Add(successor);
                    totalCost = currState.Cost + 1;
                    State newstate = new State(successor, tmpActions, totalCost);
                    queue.Enqueue(totalCost, newstate);
                }

            }
        }
        return new List<Vector3>();
    }



    private int heuristic(Vector3 current, Vector3 goal)
    {

        return (int)Math.Abs(current.x - goal.x) + (int)Math.Abs(current.y - goal.y);
    }
    private List<Vector3> Astar()
    {
        print("astar");
        int totalCost = 0;
        PriorityQueue<int, State> queue = new PriorityQueue<int, State>();

        List<Vector3> actions = new List<Vector3>();
        actions.Add(start);

        State startState = new State(start, actions, 0);
        queue.Enqueue(0, startState);
        List<Vector3> visited = new List<Vector3>();


        while (queue.Count != 0)
        {
            State currState = queue.Dequeue();

            if (!visited.Contains(currState.Position))
            {
                visited.Add(currState.Position);
                if (Vector3.Distance(currState.Position, goal) < 0.5f)
                {
                    return currState.Actions;
                }

                foreach (Vector3 successor in getSuccessor(currState.Position))
                {
                    List<Vector3> tmpActions = new List<Vector3>(currState.Actions);
                    tmpActions.Add(successor);
                    totalCost = currState.Cost + 1 + heuristic(successor, goal);
                    State newstate = new State(successor, tmpActions, totalCost);
                    queue.Enqueue(totalCost, newstate);
                }

            }
        }
        return new List<Vector3>();
    }




    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        deathSequence.spriteRenderer.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.spriteRenderer.enabled = true;
        deathSequence.Restart();
    }

}
