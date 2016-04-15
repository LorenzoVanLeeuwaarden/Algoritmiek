using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntryPoint
{
#if WINDOWS || LINUX
    public static class Program
    {
        static void CrashMe(int n)
        {
            Console.Write("Going strong at level " + n + "\r                       ");
            CrashMe(n + 1);
        }

        [STAThread]
        static void Main()
        {
            //CrashMe(0);

            var fullscreen = false;
            read_input:
            switch (
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Which assignment shall run next? (1, 2, 3, 4, or q for quit)", "Choose assignment",
                    VirtualCity.GetInitialValue()))
            {
                case "1":
                    using (var game = VirtualCity.RunAssignment1(SortSpecialBuildingsByDistance, fullscreen))
                        game.Run();
                    break;
                case "2":
                    using (
                        var game = VirtualCity.RunAssignment2(FindSpecialBuildingsWithinDistanceFromHouse, fullscreen))
                        game.Run();
                    break;
                case "3":
                    using (var game = VirtualCity.RunAssignment3(FindRoute, fullscreen))
                        game.Run();
                    break;
                case "4":
                    using (var game = VirtualCity.RunAssignment4(FindRoutesToAll, fullscreen))
                        game.Run();
                    break;
                case "q":
                    return;
            }
            goto read_input;
        }


        private static IEnumerable<Vector2> SortSpecialBuildingsByDistance(Vector2 house, IEnumerable<Vector2> specialBuildings)
        {
            var buildings = specialBuildings.ToList();
            
            //create list of buildings and their distances relative to the special building
            List<KeyValuePair<Vector2, double>> specialBuildingDistances = buildings.Select(building => new KeyValuePair<Vector2, double>(building, GetDistance(house, building))).ToList();
           
            MergeSort(specialBuildingDistances, 0, specialBuildingDistances.Count - 1);

            for(int i = 0; i < specialBuildingDistances.Count; i++)
            {
                buildings[i] = specialBuildingDistances[i].Key;
            }

            return buildings;
        }
        
        //Just in case I'm not allowed to use Vector2.Distance
        private static double GetDistance(Vector2 house, Vector2 building)
        {
            return Math.Sqrt(Math.Pow(house.X - building.X, 2) + Math.Pow(house.Y - building.Y, 2));
        }

        private static void MergeSort(List<KeyValuePair<Vector2, double>> input, int left, int right)
        {
            if (left < right)
            {
                int middle = (left + right)/2;

                MergeSort(input, left, middle);
                MergeSort(input, middle + 1, right);

                Merge(input, left, middle, right);
            }
        }

        private static void Merge(List<KeyValuePair<Vector2, double>> input, int left, int middle, int right)
        {
            int sizeLeft = middle - left + 1;
            int sizeRight = right - middle;

            var leftList = new List<KeyValuePair<Vector2, double>>();
            var rightList = new List<KeyValuePair<Vector2, double>>();

            for (int i = 0; i < sizeLeft; i++)
            {
                leftList.Add(input[left + i]);
            }

            for (int i = 0; i < sizeRight; i++)
            {
                rightList.Add(input[middle + i + 1]);
            }

            leftList.Add(new KeyValuePair<Vector2, double>(new Vector2(Single.MaxValue), Double.MaxValue));
            rightList.Add(new KeyValuePair<Vector2, double>(new Vector2(Single.MaxValue), Double.MaxValue));

            int indexLeft = 0;
            int indexRight = 0;

            for (int x = left; x <= right; x++)
            {
                if (leftList[indexLeft].Value <= rightList[indexRight].Value)
                {
                    input[x] = leftList[indexLeft];
                    indexLeft++;
                }
                else
                {
                    input[x] = rightList[indexRight];
                    indexRight++;
                }
            }

        }

        private static IEnumerable<IEnumerable<Vector2>> FindSpecialBuildingsWithinDistanceFromHouse(IEnumerable<Vector2> specialBuildings,IEnumerable<Tuple<Vector2, float>> housesAndDistances)
        {
            //Create the empty tree
            var root = new Node<Vector2>();

            //Insert all the nodes into the tree
            foreach (Vector2 building in specialBuildings)
            {
                root = Insert(building, root, true);
            }

            List<List<Vector2>> buildingsInRange = new List<List<Vector2>>();

            //loop trough every special building and find houses in range
            foreach (var specialBuilding in housesAndDistances)
            {
                List<Vector2> inRange = new List<Vector2>();

                GetBuildingsInRange(root, specialBuilding.Item1, specialBuilding.Item2, inRange);

                buildingsInRange.Add(inRange);
            }

            return buildingsInRange;
        }

        static Node<Vector2> Insert(Vector2 building, Node<Vector2> node, bool compareX)
        {
            if (node == null)
            {
                //First time running the tree is empty so this will add the root
                node = new Node<Vector2>(building, new Node<Vector2>(), new Node<Vector2>());
            }
            else if (node.Vector2.Equals(building))
            {
                //This node is already in the tree
            }
            else
            {
                if (compareX) //Alternate between X and Y values
                {
                    if (building.X < node.Vector2.X)
                    {
                        node.Left = Insert(building, node.Left,  false);
                    }
                    else
                    {
                        node.Right = Insert(building, node.Right, false);
                    }
                }
                else
                {
                    if (building.Y < node.Vector2.Y)
                    {
                        node.Left = Insert(building, node.Left, true);
                    }
                    else
                    {
                        node.Right = Insert(building, node.Right, true);
                    }
                }
            }
            return node;
        }

        private static void GetBuildingsInRange(Node<Vector2> node, Vector2 target, float range, List<Vector2> inRange)
        {
            if (node != null)
            {
                if (GetDistance(node.Vector2, target) <= range)
                {
                    inRange.Add(node.Vector2);
                }
                GetBuildingsInRange(node.Left, target, range, inRange);
                GetBuildingsInRange(node.Right, target, range, inRange);
            }
        }

        private class Node<T>
        {
            public Vector2 Vector2 { get; set; }
            public Node<T> Left { get; set; }
            public Node<T> Right { get; set; }

            public Node()
            {
            }

            public Node(Vector2 vector, Node<T> left, Node<T> right)
            {
                Vector2 = vector;
                Left = left;
                Right = right;
            }
        }
        

        private static IEnumerable<Tuple<Vector2, Vector2>> FindRoute(Vector2 startingBuilding, Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads)
        {
            
            var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
             List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() {startingRoad};
             var prevRoad = startingRoad;
             for (int i = 0; i < 30; i++)
             {
                 prevRoad =
                     (roads.Where(x => x.Item1.Equals(prevRoad.Item2))
                         .OrderBy(x => Vector2.Distance(x.Item2, destinationBuilding))
                         .First());
                 fakeBestPath.Add(prevRoad);
             }
             return fakeBestPath;
        }
        

        private static IEnumerable<IEnumerable<Tuple<Vector2, Vector2>>> FindRoutesToAll(Vector2 startingBuilding, IEnumerable<Vector2> destinationBuildings, IEnumerable<Tuple<Vector2, Vector2>> roads)
        {

            List<List<Tuple<Vector2, Vector2>>> result = new List<List<Tuple<Vector2, Vector2>>>();
            foreach (var d in destinationBuildings)
            {
                var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
                List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() {startingRoad};
                var prevRoad = startingRoad;
                for (int i = 0; i < 30; i++)
                {
                    prevRoad =
                        (roads.Where(x => x.Item1.Equals(prevRoad.Item2))
                            .OrderBy(x => Vector2.Distance(x.Item2, d))
                            .First());
                    fakeBestPath.Add(prevRoad);
                }
                result.Add(fakeBestPath);
            }
            return result;
        }
    }
#endif
}
