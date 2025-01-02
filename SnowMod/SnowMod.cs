using JaLoader;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Console = JaLoader.Console;
using HarmonyLib;
using UnityEngine.Collections;
using UnityEngine.Assertions.Must;

namespace SnowMod
{
    public class SnowMod : Mod
    {
        public override string ModID => "SnowMod";
        public override string ModName => "Snow Mod";
        public override string ModAuthor => "Leaxx";
        public override string ModDescription => "Makes the game feel cold.";
        public override string ModVersion => "1.0";
        public override string GitHubLink => "https://github.com/Jalopy-Mods/SnowMod";
        public override WhenToInit WhenToInit => WhenToInit.InGame;
        public override List<(string, string, string)> Dependencies => new List<(string, string, string)>()
        {
            ("JaLoader", "Leaxx", "1.0.0")
        };

        public override bool UseAssets => true;

        private Texture2D roadsSnowTexture;
        private Texture2D trabbiSnowTexture;

        private Texture2D treeBillboardSnowTexture;
        private Texture2D treeBillboardSnowTexture2;
        private Texture2D treeBillboardSnowTexture3;

        private Texture2D turkishBush;
        private Texture2D pineTreeTexture;

        private Texture2D dirtTextureTyres;
        private Texture2D dirtTextureCutout;
        private Texture2D dirtTextureDirty;
        private Texture2D dirtTextureFade;
        private Texture2D dirtTextureWSCleaned;
        private Texture2D dirtTextureWSSmudge;

        private Color colorDiff = new Color(0.15f, 0.15f, 0.2f);

        private static Harmony harmony;
        private bool patched;

        private List<WheelScriptPCC> alreadyChanged = new List<WheelScriptPCC>();

        public static SnowMod instance;

        private bool changingTextures = false;

        public override void EventsDeclaration()
        {
            base.EventsDeclaration();
            EventsManager.Instance.OnRouteGenerated += OnRouteGenerated;

            if(harmony == null)
            {
                harmony = new Harmony("Leaxx.SnowMod.Mod");
                Patch();
            }

            instance = this;
        }

        private void Patch()
        {
            if (patched == true)
                return;

            patched = true;

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override void SettingsDeclaration()
        {
            base.SettingsDeclaration();

            InstantiateSettings();

            AddToggle("EnableReducedGrip", "Reduced grip", false);
            AddToggle("ReplaceDirtWithSnow", "Replace dirt on the car with snow", false);
        }

        public override void Awake()
        {
            base.Awake();

            roadsSnowTexture = PNGToTexture("Roads");
            roadsSnowTexture.name = "RoadsSnowTexture";

            trabbiSnowTexture = PNGToTexture("Trabbi");
            trabbiSnowTexture.name = "TrabbiSnowTexture";

            treeBillboardSnowTexture = PNGToTexture("BillboardTrees");
            treeBillboardSnowTexture.name = "TreeBillboardSnowTexture";

            treeBillboardSnowTexture2 = PNGToTexture("BillboardTrees_02");
            treeBillboardSnowTexture2.name = "TreeBillboardSnowTexture2";

            treeBillboardSnowTexture3 = PNGToTexture("BillboardTrees_03");
            treeBillboardSnowTexture3.name = "TreeBillboardSnowTexture3";

            turkishBush = PNGToTexture("TurkishBush");
            turkishBush.name = "TurkishBush";

            pineTreeTexture = PNGToTexture("PineTree");
            pineTreeTexture.name = "PineTreeSnow";

            dirtTextureTyres = PNGToTexture("DirtTextureTyres");
            dirtTextureTyres.name = "DirtTextureTyres";

            dirtTextureCutout = PNGToTexture("DirtTextureCutout");
            dirtTextureCutout.name = "DirtTextureCutout";

            dirtTextureDirty = PNGToTexture("DirtTextureDirty");
            dirtTextureDirty.name = "DirtTextureDirty";

            dirtTextureFade = PNGToTexture("DirtTextureFade");
            dirtTextureFade.name = "DirtTextureFade";

            dirtTextureWSCleaned = PNGToTexture("DirtTextureWindscreen_Cleaned");
            dirtTextureWSCleaned.name = "DirtTextureWSCleaned";

            dirtTextureWSSmudge = PNGToTexture("DirtTextureWindscreen_Smudge");
            dirtTextureWSSmudge.name = "DirtTextureWSSmudge";
        }

        public override void Start()
        {
            base.Start();

            ChangeAtmosphere();
            ChangeRainToSnow();
            StartCoroutine(DelayChange(true));

            if (GetToggle("EnableReducedGrip"))
                ChangeGrip();

            if (GetToggle("ReplaceDirtWithSnow"))
                ReplaceDirtWithSnow();
        }

        public void OnRouteGenerated(string a, string b, int c)
        {
            Console.LogDebug("SnowMod", "Route generated, changing textures");

            if (!gameObject.activeSelf)
                return;

            StartCoroutine(DelayChange(false));
        }

        private IEnumerator DelayChange(bool initial)
        {
            var valueToWait = initial ? 0.5f : 3f;

            yield return new WaitForSeconds(valueToWait);
            ChangeRoadTextures();
        }

        private void ReplaceDirtWithSnow()
        {
            foreach (var transform in FindObjectsOfType<MeshRenderer>())
            {
                if (transform.name == "Dirt")
                {
                    switch (transform.material.mainTexture.name)
                    {
                        case "Dirt_Full":
                            transform.material.mainTexture = dirtTextureDirty;
                            break;

                        case "DirtTexture":
                            transform.material.mainTexture = dirtTextureDirty;
                            break;

                        case "WindScreen_Dirty":
                            transform.material.mainTexture = dirtTextureCutout;
                            break;

                        case "Dirt_Tyre":
                            transform.material.mainTexture = dirtTextureTyres;
                            break;

                        case "Dirt_Full_Fade":
                            transform.material.mainTexture = dirtTextureFade;
                            break;

                        case "WindScreen_Cleaned":
                            transform.material.mainTexture = dirtTextureWSCleaned;
                            break;

                        case "WindScreen_Smudge":
                            transform.material.mainTexture = dirtTextureWSSmudge;
                            break;
                    }
                }
            }
        }

        private void ChangeGrip()
        {
            var allWheels = FindObjectOfType<CarLogicC>().wheelObjects;

            alreadyChanged.Clear();

            for (int i = 0; i < allWheels.Length; i++)
                ChangeGripValues(allWheels[i].GetComponent<WheelScriptPCC>(), i);
        }

        private void ChangeRoadTextures()
        {
            if (changingTextures)
                return;

            changingTextures = true;

            foreach (var obj in FindObjectsOfType<Renderer>())
            {
                try
                {
                    if ((obj.name.Contains("Road") || obj.name.Contains("road")) && !obj.name.Contains("Road_Tyre"))
                    {
                        if (obj.material?.shader?.name?.Contains("Height") == true)
                            obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");

                        obj.material.color = new Color32(170, 170, 170, 255);

                        if (obj.materials.Count() > 1 && obj.materials[1] != null)
                            ChangeBillboardTexture(obj.sharedMaterials[1]);

                        obj.material.mainTexture = roadsSnowTexture;
                    }

                    if (obj.transform.parent != null)
                    {
                        if (obj.transform.parent.name.Contains("HUB_Pavement"))
                        {
                            if (obj.name.Contains("Plane_"))
                            {
                                obj.material.mainTexture = trabbiSnowTexture;
                                obj.material.color = new Color32(170, 170, 170, 255);
                            }
                        }

                        if (obj.transform.parent.name.Contains("pavementStraightParking"))
                            if (obj.name.Contains("Plane_"))
                            {
                                if (obj.material?.shader?.name.Contains("Height") == true)
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(170, 170, 170, 255);
                                }

                                obj.material.mainTexture = trabbiSnowTexture;
                            }

                        if (obj.transform.parent.name.Contains("HollowBuilding_European_Corner_03"))
                            if (obj.name.Contains("Plane_"))
                                obj.material.mainTexture = roadsSnowTexture;

                        if (obj.transform.parent.name.Contains("Motel"))
                            if (obj.name.Contains("Cube_1005"))
                                obj.material.mainTexture = roadsSnowTexture;

                        if (obj.transform.parent.name.Contains("LaikaBuilding"))
                            if (obj.name.Contains("Cube_1015"))
                                obj.material.mainTexture = roadsSnowTexture;

                        if (obj.name.Contains("Icosphere"))
                            obj.material.mainTexture = trabbiSnowTexture;

                        if (obj.transform.parent.name == "Tree" || obj.transform.parent.name.Contains("Group_") || obj.transform.parent.name.Contains("Fern") || obj.transform.parent.name.Contains("Tree_WhiteBirch") || obj.transform.parent.name.Contains("Bush") || obj.transform.parent.name.Contains("Road_"))
                            if (obj.name.Contains("SpruceTree"))
                            {
                                if (obj.material.shader?.name.Contains("Height") == true)
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");

                                obj.material.color = new Color32(144, 144, 144, 255);
                                obj.material.mainTexture = roadsSnowTexture;
                            }
                            else if (obj.name.Contains("Tree") && !obj.name.Contains("Bul_"))
                            {
                                if (obj.material.shader.name.Contains("Height"))
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(144, 144, 144, 255);
                                }
                                obj.material.mainTexture = trabbiSnowTexture;
                            }
                            else if (obj.name.Contains("Cylinder_"))
                            {
                                if (obj.material.shader.name.Contains("Height"))
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(144, 144, 144, 255);
                                }

                                obj.material.mainTexture = roadsSnowTexture;
                            }
                            else if (obj.name.Contains("Bush"))
                            {
                                if (obj.material.shader.name.Contains("Height"))
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");

                                if (obj.name != "TurkishBush")
                                {
                                    obj.material.mainTexture = roadsSnowTexture;
                                    obj.material.color = new Color32(144, 144, 144, 255);
                                }
                                else
                                    obj.material.mainTexture = turkishBush;
                            }

                        if (obj.transform.parent.name.Contains("Tree"))
                            if (obj.name.Contains("Yugo") || (obj.name.Contains("Cylinder") && (obj.transform.parent.name.Contains("Yugo") || obj.transform.parent.name.Contains("YUGO"))))
                            {
                                if ((obj.name.Contains("YugoTree") && obj.transform.parent.name.Contains("YugoTree")) || obj.name.Contains("Cylinder_133") || obj.name.Contains("Cylinder_139") || ((obj.name.Contains("Cylinder_461") || obj.name.Contains("Cylinder_455")) && obj.transform.parent.name.Contains("Bul")))
                                    continue;

                                if (obj.material.shader != null)
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                obj.material.color = new Color32(170, 170, 170, 255);
                                obj.material.mainTexture = roadsSnowTexture;
                            }

                        if (obj.name.Contains("sunflower"))
                            obj.material.mainTexture = treeBillboardSnowTexture2;

                        if (obj.name.Contains("PineTree"))
                        {
                            if (obj.material.shader != null)
                                obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                            obj.material.mainTexture = pineTreeTexture;
                        }

                        if (obj.name.Contains("HUN_Tree"))
                        {
                            if (obj.material.shader != null)
                                obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                            obj.material.mainTexture = pineTreeTexture;
                        }

                        if (obj.name.Contains("Bul_Tree"))
                        {
                            if (obj.material.shader != null)
                                obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                            obj.material.mainTexture = roadsSnowTexture;
                        }

                        if (obj.transform.parent.name == "trees")
                        {
                            obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                            obj.material.mainTexture = roadsSnowTexture;
                        }

                        if (obj.transform.parent.name.Contains("ScrapYard"))
                        {
                            if (obj.material.mainTexture?.name == "Trabbi_Julia2")
                                obj.material.mainTexture = trabbiSnowTexture;
                        }

                        if (obj.name.Contains("grass") || obj.name.Contains("Grass"))
                        {
                            if (obj.material.shader.name.Contains("Height"))
                                obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");

                            obj.material.color = new Color32(170, 170, 170, 255);

                            if (obj.materials.Count() > 1 && obj.materials[1] != null)
                                ChangeBillboardTexture(obj.sharedMaterials[1]);

                            obj.material.mainTexture = roadsSnowTexture;
                        }

                        if (obj.transform.parent.parent != null)
                        {
                            if (obj.transform.parent.name == "GameObject")
                                if (obj.name == "Plane_2940" && obj.transform.parent.parent.name.Contains("HUN_Letenye"))
                                {
                                    if (obj.material.shader.name.Contains("Height"))
                                    {
                                        obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                        obj.material.color = new Color32(170, 170, 170, 255);
                                    }

                                    obj.material.mainTexture = roadsSnowTexture;

                                    if (obj.materials.Count() > 1 && obj.materials[1] != null)
                                        ChangeBillboardTexture(obj.sharedMaterials[1]);
                                }
                            if (obj.transform.parent.parent.name.Contains("Hub_02"))
                                GeneralChanges(obj);
                        }

                        if (obj.transform.parent.name.Contains("HUB_") || obj.transform.parent.name.Contains("Hub_") || obj.transform.parent.name.Contains("HUN") || obj.transform.parent.name.Contains("CSFR") || obj.transform.parent.name.Contains("StartingArea") || obj.transform.parent.name.Contains("LaikaStartBuilding") || obj.transform.parent.name.Contains("BUL") || obj.transform.parent.name.Contains("TURK") || obj.transform.parent.name.Contains("Istanbul") || obj.transform.parent.name.Contains("Road_Mixed"))
                            GeneralChanges(obj);

                        if (obj.transform.parent.name.Contains("Road_Hills_"))
                            if (obj.name.Contains("arrier"))
                                obj.material.mainTexture = roadsSnowTexture;
                            else if (obj.name.Contains("road_"))
                            {
                                if (obj.material.shader.name.Contains("Height"))
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(170, 170, 170, 255);
                                }
                            }

                        if (obj.transform.parent.name.Contains("YUGO_"))
                        {
                            if (obj.name.Contains("Plane_"))
                            {
                                if (obj.material?.shader?.name.Contains("Height") == true)
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(170, 170, 170, 255);
                                }

                                obj.material.mainTexture = roadsSnowTexture;
                            }
                        }

                        if (obj.name.Contains("Barrier") || obj.name.Contains("barrier"))
                            obj.material.color = new Color32(170, 170, 170, 255);
                    }

                    if (obj.name.Contains("pavementcurb2"))
                        obj.material.mainTexture = trabbiSnowTexture;

                    if (obj.name.Contains("roundabout_grass_"))
                        obj.materials = ChangeFirstMatTexOfArray(obj.materials, roadsSnowTexture);

                    if (obj.name.Contains("Pavement"))
                    {
                        if (obj.material.shader.name.Contains("Height"))
                        {
                            obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                        }

                        obj.material.mainTexture = roadsSnowTexture;
                    }

                    if (obj.name.Contains("HUN_Hay"))
                        obj.material.mainTexture = roadsSnowTexture;
                }
                catch (Exception ex)
                {
                    Console.LogError("SnowMod", "SnowMod encountered an error while changing textures! Please take a screenshot or copy all of this and report it to the GitHub page.");

                    Console.LogError(ex);
                    Console.LogError($"Faulty object name: {obj.name}");
                    Console.LogError($"Is material null?: {obj.material == null}");
                    Console.LogError($"Is shader null?: {obj.material.shader == null}");
                    Console.LogError($"Is main texture null?: {obj.material.mainTexture == null}");

                    throw;
                }
            }

            Console.LogDebug("SnowMod", "Finished changing textures");
            changingTextures = false;
        }

        private void ChangeBillboardTexture(Material mat)
        {
            bool isBillboard = false;

            switch (mat.mainTexture?.name)
            {
                case "TreeBillboard":
                    mat.mainTexture = treeBillboardSnowTexture;
                    isBillboard = true;
                    break;

                case "TreeBillboard_02":
                    mat.mainTexture = treeBillboardSnowTexture2;
                    isBillboard = true;
                    break;

                case "TreeBillboard_03":
                    mat.mainTexture = treeBillboardSnowTexture3;
                    isBillboard = true;
                    break;
            }

            if (isBillboard)
                mat.color = Color.white;
        }

        private void GeneralChanges(Renderer obj)
        {
            if (obj.name.Contains("Sign") || obj.name.Contains("CrashBarrier"))
                return;

            var matsToCheck = obj.materials;
            Material finalToCheck = null;
            Material secondMaterial = null;

            if (matsToCheck != null && matsToCheck.Length > 0)
            {
                finalToCheck = matsToCheck[0];

                if (matsToCheck.Length > 1)
                    secondMaterial = matsToCheck[1];
            }

            if(secondMaterial != null)
                ChangeBillboardTexture(obj.sharedMaterials[1]);

            if (finalToCheck?.mainTexture?.name == "road_Julia_2" || finalToCheck.mainTexture?.name == "Trabbi_Julia2")
            {
                matsToCheck[0].mainTexture = matsToCheck[0].mainTexture.name == "Trabbi_Julia2" ? trabbiSnowTexture : roadsSnowTexture;
                matsToCheck[0].color = new Color32(170, 170, 170, 255);
                obj.materials = matsToCheck;
            }

            // additional check because some objects have reverse order of materials

            if(finalToCheck != null)
                ChangeBillboardTexture(obj.sharedMaterials[0]);

            if (secondMaterial?.mainTexture?.name == "road_Julia_2" || secondMaterial?.mainTexture?.name == "Trabbi_Julia2")
            {
                matsToCheck[1].mainTexture = matsToCheck[1].mainTexture.name == "Trabbi_Julia2" ? trabbiSnowTexture : roadsSnowTexture;
                matsToCheck[1].color = new Color32(170, 170, 170, 255);
                obj.materials = matsToCheck;
            }

            if (finalToCheck?.shader.name.Contains("HeightShader") == true)
            {
                matsToCheck[0].shader = Shader.Find("Legacy Shaders/Diffuse");
                obj.materials = matsToCheck;
            }
            else if(secondMaterial?.shader.name.Contains("HeightShader") == true)
            {
                matsToCheck[1].shader = Shader.Find("Legacy Shaders/Diffuse");
                obj.materials = matsToCheck;
            }
        }

        private Material[] ChangeFirstMatTexOfArray(Material[] materials, Texture2D newTex)
        {
            materials[0].mainTexture = newTex;

            return materials;
        }

        private void ChangeAtmosphere()
        {
            var dayNightScript = FindObjectOfType<DNC_DayNight>();

            dayNightScript.dayCycleInMinutes = 15;

            foreach (var timeOfDayTransition in dayNightScript.timeOfDayTransitions)
            {
                timeOfDayTransition.ambientLight += colorDiff;
                timeOfDayTransition.skyboxTintColor += colorDiff;
                timeOfDayTransition.fogColor += colorDiff;
                timeOfDayTransition.sunColor += colorDiff;
                timeOfDayTransition.sunTintColor += colorDiff;
                timeOfDayTransition.moonTintColor += colorDiff;
                timeOfDayTransition.auxColor1 += colorDiff;
                timeOfDayTransition.auxColor2 += colorDiff;
            }

            dayNightScript.timeOfDayTransitions[dayNightScript.timeOfDayTransitions.Length - 1].ambientLight -= new Color(0.3f, 0.3f, 0.3f);

            Type scriptType = dayNightScript.GetType();
            FieldInfo fieldInfo = scriptType.GetField("_currentTransition", BindingFlags.NonPublic | BindingFlags.Instance);

            fieldInfo.SetValue(dayNightScript, dayNightScript.timeOfDayTransitions[0]);

            dayNightScript.SendMessage("ApplyCurrentTransition");
        }

        private void ChangeRainToSnow()
        {
            var routeGen = FindObjectOfType<RouteGeneratorC>();

            #region Convert light rain

            var lightRainObj = routeGen.lightRain.transform.GetChild(0);

            var main = lightRainObj.GetComponent<ParticleSystem>().main;
            main.startSizeXMultiplier = 1;
            main.simulationSpeed = 0.75f;
            lightRainObj.GetComponent<ParticleSystemRenderer>().lengthScale = 1;

            lightRainObj.GetComponent<RainLightningC>().enabled = false;
            lightRainObj.GetComponent<AudioSource>().mute = true;

            routeGen.lightRain.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);

            #endregion

            #region Convert heavy rain

            var heavyRainObj = routeGen.heavyRain.transform.GetChild(0);

            main = heavyRainObj.GetComponent<ParticleSystem>().main;
            main.startSizeXMultiplier = 1;
            main.simulationSpeed = 0.75f;
            heavyRainObj.GetComponent<ParticleSystemRenderer>().lengthScale = 1;

            heavyRainObj.GetComponent<RainLightningC>().enabled = false;
            heavyRainObj.GetComponent<AudioSource>().mute = true;

            routeGen.heavyRain.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);

            #endregion
        }

        public void ChangeGripValues(WheelScriptPCC wheelScript, int wheelID)
        {
            if(alreadyChanged.Contains(wheelScript))
                return;

            alreadyChanged.Add(wheelScript);

            if (wheelID == 0 || wheelID == 1)
            {
                wheelScript.extremumSlip += 0.2f;
                wheelScript.asymptoteSlip -= 0.2f;
                wheelScript.brakeForce -= 700f;
                wheelScript.forwardStiffness -= 10;
                wheelScript.sidewaysStiffness -= 1;

                StartCoroutine(ForceExtremumValueChange(wheelScript, 1.18f));
            }
            else
            {
                wheelScript.extremumSlip += 0.2f;
                wheelScript.asymptoteSlip -= 0.2f;
                wheelScript.brakeForce -= 700f;
                wheelScript.forwardStiffness -= 10;
                wheelScript.sidewaysStiffness -= 1;

                StartCoroutine(ForceExtremumValueChange(wheelScript, 0.4f));
            }
        }

        private IEnumerator ForceExtremumValueChange(WheelScriptPCC wheelScript, float difference)
        {
            yield return new WaitForSeconds(5);

            wheelScript.extremumValue -= difference;
        }
    }

    [HarmonyPatch(typeof(CarPerformanceC), "UpdateGrip")]
    public static class CarPerformanceC_UpdateGrip_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(CarPerformanceC __instance, int wheelID)
        {
            if (GameObject.FindObjectOfType<SnowMod>() == null || __instance.myLogic == null || __instance.myLogic.wheelObjects == null)
                return;

            if(GameObject.FindObjectOfType<SnowMod>().GetToggleValue("EnableReducedGrip") == false)
                return;

            var actualWheelID = wheelID - 1;

            if (__instance.myLogic.wheelObjects[actualWheelID].GetComponent<WheelScriptPCC>() == null)
                return;

            WheelScriptPCC wheelScriptCObject = __instance.myLogic.wheelObjects[actualWheelID].GetComponent<WheelScriptPCC>();

            SnowMod.instance.ChangeGripValues(wheelScriptCObject, actualWheelID);
        }
    }
}
