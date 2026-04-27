using System.Collections.Generic;
using UnityEngine;

// 敵の行動の種類を増やす
public enum EnemyActionType { Attack, Defend, AddStatusCard }

[System.Serializable]
public class EnemyAction
{
    public string actionName;
    public EnemyActionType actionType;
    public int value; // ダメージやブロックの数値
    public CardData statusCard; // ★追加：AddStatusCardの時に付与するカード
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("敵のステータス")]
    public string enemyName;
    public int maxHP;
    public Sprite enemyImage;

    [Header("行動リスト (ランダムで選ばれます)")]
    public List<EnemyAction> actionList = new List<EnemyAction>();
}