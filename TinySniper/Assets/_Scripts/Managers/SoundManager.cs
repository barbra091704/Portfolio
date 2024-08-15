using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance;

    public AudioSource globalAudioSource, musicAudioSource;
    public AudioClip[] SniperSounds;
    public AudioClip[] DeathSounds;
    public AudioClip[] Music;
    public AudioClip Ambient;
    public AudioClip StealSound;
    public AudioClip ScreamingSound;
    public AudioClip OutOfAmmoSound;
    public AudioClip ReloadSound;

    public NetworkVariable<bool> isScreaming = new();

    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        PlayMusicRpc();
        PlayAmbientRpc();
    }
    [Rpc(SendTo.Everyone)]
    public void PlayHitSoundRpc()
    {
        globalAudioSource.pitch = Random.Range(1, 1.5f);
        int i = Random.Range(0, DeathSounds.Length);
        globalAudioSource.PlayOneShot(DeathSounds[i]);
    }

    [Rpc(SendTo.Everyone)]
    public void PlayMusicRpc()
    {
        musicAudioSource.clip = Music[Random.Range(0, Music.Length)];
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    [Rpc(SendTo.Everyone)]
    public void PlaySniperSoundRpc(float pitch = 1.0f)
    {
        int i = Random.Range(0, SniperSounds.Length);
        globalAudioSource.pitch = pitch;
        globalAudioSource.PlayOneShot(SniperSounds[i]);

        if (!isScreaming.Value)
        {
            if (IsServer) isScreaming.Value = true;
            musicAudioSource.volume = 0.025f;
            musicAudioSource.loop = false;
            musicAudioSource.Stop();
            musicAudioSource.clip = ScreamingSound;
            musicAudioSource.Play();
        if (IsServer)
            CancelInvoke(nameof(ResetScreaming));
            Invoke(nameof(ResetScreaming), 10f);
        }
    }

    [Rpc(SendTo.Everyone)]
    public void PlayAmbientRpc()
    {
        musicAudioSource.volume = 0.085f;
        musicAudioSource.Stop();
        musicAudioSource.clip = Ambient;
        musicAudioSource.Play();
    }

    void ResetScreaming()
    {
        SetIsScreamingRpc(false);
        PlayMusicRpc();
        PlayAmbientRpc();
    }

    public void PlayOutOfAmmoSound()
    {
        globalAudioSource.PlayOneShot(OutOfAmmoSound);
    }

    public void PlayReloadSound()
    {
        globalAudioSource.PlayOneShot(ReloadSound);
    }

    public void PlayStealSound()
    {
        globalAudioSource.PlayOneShot(StealSound);
    }
    [Rpc(SendTo.Everyone)]
    public void StartScreamingRpc()
    {
        if (!isScreaming.Value)
        {
            if (IsServer) isScreaming.Value = true;
            musicAudioSource.volume = 0.025f;
            musicAudioSource.loop = false;
            musicAudioSource.Stop();
            musicAudioSource.clip = ScreamingSound;
            musicAudioSource.Play();
            if (IsServer)
                CancelInvoke(nameof(ResetScreaming));
                Invoke(nameof(ResetScreaming), 10f);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetIsScreamingRpc(bool screaming)
    {
        isScreaming.Value = screaming;
    }
}
