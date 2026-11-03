using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace NearCameraVisible;

public sealed unsafe class VisibilityKeeper : IDisposable {
    private delegate byte ShouldDrawGameObjectDelegate(CameraBase* camera, GameObject* gameObject, Vector3* sceneCameraPos, Vector3* lookAtVector);

    private readonly Configuration configuration;
    private readonly Hook<ShouldDrawGameObjectDelegate> shouldDrawHook;

    public VisibilityKeeper(Configuration configuration) {
        this.configuration = configuration;
        shouldDrawHook = IGameInteropProvider.Get().HookFromAddress<ShouldDrawGameObjectDelegate>(
            (nint)CameraBase.MemberFunctionPointers.ShouldDrawGameObject, ShouldDrawDetour);
        shouldDrawHook.Enable();
    }

    private byte ShouldDrawDetour(CameraBase* camera, GameObject* gameObject, Vector3* sceneCameraPos, Vector3* lookAtVector) {
        var result = shouldDrawHook.Original(camera, gameObject, sceneCameraPos, lookAtVector);
        if (result != 0 || !ShouldKeepVisible(gameObject))
            return result;

        return 1;
    }

    private bool ShouldKeepVisible(GameObject* gameObject) {
        if (!configuration.Enabled || !ICondition.Get()[ConditionFlag.Mounted])
            return false;

        var player = (Character*)Control.GetLocalPlayer();
        if (player == null || (gameObject != player && gameObject != player->Mount.MountObject))
            return false;

        // In first person the character would block the view, so the game's choice stands.
        var activeCamera = CameraManager.Instance()->GetActiveCamera();
        return activeCamera == null || activeCamera->ZoomMode != CameraZoomMode.FirstPerson;
    }

    public void Dispose()
        => shouldDrawHook.Dispose();
}
