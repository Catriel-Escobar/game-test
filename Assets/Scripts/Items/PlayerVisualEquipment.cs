using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualEquipment : MonoBehaviour
{
    private static readonly Dictionary<EquipmentSlot, string[]> PiecesBySlot =
        new Dictionary<EquipmentSlot, string[]>
        {
            { EquipmentSlot.Helmet, new[] { "Casco T1", "CascoPlate T2" } },
            { EquipmentSlot.Chest, new[] { "Remera T1", "ArmaduraPlate T2" } },
            { EquipmentSlot.Gloves, new[] { "Guantes T1", "GuantePlate T2" } },
            { EquipmentSlot.Boots, new[] { "Botas T1", "BotasPlate T2" } },
            { EquipmentSlot.Cape, new[] { "Capa T2" } },
            { EquipmentSlot.Weapon, new[] { "Espada T1", "EspadaPlate T2" } },
            { EquipmentSlot.OffHand, new[] { "EscudoPlate T2" } }
        };

    private PlayerEquipment _equipment;
    private readonly Dictionary<string, GameObject> _pieces = new Dictionary<string, GameObject>();

    public void Initialize(PlayerEquipment equipment)
    {
        _equipment = equipment;
        CachePieces();
    }

    private void CachePieces()
    {
        _pieces.Clear();
        Transform[] allTransforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allTransforms.Length; i++)
        {
            if (!_pieces.ContainsKey(allTransforms[i].name))
                _pieces[allTransforms[i].name] = allTransforms[i].gameObject;
        }
    }

    public void RefreshAll()
    {
        var slotValues = new List<EquipmentSlot>(PiecesBySlot.Keys);
        for (int i = 0; i < slotValues.Count; i++)
        {
            EquipmentSlot slot = slotValues[i];
            Item equipped = _equipment.GetItemInSlot(slot);
            if (equipped != null && !string.IsNullOrEmpty(equipped.visualKey))
                ApplySlot(slot);
            else
                ClearSlot(slot);
        }
    }

    public void ApplySlot(EquipmentSlot slot)
    {
        if (!PiecesBySlot.TryGetValue(slot, out string[] candidates)) return;

        Item equipped = _equipment.GetItemInSlot(slot);
        string visualKey = equipped != null ? equipped.visualKey : "";

        for (int i = 0; i < candidates.Length; i++)
            SetPieceActive(candidates[i], candidates[i] == visualKey);
    }

    public void ClearSlot(EquipmentSlot slot)
    {
        if (!PiecesBySlot.TryGetValue(slot, out string[] candidates)) return;

        for (int i = 0; i < candidates.Length; i++)
            SetPieceActive(candidates[i], false);
    }

    private void SetPieceActive(string pieceName, bool active)
    {
        if (string.IsNullOrEmpty(pieceName)) return;

        if (_pieces.TryGetValue(pieceName, out GameObject piece))
        {
            if (piece != null && piece.activeSelf != active)
                piece.SetActive(active);
        }
    }
}
