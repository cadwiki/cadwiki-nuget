using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace cadwiki.AutoCAD2021.Base.Utilities.NodeGraph
{
    public class Node
    {
        public int NodeId;
        public Node ParentNode;
        public List<Entity> EntitiesListAtNode = new List<Entity>();
        public List<Node> NeighborList = new List<Node>();
        public Point3d NodeCoordinates;
        public bool HasNodeBeenDiscovered;
        public double DistanceFromDestination;
        public Point3d AutoCADPoint;

        public Node(int nodeId, Node parentNode, List<Entity> entitiesListAtNode, List<Node> neighborIdList, Point3d nodeCoordinates, bool hasNodeBeenDiscovered, Point3d location, double distanceFromDestination)
        {
            NodeId = nodeId;
            ParentNode = parentNode;
            EntitiesListAtNode = entitiesListAtNode;
            NeighborList = neighborIdList;
            NodeCoordinates = nodeCoordinates;
            HasNodeBeenDiscovered = hasNodeBeenDiscovered;
            AutoCADPoint = location;
            DistanceFromDestination = distanceFromDestination;
        }

        public List<int> GetNeighborIds()
        {
            if (NeighborList is not null)
            {
                var neighborIds = NeighborList.Select(p => p.NodeId).ToList();
                return neighborIds;
            }
            return new List<int>();
        }

        public List<string> GetNeighborIdsToStringList()
        {
            var neighborIds = GetNeighborIds();
            var neighborStrings = new List<string>();

            foreach (int id in neighborIds)
                neighborStrings.Add(id.ToString());
            return neighborStrings;
        }
    }

}