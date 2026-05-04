using GameNetcodeStuff;
using InsanityMod.Managers;
using Unity.Netcode;
using UnityEngine;

namespace InsanityMod.Items
{
    internal class ValueBread : GrabbableObject
    {
        public NetworkVariable<int> StackCount = new NetworkVariable<int>(
            value: 1,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        public override void ItemActivate(bool used, bool buttonDown = false)
        {
            base.ItemActivate(used, buttonDown);

            if (!buttonDown) return;
            if (!IsOwner) return;

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            InsanityManager.AddInsanity(-ModConfig.BreadInsanityReduction.Value);

            float chance = InsanityCalculator.ChokeChance(
                ModConfig.BreadChokeBaseChance.Value,
                ModConfig.BreadChokeStack.Value,
                InsanityManager.GetConsecutiveBreadUses());

            if (Random.value < chance)
            {
                int damage = Mathf.RoundToInt(player.health * 0.2f);
                player.DamagePlayer(damage, hasDamageSFX: true, callRPC: true,
                    causeOfDeath: CauseOfDeath.Suffocation);
                InsanityManager.IncrementBreadUses();
                CoronerCompat.SetChokeCause(player);
                HUDManager.Instance.DisplayTip(
                    LocalizationManager.Get("item.bread.name"),
                    LocalizationManager.Get("item.bread.choke"));
            }
            else
            {
                InsanityManager.ResetBreadUses();
            }

            ConsumeOneServingServerRpc();
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!IsServer) return;
            TryMergeWithExistingStack();
        }

        private void TryMergeWithExistingStack()
        {
            if (!IsServer) return;
            if (playerHeldBy == null) return;

            foreach (GrabbableObject slot in playerHeldBy.ItemSlots)
            {
                if (slot == null || slot == this) continue;
                if (slot is not ValueBread other) continue;
                if (other.StackCount.Value >= 10) continue;

                int combined = StackCount.Value + other.StackCount.Value;
                other.StackCount.Value = Mathf.Min(combined, 10);
                NetworkObject.Despawn(true);
                return;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ConsumeOneServingServerRpc()
        {
            StackCount.Value--;
            if (StackCount.Value <= 0)
                NetworkObject.Despawn(true);
        }
    }
}
