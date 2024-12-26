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
            ("JaLoader", "Leaxx", "2.0.1")
        };

        public override bool UseAssets => true;

        private Texture2D roadsSnowTexture;
        private Texture2D trabbiSnowTexture;
        private Texture2D treeBillboardSnowTexture;
        private Texture2D turkishBush;

        private Color colorDiff = new Color(0.15f, 0.15f, 0.2f);

        public override void EventsDeclaration()
        {
            base.EventsDeclaration();
            EventsManager.Instance.OnRouteGenerated += OnRouteGenerated;
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

            turkishBush = PNGToTexture("TurkishBush");
            turkishBush.name = "TurkishBush";
        }

        public override void Start()
        {
            base.Start();

            ChangeAtmosphere();
            ChangeRainToSnow();
            StartCoroutine(DelayChange());
        }

        public void OnRouteGenerated(string a, string b, int c)
        {
            if (!gameObject.activeSelf)
                return;

            StartCoroutine(DelayChange());
        }

        private IEnumerator DelayChange()
        {
            yield return new WaitForSeconds(3f);
            ChangeRoadTextures();
        }

        private void ChangeRoadTextures()
        {
            foreach(var obj in FindObjectsOfType<Renderer>())
            {
                if ((obj.name.Contains("Road") || obj.name.Contains("road")) && !obj.name.Contains("Road_Tyre"))
                {
                    if (obj.material?.shader?.name?.Contains("Height") == true)
                    {
                        obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                        obj.material.color = new Color32(144, 144, 144, 255);
                    }

                    obj.material.mainTexture = roadsSnowTexture;
                }
             
                if (obj.transform.parent != null)
                {
                    if (obj.transform.parent.name.Contains("HUB_Pavement"))
                    {
                        if (obj.name.Contains("Plane_"))
                            obj.material.mainTexture = trabbiSnowTexture;
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

                    if(obj.transform.parent.name.Contains("HollowBuilding_European_Corner_03"))
                        if (obj.name.Contains("Plane_"))
                            obj.material.mainTexture = roadsSnowTexture;

                    if(obj.transform.parent.name.Contains("Motel"))
                        if(obj.name.Contains("Cube_1005"))
                            obj.material.mainTexture = roadsSnowTexture;

                    if(obj.transform.parent.name.Contains("LaikaBuilding"))
                        if(obj.name.Contains("Cube_1015"))
                            obj.material.mainTexture = roadsSnowTexture;

                    if(obj.name.Contains("Icosphere"))
                        obj.material.mainTexture = trabbiSnowTexture;

                    if (obj.transform.parent.name == "Tree" || obj.transform.parent.name.Contains("Group_") || obj.transform.parent.name.Contains("Fern") || obj.transform.parent.name.Contains("Tree_WhiteBirch") || obj.transform.parent.name.Contains("Bush") || obj.transform.parent.name.Contains("Road_"))
                        if (obj.name.Contains("SpruceTree"))
                        {
                            if (obj.material.shader?.name.Contains("Height") == true)
                                obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");

                            obj.material.color = new Color32(144, 144, 144, 255);
                            obj.material.mainTexture = roadsSnowTexture;
                        }
                        else if (obj.name.Contains("Tree"))
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

                            if(obj.name != "TurkishBush")
                            {
                                obj.material.mainTexture = roadsSnowTexture;
                                obj.material.color = new Color32(144, 144, 144, 255);
                            }
                            else
                                obj.material.mainTexture = turkishBush;
                        }

                    if (obj.transform.parent.name.Contains("Tree"))
                        if (obj.name.Contains("PineTree") || obj.name.Contains("HUN_Tree") || obj.name.Contains("Yugo")|| obj.name.Contains("Bul_Tree") || (obj.name.Contains("Cylinder") && (obj.transform.parent.name.Contains("Bul_Tree") || obj.transform.parent.name.Contains("Yugo") || obj.transform.parent.name.Contains("YUGO"))))
                        {
                            if(obj.material.shader != null)
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
                        if(obj.material.mainTexture.name == "Trabbi_Julia2")
                            obj.material.mainTexture = trabbiSnowTexture;
                    }

                    if (obj.name.Contains("grass") || obj.name.Contains("Grass"))
                    {
                        if(obj.material.shader.name.Contains("Height"))
                        {
                            obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                            obj.material.color = new Color32(170, 170, 170, 255);
                        }
                        obj.material.mainTexture = roadsSnowTexture;
                    }

                    if (obj.transform.parent.parent != null)
                    {
                        if(obj.transform.parent.name == "GameObject")
                            if (obj.name == "Plane_2940" && obj.transform.parent.parent.name.Contains("HUN_Letenye"))
                            {
                                if (obj.material.shader.name.Contains("Height"))
                                {
                                    obj.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                                    obj.material.color = new Color32(170, 170, 170, 255);
                                }

                                obj.material.mainTexture = roadsSnowTexture;
                            }
                            if(obj.transform.parent.parent.name.Contains("Hub_02"))
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
                }

                if(obj.name.Contains("pavementcurb2"))
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
        }

        private void GeneralChanges(Renderer obj)
        {
            var matsToCheck = obj.materials;
            Material finalToCheck = null;
            Material secondMaterial = null;

            if (matsToCheck != null && matsToCheck.Length > 0)
            {
                finalToCheck = matsToCheck[0];

                if (matsToCheck.Length > 1)
                    secondMaterial = matsToCheck[1];
            }

            if (secondMaterial?.mainTexture?.name == "TreeBillboard_02")
                matsToCheck[1].mainTexture = treeBillboardSnowTexture;

            if (finalToCheck?.mainTexture?.name == "road_Julia_2" || finalToCheck.mainTexture?.name == "Trabbi_Julia2")
            {
                matsToCheck[0].mainTexture = matsToCheck[0].mainTexture.name == "Trabbi_Julia2" ? trabbiSnowTexture : roadsSnowTexture;
                obj.materials = matsToCheck;
            }

            // additional check because some objects have reverse order of materials

            if (finalToCheck?.mainTexture?.name == "TreeBillboard_02")
                matsToCheck[0].mainTexture = treeBillboardSnowTexture;

            if (secondMaterial?.mainTexture?.name == "road_Julia_2" || secondMaterial?.mainTexture?.name == "Trabbi_Julia2")
            {
                matsToCheck[1].mainTexture = matsToCheck[1].mainTexture.name == "Trabbi_Julia2" ? trabbiSnowTexture : roadsSnowTexture;
                obj.materials = matsToCheck;
            }

            if (finalToCheck?.shader.name.Contains("HeightShader") == true)
            {
                matsToCheck[0].shader = Shader.Find("Legacy Shaders/Diffuse");
                matsToCheck[0].color = new Color32(170, 170, 170, 255);
                obj.materials = matsToCheck;
            }
            else if(secondMaterial?.shader.name.Contains("HeightShader") == true)
            {
                matsToCheck[1].shader = Shader.Find("Legacy Shaders/Diffuse");
                matsToCheck[1].color = new Color32(170, 170, 170, 255);
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
    }
}
