using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;


public class Node
{
    public GWAState state = GWAState.ALIVE;
    public bool isRoot = false;
    public List<Arc> outputArcs;

    public UnityAction nodeEnterCallbacks;
    public UnityAction nodeExitCallbacks;

    public void OnNodeEnter() {nodeEnterCallbacks.Invoke();}
    public void OnNodeExit() {nodeExitCallbacks.Invoke();}


    public Node(GWAState iState, bool iIsRoot)
    {
        state = iState;
        isRoot = iIsRoot;
        outputArcs = new List<Arc>();
    }

    public string GetName() { return state.ToString(); }
}

public class Arc
{
    public GWAState A;
    public GWAState B;
    public Func<bool> triggers;

    public Arc( GWAState iA, GWAState iB, Func<bool> iTrigg)
    {
        A = iA;
        B = iB;
        triggers = iTrigg;
    }
}

public class AgentToken
{
    public Node currNode;
    public GWAgent agent;
    public AgentToken(Node iStartNode, GWAgent iAgent)
    {
        agent = iAgent;
        currNode = iStartNode;
    }
}

public class PetToken : AgentToken
{
    public GWPet pet;

    public PetToken(Node iStartNode, GWPet iAgent) : base (iStartNode, iAgent)
    {
        pet = iAgent;
    }
}

public class StateMachine<T, C, V>  where  T : AgentToken 
                                    where  C : TokenChecks<T>, new()
                                    where  V : NodeActionPool<T>, new()
{
    [Header("Behavior")]
    public bool playMode = false;

    [Header("Internals")]
    public Node RootNode;
    public List<Node> nodes;
    public List<Arc> arcs;
    public List<T> agents;

    public C checks;
    public V nodeActionPool;

    public StateMachine() {}


    public Node GetNodeFromState(GWAState iState)
    {
        foreach ( Node n in nodes)
        {
            if (n.state == iState)
            { return n; }
        }
        return null;
    }

    public void Build(GWAState iInitState, List<GWAState> iNodes)
    {
        nodes = new List<Node>();
        foreach ( GWAState s in iNodes)
        {
            Node newNode = new Node(s, s==iInitState);
            nodes.Add(newNode);
            if (s==iInitState)
            { RootNode = newNode; }
            
        }
        arcs = new List<Arc>();
        agents = new List<T>();

        checks = new C();
        nodeActionPool = new V();
    }

    public void EditNodeEnterCB(GWAState iState, UnityAction iCB)
    {
        Node n = GetNodeFromState(iState);
        if (n!=null)
        {
            n.nodeEnterCallbacks += iCB;
        }
    }

    public void EditNodeExitCB(GWAState iState, UnityAction iCB)
    {
        Node n = GetNodeFromState(iState);
        if (n!=null)
        {
            n.nodeExitCallbacks = iCB;
        }
    }

    public void AddConnection(GWAState iStateA, GWAState iStateB, Func<bool> iTriggers)
    {
        if (nodes==null)
        {
            return;
        }

        Arc newArc = new Arc(iStateA, iStateB, iTriggers);
        if (arcs.Contains(newArc))
            return;
        
        arcs.Add(newArc);
        foreach(Node n in nodes)
        {
            if (n.state == iStateA)
            {
                if (!n.outputArcs.Contains(newArc))
                {
                    n.outputArcs.Add(newArc);
                }
            }
        }
    }

    public void AddToken(T iTok)
    {
        if (!agents.Contains(iTok))
            agents.Add(iTok);
    }

    public void RemoveToken(T iTok)
    {
        agents = agents.Where(e => e == iTok).ToList();
    }

    public void ChangeState(T iToken, Node iDestination)
    {
        iToken.currNode.OnNodeExit();

        iToken.currNode = iDestination;

        iToken.currNode.OnNodeEnter();
    }

    public void Play(bool iShouldPlay)
    {
        playMode = iShouldPlay;
    }

    public void Update()
    {
        if (!playMode)
            return;
        
        foreach( T tok in agents)
        {
            checks.target = tok;
            nodeActionPool.target = tok;
            foreach( Arc arc in tok.currNode.outputArcs)
            {
                if (arc.triggers())
                {
                    ChangeState(tok, GetNodeFromState(arc.B));
                }
            }
        }
    }
}