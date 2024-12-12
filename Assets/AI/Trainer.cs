using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Trainer : MonoBehaviour
{
    /*[SerializeField] private int[] turretLayers;
    [SerializeField] private int[] torpedoLayers;
    [SerializeField] private int gamesPerGeneration = 10;
    [SerializeField] private GameObject turretPrefab;
    [SerializeField] private GameObject torpedoPrefab;
    [SerializeField] private float torpedoSpawnRadius = 2000f;

    [SerializeField] private string modelFolder;
    [SerializeField] private string turretFileName;
    [SerializeField] private string torpedoFileName;

    [SerializeField, Range(0.0001f, 1)] float mutationChance = 0.05f;
    [SerializeField, Range(0, 1)] float mutationStrength = 0.5f;
    [SerializeField] float gameSpeed = 1;
    [SerializeField] float resetTime;
    [SerializeField] NeuralNetwork.Activations activationFunction;

    private NeuralNetwork[] turretNetworks;
    private NeuralNetwork[] torpedoNetworks;
    private AITurret[] turrets;
    private AITorpedo[] torpedos;

    int generation = 0;
    float generationTime = 0;

    void Start()
    {
        // We want even population sizes so we can clone the top half to the bottom and mutate the bottom half
        if (gamesPerGeneration % 2 != 0)
        {
            gamesPerGeneration = 20;
        }

        InitNetworks();
        InvokeRepeating(nameof(InstantiateAgents), 0.1f, resetTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitNetworks()
    {
        turretNetworks = new NeuralNetwork[gamesPerGeneration];
        torpedoNetworks = new NeuralNetwork[gamesPerGeneration];
        for (int i = 0; i < gamesPerGeneration; i++)
        {
            NeuralNetwork turretNetwork = new NeuralNetwork(turretLayers, activationFunction);
            turretNetwork.Load(modelFolder + turretFileName);
            turretNetworks[i] = turretNetwork;

            NeuralNetwork torpedoNetwork = new NeuralNetwork(torpedoLayers, activationFunction);
            torpedoNetwork.Load(modelFolder + torpedoFileName);
            torpedoNetworks[i] = torpedoNetwork;
        }
    }

    void InstantiateAgents()
    {
        Time.timeScale = gameSpeed;

        if (turrets != null || torpedos != null)
        {
            SortNetworks();

            foreach (AITurret turret in turrets)
            {
                Destroy(turret.gameObject);
            }
            foreach (AITorpedo torpedo in torpedos)
            {
                Destroy(torpedo.gameObject);
            }
        }

        turrets = new AITurret[gamesPerGeneration];
        torpedos = new AITorpedo[gamesPerGeneration];
        for (int i = 0; i < gamesPerGeneration; i++)
        {
            AITurret turret = Instantiate(turretPrefab, transform).GetComponent<AITurret>();
            turret.network = turretNetworks[i];
            turrets[i] = turret;
            turret.name = "Turret " + i;
            turret.ID = i;

            Vector3 torpedoSpawnPosition = Random.onUnitSphere * torpedoSpawnRadius;
            AITorpedo torpedo = Instantiate(torpedoPrefab, torpedoSpawnPosition, Quaternion.LookRotation((transform.position - torpedoSpawnPosition).normalized), transform).GetComponent<AITorpedo>();
            torpedo.network = torpedoNetworks[i];
            torpedos[i] = torpedo;
            torpedos[i].Activate(turrets[i].transform, 0f);
            torpedo.name = "Torpedo " + i;

            Collider[] torpedoColliders = torpedos[i].GetComponents<Collider>();
            Collider[] turretColliders = turrets[i].GetComponents<Collider>();
            for (int j = 0; j < i; j++)
            {
                Collider[] otherTorpedoColliders = torpedos[j].GetComponents<Collider>();
                Collider[] otherTurretColliders = turrets[j].GetComponents<Collider>();
                foreach(Collider turretCollider in turretColliders)
                {
                    foreach (Collider otherCollider in otherTorpedoColliders)
                    {
                        Physics.IgnoreCollision(otherCollider, turretCollider);
                    }
                    foreach (Collider otherCollider in otherTurretColliders)
                    {
                        Physics.IgnoreCollision(otherCollider, turretCollider);
                    }
                }
                foreach (Collider torpedoCollider in torpedoColliders)
                {
                    foreach (Collider otherCollider in otherTorpedoColliders)
                    {
                        Physics.IgnoreCollision(otherCollider, torpedoCollider);
                    }
                    foreach (Collider otherCollider in otherTurretColliders)
                    {
                        Physics.IgnoreCollision(otherCollider, torpedoCollider);
                    }
                }
            }
        }

        generationTime = Time.unscaledTime;
    }

    private void SortNetworks()
    {
        generation++;
        float totalTurretFitness = 0;
        foreach (AITurret turret in turrets)
        {
            turret.CalculateFitness();
            totalTurretFitness += turret.network.fitness;
        }

        float totalTorpedoFitness = 0;
        foreach (AITorpedo torpedo in torpedos)
        {
            torpedo.CalculateFitness();
            totalTorpedoFitness += torpedo.network.fitness;
        }

        List<NeuralNetwork> flatTurretNetworks = turretNetworks.Cast<NeuralNetwork>().ToList();
        flatTurretNetworks.Sort();
        flatTurretNetworks[^1].Save(modelFolder + turretFileName);
        Debug.Log("Turret Generation " + generation + " (" + (Time.unscaledTime - generationTime).ToString() + "s)\nAverage: " + (totalTurretFitness / flatTurretNetworks.Count) + ", Best: " + flatTurretNetworks[^1].fitness + ", Worst: " + flatTurretNetworks[0].fitness);

        List<NeuralNetwork> flatTorpedoNetworks = torpedoNetworks.Cast<NeuralNetwork>().ToList();
        flatTorpedoNetworks.Sort();
        flatTorpedoNetworks[^1].Save(modelFolder + torpedoFileName);
        Debug.Log("Torpedo Generation " + generation + " (" + (Time.unscaledTime - generationTime).ToString() + "s)\nAverage: " + (totalTorpedoFitness / flatTorpedoNetworks.Count) + ", Best: " + flatTorpedoNetworks[^1].fitness + ", Worst: " + flatTorpedoNetworks[0].fitness);

        int halfLayers = gamesPerGeneration / 2;
        for (int i = 0; i < halfLayers; i++)
        {
            turretNetworks[i] = flatTurretNetworks[^(i + 1)].Copy(new NeuralNetwork(turretLayers, activationFunction));
            turretNetworks[i].Mutate(mutationChance, mutationStrength);
        }
        for (int i = halfLayers; i < gamesPerGeneration; i++)
        {
            turretNetworks[i] = flatTurretNetworks[i].Copy(new NeuralNetwork(turretLayers, activationFunction));
        }

        for (int i = 0; i < halfLayers; i++)
        {
            torpedoNetworks[i] = flatTorpedoNetworks[^(i + 1)].Copy(new NeuralNetwork(torpedoLayers, activationFunction));
            torpedoNetworks[i].Mutate(mutationChance, mutationStrength);
        }
        for (int i = halfLayers; i < gamesPerGeneration; i++)
        {
            torpedoNetworks[i] = flatTorpedoNetworks[i].Copy(new NeuralNetwork(torpedoLayers, activationFunction));
        }
    }*/
}
