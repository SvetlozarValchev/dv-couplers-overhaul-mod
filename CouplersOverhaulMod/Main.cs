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
        public static Dictionary<TrainCarType, float[]> bufferDistanceDictionary = new Dictionary<TrainCarType, float[]>();
        public static Dictionary<TrainCarType, float> trainCarLength = new Dictionary<TrainCarType, float>();
        public static Dictionary<TrainCarType, float> trainCarCenterOffset = new Dictionary<TrainCarType, float>();

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            FillBufferDistances();
            FillTrainCarLengths();
            FillTrainCarCenterOffsets();
            ReduceTrainCarBoxSize();

            return true;
        }

        static void FillBufferDistances()
        {
            bufferDistanceDictionary.Add(TrainCarType.PassengerRed, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerGreen, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerBlue, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarRed, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarPink, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarGreen, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarBrown, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.RefrigeratorWhite, new float[] { 0.05f, 0.05f });
        }

        static void FillTrainCarLengths()
        {
            trainCarLength.Add(TrainCarType.AutorackRed, 8.95f);
            trainCarLength.Add(TrainCarType.AutorackBlue, 8.95f);
            trainCarLength.Add(TrainCarType.AutorackGreen, 8.95f);
            trainCarLength.Add(TrainCarType.AutorackYellow, 8.95f);

            trainCarLength.Add(TrainCarType.PassengerRed, 12.35f);
            trainCarLength.Add(TrainCarType.PassengerGreen, 12.35f);
            trainCarLength.Add(TrainCarType.PassengerBlue, 12.35f);

            trainCarLength.Add(TrainCarType.TankOrange, 7.22f);
            trainCarLength.Add(TrainCarType.TankWhite, 7.22f);
            trainCarLength.Add(TrainCarType.TankYellow, 7.22f);
            trainCarLength.Add(TrainCarType.TankChrome, 7.22f);

            trainCarLength.Add(TrainCarType.BoxcarBrown, 7.076f);
            trainCarLength.Add(TrainCarType.BoxcarGreen, 7.076f);
            trainCarLength.Add(TrainCarType.BoxcarPink, 7.076f);
            trainCarLength.Add(TrainCarType.BoxcarRed, 7.076f);

            trainCarLength.Add(TrainCarType.HopperBrown, 8.975f);
            trainCarLength.Add(TrainCarType.HopperTeal, 8.975f);
            trainCarLength.Add(TrainCarType.HopperYellow, 8.975f);

            trainCarLength.Add(TrainCarType.FlatbedEmpty, 9.0f);
            trainCarLength.Add(TrainCarType.FlatbedStakes, 9.0f);

            trainCarLength.Add(TrainCarType.RefrigeratorWhite, 7.06f);

            trainCarLength.Add(TrainCarType.LocoShunter, 3.72f);

            //trainCarLength.Add(TrainCarType.Tender, 4f);
            //trainCarLength.Add(TrainCarType.TenderBlue, 4f);
        }

        static void FillTrainCarCenterOffsets()
        {
            trainCarCenterOffset.Add(TrainCarType.LocoShunter, 0.01f);
            trainCarCenterOffset.Add(TrainCarType.RefrigeratorWhite, 0.03f);

            trainCarCenterOffset.Add(TrainCarType.BoxcarBrown, 0.04f);
            trainCarCenterOffset.Add(TrainCarType.BoxcarGreen, 0.04f);
            trainCarCenterOffset.Add(TrainCarType.BoxcarPink, 0.04f);
            trainCarCenterOffset.Add(TrainCarType.BoxcarRed, 0.04f);
        }

        static void ReduceTrainCarBoxSize()
        {
            foreach (TrainCarType carType in (TrainCarType[])Enum.GetValues(typeof(TrainCarType)))
            {
                var carPrefab = CarTypes.GetCarPrefab(carType);

                if (!carPrefab || !trainCarLength.ContainsKey(carType)) continue;

                var trainCar = carPrefab.GetComponent<TrainCar>();
                var root = trainCar.transform.Find("[colliders]");
                var collision = root?.Find("[collision]");
                var componentsInChildren = collision.GetComponents<BoxCollider>();

                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    var boxCollider = componentsInChildren[i];
                    var boxColliderSize = boxCollider.size;
                    var boxColliderCenter = boxCollider.center;

                    boxColliderSize.z = trainCarLength[carType] * 2f;
                    boxCollider.size = boxColliderSize;

                    if (trainCarCenterOffset.ContainsKey(carType))
                    {
                        boxColliderCenter.z += trainCarCenterOffset[carType];
                        boxCollider.center = boxColliderCenter;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(TrainCarColliders), "GetCollisionBounds")]
    class TrainCarColliders_GetCollisionBounds_Patch
    {
        static void Postfix(ref Bounds __result, TrainCar car)
        {
            if (Main.trainCarLength.ContainsKey(car.carType))
            {
                var extents = __result.extents;
                var center = __result.center;

                __result.extents = new Vector3(extents.x, extents.y, Main.trainCarLength[car.carType]);

                if (Main.trainCarCenterOffset.ContainsKey(car.carType))
                {
                    __result.center = new Vector3(center.x, center.y, Main.trainCarCenterOffset[car.carType]);
                }
            }
        }
    }

    //[HarmonyPatch(typeof(YardTracksOrganizer), "GetSeparationLengthBetweenCars")]
    //class YardTracksOrganizer_GetSeparationLengthBetweenCars_Patch
    //{
    //    static void Postfix(ref float __result, int numOfCars)
    //    {
    //        __result = 0.01f * (float)(numOfCars + 1);
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
    //                code[i].operand = (double)0.01f;
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

            if (__instance.train.carType == TrainCarType.LocoSteamHeavy && __instance.coupledTo.train.carType == TrainCarType.Tender ||
                __instance.train.carType == TrainCarType.Tender && __instance.coupledTo.train.carType == TrainCarType.LocoSteamHeavy)
            {
                softJointLimit.limit = 0.05f;
            }
            else
            {
                softJointLimit.limit = 0.25f;
            }

            __result.angularXMotion = ConfigurableJointMotion.Free;
            __result.angularYMotion = ConfigurableJointMotion.Free;
            __result.angularZMotion = ConfigurableJointMotion.Free;
            __result.linearLimit = softJointLimit;
            __result.enableCollision = false;
            __result.breakForce = 1E+08f;
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
                    code[i].operand = 0.4f;
                }
            }

            return code;
        }
    }

    [HarmonyPatch(typeof(DecouplerTextRowDriver))]
    [HarmonyPatch("UpdateDisplay")]
    public class DecouplerTextRowDriver_UpdateDisplay_Patcher
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && code[i].operand is float f && f >= 0.99f && f < 1.01f)
                {
                    code[i].operand = 0.4f;
                }
            }

            return code;
        }
    }

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

        Rigidbody trainRigidbody;
        Rigidbody otherTrainRigidbody;

        void Awake()
        {
            coupler = GetComponent<Coupler>();

            trainRigidbody = coupler.train.GetComponent<Rigidbody>();

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
            otherTrainRigidbody = coupler.coupledTo.train.GetComponent<Rigidbody>();
        }

        public void Uncoupled()
        {
            coupledToCustom = null;
        }

        void FixedUpdate()
        {
            if (!coupler) return;
            if (!coupler.coupledTo) return;
            if (!CouplerAnchor) return;
            if (!coupledToCustom) return;
            if (!coupledToCustom.CouplerAnchor) return;

            float power = 50000f;
            float damper = 15000f;
            float coef = 0.4f;

            float distance = Vector3.Distance(CouplerAnchor.transform.position, coupledToCustom.CouplerAnchor.transform.position);
            float velocity = (trainRigidbody.velocity - otherTrainRigidbody.velocity).magnitude;
            float diff = Mathf.Clamp(distance, 0f, coef);
            float moveDistance = coef - diff;

            if (isCouplerFront)
            {
                MoveBuffer(bufferFL, bufferFLPosition, moveDistance * -0.5f);
                MoveBuffer(bufferFR, bufferFRPosition, moveDistance * -0.5f);
            }
            else
            {
                MoveBuffer(bufferRL, bufferRLPosition, moveDistance * 0.5f);
                MoveBuffer(bufferRR, bufferRRPosition, moveDistance * 0.5f);
            }

            float moveForce = 1f - (diff / coef);

            if (moveForce > 0.01f)
            {
                var totalForce = Mathf.Max(0, moveForce * power - velocity * damper);

                ApplyBufferForce(totalForce * 0.5f, coupler);
                ApplyBufferForce(totalForce * 0.5f, coupler.coupledTo);
            }
        }

        static void MoveBuffer(Transform buffer, Vector3 position, float distance)
        {
            if (!buffer) return;

            Vector3 pos;
            pos = position;
            pos.z += distance;
            buffer.transform.localPosition = pos;
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
}