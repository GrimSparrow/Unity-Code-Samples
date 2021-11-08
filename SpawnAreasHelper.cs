using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Drawn2U.Agents.Enemies
{
    /// <summary>
    /// Статический класс, позволяющий найти все горизонтальные поверхности композитного коллайдера с определенным
    /// углом наклона. Также позволяет получить точку спавна объекта на этой поверхности, максимально приближенную,
    /// к некоторой ожидаемой точке пространства.
    /// </summary>
    public static class SpawnAreasHelper
    {
        /// <summary>
        /// Проверить возможность появления объекта в области.
        /// </summary>
        /// <param name="parameters">Фильтр с настройками призыва объекта.</param>
        /// <param name="position">Позиция призыва объекта.</param>
        /// <returns>Набор - возможно ли призвать объект и точку призыва.</returns>
        public static bool TryGetSpawnPosition(SpawnParams parameters, out Vector2 position)
        {
            const float spawnOffset = 0.5f;
            const float raycastDistance = 5f;
            position = Vector2.zero;
            
            //Немного приращиваем координату y точки старта рейкаста, чтобы не было под землей
            var rayStartPos = parameters.InitialPosition;
            rayStartPos.y += spawnOffset;
            
            var hit = Physics2D.Raycast(rayStartPos, -Vector2.up, raycastDistance, parameters.GroundLayerMask);

            if (!hit) return false;
            
            if (TryGetHitNode(parameters, hit.point, out var spawnPosition))
            {
                position = spawnPosition;
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Получить все горизонтальные поверхности(слой Ground).
        /// </summary>
        /// <param name="colliders">Композитные коллайдеры поверхности, по которой может перемещаться ГГ и на которой
        /// появляется объект.</param>
        /// <returns>Набор горизонтальных поверхностей.</returns>
        public static Node[] GetHorizontalNodes(CompositeCollider2D[] colliders)
        {
            var horizontalNodes = new List<Node>();
    
            foreach (var collider in colliders)
            {
                var offset = (Vector2)collider.transform.position;

                for (var i = 0; i < collider.pathCount; i++)
                {
                    var points = new Vector2[collider.GetPathPointCount(i)];
                    var pathLength = collider.GetPath(i, points);

                    for (var j = 0; j < pathLength - 1; j++)
                    {
                        var counter = 0;
                        var currentEndPoint = j + 1;

                        while (currentEndPoint < pathLength - 1 && IsOnOneLine(points[j], points[currentEndPoint]))
                        {
                            counter++;
                            currentEndPoint++;
                        }

                        if (counter < 1) continue;

                        if (IsLengthSufficient(points[j], points[currentEndPoint - 1]))
                        {
                            horizontalNodes.Add(new Node(points[j] + offset , points[currentEndPoint - 1] + offset));
                        }
                        
                        j = currentEndPoint - 1;
                    }
                }
            }

            return horizontalNodes.ToArray();
        }
        
        /// <summary>
        /// Лежат ли две точки на одной линии.
        /// </summary>
        /// <param name="start">Начальная точка.</param>
        /// <param name="end">Конечная точка.</param>
        private static bool IsOnOneLine(in Vector2 start, in Vector2 end)
        {
            const float tolerance = 0.1f;

            return Mathf.Abs(start.y - end.y) < tolerance;
        }

        /// <summary>
        /// Достаточна ли длина указанного горизонтального отрезка.
        /// </summary>
        /// <param name="start">Начало отрезка.</param>
        /// <param name="end">Конец отрезка.</param>
        private static bool IsLengthSufficient(in Vector2 start, in Vector2 end)
        {
            const float minNodeLength = 2.8f;

            return Mathf.Abs(start.x - end.x) > minNodeLength;
        }

        /// <summary>
        /// Получить ближайшую к вызывающему объекту горизонтальную поверхность.
        /// </summary>
        /// <param name="parameters">Фильтр с настройками призыва объекта.</param>
        /// <param name="hitPoint">Позиция вызывающего объекта..</param>
        /// <param name="spawnPosition">Позиция призыва объекта.</param>
        /// <returns>Ближайшая горизонтальная поверхность</returns>
        private static bool TryGetHitNode(SpawnParams parameters, Vector2 hitPoint, out Vector2 spawnPosition)
        {
            var pointsOnSurface = parameters.GroundColliders.Select(collider => collider.ClosestPoint(hitPoint)).ToList();

            foreach (var pointOnSurface in pointsOnSurface)
            {
                var closestSurfaceNode = new Node();
                spawnPosition = Vector2.zero;

                foreach (var node in parameters.Nodes)
                {
                    var isOnNode = IsPointOnNode(pointOnSurface, node);

                    if (!isOnNode) continue;
                
                    closestSurfaceNode = node;
                    break;
                }

                if (!closestSurfaceNode.Equals(default(Node)) && CheckObstacles(parameters, closestSurfaceNode, out spawnPosition))
                {
                    return true;
                }
            }

            spawnPosition = Vector2.zero;
            
            return false;
        }

        /// <summary>
        /// Проверить, находится ли указанная точка на определенной ноде.
        /// </summary>
        /// <param name="pointOnSurface">Проверяемая точка.</param>
        /// <param name="nodeToCheck">Нода, в границах которой, проверяем указанную точку.</param>
        private static bool IsPointOnNode(Vector2 pointOnSurface, Node nodeToCheck)
        {
            if (!IsOnOneLine(pointOnSurface, nodeToCheck.Start)) return false;
            
            //Проверяем находится ли точка в отрезке(на ноде).
            return nodeToCheck.Start.x - nodeToCheck.End.x > 0
                ? nodeToCheck.End.x <= pointOnSurface.x && pointOnSurface.x <= nodeToCheck.Start.x
                : nodeToCheck.Start.x <= pointOnSurface.x && pointOnSurface.x <= nodeToCheck.End.x;
        }
        
        /// <summary>
        /// Проверка области появления на препятствия.
        /// </summary>
        /// <param name="parameters">Фильтр с настройками призыва объекта.</param>
        /// <param name="node">Поверхность, на которой должен появиться объект.</param>
        /// <param name="spawnPosition">Позиция призыва объекта.</param>
        /// <returns>Есть ли свободная позиция на поверхности и ее координаты</returns>
        private static bool CheckObstacles(SpawnParams parameters, Node node, out Vector2 spawnPosition)
        {
            const float startPositionXOffset = 2f;
            const float startPositionYOffset = 0.1f;
            
            var raycastHits = new RaycastHit2D[10];
            var startPosition = node.End;
            spawnPosition = Vector2.zero;
            
            startPosition.y += parameters.ObjectCollider.size.y / 2 + startPositionYOffset;
            //пока немного смещаем точку появления вправо, чтобы не было появления на ступеньках, если скраю
            startPosition.x += startPositionXOffset;

            var isAreaFree = false;
            var distanceToCharacter = float.MaxValue;

            while (startPosition.x < node.Start.x - 2f)
            {
                var resultCount = Physics2D.BoxCast(startPosition, parameters.ObjectCollider.size, 0f, Vector2.zero, parameters.ContactFilter, raycastHits);
                
                if (resultCount > 0)
                {
                    startPosition.x += parameters.ObjectCollider.size.x;
                }
                else
                {
                    //расстояние от объекта до точки появления, для поиска ближайшей точки
                    var currentDistance = Vector2.Distance(parameters.TargetPosition, startPosition);
                    
                    if (distanceToCharacter > currentDistance)
                    {
                        distanceToCharacter = currentDistance;
                        spawnPosition = startPosition;
                    }
                    startPosition.x += parameters.ObjectCollider.size.x;
                    isAreaFree = true;
                }
            }
            spawnPosition.y  -= parameters.ObjectCollider.size.y / 2;
            
            return isAreaFree;
        }
    }
}
