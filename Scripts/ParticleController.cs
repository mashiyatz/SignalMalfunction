using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public enum State {Start, Initialize, Idle, Confession, Transition};
    public State currentState;

    public ParticleSystem bodyParticles;
    public ParticleSystem jointParticles;
    public SkinnedMeshRenderer bodyMeshRenderer;
    public SkinnedMeshRenderer jointsMeshRenderer;

    public Vector3 startPosition;
    public Vector3 startRotation;
    public AudioClip[] audioClips;

    [HideInInspector] public bool wasMousePressed = false;

    private Animator animator;
    private AudioSource audioSource;
    private ParticleSystem.EmissionModule emissionBody;
    private ParticleSystem.EmissionModule emissionJoints;
    private ParticleSystem.MainModule mainBody;
    private ParticleSystem.MainModule mainJoints;
    private float emissionBodyRate;
    private float emissionJointsRate;

    private float timeAtStartOfState;
    private float timeAtStart;
    private float timeSinceStart;

    private int animationIndex;
    private bool isIncreasingEmission;
    private bool isDecreasingEmission;
    private bool isOpaque;

    void Start()
    {
        emissionBody = bodyParticles.emission;
        emissionJoints = jointParticles.emission;
        mainBody = bodyParticles.main;
        mainJoints = jointParticles.main;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        emissionBody.enabled = false;
        emissionJoints.enabled = false;

        emissionBodyRate = 0;
        emissionJointsRate = 0;

        emissionBody.rateOverTime = emissionBodyRate;
        emissionJoints.rateOverTime = emissionJointsRate;

        SetMeshOpacity(0);

        animationIndex = Random.Range(0, animator.GetInteger("numberOfAnimations"));
        isIncreasingEmission = false;
        isDecreasingEmission = false;
        isOpaque = false;
        wasMousePressed = false;

        timeAtStart = Time.time;
        timeAtStartOfState = Time.time;
        currentState = State.Start;
    }

    void Update()
    {
        // timeSinceStart = Time.time - timeAtStart;

        if (currentState == State.Start)
        {
            if (Time.time - timeAtStartOfState > 1f)
            {
                emissionBody.enabled = true;
                emissionJoints.enabled = true;

                transform.localPosition = startPosition;
                transform.localEulerAngles = startRotation;

                isIncreasingEmission = false;
                isDecreasingEmission = false;
                wasMousePressed = false;

                animationIndex += 1;
                if (animationIndex >= animator.GetInteger("numberOfAnimations")) animationIndex = 0;
                animator.SetInteger("index", animationIndex); 
                animator.SetTrigger("trigger");

                audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
                currentState = State.Initialize;
            }
        }
        else if (currentState == State.Initialize)
        {
            IncreaseEmission();
            if (emissionBody.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionBody &&
                emissionJoints.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionJoints) currentState = State.Idle;
        }
        else if (currentState == State.Idle)
        {
            // if (timeSinceStart > ParticleSystemConstants.timeToTransition && !wasMousePressed) currentState = State.Transition;
            if (!audioSource.isPlaying) { 
                currentState = State.Transition;
                ParticleSystemConstants.isListening = false;
                StartCoroutine(MoveCamera(ParticleSystemConstants.cameraDefaultPos));
                StartCoroutine(DecreaseMeshOpacityOverTime());
                return; 
            }

            ChangeMeshOpacityWithEmission();

            if (wasMousePressed)
            {
                if (emissionBody.rateOverTime.constant == 0 && emissionJoints.rateOverTime.constant == 0)
                {
                    isIncreasingEmission = true;
                    isDecreasingEmission = false;
                    StartCoroutine(MoveCamera(ParticleSystemConstants.cameraDefaultPos));
                    jointsMeshRenderer.material = ParticleSystemConstants.jointsMaterialTransparent;
                    bodyMeshRenderer.material = ParticleSystemConstants.bodyMaterialTransparent;
                    isOpaque = false;
                    ParticleSystemConstants.isListening = false;
                    SetMeshOpacity(1f);
                }
                if (emissionBody.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionBody && emissionJoints.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionJoints)
                {
                    if (!ParticleSystemConstants.isListening)
                    {
                        isIncreasingEmission = false;
                        isDecreasingEmission = true;
                    }
                }
                wasMousePressed = false;
            }

            if (isDecreasingEmission)
            {
                DecreaseEmission();
                ParticleSystemConstants.mainCamera.position = Vector3.MoveTowards(
                    ParticleSystemConstants.mainCamera.position,
                    transform.position + new Vector3(0, 1.35f, -0.75f),
                    0.05f
                );
                ParticleSystemConstants.isListening = true;
                if (emissionBody.rateOverTime.constant == 0 && emissionJoints.rateOverTime.constant == 0)
                {
                    jointsMeshRenderer.material = ParticleSystemConstants.jointsMaterialOpaque;
                    bodyMeshRenderer.material = ParticleSystemConstants.bodyMaterialOpaque;
                    isOpaque = true;
                    isDecreasingEmission = false;
                }
            }
            else if (isIncreasingEmission)
            {
                IncreaseEmission();
                if (emissionBody.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionBody && emissionJoints.rateOverTime.constant == ParticleSystemConstants.rateOfEmissionJoints)
                {
                    isIncreasingEmission = false;
                }
            }

            // change color and velocity of particle based on volume of sound from audioSource
            // if using color by speed, just need to update speed
           
        }
       
        else if (currentState == State.Transition)
        {
            mainJoints.startSpeed = Mathf.Lerp(0, 3, 1 - emissionJoints.rateOverTime.constant / ParticleSystemConstants.rateOfEmissionJoints);
            mainBody.startSpeed = Mathf.Lerp(0, 3, 1 - emissionBody.rateOverTime.constant / ParticleSystemConstants.rateOfEmissionBody);
            DecreaseEmission();

            if (emissionBody.rateOverTime.constant == 0 && emissionJoints.rateOverTime.constant == 0)
            {
                mainBody.startSpeed = 0;
                mainJoints.startSpeed = 0;
                // timeAtStart = Time.time;
                timeAtStartOfState = Time.time;
                currentState = State.Start;
            }
        }
    }

    private void ChangeMeshOpacityWithEmission()
    {
        bodyMeshRenderer.material.color = new Color(
            bodyMeshRenderer.material.color.r,
            bodyMeshRenderer.material.color.g,
            bodyMeshRenderer.material.color.b,
            Mathf.Lerp(0, 1, 1 - emissionBody.rateOverTime.constant / ParticleSystemConstants.rateOfEmissionBody)
        );
        jointsMeshRenderer.material.color = new Color(
            jointsMeshRenderer.material.color.r,
            jointsMeshRenderer.material.color.g,
            jointsMeshRenderer.material.color.b,
            Mathf.Lerp(0, 1, 1 - emissionJoints.rateOverTime.constant / ParticleSystemConstants.rateOfEmissionJoints)
        );
    }

    private void SetMeshOpacity(float value)
    {
        bodyMeshRenderer.material.color = new Color(
            bodyMeshRenderer.material.color.r,
            bodyMeshRenderer.material.color.g,
            bodyMeshRenderer.material.color.b,
            value
        );
        jointsMeshRenderer.material.color = new Color(
            jointsMeshRenderer.material.color.r,
            jointsMeshRenderer.material.color.g,
            jointsMeshRenderer.material.color.b,
            value
        );
    }

    IEnumerator DecreaseMeshOpacityOverTime()
    {

        if (isOpaque)
        {
            isOpaque = false;
            jointsMeshRenderer.material = ParticleSystemConstants.jointsMaterialTransparent;
            bodyMeshRenderer.material = ParticleSystemConstants.bodyMaterialTransparent;
            SetMeshOpacity(1f);
        }

        while (bodyMeshRenderer.material.color.a > 0.01f || jointsMeshRenderer.material.color.a > 0.01f)
        {
            bodyMeshRenderer.material.color = new Color(
                bodyMeshRenderer.material.color.r,
                bodyMeshRenderer.material.color.g,
                bodyMeshRenderer.material.color.b,
                bodyMeshRenderer.material.color.a - 0.01f
            );
            jointsMeshRenderer.material.color = new Color(
                jointsMeshRenderer.material.color.r,
                jointsMeshRenderer.material.color.g,
                jointsMeshRenderer.material.color.b,
                jointsMeshRenderer.material.color.a - 0.01f
            );
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator MoveCamera(Vector3 destination)
    {
        Vector3 startingPosition = ParticleSystemConstants.mainCamera.position;
        while (ParticleSystemConstants.mainCamera.position != destination) {
            ParticleSystemConstants.mainCamera.position = Vector3.MoveTowards(
                ParticleSystemConstants.mainCamera.position,
                destination,
                Mathf.Lerp(
                    0.05f, 
                    0.01f, 
                    Vector3.Distance(ParticleSystemConstants.mainCamera.position, destination) / Vector3.Distance(startingPosition, destination))
            );
            yield return new WaitForEndOfFrame();
        }
    }

    private void IncreaseEmission()
    {
        if (emissionBody.rateOverTime.constant < ParticleSystemConstants.rateOfEmissionBody)
        {
            emissionBodyRate += ParticleSystemConstants.rateOfEmissionChange;
            emissionBody.rateOverTime = emissionBodyRate;
        }
        if (emissionJoints.rateOverTime.constant < ParticleSystemConstants.rateOfEmissionJoints)
        {
            emissionJointsRate += ParticleSystemConstants.rateOfEmissionChange;
            emissionJoints.rateOverTime = emissionJointsRate;
        }
    }

    private void DecreaseEmission()
    {
        if (emissionBody.rateOverTime.constant > 0)
        {
            emissionBodyRate -= ParticleSystemConstants.rateOfEmissionChange * 2;
            emissionBody.rateOverTime = emissionBodyRate;
        }
        if (emissionJoints.rateOverTime.constant > 0)
        {
            emissionJointsRate -= ParticleSystemConstants.rateOfEmissionChange * 2;
            emissionJoints.rateOverTime = emissionJointsRate;
        }
    }

}
