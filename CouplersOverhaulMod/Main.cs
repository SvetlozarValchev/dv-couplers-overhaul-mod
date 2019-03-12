using System;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using UnityEngine;
using System.Reflection.Emit;

namespace CouplersOverhaulMod
{
    public class Main
    {
        static float bufferSign = 1f;
        public static Dictionary<TrainCarType, float[]> bufferDistanceDictionary = new Dictionary<TrainCarType, float[]>();

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnUpdate = OnUpdate;

            Utility.buffers = new List<Transform>();

            ReduceTrainCarBoxSize();
            FillBufferDistances();


            return true;
        }

        static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    for (var i = 0; i < Utility.buffers.Count; i++)
            //    {
            //        var buffers = Utility.buffers[i];

            //        if (!buffers) continue;

            //        buffers.gameObject.SetActive(!buffers.gameObject.activeSelf);
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    bufferSign = bufferSign > 0 ? -1f : 1f;

            //    for (var i = 0; i < Utility.buffers.Count; i++)
            //    {
            //        var buffers = Utility.buffers[i];

            //        if (!buffers) continue;

            //        Transform bufferFL = buffers.Find("FL buffer_square");
            //        Transform bufferFR = buffers.Find("FR buffer_square");
            //        Transform bufferRL = buffers.Find("RL buffer_square");
            //        Transform bufferRR = buffers.Find("RR buffer_square");

            //        if (!bufferFL) bufferFL = buffers.Find("FL_buffer_circle");
            //        if (!bufferFL) bufferFL = buffers.Find("ext buffer_FL");
            //        if (!bufferFR) bufferFR = buffers.Find("FR_buffer_circle");
            //        if (!bufferFR) bufferFR = buffers.Find("ext buffer_FR");
            //        if (!bufferRL) bufferRL = buffers.Find("RL_buffer_circle");
            //        if (!bufferRL) bufferRL = buffers.Find("ext buffer_RL");
            //        if (!bufferRR) bufferRR = buffers.Find("RR_buffer_circle");
            //        if (!bufferRR) bufferRR = buffers.Find("ext buffer_RR");

            //        SetBufferPosition(bufferFL, true);
            //        SetBufferPosition(bufferFR, true);
            //        SetBufferPosition(bufferRL);
            //        SetBufferPosition(bufferRR);
            //    }
            //}
        }

        //static void SetBufferPosition(Transform buffer, bool positive = false)
        //{
        //    if (!buffer) return;

        //    Vector3 position = buffer.localPosition;

        //    if (positive)
        //    {
        //        position.z += 0.2f * bufferSign;
        //    } else
        //    {
        //        position.z -= 0.2f * bufferSign;
        //    }

        //    buffer.localPosition = position;
        //}

        static void ReduceTrainCarBoxSize()
        {
            foreach (TrainCarType carType in (TrainCarType[])Enum.GetValues(typeof(TrainCarType)))
            {
                var carPrefab = CarTypes.GetCarPrefab(carType);

                if (!carPrefab) continue;

                var trainCar = carPrefab.GetComponent<TrainCar>();
                var root = trainCar.transform.Find("[colliders]");
                var collision = root?.Find("[collision]");
                var componentsInChildren = collision.GetComponents<BoxCollider>();

                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    var boxCollider = componentsInChildren[i];
                    var boxColliderSize = boxCollider.size;

                    boxColliderSize.z = boxColliderSize.z - 0.4f;
                    boxCollider.size = boxColliderSize;
                }
            }
        }

        static void FillBufferDistances()
        {
            bufferDistanceDictionary.Add(TrainCarType.PassengerRed, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerGreen, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerBlue, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarRed, new float[] { 0.08f, 0.08f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarPink, new float[] { 0.08f, 0.08f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarGreen, new float[] { 0.08f, 0.08f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarBrown, new float[] { 0.08f, 0.08f });
            bufferDistanceDictionary.Add(TrainCarType.RefrigeratorWhite, new float[] { 0.1f, 0.1f });
        }
    }

    //[HarmonyPatch(typeof(YardTracksOrganizer), "GetSeparationLengthBetweenCars")]
    //class YardTracksOrganizer_GetSeparationLengthBetweenCars_Patch
    //{
    //    static void Postfix(ref float __result, int numOfCars)
    //    {
    //        __result = 0.1f * (float)(numOfCars + 1);
    //    }
    //}

    //[HarmonyPatch(typeof(CarSpawner))]
    //[HarmonyPatch("SpawnCarTypesOnTrack")]
    //public class CarSpawner_SpawnCarTypesOnTrack_Patcher
    //{
    //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //    {
    //        var code = new List<CodeInstruction>(instructions);

    //        for (int i = 0; i < code.Count - 1; i++)
    //        {
    //            if (code[i].opcode == OpCodes.Ldc_R8 && code[i].operand is double f && f >= 0.3 && f < 0.31)
    //            {
    //                code[i].operand = (double)0.1;
    //            }
    //        }

    //        return code;
    //    }
    //}

    //[HarmonyPatch(typeof(CarSpawner))]
    //[HarmonyPatch("GetUninitializedSpawnData")]
    //public class CarSpawner_GetUninitializedSpawnData_Patcher
    //{
    //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //    {
    //        var code = new List<CodeInstruction>(instructions);

    //        for (int i = 0; i < code.Count - 1; i++)
    //        {
    //            if (code[i].opcode == OpCodes.Ldc_R4 && code[i].operand is float f && f >= 0.01f && f < 0.11f)
    //            {
    //                code[i].operand = 0.01f;
    //            }
    //        }

    //        return code;
    //    }
    //}

    [HarmonyPatch(typeof(Coupler), "CoupleTo")]
    class Coupler_CoupleTo_Patch
    {
        static void Postfix(Coupler __instance)
        {
            __instance.GetComponent<CouplerCustom>().Coupled();
        }
    }

    [HarmonyPatch(typeof(Coupler), "Uncouple")]
    class Coupler_Uncouple_Patch
    {
        static void Postfix(Coupler __instance)
        {
            __instance.GetComponent<CouplerCustom>().Uncoupled();
        }
    }

    [HarmonyPatch(typeof(Coupler), "Start")]
    class Coupler_Start_Patch
    {
        static void Prefix(Coupler __instance)
        {
            var position = __instance.transform.localPosition;

            float coef = 0.37f;

            if (__instance.train.carType == TrainCarType.LocoShunter)
            {
                coef = 0.3f;
            }

            if ((__instance.train.carType == TrainCarType.LocoSteamHeavy || __instance.train.carType == TrainCarType.LocoSteamHeavyBlue) &&
                __instance.train.rearCoupler.Equals(__instance))
            {
                coef -= 0.8f;
            }

            if ((__instance.train.carType == TrainCarType.Tender || __instance.train.carType == TrainCarType.TenderBlue) &&
                __instance.train.frontCoupler.Equals(__instance))
            {
                coef -= 0.7f;
            }

            if (__instance.train.frontCoupler.Equals(__instance))
            {
                position.z -= coef;
            }
            else
            {
                position.z += coef;
            }

            __instance.transform.localPosition = position;

            __instance.gameObject.AddComponent<CouplerCustom>();

            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //go.transform.parent = __instance.transform;
            //go.transform.localPosition = Vector3.zero;
            //go.transform.localScale *= 0.25f;

            //if (__instance.train.frontCoupler.Equals(__instance))
            //{
            //    go.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
            //}
        }
    }

    [HarmonyPatch(typeof(Coupler), "CreateJoint")]
    class Coupler_CreateJoint_Patch
    {
        static void Postfix(Coupler __instance, ref ConfigurableJoint __result)
        {
            SoftJointLimit softJointLimit = new SoftJointLimit();

            __result.angularXMotion = ConfigurableJointMotion.Free;
            __result.angularYMotion = ConfigurableJointMotion.Free;
            __result.angularZMotion = ConfigurableJointMotion.Free;

            if (__instance.train.carType == TrainCarType.LocoSteamHeavy && __instance.coupledTo.train.carType == TrainCarType.Tender ||
                __instance.train.carType == TrainCarType.Tender && __instance.coupledTo.train.carType == TrainCarType.LocoSteamHeavy)
            {
                softJointLimit.limit = 0.05f;
            }
            else
            {
                softJointLimit.limit = 0.25f;
            }
            __result.linearLimit = softJointLimit;
            softJointLimit.limit = -90f;
            __result.lowAngularXLimit = softJointLimit;
            softJointLimit.limit = 90f;
            __result.highAngularXLimit = softJointLimit;
            __result.angularYLimit = softJointLimit;
            __result.angularZLimit = softJointLimit;
            __result.enableCollision = false;
            __result.breakForce = 1E+12f;
            //}
        }
    }

    [HarmonyPatch(typeof(DecouplerDeviceLogic))]
    [HarmonyPatch("Couple")]
    public class DecouplerDeviceLogic_Couple_Patcher
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && code[i].operand is float f && f >= 0.99f && f < 1.01f)
                {
                    code[i].operand = 0.3f;
                }
            }

            return code;
        }
    }

    //[HarmonyPatch(typeof(TrainCar), "Start")]
    //class TrainCar_Start_Patch
    //{
    //    static void Postfix(TrainCar __instance)
    //    {
    //        Transform buffers;

    //        if (__instance.carType == TrainCarType.LocoShunter)
    //        {
    //            buffers = __instance.transform.Find("shunter_ext");
    //        }
    //        else
    //        {
    //            buffers = __instance.transform.Find("Buffers");
    //        }

    //        if (buffers != null)
    //        {
    //            Utility.buffers.Add(buffers);
    //        }
    //    }
    //}

    class CouplerCustom : MonoBehaviour
    {
        Coupler coupler;

        Transform bufferFL;
        Transform bufferFR;
        Transform bufferRL;
        Transform bufferRR;

        Vector3 bufferFLPosition;
        Vector3 bufferFRPosition;
        Vector3 bufferRLPosition;
        Vector3 bufferRRPosition;

        bool isCouplerFront;
        CouplerCustom coupledToCustom;

        public GameObject CouplerAnchor { get; private set; }

        float prevDist;

        void Start()
        {
            coupler = GetComponent<Coupler>();

            isCouplerFront = coupler.train.frontCoupler.Equals(coupler);

            SetAnchor();
            FindBuffers();
        }

        void SetAnchor()
        {
            if ((coupler.train.carType == TrainCarType.LocoSteamHeavy || coupler.train.carType == TrainCarType.LocoSteamHeavyBlue) &&
                coupler.train.rearCoupler.Equals(coupler))
            {
                return;
            }
            else if ((coupler.train.carType == TrainCarType.Tender || coupler.train.carType == TrainCarType.LocoSteamHeavyBlue) &&
              coupler.train.frontCoupler.Equals(coupler))
            {
                return;
            }

            var position = Vector3.zero;
            position.z -= 0.125f;

            CouplerAnchor = new GameObject();
            CouplerAnchor.transform.parent = transform;
            CouplerAnchor.transform.localPosition = position;
        }

        void FindBuffers()
        {
            Transform buffers;

            if (coupler.train.carType == TrainCarType.LocoShunter)
            {
                buffers = coupler.train.transform.Find("shunter_ext");
            }
            else
            {
                buffers = coupler.train.transform.Find("Buffers");
            }

            if (!buffers) return;

            bufferFL = buffers.Find("FL buffer_square");
            bufferFR = buffers.Find("FR buffer_square");
            bufferRL = buffers.Find("RL buffer_square");
            bufferRR = buffers.Find("RR buffer_square");

            if (!bufferFL) bufferFL = buffers.Find("FL_buffer_circle");
            if (!bufferFL) bufferFL = buffers.Find("ext buffer_FL");
            if (!bufferFR) bufferFR = buffers.Find("FR_buffer_circle");
            if (!bufferFR) bufferFR = buffers.Find("ext buffer_FR");
            if (!bufferRL) bufferRL = buffers.Find("RL_buffer_circle");
            if (!bufferRL) bufferRL = buffers.Find("ext buffer_RL");
            if (!bufferRR) bufferRR = buffers.Find("RR_buffer_circle");
            if (!bufferRR) bufferRR = buffers.Find("ext buffer_RR");

            if (bufferFL) bufferFLPosition = bufferFL.transform.localPosition;
            if (bufferFR) bufferFRPosition = bufferFR.transform.localPosition;
            if (bufferRL) bufferRLPosition = bufferRL.transform.localPosition;
            if (bufferRR) bufferRRPosition = bufferRR.transform.localPosition;

            if (Main.bufferDistanceDictionary.ContainsKey(coupler.train.carType))
            {
                Vector3 position;
                float[] offset = Main.bufferDistanceDictionary[coupler.train.carType];

                if (bufferFL)
                {
                    position = bufferFL.transform.localPosition;
                    position.z += offset[0];
                    bufferFL.transform.localPosition = position;
                }

                if (bufferFR)
                {
                    position = bufferFR.transform.localPosition;
                    position.z += offset[0];
                    bufferFR.transform.localPosition = position;
                }
                if (bufferRL)
                {
                    position = bufferRL.transform.localPosition;
                    position.z -= offset[1];
                    bufferRL.transform.localPosition = position;
                }

                if (bufferRR)
                {
                    position = bufferRR.transform.localPosition;
                    position.z -= offset[1];
                    bufferRR.transform.localPosition = position;
                }
            }
        }

        public void Coupled()
        {
            coupledToCustom = coupler.coupledTo.GetComponent<CouplerCustom>();
        }

        public void Uncoupled()
        {
            coupledToCustom = null;
        }

        void FixedUpdate()
        {
            if (!coupler) return;
            if (!CouplerAnchor) return;
            if (!coupledToCustom) return;
            if (!coupledToCustom.CouplerAnchor) return;

            float dist = Vector3.Distance(CouplerAnchor.transform.position, coupledToCustom.CouplerAnchor.transform.position);
            float deltaDist = Mathf.Abs(prevDist - dist);
            prevDist = dist;

            float damper = 150000f;
            float coef = 0.4f;
            float diff = Mathf.Clamp(dist, 0f, coef);
            float moveDistance = coef - diff;
            float moveForce = 1f - (diff / coef);
            Vector3 pos;

            if (isCouplerFront)
            {
                if (bufferFL)
                {
                    pos = bufferFLPosition;
                    pos.z -= moveDistance * 0.5f;
                    bufferFL.transform.localPosition = pos;
                }

                if (bufferFR)
                {
                    pos = bufferFRPosition;
                    pos.z -= moveDistance * 0.5f;
                    bufferFR.transform.localPosition = pos;
                }
            }
            else
            {
                if (bufferRL)
                {
                    pos = bufferRLPosition;
                    pos.z += moveDistance * 0.5f;
                    bufferRL.transform.localPosition = pos;
                }

                if (bufferRR)
                {
                    pos = bufferRRPosition;
                    pos.z += moveDistance * 0.5f;
                    bufferRR.transform.localPosition = pos;
                }
            }

            if (moveForce > 0.01f)
            {
                var power = 150000f;
                var totalForce = moveForce * power - deltaDist * damper;

                ApplyBufferForce(totalForce * 0.5f, coupler);
                ApplyBufferForce(totalForce * 0.5f, coupler.coupledTo);
            }
        }

        static void ApplyBufferForce(float force, Coupler coupler)
        {
            var bogies = coupler.train.Bogies;
            var direction = 1f;

            if (coupler.train.frontCoupler.Equals(coupler))
            {
                direction = -1f;
            }

            for (var i = 0; i < bogies.Length; i++)
            {
                bogies[i].ApplyForce(force * direction);
            }
        }
    }

    class Utility
    {
        public static List<Transform> buffers;

        public static void ListChildren(Transform transform, int indent = 0)
        {
            var indentText = "";
            for (var i = 0; i < indent; i++)
            {
                indentText += ' ';
            }

            foreach (Transform child in transform)
            {
                var components = child.GetComponents(typeof(Component));

                for (var i = 0; i < components.Length; i++)
                {
                    Debug.Log(indentText + components[i].name + " " + components[i].tag + " " + components[i].GetType());

                    ListChildren(child, indent + 4);
                }
            }
        }
    }
}