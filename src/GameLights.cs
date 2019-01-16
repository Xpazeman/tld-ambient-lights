using UnityEngine;
using Harmony;
using System;
using System.Collections.Generic;

namespace AmbientLights
{
    class GameLights
    {
        public static GameObject gameLights = new GameObject();

        public static List<Light> gameLightsList = new List<Light>();
        public static List<Light> gameSpotLightsList = new List<Light>();
        public static List<Light> gameExtraLightsList = new List<Light>();
        public static List<Color> gameExtraLightsColors = new List<Color>();

        public static TodAmbientLight gameAmbientLight = null;

        public static void AddGameLights(InteriorLightingManager mngr)
        {
            GameLights.gameLights = new GameObject();

            //Window Lights
            List<InteriorLightingGroup> lightGroups = Traverse.Create(mngr).Field("m_LightGroupList").GetValue<List<InteriorLightingGroup>>();

            foreach (InteriorLightingGroup group in lightGroups)
            {
                List<Light> lights = group.GetLights();

                foreach (Light light in lights)
                {
                    GameObject lightMark;

                    if (light.type == LightType.Point)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        (lightMark.GetComponent(typeof(SphereCollider)) as Collider).enabled = false;
                        gameLightsList.Add(light);
                    }
                    else if (light.type == LightType.Spot)
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        lightMark.transform.rotation = light.gameObject.transform.rotation;

                        gameSpotLightsList.Add(light);
                    }
                    else
                    {
                        lightMark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    }

                    lightMark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    lightMark.transform.position = light.gameObject.transform.position;

                    lightMark.transform.parent = gameLights.transform;

                    foreach (Renderer rend in lightMark.GetComponentsInChildren<Renderer>())
                    {
                        rend.material.color = new Color(1f, 0, 0);
                        rend.receiveShadows = false;
                    }
                }
            }

            //Fill Lights
            List<Light> looseLights = Traverse.Create(mngr).Field("m_LooseLightList").GetValue<List<Light>>();
            List<Light> looseLightsMidday = Traverse.Create(mngr).Field("m_LooseLightsMiddayList").GetValue<List<Light>>();

            List<Light> extraLights = new List<Light>();

            if (looseLights != null)
                looseLights.ForEach(l => extraLights.Add(l));

            if (looseLightsMidday != null)
                looseLightsMidday.ForEach(l => extraLights.Add(l));

            gameExtraLightsList = extraLights;

            foreach (Light light in extraLights)
            {
                GameObject eLightMark;

                eLightMark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                (eLightMark.GetComponent(typeof(SphereCollider)) as Collider).enabled = false;
                eLightMark.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                eLightMark.transform.position = light.gameObject.transform.position;

                eLightMark.transform.parent = gameLights.transform;

                foreach (Renderer rend in eLightMark.GetComponentsInChildren<Renderer>())
                {
                    Color rendColor = light.color;
                    rendColor.a = light.intensity;
                    rend.material.color = light.color;
                    rend.receiveShadows = false;
                }

                gameExtraLightsColors.Add(light.color);
            }

            if (!AmbientLights.showGameLights)
            {
                gameLights.SetActive(false);
            }

            //Main Ambient Light
            gameAmbientLight = Traverse.Create(mngr).Field("m_AmbientLight").GetValue<TodAmbientLight>();

            AmbientLights.SetupGameLights();
        }

        public static void UpdateLights()
        {
            if (AmbientLights.lightOverride)
                return;

            foreach (Light sLight in gameSpotLightsList)
            {
                if (!AmbientLights.enableGameLights)
                    sLight.intensity = 0f;
            }

            int eIndex = 0;
            foreach (Light eLight in gameExtraLightsList)
            {
                if (!AmbientLights.enableGameLights)
                {
                    eLight.intensity = 0;

                }
                else
                {

                    ColorHSV lColor = gameExtraLightsColors[eIndex];
                    lColor.s *= AmbientLights.options.fillColorLevel;
                    eLight.color = lColor;

                    eLight.intensity *= AmbientLights.options.fillLevel;

                    eIndex++;
                }
            }

            if (gameAmbientLight != null && !AmbientLights.enableGameLights)
            {
                gameAmbientLight.SetAmbientLightValue(0, 0);
            }
        }

        public static void UpdateAmbience(TodAmbientLight TodLightInstance, ref float multiplier)
        {
            if (AmbientLights.lightOverride)
                return;
            
            multiplier *= AmbientLights.options.ambienceLevel;

            UniStormWeatherSystem uniStorm = GameManager.GetUniStorm();
            TODStateConfig state = uniStorm.GetActiveTODState();

            Color bColor = state.m_FogColor;

            bColor = AmbientLights.config.ApplyWeatherMod(bColor);

            ColorHSV fColor = bColor;
            fColor.s *= 0.5f;
            fColor.v = 0.1f;

            TodLightInstance.m_AmbientIndoorsDay = fColor;
        }
    }
}
