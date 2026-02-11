using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class GameInventory : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public int quantity;
        public int maxStackSize = 1;
        public Sprite icon;
        public string description;

        public InventoryItem(string id, string name, ItemType type, int qty = 1)
        {
            itemId = id;
            itemName = name;
            itemType = type;
            quantity = qty;
        }
    }

    public enum ItemType
    {
        Weapon,
        Ammo,
        Grenade,
        Healing,
        Shield,
        Tactical,
        Legend,
        Other
    }

    [SerializeField] private int maxInventorySlots = 10;
    [SerializeField] private int weaponSlots = 2;
    
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public InventoryItem[] weaponLoadout;
    private int currentWeaponIndex = 0;
    
    private int shields = 100;
    private int maxShields = 100;
    private int health = 100;
    private int maxHealth = 100;

    public delegate void InventoryChangedDelegate();
    public event InventoryChangedDelegate OnInventoryChanged;

    void Start()
    {
        weaponLoadout = new InventoryItem[weaponSlots];
        InitializeInventory();
    }

    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.E))
        {
            var ezze = new InventoryItem("2", "gode d'elizabeth", ItemType.Weapon, 2);
            AddItem(ezze);
        }
        
        HandleWeaponSwitch();
    }

    private void InitializeInventory()
    {
        inventory.Clear();
        // Ajouter des éléments de démarrage si nécessaire
    }

    /// <summary>
    /// Ajoute un item à l'inventaire
    /// </summary>
    public bool AddItem(InventoryItem item)
    {
        // Vérifier si l'item peut être stacké
        if (item.maxStackSize > 1)
        {
            InventoryItem existingItem = inventory.Find(x => x.itemId == item.itemId);
            if (existingItem != null)
            {
                if (existingItem.quantity + item.quantity <= item.maxStackSize)
                {
                    existingItem.quantity += item.quantity;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // Ajouter comme nouvel item
        if (inventory.Count < maxInventorySlots)
        {
            inventory.Add(item);
            OnInventoryChanged?.Invoke();
            return true;
        }

        Debug.Log("Inventaire plein !");
        return false;
    }

    /// <summary>
    /// Retire un item de l'inventaire
    /// </summary>
    public bool RemoveItem(string itemId, int quantity = 1)
    {
        InventoryItem item = inventory.Find(x => x.itemId == itemId);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
            {
                inventory.Remove(item);
            }
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Équipe une arme
    /// </summary>
    public void EquipWeapon(InventoryItem weapon, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots && weapon.itemType == ItemType.Weapon)
        {
            weaponLoadout[slotIndex] = weapon;
            currentWeaponIndex = slotIndex;
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Bascule entre les armes
    /// </summary>
    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % weaponSlots;
            if (weaponLoadout[currentWeaponIndex] != null)
            {
                Debug.Log($"Arme équipée : {weaponLoadout[currentWeaponIndex].itemName}");
            }
        }
    }

    /// <summary>
    /// Utilise un item (soins, grenade, etc.)
    /// </summary>
    public void UseItem(string itemId)
    {
        InventoryItem item = inventory.Find(x => x.itemId == itemId);
        if (item != null)
        {
            switch (item.itemType)
            {
                case ItemType.Healing:
                    HealPlayer(item.itemName);
                    break;
                case ItemType.Shield:
                    RestoreShields(item.itemName);
                    break;
                case ItemType.Grenade:
                    ThrowGrenade(item.itemName);
                    break;
            }
            RemoveItem(itemId, 1);
        }
    }

    private void HealPlayer(string healType)
    {
        int healAmount = healType.Contains("Médkit") ? 25 : 10;
        health = Mathf.Min(health + healAmount, maxHealth);
        Debug.Log($"Santé restaurée : {health}/{maxHealth}");
    }

    private void RestoreShields(string shieldType)
    {
        int shieldAmount = shieldType.Contains("Batterie") ? 25 : 15;
        shields = Mathf.Min(shields + shieldAmount, maxShields);
        Debug.Log($"Boucliers restaurés : {shields}/{maxShields}");
    }

    private void ThrowGrenade(string grenadeType)
    {
        Debug.Log($"Grenade lancée : {grenadeType}");
        // Logique de lancer une grenade
    }

    public List<InventoryItem> GetInventory() => new List<InventoryItem>(inventory);
    public InventoryItem GetCurrentWeapon() => weaponLoadout[currentWeaponIndex];
    public int GetInventorySlotCount() => inventory.Count;
    public int GetMaxSlots() => maxInventorySlots;
    public int GetHealth() => health;
    public int GetShields() => shields;
    public int GetMaxHealth() => maxHealth;
    public int GetMaxShields() => maxShields;

    [PunRPC]
    void RPC_UseItem(string itemId)
    {
        UseItem(itemId);
    }

    public void UseItemNetworked(string itemId)
    {
        photonView.RPC("RPC_UseItem", RpcTarget.All, itemId);
    }

   
}
