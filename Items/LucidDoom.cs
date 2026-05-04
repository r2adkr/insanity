using GameNetcodeStuff;
using InsanityMod.Managers;
using Unity.Netcode;

namespace InsanityMod.Items
{
    public class LucidDoom : GrabbableObject
    {
        public override void ItemActivate(bool used, bool buttonDown = false)
        {
            base.ItemActivate(used, buttonDown);

            if (!buttonDown) return;
            if (!IsOwner) return;

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null) return;

            VFXManager.ClearEffect();

            int damage = player.health - 1;
            if (damage > 0)
                player.DamagePlayer(damage, hasDamageSFX: false, callRPC: true,
                    causeOfDeath: CauseOfDeath.Unknown);

            CoronerCompat.MarkLucidDoomUser(player);

            HUDManager.Instance.DisplayTip(
                LocalizationManager.Get("item.potion.name"),
                LocalizationManager.Get("item.potion.use"),
                isWarning: false);

            ConsumeServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ConsumeServerRpc()
        {
            NetworkObject.Despawn(true);
        }
    }
}
