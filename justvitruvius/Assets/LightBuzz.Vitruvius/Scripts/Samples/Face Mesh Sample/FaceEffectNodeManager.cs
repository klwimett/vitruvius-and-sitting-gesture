using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.Vitruvius.Avateering;

[ExecuteInEditMode]
public class FaceEffectNodeManager : MonoBehaviour
{
    [HideInInspector, SerializeField]
    FaceEffectNode[] faceEffectNodes = null;

    public float maxDistance = 0.5f;
    [Range(0, 10)]
    public int pointsPerNode = 3;

    public bool refresh = false;

    void OnDrawGizmos()
    {
        if (faceEffectNodes == null) return;

        for (int i = 0; i < Avateering.FACE_NODE_COUNT; i++)
        {
            if (faceEffectNodes[i].neighbourNodes == null) continue;

            for (int j = 0, count = faceEffectNodes[i].neighbourNodes.Length; j < count; j++)
            {
                Gizmos.DrawLine(faceEffectNodes[i].node.position, faceEffectNodes[i].neighbourNodes[j].position);
            }
        }
    }

    void Update()
    {
        if (refresh)
        {
            RefreshNodes();
            refresh = false;
        }
    }

    void RefreshNodes()
    {
        if (faceEffectNodes == null)
        {
            faceEffectNodes = new FaceEffectNode[Avateering.FACE_NODE_COUNT];
        }

        for (int i = 0; i < Avateering.FACE_NODE_COUNT; i++)
        {
            if (faceEffectNodes[i] == null)
            {
                faceEffectNodes[i] = new FaceEffectNode();
            }
            else
            {
                faceEffectNodes[i].neighbourNodes = null;
            }

            faceEffectNodes[i].node = transform.GetChild(i);
        }

        FaceEffectNode temp;
        List<Transform> nodeList;
        for (int i = 0; i < Avateering.FACE_NODE_COUNT; i++)
        {
            nodeList = new List<Transform>();
            for (int j = 0; j < Avateering.FACE_NODE_COUNT; j++)
            {
                if (j == i) continue;

                temp = faceEffectNodes[j];

                if (temp.neighbourNodes != null && temp.neighbourNodes.Contains(faceEffectNodes[i].node))
                {
                    continue;
                }

                if ((temp.node.position - faceEffectNodes[i].node.position).magnitude <= maxDistance)
                {
                    nodeList.Add(temp.node);
                }
            }

            if (nodeList.Count > pointsPerNode)
            {
                nodeList = nodeList.OrderBy(x => (faceEffectNodes[i].node.position - x.position).magnitude).ToList();
                nodeList.RemoveRange(pointsPerNode, nodeList.Count - pointsPerNode);
            }

            faceEffectNodes[i].neighbourNodes = nodeList.ToArray();
        }
    }
}