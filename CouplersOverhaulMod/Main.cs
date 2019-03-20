using System;
using System.Collections.Generic;
using System.Reflection;
using UnityModManagerNet;
using Harmony12;
using UnityEngine;
using System.Reflection.Emit;

namespace CouplersOverhaulMod
{
    public struct TrainCarBoundsCorrection
    {
        public float size;
        public float center;

        public TrainCarBoundsCorrection(float size = 0f, float center = 0f)
        {
            this.size = size;
            this.center = center;
        }
    }

    public class Main
    {
        public static Dictionary<TrainCarType, float[]> bufferDistanceDictionary = new Dictionary<TrainCarType, float[]>();
        public static Dictionary<TrainCarType, TrainCarBoundsCorrection> trainCarBounds = new Dictionary<TrainCarType, TrainCarBoundsCorrection>();

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            var harmony = HarmonyInstance.Create(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            FillDictionaries();

            return true;
        }

        static void FillDictionaries()
        {
            bufferDistanceDictionary.Add(TrainCarType.PassengerRed, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerGreen, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.PassengerBlue, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarRed, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarPink, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarGreen, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.BoxcarBrown, new float[] { 0.05f, 0.05f });
            bufferDistanceDictionary.Add(TrainCarType.RefrigeratorWhite, new float[] { 0.05f, 0.05f });

            trainCarBounds.Add(TrainCarType.RefrigeratorWhite, new TrainCarBoundsCorrection(-0.3f, 0f));
        }
    }

    [HarmonyPatch(typeof(TrainCar), "Awake")]
    class TrainCar_Awake_Patch
    {
        static void Prefix(TrainCar __instance)
        {
            var root = __instance.transform.Find("[colliders]");
            var collision = root?.Find("[collision]");

            var componentsInChildren = collision.GetComponents<BoxCollider>();
            var lastCollider = componentsInChildren[componentsInChildren.Length - 1];
            var boxCollider = lastCollider;

            if (Main.trainCarBounds.ContainsKey(__instance.carType))
            {
                var trainCarBounds = Main.trainCarBounds[__instance.carType];

                boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z + trainCarBounds.size);
                boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y, 0f);

                var collisionPosition = collision.transform.localPosition;
                collisionPosition.z = 0f;
                collision.transform.localPosition = collisionPosition;
            }
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
                    code[i].operand = 0.6f;
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
                    code[i].operand = 0.6f;
                }
            }

            return code;
        }
    }

    [HarmonyPatch(typeof(Coupler), "Start")]
    class Coupler_Start_Patch
    {
        static void Postfix(Coupler __instance)
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

            if ((__instance.train.carType == TrainCarType.Tender || __instance.train.carType == TrainCarType.TenderBlue) &&
    __instance.train.rearCoupler.Equals(__instance))
            {
                coef = 0.2f;
            }

            if (__instance.train.carType == TrainCarType.RefrigeratorWhite)
            {
                coef = 0.4f;
            }

            if (__instance.train.carType == TrainCarType.BoxcarBrown || __instance.train.carType == TrainCarType.BoxcarGreen || __instance.train.carType == TrainCarType.BoxcarPink || __instance.train.carType == TrainCarType.BoxcarRed)
            {
                coef = 0.4f;
            }

            if (__instance.train.carType == TrainCarType.PassengerBlue || __instance.train.carType == TrainCarType.PassengerGreen)
            {
                coef = 0.4f;
            }

            if (__instance.train.carType == TrainCarType.PassengerRed)
            {
                coef = 0.42f;
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
        }
    }

    [HarmonyPatch(typeof(Coupler), "CreateJoint")]
    class Coupler_CreateJoint_Patch
    {
        static Coupler instance;

        static void Postfix(Coupler __instance, ref ConfigurableJoint __result)
        {
            instance = __instance;

            SoftJointLimit softJointLimit = new SoftJointLimit();

            if (__instance.train.carType == TrainCarType.LocoSteamHeavy && __instance.coupledTo.train.carType == TrainCarType.Tender ||
                __instance.train.carType == TrainCarType.Tender && __instance.coupledTo.train.carType == TrainCarType.LocoSteamHeavy)
            {
                softJointLimit.limit = 0.05f;
                __result.linearLimit = softJointLimit;
            }
            else
            {
                instance.StartCoroutine(__instance.GetComponent<CouplerCustom>().ReduceLimit(0.25f, __result));
            }

            __result.enableCollision = false;
        }
    }

    //[HarmonyPatch(typeof(TrainCar), "Start")]
    //class TrainCar_Start_Patch
    //{
    //    static void Postfix(TrainCar __instance)
    //    {
    //        var collision = __instance.transform.Find("[collision]");
    //        var colliders = collision.GetComponents<BoxCollider>();

    //        for (var i = 0; i < colliders.Length; i++)
    //        {
    //            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //            GameObject.Destroy(go.GetComponent<BoxCollider>());
    //            go.transform.parent = collision.transform;
    //            go.transform.localPosition = colliders[i].center;
    //            go.transform.localScale = colliders[i].size;
    //            go.transform.localRotation = Quaternion.identity;
    //            go.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/TransparentColor");
    //            go.GetComponent<Renderer>().material.color = new Color(0f, 0f, 1f, 0.3f);
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

        Rigidbody trainRigidbody;
        Rigidbody otherTrainRigidbody;

        bool reduceDistance = false;
        float currentDistance;
        float targetDistance;
        ConfigurableJoint jointToReduce;

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

        public IEnumerator<object> ReduceLimit(float target, ConfigurableJoint joint)
        {
            yield return (object)WaitFor.SecondsRealtime(1f);

            reduceDistance = true;
            currentDistance = joint.linearLimit.limit;
            targetDistance = target;
            jointToReduce = joint;
        }

        void ReduceDistance()
        {
            if (reduceDistance && jointToReduce)
            {
                currentDistance -= 0.005f;

                if (currentDistance <= targetDistance)
                {
                    reduceDistance = false;
                    currentDistance = targetDistance;
                }

                SoftJointLimit softJointLimit = new SoftJointLimit();
                softJointLimit.limit = currentDistance;
                jointToReduce.linearLimit = softJointLimit;
            }
        }

        void FixedUpdate()
        {
            ReduceDistance();

            if (!coupler || !coupler.coupledTo || !coupler.train || !coupler.coupledTo.train || coupler.train.derailed || coupler.coupledTo.train.derailed || !CouplerAnchor) return;

            if (!coupledToCustom || !otherTrainRigidbody)
            {
                coupledToCustom = coupler.coupledTo.GetComponent<CouplerCustom>();
                otherTrainRigidbody = coupler.coupledTo.train.GetComponent<Rigidbody>();
            }

            if (!coupledToCustom || !otherTrainRigidbody || !coupledToCustom.CouplerAnchor) return;

            float power = 150000f;
            float damper = 5000f;
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

                ApplyTrainForce(totalForce * 0.5f, coupler);
                ApplyTrainForce(totalForce * 0.5f, coupler.coupledTo);
            }
        }

        void MoveBuffer(Transform buffer, Vector3 position, float distance)
        {
            if (!buffer) return;

            Vector3 pos;
            pos = position;
            pos.z += distance;
            buffer.transform.localPosition = pos;
        }

        void ApplyTrainForce(float force, Coupler coupler)
        {
            var dir = 1f;

            if (coupler.train.frontCoupler.Equals(coupler))
            {
                dir = -1f;
            }

            coupler.train.rb.AddForce(coupler.train.transform.forward * force * dir);
        }
    }
}