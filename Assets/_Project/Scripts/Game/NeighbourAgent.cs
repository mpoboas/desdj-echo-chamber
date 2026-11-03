using UnityEngine;
using UnityEngine.AI;

namespace EchoChamber
{
    /// <summary>
    /// 3D body of one neighbour. The simulation is authoritative for where they are and what they do;
    /// this follows it on the NavMesh, animates, and shows the visible reaction symbol.
    /// Never shows hidden states.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NeighbourAgent : MonoBehaviour
    {
        public int index;
        public Transform visual;
        public SpriteRenderer iconDisc;
        public TextMesh iconGlyph;
        public GameObject protestSign;
        public float catchUpSpeed = 1.35f;

        private NavMeshAgent agent;
        private RigPose rig;
        private float bob;
        private Camera cam;
        private Vector3 lastPos;

        public Neighbour State => GameManager.Instance != null && GameManager.Instance.Sim != null
            ? GameManager.Instance.Sim.Neighbours[index] : null;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            bob = Random.value * 6f;
        }

        private void Start()
        {
            if (visual != null) rig = new RigPose(visual);
            cam = Camera.main;
            lastPos = transform.position;
            if (iconDisc != null) iconDisc.gameObject.SetActive(false);
        }

        private bool IsInsideBuilding(Neighbour n)
        {
            var content = GameManager.Instance.Content;
            foreach (var p in content.places)
            {
                if (p.open) continue;
                float[] r = p.rect;
                if (n.X >= r[0] && n.X <= r[0] + r[2] && n.Y >= r[1] && n.Y <= r[1] + r[3]) return true;
            }
            return false;
        }

        private void Update()
        {
            var n = State;
            if (n == null) return;
            var sim = GameManager.Instance.Sim;
            Vector3 target = WorldMap.ToWorld(n.X, n.Y);
            bool inside = IsInsideBuilding(n);

            if (visual != null && visual.gameObject.activeSelf == inside) visual.gameObject.SetActive(!inside);

            if (inside)
            {
                if (agent.enabled) agent.enabled = false;
                transform.position = target;
            }
            else
            {
                if (!agent.enabled)
                {
                    transform.position = target;
                    agent.enabled = true;
                }
                if (!agent.isOnNavMesh)
                {
                    transform.position = target;
                }
                else
                {
                    float dist = Vector3.Distance(transform.position, target);
                    if (dist > 0.9f)
                    {
                        agent.Warp(target);
                    }
                    else
                    {
                        float simSpeed = sim.Tuning.walkSpeed * WorldMap.UnitsPerLogical * (n.Stress > sim.Tuning.fastStressThreshold ? sim.Tuning.fastSpeedMultiplier : 1f);
                        agent.speed = Mathf.Max(0.2f, simSpeed * catchUpSpeed + dist * 2f);
                        agent.acceleration = 12f;
                        agent.SetDestination(target);
                    }
                }
            }

            Animate(n, sim);
            UpdateIcon(n);
        }

        private void Animate(Neighbour n, GameSimulation sim)
        {
            if (rig == null || !rig.HasBones || !visual.gameObject.activeSelf) return;
            bool moving = (transform.position - lastPos).sqrMagnitude > 0.0000025f || n.IsMoving && !n.Waiting;
            lastPos = transform.position;
            RigPose.Mode mode = RigPose.Mode.Idle;
            if (moving) mode = RigPose.Mode.Walk;
            else if (n.Place == PlaceId.Junta) mode = RigPose.Mode.Protest;
            else if (sim.Rings.FestaActive && n.Place == PlaceId.Praca) mode = RigPose.Mode.Party;
            else if (n.Action == ActionKind.Ecra) mode = RigPose.Mode.Phone;
            else if (n.Stress > 0.68f) mode = RigPose.Mode.Tense;
            rig.Apply(mode, Time.time, bob, n.Stress > sim.Tuning.fastStressThreshold);

            if (!moving)
            {
                Quaternion face = Quaternion.identity; // towards the window / camera (+Z)
                transform.rotation = Quaternion.Slerp(transform.rotation, face, Time.deltaTime * 6f);
            }
            if (protestSign != null) protestSign.SetActive(n.Place == PlaceId.Junta && n.Id % 2 == 0);
        }

        private void UpdateIcon(Neighbour n)
        {
            if (iconDisc == null) return;
            bool show = false;
            ReactionKind kind = ReactionKind.Indiferente;
            string glyph = "";
            Color col = Color.white;
            if (n.IconTime > 0f)
            {
                show = true;
                col = ReactionStyle.CategoryColor(n.IconCategory);
                glyph = ReactionStyle.CategoryGlyph(n.IconCategory);
            }
            else if (n.Reaction != null && n.Reaction.TimeLeft > 0f)
            {
                show = true;
                kind = n.Reaction.Kind;
                col = ReactionStyle.Color(kind);
                glyph = ReactionStyle.Glyph(kind);
            }
            else if (n.Action == ActionKind.Ecra && visual.gameObject.activeSelf)
            {
                show = true;
                col = ReactionStyle.Hex("#4C7DFF");
                glyph = "o";
            }
            if (iconDisc.gameObject.activeSelf != show) iconDisc.gameObject.SetActive(show);
            if (!show) return;
            iconDisc.color = col;
            if (iconGlyph != null) iconGlyph.text = glyph;
            if (cam == null) cam = Camera.main;
            if (cam != null) iconDisc.transform.rotation = cam.transform.rotation;
        }
    }
}
