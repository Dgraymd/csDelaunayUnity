﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    public class EdgeReorderer
    {
        public List<bool> EdgeOrientations { get; private set; }

        static EdgeReorderer instance;
        const int BUFFER_CAPACITY = 64;

        List<Edge> newEdgesBuffer;
        List<bool> doneBuffer;

        public static void CreateInstance()
        {
            if (instance == null)
                instance = new EdgeReorderer();
            else
            {
                instance.Clear();
            }
        }

        EdgeReorderer()
        {
            EdgeOrientations = new List<bool>(BUFFER_CAPACITY);

            newEdgesBuffer = new List<Edge>(BUFFER_CAPACITY);
            doneBuffer = new List<bool>(BUFFER_CAPACITY);
        }

        public static void Reorder(ref List<Edge> origEdges, ref List<bool> edgeOrientations, Type criterion)
        {
            CreateInstance();

            if (origEdges == null || origEdges.Count == 0)
                return;

            instance.ReorderEdges(origEdges, criterion);

            origEdges.Clear();
            for (int i = 0; i < instance.newEdgesBuffer.Count; i++)
                origEdges.Add(instance.newEdgesBuffer[i]);

            // Copy EdgeOrientations to edgeOrientations
            edgeOrientations.Clear();
            for (int i = 0; i < instance.EdgeOrientations.Count; i++)
                edgeOrientations.Add(instance.EdgeOrientations[i]);
        }

        public void Clear()
        {
            EdgeOrientations.Clear();

            newEdgesBuffer.Clear();
            doneBuffer.Clear();
        }

        public static string GetCapacities()
        {
            return $"EdgeOrientations: {instance.EdgeOrientations.Capacity}, newEdgesBuffer: {instance.newEdgesBuffer.Capacity}, doneBuffer: {instance.doneBuffer.Capacity}";
        }

        void ReorderEdges(List<Edge> origEdges, Type criterion)
        {
            Profiler.BeginSample("Reorder start");
            int i;
            int n = origEdges.Count;
            Edge edge;
            // We're going to reorder the edges in order of traversal
            //List<bool> done = new List<bool>(); // alloc
            int nDone = 0;
            for (int b = 0; b < n; b++) doneBuffer.Add(false);
            //List<Edge> newEdges = new List<Edge>(); // alloc

            i = 0;
            edge = origEdges[i];
            newEdgesBuffer.Add(edge); // extend alloc
            EdgeOrientations.Add(false); // extend alloc
            ICoord firstPoint;
            ICoord lastPoint;
            Profiler.EndSample();

            Profiler.BeginSample("Criterion search");
            if (criterion == typeof(Vertex))
            {
                firstPoint = edge.LeftVertex;
                lastPoint = edge.RightVertex;
            }
            else
            {
                firstPoint = edge.LeftSite;
                lastPoint = edge.RightSite;
            }
            Profiler.EndSample();

            if (firstPoint == Vertex.VERTEX_AT_INFINITY || lastPoint == Vertex.VERTEX_AT_INFINITY)
            {
                UnityEngine.Debug.LogError("Puk");
                return;
            }

            doneBuffer[i] = true;
            nDone++;

            Profiler.BeginSample("While");
            while (nDone < n)
            {
                for (i = 1; i < n; i++)
                {
                    if (doneBuffer[i])
                    {
                        continue;
                    }
                    edge = origEdges[i];
                    ICoord leftPoint;
                    ICoord rightPoint;
                    if (criterion == typeof(Vertex))
                    {
                        leftPoint = edge.LeftVertex;
                        rightPoint = edge.RightVertex;
                    }
                    else
                    {
                        leftPoint = edge.LeftSite;
                        rightPoint = edge.RightSite;
                    }
                    if (leftPoint == Vertex.VERTEX_AT_INFINITY || rightPoint == Vertex.VERTEX_AT_INFINITY)
                    {
                        UnityEngine.Debug.LogError("Puk2");
                        return;
                    }
                    if (leftPoint == lastPoint)
                    {
                        lastPoint = rightPoint;
                        EdgeOrientations.Add(false); // extend alloc
                        newEdgesBuffer.Add(edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (rightPoint == firstPoint)
                    {
                        firstPoint = leftPoint;
                        EdgeOrientations.Insert(0, false); // extend alloc
                        newEdgesBuffer.Insert(0, edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (leftPoint == firstPoint)
                    {
                        firstPoint = rightPoint;
                        EdgeOrientations.Insert(0, true); // extend alloc
                        newEdgesBuffer.Insert(0, edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (rightPoint == lastPoint)
                    {
                        lastPoint = leftPoint;
                        EdgeOrientations.Add(true); // extend alloc
                        newEdgesBuffer.Add(edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    if (doneBuffer[i])
                    {
                        nDone++;
                    }
                }
            }
            Profiler.EndSample();
        }
    }
}
