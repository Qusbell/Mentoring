using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구조체 기반 개선된 SoundManager
/// 각 사운드마다 개별 볼륨 조절 가능
/// 드래그 앤 드롭으로 쉬운 설정
/// </summary>
public class SoundManager : SingletonT<SoundManager>
{
    #region ===== 사운드 클립 구조체 =====

    [System.Serializable]
    public class SoundClip
    {
        [Tooltip("사운드 파일")]
        public AudioClip clip;

        [Tooltip("개별 볼륨 (0~1)")]
        [Range(0f, 1f)]
        public float volume = 1f;

        // 생성자
        public SoundClip()
        {
            clip = null;
            volume = 1f;
        }

        public SoundClip(AudioClip audioClip, float vol = 1f)
        {
            clip = audioClip;
            volume = vol;
        }
    }

    #endregion

    #region ===== 배경음악 =====

    [Header("=== 배경음악 ===")]
    [Tooltip("배경음악 파일들")]
    public SoundClip[] backgroundMusics;

    #endregion

    #region ===== 플레이어 기본 액션 사운드 =====

    [Header("=== 플레이어 기본 액션 사운드 ===")]
    [Tooltip("플레이어 이동 소리")]
    public SoundClip playerMove;

    [Tooltip("플레이어 점프 소리")]
    public SoundClip playerJump;

    [Tooltip("플레이어 착지 소리")]
    public SoundClip playerLand;

    [Tooltip("플레이어 피격 소리")]
    public SoundClip playerHit;

    [Tooltip("플레이어 사망 소리")]
    public SoundClip playerDeath;

    #endregion

    #region ===== 플레이어 공격 사운드 =====

    [Header("=== 플레이어 공격 사운드 ===")]
    [Tooltip("기본 공격 소리")]
    public SoundClip playerBasicAttack;

    [Tooltip("낙하 공격 시전 소리")]
    public SoundClip playerDropAttack;

    [Tooltip("낙하 공격 착지 임팩트 소리")]
    public SoundClip playerDropImpact;

    [Tooltip("닷지 공격 소리")]
    public SoundClip playerDodgeAttack;

    #endregion

    #region ===== 몬스터 공통 사운드 =====

    [Header("=== 몬스터 공통 사운드 ===")]
    [Tooltip("몬스터 스폰 소리")]
    public SoundClip monsterSpawn;

    [Tooltip("몬스터 피격 소리")]
    public SoundClip monsterHit;

    #endregion

    #region ===== 몬스터별 공격 사운드 =====

    [Header("=== 몬스터별 공격 사운드 ===")]
    [Tooltip("미니언 일반 공격")]
    public SoundClip minionAttack;

    [Tooltip("궁수 화살 발사")]
    public SoundClip archerFire;

    [Tooltip("마법사 주문 시전")]
    public SoundClip mageSpell;

    [Tooltip("방패병 돌진 공격")]
    public SoundClip shielderCharge;

    #endregion

    #region ===== 큐브 시스템 사운드 =====

    [Header("=== 큐브 시스템 사운드 ===")]
    [Tooltip("큐브 이동 시작 소리")]
    public SoundClip cubeMoveStart;

    [Tooltip("큐브 이동 중 소리 (루프)")]
    public SoundClip cubeMoveLoop;

    [Tooltip("큐브 이동 도착 소리")]
    public SoundClip cubeMoveEnd;

    [Tooltip("큐브 붕괴 경고 소리")]
    public SoundClip cubeCollapseWarning;

    [Tooltip("큐브 흔들림 소리 (루프)")]
    public SoundClip cubeCollapseShake;

    [Tooltip("큐브 붕괴 떨어짐 소리")]
    public SoundClip cubeCollapseFall;

    #endregion

    #region ===== 멧돼지 시스템 사운드 =====

    [Header("=== 멧돼지 시스템 사운드 ===")]
    [Tooltip("멧돼지 경고 소리")]
    public SoundClip boarWarning;

    [Tooltip("멧돼지 돌진 소리")]
    public SoundClip boarCharge;

    [Tooltip("멧돼지 충돌 소리")]
    public SoundClip boarCrash;

    #endregion

    #region ===== UI 및 시스템 사운드 =====

    [Header("=== UI 및 시스템 사운드 ===")]
    [Tooltip("UI 클릭 소리")]
    public SoundClip uiClick;

    [Tooltip("UI 대화 출력 소리")]
    public SoundClip uiDialog;

    [Tooltip("사운드 조절 소리")]
    public SoundClip volumeSlider;

    #endregion

    #region ===== 내부 변수 =====

    // PlayerPrefs 키 상수
    private const string MusicVolumeKey = "MusicVolume";
    private const string EffectVolumeKey = "EffectVolume";

    // 현재 볼륨 값들
    private float currentMusicVolume = 1.0f;
    private float currentEffectVolume = 1.0f;

    // 자동 생성되는 AudioSource들
    private AudioSource musicSource;        // 배경음악용
    private AudioSource effectSource;       // 모든 효과음용
    private AudioSource loopSource;         // 루프 사운드용 (큐브 이동, 흔들림 등)

    // 현재 재생 중인 배경음악 인덱스
    private int currentMusicIndex = -1;

    #endregion

    #region ===== 초기화 =====

    protected void Awake()
    {
        // 싱글톤 설정 (기존 코드 유지)

        // AudioSource 자동 생성
        CreateAudioSources();

        // 모든 효과음 정지
        StopAllEffectSounds();
    }

    private void Start()
    {
        // 저장된 볼륨 값 불러오기 및 적용
        float savedMusicVolume = LoadVolume(MusicVolumeKey, 1.0f);
        float savedEffectVolume = LoadVolume(EffectVolumeKey, 1.0f);

        SetMusicVolume(savedMusicVolume);
        SetEffectVolume(savedEffectVolume);

        // 첫 번째 배경 음악 재생
        if (backgroundMusics != null && backgroundMusics.Length > 0)
        {
            PlayBackgroundMusic(0);
        }
    }

    /// <summary>
    /// AudioSource 자동 생성
    /// </summary>
    private void CreateAudioSources()
    {
        // 배경음악용 AudioSource
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // 효과음용 AudioSource  
        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.loop = false;
        effectSource.playOnAwake = false;

        // 루프 사운드용 AudioSource
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;

        Debug.Log("[SoundManager] AudioSource 자동 생성 완료 (총 3개)");
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    private void StopAllEffectSounds()
    {
        if (effectSource != null && effectSource.isPlaying)
            effectSource.Stop();

        if (loopSource != null && loopSource.isPlaying)
            loopSource.Stop();
    }

    #endregion

    #region ===== 볼륨 관리 =====

    /// <summary>
    /// 음악 볼륨 설정
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = volume;

        if (musicSource != null)
        {
            musicSource.volume = volume;
        }

        SaveVolume(MusicVolumeKey, volume);
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetEffectVolume(float volume)
    {
        currentEffectVolume = volume;

        if (effectSource != null)
        {
            effectSource.volume = volume;
        }

        if (loopSource != null)
        {
            loopSource.volume = volume;
        }

        SaveVolume(EffectVolumeKey, volume);
    }

    /// <summary>
    /// 볼륨 저장
    /// </summary>
    public void SaveVolume(string key, float volume)
    {
        PlayerPrefs.SetFloat(key, volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 볼륨 불러오기
    /// </summary>
    public float LoadVolume(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    #endregion

    #region ===== 배경음악 관리 =====

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    public void PlayBackgroundMusic(int index)
    {
        if (backgroundMusics == null || index < 0 || index >= backgroundMusics.Length)
            return;

        SoundClip musicClip = backgroundMusics[index];
        if (musicClip == null || musicClip.clip == null)
            return;

        // 현재 음악 정지
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // 새 음악 재생
        musicSource.clip = musicClip.clip;
        musicSource.volume = musicClip.volume * currentMusicVolume;
        musicSource.Play();

        currentMusicIndex = index;
    }

    /// <summary>
    /// 모든 배경음악 정지
    /// </summary>
    public void StopAllBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
        currentMusicIndex = -1;
    }

    #endregion

    #region ===== 플레이어 기본 액션 사운드 메서드들 =====

    public void PlayPlayerMove()
    {
        PlaySound(playerMove);
    }

    public void PlayPlayerJump()
    {
        PlaySound(playerJump);
    }

    public void PlayPlayerLand()
    {
        PlaySound(playerLand);
    }

    public void PlayPlayerHit()
    {
        PlaySound(playerHit);
    }

    public void PlayPlayerDeath()
    {
        PlaySound(playerDeath);
    }

    #endregion

    #region ===== 플레이어 공격 사운드 메서드들 =====

    public void PlayPlayerBasicAttack()
    {
        PlaySound(playerBasicAttack);
    }

    public void PlayPlayerDropAttack()
    {
        PlaySound(playerDropAttack);
    }

    public void PlayPlayerDropImpact()
    {
        PlaySound(playerDropImpact);
    }

    public void PlayPlayerDodgeAttack()
    {
        PlaySound(playerDodgeAttack);
    }

    /// <summary>
    /// AttackName에 따른 플레이어 공격 사운드 재생
    /// </summary>
    public void PlayPlayerAttackByType(AttackName attackType)
    {
        switch (attackType)
        {
            case AttackName.Player_BasicAttack:
                PlayPlayerBasicAttack();
                break;
            case AttackName.Player_JumpComboAttack:
                PlayPlayerBasicAttack(); // 점프 공격도 기본 공격 소리 사용
                break;
            case AttackName.Player_DodgeComboAttack:
                PlayPlayerDodgeAttack();
                break;
            case AttackName.Player_WhenDodge:
                PlayPlayerDodgeAttack();
                break;
        }
    }

    #endregion

    #region ===== 몬스터 사운드 메서드들 =====

    // 공통 몬스터 사운드
    public void PlayMonsterSpawn()
    {
        PlaySound(monsterSpawn);
    }

    public void PlayMonsterHit()
    {
        PlaySound(monsterHit);
    }

    // 몬스터별 공격 사운드
    public void PlayArcherFire()
    {
        PlaySound(archerFire);
    }

    public void PlayMageSpell()
    {
        PlaySound(mageSpell);
    }

    public void PlayMinionAttack()
    {
        PlaySound(minionAttack);
    }

    public void PlayShielderCharge()
    {
        PlaySound(shielderCharge);
    }

    /// <summary>
    /// AttackName에 따른 몬스터 공격 사운드 재생
    /// </summary>
    public void PlayMonsterAttackByType(AttackName attackType)
    {
        switch (attackType)
        {
            case AttackName.Monster_ArcherFireAttack:
                PlayArcherFire();
                break;
            case AttackName.Monster_MageSpellAttack:
                PlayMageSpell();
                break;
            case AttackName.Monster_MinionNormalAttack:
                PlayMinionAttack();
                break;
            case AttackName.Monster_ShieldChargeAttack:
                PlayShielderCharge();
                break;
        }
    }

    #endregion

    #region ===== 큐브 시스템 사운드 메서드들 =====

    public void PlayCubeMoveStart()
    {
        PlaySound(cubeMoveStart);
    }

    public void PlayCubeMoveLoop()
    {
        PlayLoopSound(cubeMoveLoop);
    }

    public void StopCubeMoveLoop()
    {
        StopLoopSound();
    }

    public void PlayCubeMoveEnd()
    {
        PlaySound(cubeMoveEnd);
    }

    public void PlayCubeCollapseWarning()
    {
        PlaySound(cubeCollapseWarning);
    }

    public void PlayCubeCollapseShake()
    {
        PlayLoopSound(cubeCollapseShake);
    }

    public void StopCubeCollapseShake()
    {
        StopLoopSound();
    }

    public void PlayCubeCollapseFall()
    {
        PlaySound(cubeCollapseFall);
    }

    #endregion

    #region ===== 멧돼지 시스템 사운드 메서드들 =====

    public void PlayBoarWarning()
    {
        PlaySound(boarWarning);
    }

    public void PlayBoarCharge()
    {
        PlaySound(boarCharge);
    }

    public void PlayBoarCrash()
    {
        PlaySound(boarCrash);
    }

    #endregion

    #region ===== UI 사운드 메서드들 =====

    public void PlayUIClick()
    {
        PlaySound(uiClick);
    }

    public void PlayUIDialog()
    {
        PlaySound(uiDialog);
    }

    public void PlayVolumeSlider()
    {
        PlaySound(volumeSlider);
    }

    #endregion

    #region ===== 헬퍼 메서드들 =====

    /// <summary>
    /// 일반 사운드 재생 (SoundClip 사용)
    /// </summary>
    private void PlaySound(SoundClip soundClip)
    {
        if (effectSource != null && soundClip != null && soundClip.clip != null)
        {
            float finalVolume = soundClip.volume * currentEffectVolume;
            effectSource.PlayOneShot(soundClip.clip, finalVolume);
        }
    }

    /// <summary>
    /// 루프 사운드 재생 (SoundClip 사용)
    /// </summary>
    private void PlayLoopSound(SoundClip soundClip)
    {
        if (loopSource != null && soundClip != null && soundClip.clip != null)
        {
            if (loopSource.isPlaying)
                loopSource.Stop();

            loopSource.clip = soundClip.clip;
            loopSource.volume = soundClip.volume * currentEffectVolume;
            loopSource.Play();
        }
    }

    /// <summary>
    /// 루프 사운드 정지
    /// </summary>
    private void StopLoopSound()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    #endregion

    #region ===== 공개 유틸리티 메서드들 =====

    /// <summary>
    /// 현재 음악 볼륨 반환
    /// </summary>
    public float GetMusicVolume()
    {
        return currentMusicVolume;
    }

    /// <summary>
    /// 현재 효과음 볼륨 반환
    /// </summary>
    public float GetEffectVolume()
    {
        return currentEffectVolume;
    }

    /// <summary>
    /// 특정 AudioSource가 재생 중인지 확인
    /// </summary>
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    public bool IsEffectPlaying()
    {
        return effectSource != null && effectSource.isPlaying;
    }

    public bool IsLoopPlaying()
    {
        return loopSource != null && loopSource.isPlaying;
    }

    /// <summary>
    /// 모든 사운드 일시 정지
    /// </summary>
    public void PauseAllSounds()
    {
        AudioListener.pause = true;
    }

    /// <summary>
    /// 모든 사운드 재개
    /// </summary>
    public void ResumeAllSounds()
    {
        AudioListener.pause = false;
    }

    #endregion
}