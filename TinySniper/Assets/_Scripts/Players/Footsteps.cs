using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Footsteps : NetworkBehaviour
{
    private AudioSource footstepAudioSource;

    public AudioClip[] footstepClips; // Array of footstep sounds
    public float footstepInterval = 0.5f; // Time interval between footsteps
    private bool isPlayingFootsteps = false;

    bool isPlayer;

    private CivilianAI civilianAI;
    private PlayerMovement playerMovement;

    void Start()
    {
        footstepAudioSource = GetComponent<AudioSource>();

        if (!IsOwner) return;

        if (TryGetComponent(out PlayerMovement component))
        {
            if (component != null)
            {
                playerMovement = component;
                isPlayer = true;
            }
        }
        else
        {
            civilianAI = GetComponent<CivilianAI>();
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (isPlayer && Input.GetAxisRaw("Horizontal") > 0)
        {
            if (!isPlayingFootsteps)
            {
                StartCoroutine(PlayFootstepsPlayer());
            }
        }
        else if (!isPlayer && civilianAI.isMoving)
        {
            if (!isPlayingFootsteps)
            {
                StartCoroutine(PlayFootstepsCivilian());
            }
        }
        else
        {
            StopAllCoroutines(); // Stop the footstep sound if the player stops moving
            isPlayingFootsteps = false;
        }
    }
 
    private IEnumerator PlayFootstepsCivilian()
    {
        isPlayingFootsteps = true;

        while (civilianAI.isMoving)
        {
            PlayRandomFootstepRpc();
            if (SoundManager.Instance.isScreaming.Value)
            {
                yield return new WaitForSeconds(footstepInterval * 0.5f + UnityEngine.Random.Range(0, 0.1f));
            }
            else
            {
                yield return new WaitForSeconds(footstepInterval + UnityEngine.Random.Range(0, 0.1f));
            }
        }

        isPlayingFootsteps = false;
    }

    private IEnumerator PlayFootstepsPlayer()
    {
        isPlayingFootsteps = true;

        while (Input.GetAxisRaw("Horizontal") > 0)
        {
            PlayRandomFootstepRpc();
            if (playerMovement.currentSpeed == playerMovement.walkSpeed)
            {
                yield return new WaitForSeconds(footstepInterval + UnityEngine.Random.Range(0, 0.1f));
            }
            else if (playerMovement.currentSpeed == playerMovement.runSpeed)
            {
                yield return new WaitForSeconds(footstepInterval * 0.5f + UnityEngine.Random.Range(0, 0.1f));
            }
        }

        isPlayingFootsteps = false;
    }

    [Rpc(SendTo.Everyone)]
    private void PlayRandomFootstepRpc()
    {
        if (footstepClips.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, footstepClips.Length);
            footstepAudioSource.PlayOneShot(footstepClips[randomIndex]);
        }
    }
}
