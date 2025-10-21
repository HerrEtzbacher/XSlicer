using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public static class MeshSlicer
    {
        public static Mesh[] SliceMesh(Mesh mesh, Vector3 cutOrigin, Vector3 cutNormal)
        {
            Plane plane = new Plane(cutNormal, cutOrigin);
            MeshContructionHelper positiveMesh = new MeshContructionHelper();
            MeshContructionHelper negativeMesh = new MeshContructionHelper();

            int[] meshTriangles = mesh.triangles;
            List<VertexData> pointsAlongPlane = new List<VertexData>();
            for (int i = 0; i < meshTriangles.Length; i += 3)
            {
                VertexData vertexA = GetVertexData(mesh, plane, meshTriangles[i]);
                VertexData vertexB = GetVertexData(mesh, plane, meshTriangles[i+1]);
                VertexData vertexC = GetVertexData(mesh, plane, meshTriangles[i+2]);

                bool isABSameSide = vertexA.Side == vertexB.Side;
                bool isBCSameSide = vertexB.Side == vertexC.Side;
                
                if (isABSameSide && isBCSameSide)
                {            
                    MeshContructionHelper helper = vertexA.Side ? positiveMesh : negativeMesh;
                    helper.AddMeshSection(vertexA, vertexB, vertexC);
                } 
                else
                {
                    VertexData intersectionD;
                    VertexData intersectionE;
                    MeshContructionHelper helperA = vertexA.Side ? positiveMesh : negativeMesh;
                    MeshContructionHelper helperB = vertexB.Side ? positiveMesh : negativeMesh;
                    MeshContructionHelper helperC = vertexC.Side ? positiveMesh : negativeMesh;
          
                    if (isABSameSide)
                    {
                        intersectionD = GetIntersectionVertex(vertexA, vertexC, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexB, vertexC, cutOrigin, cutNormal);

                        helperA.AddMeshSection(vertexA, vertexB, intersectionE );
                        helperA.AddMeshSection(vertexA,intersectionE, intersectionD);
                        helperC.AddMeshSection(intersectionE,vertexC, intersectionD );
                    }
                    else if (isBCSameSide)
                    {
                        intersectionD = GetIntersectionVertex(vertexB, vertexA, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexC, vertexA, cutOrigin, cutNormal);

                        helperB.AddMeshSection(vertexB, vertexC, intersectionE);
                        helperB.AddMeshSection(vertexB,intersectionE, intersectionD);
                        helperA.AddMeshSection(intersectionE, vertexA,intersectionD);
                    }
                    else
                    {
                        intersectionD = GetIntersectionVertex(vertexA, vertexB, cutOrigin, cutNormal);
                        intersectionE = GetIntersectionVertex(vertexC, vertexB, cutOrigin, cutNormal);

                        helperA.AddMeshSection(vertexA,  intersectionE, vertexC);
                        helperA.AddMeshSection(intersectionD, intersectionE, vertexA);
                        helperB.AddMeshSection(vertexB,intersectionE, intersectionD);
                    }
                    
                    pointsAlongPlane.Add(intersectionD);
                    pointsAlongPlane.Add(intersectionE);
                }
            }
            JoinPointsAlongPlane(ref positiveMesh, ref negativeMesh,  cutNormal, pointsAlongPlane);

            return new[] { positiveMesh.ConstructMesh(), negativeMesh.ConstructMesh()};
        }
        
        private static VertexData GetVertexData(Mesh mesh, Plane plane, int index)
        {
            Vector3 position = mesh.vertices[index];
            VertexData vertexData = new VertexData()
            {
                Postion = position,
                Side = plane.GetSide(position),
                Uv = mesh.uv[index],
                Normal = mesh.normals[index]
            };
            return vertexData;
        }
        
        public static bool PointIntersectsAPlane(Vector3 from, Vector3 to, Vector3 planeOrigin, Vector3 normal, out Vector3 result)
        {
            Vector3 translation = to - from;
            float dot = Vector3.Dot(normal, translation);
            if (Mathf.Abs(dot) > Single.Epsilon)
            {
                Vector3 fromOrigin = from - planeOrigin;
                float fac = -Vector3.Dot(normal, fromOrigin) / dot;
                translation = translation * fac;
                result = from + translation;
                return true;
            }
        
            result = Vector3.zero;
            return false;
        }
        
        private static VertexData GetIntersectionVertex(VertexData vertexA, VertexData vertexB, Vector3 planeOrigin, Vector3 normal)
        {
            PointIntersectsAPlane(vertexA.Postion, vertexB.Postion, planeOrigin, normal, out Vector3 result);
            float distanceA = Vector3.Distance(vertexA.Postion, result);
            float distanceB = Vector3.Distance(vertexB.Postion, result);
            float t = distanceA/(distanceA+distanceB);
        
            return new VertexData()
            {
                Postion = result,
                Normal = normal,
                Uv = VertexUtility.InterpolateUvs(vertexA.Uv, vertexB.Uv, t)
            };
        }
        
        private static void JoinPointsAlongPlane(ref MeshContructionHelper positive, ref MeshContructionHelper negative, Vector3 cutNormal, List<VertexData> pointsAlongPlane)
        {
            VertexData halfway = new VertexData()
            {
                Postion = VertexUtility.GetHalfwayPoint(pointsAlongPlane)
            };
        
            for (int i = 0; i <pointsAlongPlane.Count; i += 2)
            {
                VertexData firstVertex = pointsAlongPlane[i];
                VertexData secondVertex =  pointsAlongPlane[i+1];

                Vector3 normal = VertexUtility.ComputeNormal(halfway, secondVertex, firstVertex);
                halfway.Normal = Vector3.forward;

                float dot = Vector3.Dot(normal, cutNormal);

                if(dot > 0)
                {             
                    positive.AddMeshSection(firstVertex, secondVertex, halfway);
                    negative.AddMeshSection( secondVertex, firstVertex,halfway);
                }
                else
                {
                    negative.AddMeshSection(firstVertex, secondVertex, halfway);
                    positive.AddMeshSection( secondVertex, firstVertex,halfway);
                }      
            }
        }
    }
    
    public struct VertexData
    {
        public Vector3 Postion;
        public Vector2 Uv;
        public Vector3 Normal;
        public bool Side;
    }
    
    public static class VertexUtility
    {
        public static Vector2 InterpolateUvs(Vector2 uv1, Vector2 uv2, float distance)
        {
            Vector2 uv = Vector2.Lerp(uv1, uv2, distance);
            return uv;
        }
        
        public static Vector3 ComputeNormal(VertexData vertexA, VertexData vertexB, VertexData vertexC)
        {
            Vector3 sideL = vertexB.Postion - vertexA.Postion;
            Vector3 sideR = vertexC.Postion - vertexA.Postion;

            Vector3 normal = Vector3.Cross(sideL, sideR);

            return normal.normalized;
        }
        
        public static Vector3 GetHalfwayPoint(List<VertexData> pointsAlongPlane)
        {
            if(pointsAlongPlane.Count > 0)
            {
                Vector3 firstPoint = pointsAlongPlane[0].Postion;
                Vector3 furthestPoint = Vector3.zero;
                float distance = 0f;

                for (int index = 0; index < pointsAlongPlane.Count; index++)
                {
                    Vector3 point = pointsAlongPlane[index].Postion;
                    float currentDistance = 0f;
                    currentDistance = Vector3.Distance(firstPoint, point);

                    if (currentDistance > distance)
                    {
                        distance = currentDistance;
                        furthestPoint = point;
                    }
                }

                return Vector3.Lerp(firstPoint, furthestPoint, 0.5f);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}