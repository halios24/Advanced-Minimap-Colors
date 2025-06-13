using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace AdvancedMinimapColors;

public class PatchColour
{
    // Define the colors here, using the values from ModSettings
    private static Color LocalPlayerRedTeamBodyColor => new Color(Plugin.modSettings.localPlayerRedTeamRedValue,
        Plugin.modSettings.localPlayerRedTeamGreenValue,
        Plugin.modSettings.localPlayerRedTeamBlueValue);

    private static Color LocalPlayerBlueTeamBodyColor => new Color(Plugin.modSettings.localPlayerBlueTeamRedValue,
        Plugin.modSettings.localPlayerBlueTeamGreenValue,
        Plugin.modSettings.localPlayerBlueTeamBlueValue);

    private static Color NonLocalRedTeamBodyColor => new Color(Plugin.modSettings.nonLocalRedTeamRedValue,
        Plugin.modSettings.nonLocalRedTeamGreenValue,
        Plugin.modSettings.nonLocalRedTeamBlueValue);

    private static Color NonLocalBlueTeamBodyColor => new Color(Plugin.modSettings.nonLocalBlueTeamRedValue,
        Plugin.modSettings.nonLocalBlueTeamGreenValue,
        Plugin.modSettings.nonLocalBlueTeamBlueValue);

    private static Color RedTeamText => new Color(Plugin.modSettings.RedTeamTextRedValue,
        Plugin.modSettings.RedTeamTextGreenValue,
        Plugin.modSettings.RedTeamTextBlueValue);

    private static Color BlueTeamText => new Color(Plugin.modSettings.BlueTeamTextRedValue,
        Plugin.modSettings.BlueTeamTextGreenValue,
        Plugin.modSettings.BlueTeamTextBlueValue);

    [HarmonyPatch(typeof(UIMinimap), nameof(UIMinimap.UpdatePlayerBody))]
    public class PatchMinimapPlayerColor_UpdatePlayerBody
    {
        public static void Postfix(UIMinimap __instance, PlayerBodyV2 playerBody
        )
        {
            if (playerBody == null || __instance == null) return;

            Player player = playerBody.Player;
            if (player == null) return;


            var playerBodyVisualElementMapField =
                AccessTools.Field(typeof(UIMinimap), "playerBodyVisualElementMap");
            if (playerBodyVisualElementMapField == null)
            {
                Plugin.LogError("Could not find 'playerBodyVisualElementMap' field in UIMinimap.");
                return;
            }

            Dictionary<PlayerBodyV2, VisualElement> playerBodyVisualElementMap =
                (Dictionary<PlayerBodyV2, VisualElement>)playerBodyVisualElementMapField.GetValue(__instance);

            if (playerBodyVisualElementMap == null || !playerBodyVisualElementMap.ContainsKey(playerBody))
            {
                // This scenario means the map doesn't contain the playerBody,
                // which the original method also checks for before proceeding.
                return;
            }

            VisualElement bodyVisualElement = playerBodyVisualElementMap[playerBody];
            
            VisualElement visualElement1 =
                (VisualElement)bodyVisualElement.Query<VisualElement>("Body", (string)null);
            Label label = (Label)bodyVisualElement.Query<Label>("Number", (string)null);
            
            if (visualElement1 == null || label == null) return; // Ensure elements exist

            // Determine player and team combination
            bool isLocalPlayer = player.IsLocalPlayer;
            PlayerTeam playerTeam = player.Team.Value;

            // Apply body color based on conditions
            if (isLocalPlayer)
            {
                if (playerTeam == PlayerTeam.Red)
                {
                    visualElement1.style.unityBackgroundImageTintColor =
                        new StyleColor(LocalPlayerRedTeamBodyColor);
                    label.style.color = new StyleColor(RedTeamText);
                }
                else if (playerTeam == PlayerTeam.Blue)
                {
                    visualElement1.style.unityBackgroundImageTintColor =
                        new StyleColor(LocalPlayerBlueTeamBodyColor);
                    label.style.color = new StyleColor(BlueTeamText);
                }
            }
            else // Not a local player
            {
                if (playerTeam == PlayerTeam.Red)
                {
                    visualElement1.style.unityBackgroundImageTintColor = new StyleColor(NonLocalRedTeamBodyColor);
                    label.style.color = new StyleColor(RedTeamText);

                }
                else if (playerTeam == PlayerTeam.Blue)
                {
                    visualElement1.style.unityBackgroundImageTintColor = new StyleColor(NonLocalBlueTeamBodyColor);
                    label.style.color = new StyleColor(BlueTeamText);
                }
            }
        }
    }
}