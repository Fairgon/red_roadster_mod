using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using ShipMaker.Data;
using System.Linq;

namespace RedRoadster
{
    [BepInPlugin("redroadster.starvalor.mod", "Red Roadster Mod", "2.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Settings _settings = new Settings();

        private readonly string configName = "RedRoadsterMod.json";
        private readonly string[] languages = new string[] { "english", "portuguese", "german", "spanish", "french", 
                                                             "russian", "chinese", "vietnamese", "korean", "korean",
                                                             "polish", "italian" };

        public const string pluginGuid = "redroadster.starvalor.mod";
        public const string pluginName = "Red Roadster";
        public const string pluginVersion = "2.0.0";

        private string localizationFolder = "Localization";

        private static string ShipFolder = "";

        private static List<MShipData> Bundles = new List<MShipData>();
        private static List<LocalizationData> Localization = new List<LocalizationData>();

        private bool saveBonuses = false;
        private string path = "";

        private bool windowState = false;

        public void Awake()
        {
            ShipFolder = BepInEx.Paths.PluginPath + "/Ships Data";
            path = BepInEx.Paths.BepInExConfigPath.Replace("BepInEx.cfg", "") + configName;

            LoadLoaclizationData();
            LoadConfig();

            Harmony.CreateAndPatchAll(typeof(Main), (string)null);

            if(saveBonuses)
                SaveShipBonuses();
        }

        private void LoadConfig()
        {
            string settingsData = "";

            if (!File.Exists(path))
            {
                SaveConfig();

                return;
            }

            settingsData = File.ReadAllText(path);

            _settings = JsonUtility.FromJson<Settings>(settingsData);
        }

        private void SaveConfig()
        {
            string settingsData = "";

            settingsData = JsonUtility.ToJson(_settings, true);

            File.WriteAllText(path, settingsData);
        }

        private static void SaveShipBonuses()
        {
            BonusesData data = new BonusesData();

            data.Names = new List<string>(new List<string>(Resources.LoadAll<ShipBonus>("ShipBonus/").Select(x => x.name)));

            string stringData = JsonUtility.ToJson(data, true);

            File.WriteAllText(BepInEx.Paths.BepInExRootPath + "/ShipBonuses.json", stringData);
        }

        private void LoadLoaclizationData()
        {
            string data = "";

            foreach(string path in Directory.GetFiles(ShipFolder + "/" + localizationFolder))
            {
                data = File.ReadAllText(path);

                Localization.Add(JsonUtility.FromJson<LocalizationData>(data));
            }
        }

        [HarmonyPatch(typeof(Lang), "Get", new[] { typeof(int), typeof(int) })]
        [HarmonyPostfix]
        private static void LangGet_Post(int sectionIndex, int code, ref string __result)
        {
            if (Bundles.Count == 0)
                return;

            if (!Bundles.Select(x => x.Id).Contains(code))
                return;
            
            __result = GetLocalization(code, Lang.GetLanguageName(Lang.current));
        }

        private static string GetLocalization(int id, string language)
        {
            LocalizationData data = Localization.Find(x => x.id == id && x.language == language);

            if (data != null) 
                return data.value;

            data = Localization.Find(x => x.id == id && x.language == "english");

            if (data != null) 
                return data.value;

            Debug.LogWarning("Localization file for " + id + ", not found.");

            return ShipDB.GetModel(id).shipModelName;
        }

        [HarmonyPatch(typeof(ObjManager), "GetShip")]
        [HarmonyPrefix]
        private static void ObjManagerGetShip_Pre(string str, List<ObjBuffer> ___shipList)
        {
            bool contains = false;
            int curId = 0;

            foreach(int id in Bundles.Select(x => x.Id))
            {
                contains = str.Contains(id.ToString());

                if(contains)
                {
                    curId = id;
                    break;
                }
            }

            if (!contains)
                return;

            if (___shipList == null)
                ___shipList = new List<ObjBuffer>();

            
            ObjBuffer objBuffer = new ObjBuffer();
            objBuffer.name = str;

            objBuffer.obj = Bundles.Find(x => x.Id == curId).Ship;

            Transform transform = objBuffer.obj.transform.Find("WeaponSlots");
            
            for (int index = 0; index < transform.childCount; ++index)
            {
                WeaponTurret component = transform.GetChild(index).GetComponent<WeaponTurret>();

                if ((bool)component)
                    component.GetBaseAngles();
            }

            ___shipList.Add(objBuffer);
        }

        private static GameObject CreateShip(List<WeaponTurretData> turrets, ShipModelData data, AssetBundle ab)
        {
            GameObject ship = ab.LoadAsset<GameObject>("ShipBlank");

            ship.AddComponent<ShipModel>().data = data;

            Transform ws = ship.transform.Find("WeaponSlots");
            
            foreach(WeaponTurretData turret in turrets)
            {
                foreach (Transform t in ws.GetComponentsInChildren<Transform>())
                {
                    if (t.name != turret.turretName)
                        continue;

                    WeaponTurret component = t.gameObject.AddComponent<WeaponTurret>();

                    SutupTurret(turret, ref component);
                }
            }

            return ship;
        }

        /// <summary>
        ///  Закидываем корабли в список. 
        /// </summary>
        /// <param name="___market"></param>
        /// <param name="___dockingUI"></param>
        [HarmonyPatch(typeof(ShipDB), "LoadDatabaseForce")]
        [HarmonyPostfix]
        private static void ShipDBLoadDatabaseForce_Post(List<ShipModelData> ___shipModels)
        {
            List<string> files = new List<string>(Directory.GetFiles(ShipFolder));
            files.RemoveAll(x => x.Contains(".manifest"));

            AssetBundle ab;
            TextAsset textAsset;
            MShipModelData data;
            Sprite sprite;
            List<WeaponTurretData> turrets = new List<WeaponTurretData>();

            foreach (string fileName in files)
            {
                ShipModelData shipModelData = new ShipModelData();
                MShipData mShipData = new MShipData();

                turrets.Clear();

                ab = AssetBundle.LoadFromFile(fileName);

                textAsset = ab.LoadAsset<TextAsset>("ShipData");

                data = JsonUtility.FromJson<MShipModelData>(textAsset.text);

                sprite = ab.LoadAsset<Sprite>("Icon");

                shipModelData = CreateShipModelData(data, sprite);

                foreach(TextAsset turretAsset in ab.LoadAllAssets<TextAsset>().ToList().FindAll(x => x.name.Contains("Turret")))
                {
                    turrets.Add(JsonUtility.FromJson<WeaponTurretData>(turretAsset.text));

                    //Debug.Log(turretAsset.text);
                }

                mShipData.Id = data.id;
                mShipData.Ship = CreateShip(turrets, shipModelData, ab);
                mShipData.Data = ab;

                shipModelData.weaponSlotsGO = mShipData.Ship.transform.Find("WeaponSlots");

                List<ShipBonus> bonuses = CreateShipBonuses(data);

                if (bonuses.Count > 0)
                {
                    shipModelData.modelBonus = new ShipBonus[bonuses.Count];

                    bonuses.CopyTo(shipModelData.modelBonus);
                }
                

                Bundles.Add(mShipData);

                ___shipModels.Add(shipModelData);
            }
        }

        private static List<ShipBonus> CreateShipBonuses(MShipModelData data)
        {
            List<ShipBonus> list = new List<ShipBonus>();

            foreach (string name in data.modelBonus)
            {
                list.Add(Resources.Load<ShipBonus>("ShipBonus/" + name));
            }

            list.RemoveAll(x => x == null);

            return list;
        }

        private static void SutupTurret(WeaponTurretData data, ref WeaponTurret turret)
        {
            turret.active = true;
            turret.turretIndex = data.turretIndex;
            turret.type = data.type;
            turret.degreesLimit = data.degreesLimit;
            turret.turnSpeed = data.turnSpeed;
            turret.spriteName = data.spriteName;
            turret.spinalMount = data.spinalMount;
            turret.totalSpace = data.totalSpace;
            turret.maxInstalledWeapons = data.maxInstalledWeapons;
            turret.iconTranslate = data.iconTranslate;

            turret.turretMode = data.turretMode;

            turret.manned = data.manned;
            turret.alternateFire = data.alternateFire;
            turret.hasSpecialStats = data.hasSpecialStats;

            turret.baseWeaponMods = data.baseWeaponMods;

            List<Transform> turnAlong = new List<Transform>();
            List<Transform> extraBarrels = new List<Transform>();

            if(data.extraGuns.Length > 0)
            {
                Transform t;
                Transform model = turret.transform.parent.parent.Find("Model");

                Transform battery = model.Find(data.extraGunsParent);

                for (int index = 0; index < battery.childCount; ++index)
                {
                    t = battery.GetChild(index);

                    if (data.extraGuns.Contains(t.name))
                    {
                        turnAlong.Add(t);
                    }
                }

                turret.turnAlong = new Transform[turnAlong.Count];
                turnAlong.CopyTo(turret.turnAlong);
            }
            
            turnAlong.Add(turret.transform);

            foreach(Transform t in turnAlong)
            {
                foreach(Transform gunTip in t.GetComponentsInChildren<Transform>())
                {
                    if (gunTip.name.Contains("GunTip_"))
                        extraBarrels.Add(gunTip);
                }
            }

            if(extraBarrels.Count != 0)
            {
                turret.extraBarrels = new Transform[extraBarrels.Count];
                extraBarrels.CopyTo(turret.extraBarrels);
            }
        }

        private static ShipModelData CreateShipModelData(MShipModelData data, Sprite icon)
        {
            ShipModelData shipModelData = new ShipModelData()
            {
                id = data.id,
                shipModelName = data.shipModelName,
                manufacturer = data.manufacturer,
                shipClass = data.shipClass,
                shipRole = data.shipRole,
                sellChance = data.sellChance,
                level = data.level,
                hullPoints = data.hullPoints,

                weaponSpace = data.weaponSpace,
                equipSpace = data.equipSpace,
                cargoSpace = data.cargoSpace,

                hangarDroneSpace = data.hangarDroneSpace,
                hangarShipSpace = data.hangarShipSpace,

                crewSpace = data.crewSpace,
                passengers = data.passengers,

                speed = data.speed,
                agility = data.agility,
                mass = data.mass,
                sizeScale = data.sizeScale,
                sortPower = data.sortPower,

                image = icon,

                drawScale = data.drawScale,
                rarity = data.rarity,

                repReq = data.repReq,

                factions = data.factions,

                craftingMaterials = data.craftingMaterials,

                extraSurFXScale = data.extraSurFXScale
            };

            return shipModelData;
        }


        [HarmonyPatch(typeof(Market), "OpenMarket")]
        [HarmonyPostfix]
        private static void MarketOpenMarket_Post(ref List<MarketItem> ___market, ref DockingUI ___dockingUI)
        {
            if (!_settings.ShowNewShipsInStation)
                return;

            MarketItem item;

            List<MShipModelData> models = new List<MShipModelData>(Bundles.Select(x => JsonUtility.FromJson<MShipModelData>(x.Data.LoadAsset<TextAsset>("ShipData").text)));

            foreach (MShipModelData entire in models)
            {
                if (___market.Find(x => (x.itemType == 4 && x.itemID == entire.id)) != null)
                    continue;

                item = new MarketItem(4, entire.id, entire.rarity, entire.rarity <= 1 ? Random.Range(1, 4) : 1, (CI_Data)null);
                item.lastStockCount = item.stock;

                ___market.Add(item);
            }
        }

        public void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                windowState = true;

            if (!windowState)
                return;

            GUILayout.Window(0, new Rect((Screen.width / 2) - 150, 10, 300, 90), MainWindow, "Red Roadster Mod");
        }

        public void MainWindow(int windowId)
        {
            _settings.ShowNewShipsInStation = GUILayout.Toggle(_settings.ShowNewShipsInStation, "Show New Ships");

            if (GUILayout.Button("Upload Ship Bonuses"))
                SaveShipBonuses();

            if (GUILayout.Button("Save"))
                SaveConfig();

            if (GUILayout.Button("Close"))
                windowState = false;
        }
    }
}
