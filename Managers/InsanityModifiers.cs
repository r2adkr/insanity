using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

namespace InsanityMod.Managers
{
    internal static class InsanityModifiers
    {
        private static readonly Dictionary<string, float> EnemyRates = new()
        {
            { "Flowerman",        2.0f },
            { "Crawler",          0.8f },
            { "Spring",           1.5f },
            { "Hoarding bug",     0.1f },
            { "Centipede",        0.6f },
            { "SandSpider",       0.9f },
            { "BushWolf",         1.2f },
            { "ForestGiant",      1.4f },
            { "DressGirl",        2.0f },
            { "Jester",           1.5f },
            { "Blob",             0.3f },
            { "Puffer",           0.4f },
            { "MaskedPlayerEnemy",1.6f },
            { "Maneater",         1.4f },
            { "BaboonHawk",       0.5f },
            { "RedLocustBees",    0.2f },
            { "DocileLocustBees", 0.0f },
            { "DoublewingBird",   0.0f },
            { "SandWorm",         1.8f },
            { "Tulip Snake",      0.3f },
            { "Butler",           1.4f },
            { "Butler Bees",      0.2f },
            { "ClaySurgeon",      1.3f },
        };

        private const float DefaultEnemyRate = 0.3f;

        public static float ComputeBonusRate(PlayerControllerB local)
        {
            if (local == null) return 0f;

            float rate = 0f;
            rate += MobVisibilityBonus(local);
            rate -= ProximityBuff(local);
            rate -= LightBuff(local);
            return rate;
        }

        public static float MobVisibilityBonus(PlayerControllerB local)
        {
            var camera = local.gameplayCamera;
            if (camera == null) return 0f;

            var rm = RoundManager.Instance;
            if (rm == null || rm.SpawnedEnemies == null) return 0f;

            float scale     = ModConfig.MobVisibilityScale.Value;
            float maxRange  = ModConfig.MobVisibilityRange.Value;
            float maxRangeSq = maxRange * maxRange;
            Vector3 camPos   = camera.transform.position;
            Vector3 camFwd   = camera.transform.forward;
            float halfFov    = camera.fieldOfView * 0.5f + 5f;

            float total = 0f;
            for (int i = 0; i < rm.SpawnedEnemies.Count; i++)
            {
                var enemy = rm.SpawnedEnemies[i];
                if (enemy == null || enemy.isEnemyDead || enemy.enemyType == null) continue;

                Vector3 toEnemy = enemy.transform.position - camPos;
                float distSq = toEnemy.sqrMagnitude;
                if (distSq > maxRangeSq || distSq < 0.01f) continue;

                float dist  = Mathf.Sqrt(distSq);
                Vector3 dir = toEnemy / dist;
                if (Vector3.Angle(camFwd, dir) > halfFov) continue;

                if (Physics.Linecast(camPos, enemy.transform.position, out RaycastHit hit,
                                     ~0, QueryTriggerInteraction.Ignore))
                {
                    if (!hit.collider.transform.IsChildOf(enemy.transform)) continue;
                }

                float perEnemy = GetRateForEnemy(enemy);
                total += perEnemy;
            }
            return total * scale;
        }

        public static float ProximityBuff(PlayerControllerB local)
        {
            var allPlayers = StartOfRound.Instance?.allPlayerScripts;
            if (allPlayers == null) return 0f;

            float range   = ModConfig.TeammateBuffRange.Value;
            float rangeSq = range * range;
            Vector3 pos   = local.transform.position;

            for (int i = 0; i < allPlayers.Length; i++)
            {
                var p = allPlayers[i];
                if (p == null || p == local) continue;
                if (p.isPlayerDead || !p.isPlayerControlled) continue;
                if ((p.transform.position - pos).sqrMagnitude <= rangeSq)
                    return ModConfig.TeammateBuffRate.Value;
            }
            return 0f;
        }

        public static float LightBuff(PlayerControllerB local)
        {
            if (local.isInHangarShipRoom) return ModConfig.LightBuffRate.Value;
            if (local.helmetLight != null && local.helmetLight.enabled) return ModConfig.LightBuffRate.Value;

            if (local.ItemSlots != null)
            {
                foreach (var item in local.ItemSlots)
                {
                    if (item is FlashlightItem fl && fl.isBeingUsed) return ModConfig.LightBuffRate.Value;
                }
            }
            return 0f;
        }

        public static bool IsPositionVisible(PlayerControllerB local, Vector3 worldPos, float maxRange)
        {
            var camera = local.gameplayCamera;
            if (camera == null) return false;

            Vector3 toTarget = worldPos - camera.transform.position;
            float distSq = toTarget.sqrMagnitude;
            if (distSq > maxRange * maxRange || distSq < 0.01f) return false;

            float dist  = Mathf.Sqrt(distSq);
            Vector3 dir = toTarget / dist;
            if (Vector3.Angle(camera.transform.forward, dir) > camera.fieldOfView * 0.5f + 5f) return false;

            if (Physics.Linecast(camera.transform.position, worldPos, out RaycastHit hit,
                                 ~0, QueryTriggerInteraction.Ignore))
            {
                if ((hit.point - worldPos).sqrMagnitude > 1f) return false;
            }
            return true;
        }

        private static float GetRateForEnemy(EnemyAI enemy)
        {
            string name = enemy.enemyType?.enemyName ?? "";
            if (EnemyRates.TryGetValue(name, out float rate)) return rate;
            return DefaultEnemyRate;
        }
    }
}
