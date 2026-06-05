using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounter", menuName = "CardGame/EncounterData")]
public class EncounterData : ScriptableObject
{
    [Header("バトル名（例：スライム3兄弟、ゴブリンコンビなど）")]
    public string encounterName;

    [Header("このバトルに出現する敵のリスト（1〜3体）")]
    public List<EnemyData> enemies;
}