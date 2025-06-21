using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Collections.Generic;

namespace AdvancedMinimapColors;

public class PatchColour
{
    // Set colors from ModSettings values
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
    
    private static Color PuckIconColor => new Color(Plugin.modSettings.puckIconRedValue,
                                                    Plugin.modSettings.puckIconGreenValue,
                                                    Plugin.modSettings.puckIconBlueValue);

    // Cache FieldInfo for performance
    public static FieldInfo _playerBodyVisualElementMapField; // Changed to public static
    public static FieldInfo _puckVisualElementMapField;       // Changed to public static

    // Static constructor to initialize FieldInfo once when this class is first loaded
    static PatchColour() // The static constructor should be for this class (PatchColour)
    {
        _playerBodyVisualElementMapField = AccessTools.Field(typeof(UIMinimap), "playerBodyVisualElementMap");
        _puckVisualElementMapField = AccessTools.Field(typeof(UIMinimap), "puckVisualElementMap");

        if (_playerBodyVisualElementMapField == null)
            Plugin.LogError("Failed to find 'playerBodyVisualElementMap' field in UIMinimap. Player colors might not apply.");
        if (_puckVisualElementMapField == null)
            Plugin.LogError("Failed to find 'puckVisualElementMap' field in UIMinimap. Puck color might not apply.");
    }
    
    [HarmonyPatch(typeof(UIMinimap), "Update")]
    public class PatchMinimapUpdate
    {
        public static void Postfix(UIMinimap __instance)
        {
            if (__instance == null) return;

            // Player Body Coloring 
            if (PatchColour._playerBodyVisualElementMapField != null)
            {
                Dictionary<PlayerBodyV2, VisualElement> playerBodyVisualElementMap =
                    (Dictionary<PlayerBodyV2, VisualElement>)PatchColour._playerBodyVisualElementMapField.GetValue(__instance);

                if (playerBodyVisualElementMap != null)
                {
                    foreach (KeyValuePair<PlayerBodyV2, VisualElement> entry in playerBodyVisualElementMap)
                    {
                        PlayerBodyV2 playerBody = entry.Key;
                        VisualElement bodyVisualElement = entry.Value;

                        if (playerBody == null) continue;
                        Player player = playerBody.Player;
                        if (player == null) continue;

                        VisualElement visualElement1 =
                            (VisualElement)bodyVisualElement.Query<VisualElement>("Body", (string)null);
                        Label label = (Label)bodyVisualElement.Query<Label>("Number", (string)null);

                        if (visualElement1 == null || label == null) continue;

                        bool isLocalPlayer = player.IsLocalPlayer;
                        PlayerTeam playerTeam = player.Team.Value;

                        if (isLocalPlayer)
                        {
                            if (playerTeam == PlayerTeam.Red)
                            {
                                visualElement1.style.unityBackgroundImageTintColor =
                                    new StyleColor(PatchColour.LocalPlayerRedTeamBodyColor);
                                label.style.color = new StyleColor(PatchColour.RedTeamText);
                            }
                            else if (playerTeam == PlayerTeam.Blue)
                            {
                                visualElement1.style.unityBackgroundImageTintColor =
                                    new StyleColor(PatchColour.LocalPlayerBlueTeamBodyColor);
                                label.style.color = new StyleColor(PatchColour.BlueTeamText);
                            }
                        }
                        else
                        {
                            if (playerTeam == PlayerTeam.Red)
                            {
                                visualElement1.style.unityBackgroundImageTintColor =
                                    new StyleColor(PatchColour.NonLocalRedTeamBodyColor);
                                label.style.color = new StyleColor(PatchColour.RedTeamText);
                            }
                            else if (playerTeam == PlayerTeam.Blue)
                            {
                                visualElement1.style.unityBackgroundImageTintColor =
                                    new StyleColor(PatchColour.NonLocalBlueTeamBodyColor);
                                label.style.color = new StyleColor(PatchColour.BlueTeamText);
                            }
                        }
                    }
                }
            }
            
            // Puck Coloring
            if (PatchColour._puckVisualElementMapField != null)
            {
                Dictionary<Puck, VisualElement> puckVisualElementMap =
                    (Dictionary<Puck, VisualElement>)PatchColour._puckVisualElementMapField.GetValue(__instance);

                if (puckVisualElementMap != null)
                {
                    foreach (KeyValuePair<Puck, VisualElement> entry in puckVisualElementMap)
                    {
                        Puck puck = entry.Key;
                        VisualElement minimapPuckRootElement = entry.Value; // This is the "MinimapPuck" root

                        if (puck == null || minimapPuckRootElement == null) continue;

                        VisualElement actualPuckVisual = (VisualElement)minimapPuckRootElement.Query<VisualElement>("Puck", (string)null);

                        if (actualPuckVisual != null)
                        {
                            actualPuckVisual.style.unityBackgroundImageTintColor = new StyleColor(PatchColour.PuckIconColor);
                        }
                        else
                        {
                            Plugin.LogError($"Puck visual element 'Puck' child not found for {puck.name}. Cannot apply color.");
                        }
                    }
                }
            }
        }
    }
}
