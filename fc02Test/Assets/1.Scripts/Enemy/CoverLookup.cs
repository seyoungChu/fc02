using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FC;

namespace FC
{
    public class CoverLookup : MonoBehaviour
    {
        private List<Vector3[]> allCoverSpots; // The level avaliable cover spots.
        private GameObject[] covers; // The level covers.
        private List<int> coverHashCodes; // Cover unique IDs.

        private Dictionary<float, Vector3> filteredSpots; // The potential spots and its distance to NPC.

        // Get all active cover objects of the level.
        private GameObject[] GetObjectsInLayerMask(int layerMask)
        {
            var ret = new List<GameObject>();
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                // The layer mask contains the object layer?
                if (go.activeInHierarchy && layerMask == (layerMask | (1 << go.layer)))
                {
                    ret.Add(go);
                }
            }

            return ret.ToArray();
        }

        // Get the navmesh point closest to the reference cover spot.
        private void ProcessPoint(List<Vector3> ls, Vector3 naivePoint, float range)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(naivePoint, out hit, range, NavMesh.AllAreas))
            {
                ls.Add(hit.position);
            }
        }

        // Calculate the cover potential spots.
        private Vector3[] GetSpots(GameObject go, LayerMask obstacleMask)
        {
            List<Vector3> bounds = new List<Vector3>();
            // Get spots for all colliders in Game Object.
            foreach (Collider col in go.GetComponents<Collider>())
            {
                float baseHeight = (col.bounds.center - col.bounds.extents).y;
                float range = 2 * col.bounds.extents.y;

                Vector3 deslocForward = go.transform.forward * go.transform.localScale.z * 0.5f;
                Vector3 deslocRight = go.transform.right * go.transform.localScale.x * 0.5f;

                // Is it a custom mesh collider?
                if (go.GetComponent<MeshCollider>())
                {
                    float maxBounds = go.GetComponent<MeshCollider>().bounds.extents.z +
                                      go.GetComponent<MeshCollider>().bounds.extents.x;
                    Vector3 originFwd = col.bounds.center + go.transform.forward * maxBounds;
                    Vector3 originRight = col.bounds.center + go.transform.right * maxBounds;
                    if (Physics.Raycast(originFwd, col.bounds.center - originFwd, out RaycastHit hit, maxBounds,
                        obstacleMask))
                    {
                        deslocForward = hit.point - col.bounds.center;
                    }

                    if (Physics.Raycast(originRight, col.bounds.center - originRight, out hit, maxBounds, obstacleMask))
                    {
                        deslocRight = hit.point - col.bounds.center;
                    }
                }
                // Is it a primitive collider with different dimensions?
                else if (Vector3.Equals(go.transform.localScale, Vector3.one))
                {
                    deslocForward = go.transform.forward * col.bounds.extents.z;
                    deslocRight = go.transform.right * col.bounds.extents.x;
                }

                // Calculate spot points around the cover.
                float edgeFactor = 0.75f;
                ProcessPoint(bounds, col.bounds.center + deslocRight + deslocForward * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center + deslocForward + deslocRight * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center + deslocForward, range);
                ProcessPoint(bounds, col.bounds.center + deslocForward - deslocRight * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center - deslocRight + deslocForward * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center + deslocRight, range);
                ProcessPoint(bounds, col.bounds.center + deslocRight - deslocForward * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center - deslocForward + deslocRight * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center - deslocForward, range);
                ProcessPoint(bounds, col.bounds.center - deslocForward - deslocRight * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center - deslocRight - deslocForward * edgeFactor, range);
                ProcessPoint(bounds, col.bounds.center - deslocRight, range);
            }

            return bounds.ToArray();
        }


        public void Setup(LayerMask coverMask)
        {
            // Grab all level covers.
            covers = GetObjectsInLayerMask(coverMask);

            // Set up the references.
            coverHashCodes = new List<int>();
            allCoverSpots = new List<Vector3[]>();
            foreach (GameObject cover in covers)
            {
                allCoverSpots.Add(GetSpots(cover, coverMask));
                coverHashCodes.Add(cover.GetHashCode());
            }
        }


        // Check if target is in path to spot. True if target is within angle and closer than the spot.
        //목표물이 경로에 있는지 확인하십시오. 대상이 각도 내에 있고 지점보다 가까이 있으면 참입니다.
        private bool TargetInPath(Vector3 origin, Vector3 spot, Vector3 target, float angle)
        {
            Vector3 dirToTarget = (target - origin).normalized;
            Vector3 dirToSpot = (spot - origin).normalized;

            // The angle between vectors origin-spot and origin-target is within angle?
            if (Vector3.Angle(dirToSpot, dirToTarget) <= angle)
            {
                // Target is in the direction of the spot, check distances.
                float targetDist = (target - origin).sqrMagnitude;
                float spotDist = (spot - origin).sqrMagnitude;
                // Is the target closest than the spot?
                return (targetDist <= spotDist);
            }

            // Target is not within angle, so not in path.
            return false;
        }


        // Filter cover spots, returning only the possible ones.
        //가장가까운 유효한 지점을 찾아준다. 거리도 같이 준다.
        private ArrayList FilterSpots(StateController controller)
        {
            float minDist = Mathf.Infinity;
            filteredSpots = new Dictionary<float, Vector3>();
            int nextCoverHash = -1;
            for (int i = 0; i < allCoverSpots.Count; i++)
            {
                // Ignore disabled covers and current cover used by the NPC.
                if (!covers[i].activeSelf || coverHashCodes[i] == controller.coverHash)
                    continue;
                // Iterate over all cover spots on the level
                foreach (Vector3 spot in allCoverSpots[i])
                {
                    Vector3 vectorDist = controller.personalTarget - spot;
                    float searchDist = (controller.transform.position - spot).sqrMagnitude;
                    // Does this spot is within view range?
                    if (vectorDist.sqrMagnitude <= controller.viewRadius * controller.viewRadius &&
                        Physics.Raycast(spot, vectorDist, out RaycastHit hit, vectorDist.sqrMagnitude,
                            controller.generalStats.coverMask))
                    {
                        // Does this spot provides cover protection from the player?
                        if (hit.collider == covers[i].GetComponent<Collider>() &&
                            // Ensure the player is not between NPC and the spot. Use a quarter of FOV angle as reference.
                            //플레이어가 NPC와 스팟 사이에 있지 않은지 확인하십시오. 기준으로 1/4의 FOV 각도를 사용하십시오.
                            //타겟보단 멀리 있는 건 거른다.
                            !TargetInPath(controller.transform.position, spot, controller.personalTarget,
                                controller.viewAngle / 4))
                        {
                            // Add the spot as a potential one.
                            if (!filteredSpots.ContainsKey(searchDist))
                            {
                                filteredSpots.Add(searchDist, spot);
                            }
                            else
                                continue;

                            // Select the nearest filtered spot.
                            if (minDist > searchDist)
                            {
                                minDist = searchDist;
                                nextCoverHash = coverHashCodes[i];
                            }
                        }
                    }
                }
            }

            ArrayList returnArray = new ArrayList();
            returnArray.Add(nextCoverHash);
            returnArray.Add(minDist);
            // Return the nearest filtered spot.
            return returnArray;
        }

        // Get the best cover spot, considering NPC and target positions.
        public ArrayList GetBestCoverSpot(StateController controller)
        {
            ArrayList nextCoverData = FilterSpots(controller);
            int nextCoverHash = (int) nextCoverData[0];
            float minDist = (float) nextCoverData[1];
            ArrayList returnArray = new ArrayList();
            // No potential cover spot.
            if (filteredSpots.Count == 0)
            {
                returnArray.Add(-1);
                returnArray.Add(Vector3.positiveInfinity);
            }
            // Return best pontential spot.
            else
            {
                returnArray.Add(nextCoverHash);
                returnArray.Add(filteredSpots[minDist]);
            }

            return returnArray;
        }
    }
}