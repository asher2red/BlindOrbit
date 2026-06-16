using BlindOrbit.Core;
using BlindOrbit.Managers;
using BlindOrbit.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlindOrbit.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(FuelSystem))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] float turnAcceleration = 165f;
        [SerializeField] float maxAngularSpeed = 155f;
        [SerializeField] float angularDampingPerSecond = 4.2f;
        [SerializeField] float turnFuelPerSecond = 0.28f;

        [Header("Forward Thrust")]
        [SerializeField] float thrustForce = 4.8f;
        [SerializeField] float forwardFuelPerSecond = 2.8f;

        [Header("Linear Resistance")]
        [Tooltip("Manual per-second damping for translational velocity. Applied in FixedUpdate after thrust so acceleration and resistance stay synchronized.")]
        [SerializeField, Min(0f)] float linearDamping = 0.55f;

        [Header("Thruster VFX")]
        [SerializeField, Min(1)] int maxActiveParticlesPerThruster = 64;

        Rigidbody2D body;
        FuelSystem fuel;
        StageManager stageManager;
        AudioManager audioManager;
        ParticleSystem rightSideThruster;
        ParticleSystem leftSideThruster;
        ParticleSystem mainThruster;
        bool turnLeftHeld;
        bool turnRightHeld;
        bool forwardHeld;
        bool acceptingInput;
        bool leftTurnEffectActive;
        bool rightTurnEffectActive;
        bool forwardEffectActive;

        public Rigidbody2D Body => body;
        public FuelSystem Fuel => fuel;
        public Vector2 Velocity => body == null ? Vector2.zero : body.linearVelocity;

        public void Initialize(StageManager owner, Vector2 position, Vector2 initialVelocity, float fuelAmount, AudioManager audio)
        {
            stageManager = owner;
            audioManager = audio;
            body = GetComponent<Rigidbody2D>();
            fuel = GetComponent<FuelSystem>();

            transform.position = position;
            transform.rotation = Quaternion.identity;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.freezeRotation = false;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            fuel.ResetFuel(fuelAmount);
            acceptingInput = true;
            ClearHeldInputs();
            SetThrusterEffects(false, false, false, true);
        }

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            fuel = GetComponent<FuelSystem>();

            BuildShipVisual();

            var collider = GetComponent<CircleCollider2D>();
            collider.radius = 0.34f;

            rightSideThruster = CreateThrusterEffect("Right Side Thruster", new Vector3(0.49f, 0.25f, 0f), new Vector2(3.0f, 0f), 0.22f, 0.34f, 0.46f, new Color(0.72f, 0.92f, 1f, 0.88f));
            leftSideThruster = CreateThrusterEffect("Left Side Thruster", new Vector3(-0.49f, 0.25f, 0f), new Vector2(-3.0f, 0f), 0.22f, 0.34f, 0.46f, new Color(0.72f, 0.92f, 1f, 0.88f));
            mainThruster = CreateThrusterEffect("Main Engine Thruster", new Vector3(0f, -0.66f, 0f), new Vector2(0f, -4.8f), 0.44f, 0.48f, 0.78f, new Color(0.82f, 0.95f, 1f, 0.95f));
            SetThrusterEffects(false, false, false, true);
        }

        void FixedUpdate()
        {
            if (!acceptingInput || stageManager == null || stageManager.State != GameState.Playing)
            {
                ApplyLinearDamping();
                ApplyAngularDamping();
                SetThrusterEffects(false, false, false);
                return;
            }

            var input = ReadControlInput();
            var rotationResult = ApplyRotation(input);
            var forwardConsumedFuel = ApplyForwardThrust(input.forward);
            ApplyLinearDamping();
            ApplyAngularDamping();
            SetThrusterEffects(rotationResult.leftConsumedFuel, rotationResult.rightConsumedFuel, forwardConsumedFuel);
        }

        public void StopInput()
        {
            acceptingInput = false;
            ClearHeldInputs();
            SetThrusterEffects(false, false, false, true);
        }

        void OnDisable()
        {
            ClearHeldInputs();
            SetThrusterEffects(false, false, false);
        }

        public void SetTurnLeftHeld(bool isHeld)
        {
            turnLeftHeld = isHeld;
        }

        public void SetForwardHeld(bool isHeld)
        {
            forwardHeld = isHeld;
        }

        public void SetTurnRightHeld(bool isHeld)
        {
            turnRightHeld = isHeld;
        }

        RotationFuelResult ApplyRotation(ControlInput input)
        {
            var direction = 0f;
            if (input.left && !input.right)
            {
                direction = 1f;
            }
            else if (input.right && !input.left)
            {
                direction = -1f;
            }

            if (Mathf.Approximately(direction, 0f) || fuel.IsEmpty)
            {
                return default;
            }

            var requestedFuel = turnFuelPerSecond * Time.fixedDeltaTime;
            if (fuel.Consume(requestedFuel) <= 0f)
            {
                return default;
            }

            body.angularVelocity += direction * turnAcceleration * Time.fixedDeltaTime;
            body.angularVelocity = Mathf.Clamp(body.angularVelocity, -maxAngularSpeed, maxAngularSpeed);
            audioManager?.PlayThruster();
            return new RotationFuelResult(direction > 0f, direction < 0f);
        }

        bool ApplyForwardThrust(bool isThrusting)
        {
            if (!isThrusting || fuel.IsEmpty)
            {
                return false;
            }

            var requestedFuel = forwardFuelPerSecond * Time.fixedDeltaTime;
            var consumedFuel = fuel.Consume(requestedFuel);
            if (consumedFuel <= 0f)
            {
                return false;
            }

            body.AddForce(transform.up * thrustForce * (consumedFuel / requestedFuel), ForceMode2D.Force);
            audioManager?.PlayThruster();
            return true;
        }

        void ApplyAngularDamping()
        {
            if (body == null)
            {
                return;
            }

            body.angularVelocity *= Mathf.Exp(-angularDampingPerSecond * Time.fixedDeltaTime);
        }

        void ApplyLinearDamping()
        {
            if (body == null || linearDamping <= 0f)
            {
                return;
            }

            body.linearVelocity *= Mathf.Exp(-linearDamping * Time.fixedDeltaTime);
        }

        ControlInput ReadControlInput()
        {
            var keyboard = Keyboard.current;
            var keyboardLeft = keyboard != null && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            var keyboardForward = keyboard != null && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed);
            var keyboardRight = keyboard != null && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);

            return new ControlInput(
                turnLeftHeld || keyboardLeft,
                forwardHeld || keyboardForward,
                turnRightHeld || keyboardRight);
        }

        void ClearHeldInputs()
        {
            turnLeftHeld = false;
            turnRightHeld = false;
            forwardHeld = false;
        }

        void BuildShipVisual()
        {
            CreateShipPart("Rear Hull", PlaceholderSpriteFactory.Square(), new Vector2(0f, -0.28f), new Vector2(0.62f, 0.42f), 0f, new Color(0.16f, 0.25f, 0.34f, 1f), 9);
            CreateShipPart("Main Fuselage", PlaceholderSpriteFactory.Square(), new Vector2(0f, 0.08f), new Vector2(0.46f, 0.86f), 0f, new Color(0.58f, 0.88f, 0.96f, 1f), 10);
            CreateShipPart("Rounded Nose", PlaceholderSpriteFactory.Circle(), new Vector2(0f, 0.55f), new Vector2(0.43f, 0.36f), 0f, new Color(0.7f, 0.96f, 1f, 1f), 11);
            CreateShipPart("Left Wing", PlaceholderSpriteFactory.Triangle(), new Vector2(-0.42f, -0.16f), new Vector2(0.34f, 0.46f), 90f, new Color(0.24f, 0.45f, 0.6f, 1f), 9);
            CreateShipPart("Right Wing", PlaceholderSpriteFactory.Triangle(), new Vector2(0.42f, -0.16f), new Vector2(0.34f, 0.46f), -90f, new Color(0.24f, 0.45f, 0.6f, 1f), 9);
            CreateShipPart("Main Engine Nozzle", PlaceholderSpriteFactory.Square(), new Vector2(0f, -0.58f), new Vector2(0.28f, 0.2f), 0f, new Color(0.05f, 0.09f, 0.12f, 1f), 12);
            CreateShipPart("Left Attitude Nozzle", PlaceholderSpriteFactory.Square(), new Vector2(-0.41f, 0.25f), new Vector2(0.18f, 0.11f), 0f, new Color(0.05f, 0.09f, 0.12f, 1f), 12);
            CreateShipPart("Right Attitude Nozzle", PlaceholderSpriteFactory.Square(), new Vector2(0.41f, 0.25f), new Vector2(0.18f, 0.11f), 0f, new Color(0.05f, 0.09f, 0.12f, 1f), 12);
            CreateShipPart("Cockpit", PlaceholderSpriteFactory.Circle(), new Vector2(0f, 0.22f), new Vector2(0.22f, 0.28f), 0f, new Color(0.08f, 0.18f, 0.27f, 1f), 13);
        }

        void CreateShipPart(string partName, Sprite sprite, Vector2 localPosition, Vector2 scale, float rotation, Color color, int sortingOrder)
        {
            var part = new GameObject(partName, typeof(SpriteRenderer));
            part.transform.SetParent(transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            part.transform.localScale = new Vector3(scale.x, scale.y, 1f);

            var renderer = part.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
        }

        ParticleSystem CreateThrusterEffect(string effectName, Vector3 localPosition, Vector2 localVelocity, float size, float minLifetime, float maxLifetime, Color color)
        {
            var effectObject = new GameObject(effectName, typeof(ParticleSystem));
            effectObject.transform.SetParent(transform, false);
            effectObject.transform.localPosition = localPosition;

            var particles = effectObject.GetComponent<ParticleSystem>();
            var main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.55f, size);
            main.startColor = color;
            main.maxParticles = maxActiveParticlesPerThruster;

            var emission = particles.emission;
            emission.rateOverTime = 0f;

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = size * 0.08f;

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            var axisSpeed = Mathf.Max(Mathf.Abs(localVelocity.x), Mathf.Abs(localVelocity.y), 0.001f);
            var axisJitter = axisSpeed * 0.18f;
            var coneSpread = axisSpeed * 0.58f;
            if (Mathf.Abs(localVelocity.x) >= Mathf.Abs(localVelocity.y))
            {
                velocity.x = DirectionalVelocityCurve(localVelocity.x, axisJitter);
                velocity.y = DirectionalVelocityCurve(0f, coneSpread);
            }
            else
            {
                velocity.x = DirectionalVelocityCurve(0f, coneSpread);
                velocity.y = DirectionalVelocityCurve(localVelocity.y, axisJitter);
            }
            velocity.z = DirectionalVelocityCurve(0f, 0.001f);

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1.16f));

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.42f, 0.55f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 12;

            return particles;
        }

        static ParticleSystem.MinMaxCurve DirectionalVelocityCurve(float value, float spread)
        {
            return new ParticleSystem.MinMaxCurve(value - spread, value + spread);
        }

        void SetThrusterEffects(bool leftTurn, bool rightTurn, bool forward, bool force = false)
        {
            SetEmission(rightSideThruster, leftTurn, leftTurnEffectActive, 14f, force);
            SetEmission(leftSideThruster, rightTurn, rightTurnEffectActive, 14f, force);
            SetEmission(mainThruster, forward, forwardEffectActive, 30f, force);
            leftTurnEffectActive = leftTurn;
            rightTurnEffectActive = rightTurn;
            forwardEffectActive = forward;
        }

        static void SetEmission(ParticleSystem particles, bool shouldEmit, bool wasEmitting, float rate, bool force)
        {
            if (particles == null)
            {
                return;
            }

            var emission = particles.emission;
            emission.enabled = shouldEmit;
            emission.rateOverTime = shouldEmit ? rate : 0f;

            if (shouldEmit)
            {
                if (!wasEmitting || force || !particles.isEmitting)
                {
                    particles.Play(false);
                }

                particles.Emit(1);
            }
            else if (wasEmitting || force)
            {
                particles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            stageManager?.FailStage("Collision");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<GoalArea>(out _))
            {
                stageManager?.ClearStage();
            }
        }

        readonly struct ControlInput
        {
            public readonly bool left;
            public readonly bool forward;
            public readonly bool right;

            public ControlInput(bool left, bool forward, bool right)
            {
                this.left = left;
                this.forward = forward;
                this.right = right;
            }
        }

        readonly struct RotationFuelResult
        {
            public readonly bool leftConsumedFuel;
            public readonly bool rightConsumedFuel;

            public RotationFuelResult(bool leftConsumedFuel, bool rightConsumedFuel)
            {
                this.leftConsumedFuel = leftConsumedFuel;
                this.rightConsumedFuel = rightConsumedFuel;
            }
        }
    }
}
