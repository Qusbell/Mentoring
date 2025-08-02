using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 큐브 액션 게임의 모든 사운드를 관리하는 싱글톤 매니저
/// Player, Monster, Cube, Boar 등 모든 시스템의 사운드 통합 관리
/// </summary>
public class SoundManager : SingletonT<SoundManager>
{
    [Header("=== 배경음악 ===")]
    public AudioSource[] backgroundMusicSources; // 배경 음악들 (스테이지별)

    [Header("=== 플레이어 기본 액션 사운드 ===")]
    public AudioSource playerMoveSound;           // 플레이어 이동 소리
    public AudioSource playerJumpSound;           // 플레이어 점프 소리
    public AudioSource playerLandSound;           // 플레이어 착지 소리
    public AudioSource playerHitSound;            // 플레이어 피격 소리
    public AudioSource playerDeathSound;          // 플레이어 사망 소리

    [Header("=== 플레이어 공격 사운드 ===")]
    public AudioSource playerBasicAttackSound;    // 기본 공격
    public AudioSource playerDropAttackSound;     // 낙하 공격 시전 소리
    public AudioSource playerDropImpactSound;     // 낙하 공격 착지 임팩트 소리
    public AudioSource playerDodgeAttackSound;    // 닷지 공격

    [Header("=== 몬스터 공통 사운드 ===")]
    public AudioSource monsterSpawnSound;         // 몬스터 스폰 소리 (공통)
    public AudioSource monsterHitSound;           // 몬스터 피격 소리 (공통)

    [Header("=== 몬스터별 공격 사운드 ===")]
    public AudioSource minionAttackSound;         // 미니언 일반 공격
    public AudioSource archerFireSound;           // 궁수 화살 발사
    public AudioSource mageSpellSound;            // 마법사 주문 시전
    public AudioSource shielderChargeSound;       // 방패병 돌진 공격

    [Header("=== 큐브 시스템 사운드 ===")]
    public AudioSource cubeMoveStartSound;        // 큐브 이동 시작 소리
    public AudioSource cubeMoveLoopSound;         // 큐브 이동 중 소리
    public AudioSource cubeMoveEndSound;          // 큐브 이동 도착 소리
    public AudioSource cubeCollapseWarningSound;  // 큐브 붕괴 경고 소리
    public AudioSource cubeCollapseShakeSound;    // 큐브 흔들림 소리
    public AudioSource cubeCollapseFallSound;     // 큐브 붕괴 떨어짐 소리

    [Header("=== 멧돼지 시스템 사운드 ===")]
    public AudioSource boarWarningSound;          // 멧돼지 경고 소리
    public AudioSource boarChargeSound;           // 멧돼지 돌진 소리
    public AudioSource boarCrashSound;            // 멧돼지 충돌 소리

    [Header("=== UI 및 시스템 사운드 ===")]
    public AudioSource uiClickSound;              // UI 클릭 소리
    public AudioSource uiDialogSound;             // UI 대화 출력 소리
    public AudioSource volumeSliderSound;         // 사운드 조절 소리

    // PlayerPrefs 키 상수
    private const string MusicVolumeKey = "MusicVolume";
    private const string EffectVolumeKey = "EffectVolume";

    // 현재 볼륨 값들
    private float currentMusicVolume = 1.0f;
    private float currentEffectVolume = 1.0f;

    #region ===== 초기화 =====

    protected void Awake()
    {
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
        if (backgroundMusicSources != null && backgroundMusicSources.Length > 0)
        {
            PlayBackgroundMusic(0);
        }
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    private void StopAllEffectSounds()
    {
        // 플레이어 기본 액션 사운드 정지
        SafeStop(playerMoveSound);
        SafeStop(playerJumpSound);
        SafeStop(playerLandSound);
        SafeStop(playerHitSound);
        SafeStop(playerDeathSound);

        // 플레이어 공격 사운드 정지
        SafeStop(playerBasicAttackSound);
        SafeStop(playerDropAttackSound);
        SafeStop(playerDropImpactSound);
        SafeStop(playerDodgeAttackSound);

        // 몬스터 사운드 정지
        SafeStop(monsterSpawnSound);
        SafeStop(minionAttackSound);
        SafeStop(monsterHitSound);
        SafeStop(archerFireSound);
        SafeStop(mageSpellSound);
        SafeStop(shielderChargeSound);

        // 큐브 시스템 사운드 정지
        SafeStop(cubeMoveStartSound);
        SafeStop(cubeMoveLoopSound);
        SafeStop(cubeMoveEndSound);
        SafeStop(cubeCollapseWarningSound);
        SafeStop(cubeCollapseShakeSound);
        SafeStop(cubeCollapseFallSound);

        // 멧돼지 시스템 사운드 정지
        SafeStop(boarWarningSound);
        SafeStop(boarChargeSound);
        SafeStop(boarCrashSound);

        // UI 사운드 정지
        SafeStop(uiClickSound);
        SafeStop(uiDialogSound);
        SafeStop(volumeSliderSound);
    }

    /// <summary>
    /// 안전한 AudioSource 정지
    /// </summary>
    private void SafeStop(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    #endregion

    #region ===== 볼륨 관리 =====

    /// <summary>
    /// 음악 볼륨 설정
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = volume;

        if (backgroundMusicSources != null)
        {
            foreach (var source in backgroundMusicSources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }
        }

        SaveVolume(MusicVolumeKey, volume);
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetEffectVolume(float volume)
    {
        currentEffectVolume = volume;

        // 모든 효과음 AudioSource에 볼륨 적용
        SetVolumeForSource(playerMoveSound, volume);
        SetVolumeForSource(playerJumpSound, volume);
        SetVolumeForSource(playerLandSound, volume);
        SetVolumeForSource(playerHitSound, volume);
        SetVolumeForSource(playerDeathSound, volume);

        SetVolumeForSource(playerBasicAttackSound, volume);
        SetVolumeForSource(playerDropAttackSound, volume);
        SetVolumeForSource(playerDropImpactSound, volume);
        SetVolumeForSource(playerDodgeAttackSound, volume);

        SetVolumeForSource(monsterSpawnSound, volume);
        SetVolumeForSource(monsterHitSound, volume);
        SetVolumeForSource(minionAttackSound, volume);
        SetVolumeForSource(archerFireSound, volume);
        SetVolumeForSource(mageSpellSound, volume);
        SetVolumeForSource(shielderChargeSound, volume);

        SetVolumeForSource(cubeMoveStartSound, volume);
        SetVolumeForSource(cubeMoveLoopSound, volume);
        SetVolumeForSource(cubeMoveEndSound, volume);
        SetVolumeForSource(cubeCollapseWarningSound, volume);
        SetVolumeForSource(cubeCollapseShakeSound, volume);
        SetVolumeForSource(cubeCollapseFallSound, volume);

        SetVolumeForSource(boarWarningSound, volume);
        SetVolumeForSource(boarChargeSound, volume);
        SetVolumeForSource(boarCrashSound, volume);

        SetVolumeForSource(uiClickSound, volume);
        SetVolumeForSource(uiDialogSound, volume);
        SetVolumeForSource(volumeSliderSound, volume);

        SaveVolume(EffectVolumeKey, volume);
    }

    /// <summary>
    /// 개별 AudioSource 볼륨 설정
    /// </summary>
    private void SetVolumeForSource(AudioSource source, float volume)
    {
        if (source != null)
        {
            source.volume = volume;
        }
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
        if (backgroundMusicSources == null || index < 0 || index >= backgroundMusicSources.Length)
            return;

        // 모든 배경음악 정지
        foreach (AudioSource source in backgroundMusicSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
                source.time = 0;
                source.loop = false;
            }
        }

        // 선택한 배경음악 재생
        if (backgroundMusicSources[index] != null)
        {
            backgroundMusicSources[index].loop = true;
            backgroundMusicSources[index].Play();
        }
    }

    /// <summary>
    /// 모든 배경음악 정지
    /// </summary>
    public void StopAllBackgroundMusic()
    {
        if (backgroundMusicSources == null) return;

        foreach (AudioSource source in backgroundMusicSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    #endregion

    #region ===== 플레이어 기본 액션 사운드 메서드들 =====

    public void PlayPlayerMove()
    {
        PlaySound(playerMoveSound);
    }

    public void PlayPlayerJump()
    {
        PlaySound(playerJumpSound);
    }

    public void PlayPlayerLand()
    {
        PlaySound(playerLandSound);
    }

    public void PlayPlayerHit()
    {
        PlaySound(playerHitSound);
    }

    public void PlayPlayerDeath()
    {
        PlaySound(playerDeathSound);
    }

    #endregion

    #region ===== 플레이어 공격 사운드 메서드들 =====

    public void PlayPlayerBasicAttack()
    {
        PlaySound(playerBasicAttackSound);
    }

    public void PlayPlayerDropAttack()
    {
        PlaySound(playerDropAttackSound);
    }

    public void PlayPlayerDropImpact()
    {
        PlaySound(playerDropImpactSound);
    }

    public void PlayPlayerDodgeAttack()
    {
        PlaySound(playerDodgeAttackSound);
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
        PlaySound(monsterSpawnSound);
    }

    public void PlayMonsterHit()
    {
        PlaySound(monsterHitSound);
    }

    // 몬스터별 공격 사운드
    public void PlayArcherFire()
    {
        PlaySound(archerFireSound);
    }

    public void PlayMageSpell()
    {
        PlaySound(mageSpellSound);
    }

    public void PlayMinionAttack()
    {
        PlaySound(minionAttackSound);
    }

    public void PlayShielderCharge()
    {
        PlaySound(shielderChargeSound);
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
        PlaySound(cubeMoveStartSound);
    }

    public void PlayCubeMoveLoop()
    {
        PlayLoopSound(cubeMoveLoopSound);
    }

    public void StopCubeMoveLoop()
    {
        StopSound(cubeMoveLoopSound);
    }

    public void PlayCubeMoveEnd()
    {
        PlaySound(cubeMoveEndSound);
    }

    public void PlayCubeCollapseWarning()
    {
        PlaySound(cubeCollapseWarningSound);
    }

    public void PlayCubeCollapseShake()
    {
        PlayLoopSound(cubeCollapseShakeSound);
    }

    public void StopCubeCollapseShake()
    {
        StopSound(cubeCollapseShakeSound);
    }

    public void PlayCubeCollapseFall()
    {
        PlaySound(cubeCollapseFallSound);
    }

    #endregion

    #region ===== 멧돼지 시스템 사운드 메서드들 =====

    public void PlayBoarWarning()
    {
        PlaySound(boarWarningSound);
    }

    public void PlayBoarCharge()
    {
        PlaySound(boarChargeSound);
    }

    public void PlayBoarCrash()
    {
        PlaySound(boarCrashSound);
    }

    #endregion

    #region ===== UI 사운드 메서드들 =====

    public void PlayUIClick()
    {
        PlaySound(uiClickSound);
    }

    public void PlayUIDialog()
    {
        PlaySound(uiDialogSound);
    }

    public void PlayVolumeSlider()
    {
        PlaySound(volumeSliderSound);
    }

    #endregion

    #region ===== 헬퍼 메서드들 =====

    /// <summary>
    /// 일반 사운드 재생
    /// </summary>
    private void PlaySound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.Play();
        }
    }

    /// <summary>
    /// 루프 사운드 재생
    /// </summary>
    private void PlayLoopSound(AudioSource source)
    {
        if (source != null && source.clip != null && !source.isPlaying)
        {
            source.loop = true;
            source.Play();
        }
    }

    /// <summary>
    /// 사운드 정지
    /// </summary>
    private void StopSound(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
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
    public bool IsPlaying(AudioSource source)
    {
        return source != null && source.isPlaying;
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