using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using TMPro;
using UnityEngine.UI;

public class Ship : MonoBehaviour
{
    private Collider shipCollider;
    public PhysicsHandler physicsHandler;

    public GameObject radarModel;
    [SerializeField] protected ParticleSystem rocketTrail;
    public LayerMask ignoreLayers;

    public bool isPilot;
    public Camera pilotCamera;
    private Dictionary<Transform, Transform> playerParentPair = new Dictionary<Transform, Transform>();

    [SerializeField] protected HUDSystem _HUDSystem;
    [SerializeField] private GameObject pilotUI;

    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private Transform velocityDirectionPivot;

    [SerializeField] private Transform cockpitOffset;

    private List<Collider> collidersInShip = new List<Collider>();

    protected float health = 100f;
    private float maxHealth = 100f;
    protected float fuel = 100f;
    private float maxFuel = 100f;

    private Vector3 lastVelocity;
    public Vector3 acceleration;

    void Start()
    {
        shipCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (isPilot)
        {
            _HUDSystem.HUDPivot.SetPositionAndRotation(FlatCamera.instance.transform.position, FlatCamera.instance.transform.rotation);
        }

        speedText.text = "Speed: " + CustomMethods.SpeedToFormattedString(physicsHandler.velocity.magnitude, 2);
        if(physicsHandler.velocity != Vector3d.zero)
            velocityDirectionPivot.rotation = Quaternion.LookRotation(physicsHandler.velocity.ToVector3(), transform.up);
    }

    private void FixedUpdate()
    {
        acceleration = (physicsHandler.velocity.ToVector3() - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = physicsHandler.velocity.ToVector3();
    }

    public void AddCollider(Collider collider)
    {
        collidersInShip.Add(collider);
        Physics.IgnoreCollision(collider, shipCollider);
    }

    public void RemoveCollider(Collider collider)
    {
        collidersInShip.Remove(collider);
        Physics.IgnoreCollision(collider, shipCollider, false);
    }

    public void EnterShip(Transform player)
    {
        if (!playerParentPair.ContainsKey(player))
        {
            playerParentPair.Add(player, player.parent);
            player.SetParent(transform);
        }
    }

    public void ExitShip(Transform player)
    {
        if (playerParentPair.TryGetValue(player, out Transform playerParent))
        {
            player.SetParent(playerParent);
            playerParentPair.Remove(player);
        }
    }

    public void SetPilot(int state)
    {
        isPilot = state == 1;
        pilotUI.SetActive(isPilot);
        StartCoroutine(AlignPilot());
    }

    IEnumerator AlignPilot()
    {
        Fader.instance.StartFade(0f, 1f, 1f);
        yield return new WaitForSeconds(1f);
        VRControl.instance.transform.SetPositionAndRotation(cockpitOffset.position, cockpitOffset.rotation);
        Fader.instance.StartFade(1f, 0f, 1f);
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log(collision.transform.name + ": " + collision.collider.name + " layer " + collision.collider.gameObject.layer + " " + collision.body.name);
    }
}
