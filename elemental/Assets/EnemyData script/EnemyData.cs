using System.Collections.Generic;
using UnityEngine;

// 敵の行動の種類
public enum EnemyActionType { Attack, Defend, AddStatusCard }

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public EnemyActionType actionType;
    public int value; // ダメージやブロックの数値
    public CardData statusCard; // AddStatusCardの時に付与するカード

    [Header("AI条件設定")]
    [Tooltip("チェックを入れると、敵のHPが50%以下の時しか使ってこない大技になります")]
    public bool isPhase2Only = false; 

    [Tooltip("チェックを入れると、戦闘中に1回しか使ってこなくなります（バフや特殊行動など）")]
    public bool isOneTimeOnly = false;
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("敵のステータス")]
    public string enemyName;
    public int maxHP;
    public Sprite enemyImage;

    [Header("行動リスト")]
    public List<EnemyAction> actionList = new List<EnemyAction>();
    public int spawnCount = 1;
}