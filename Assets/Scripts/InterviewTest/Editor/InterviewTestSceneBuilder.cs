#if UNITY_EDITOR
using baodeag.InterviewTest;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace baodeag.InterviewTest.Editor
{
    public static class InterviewTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scene/Interview_Test_Landscape.unity";
        private const string PrefabFolder = "Assets/InterviewTest/Prefabs";
        private const string DataFolder = "Assets/InterviewTest/Data";
        private const string AnimatorPath = DataFolder + "/Interview_Player.controller";
        private const string PlayerPrefabPath = PrefabFolder + "/Interview Player.prefab";
        private const string GemPrefabPath = PrefabFolder + "/Interview Gem.prefab";

        [MenuItem("Interview Test/Build Gem Collector Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            EnsureLayers();

            Material commonMaterial = CreateGemMaterial("Common Gem Material", new Color(0.2f, 0.95f, 1f));
            Material rareMaterial = CreateGemMaterial("Rare Gem Material", new Color(0.45f, 1f, 0.3f));
            Material epicMaterial = CreateGemMaterial("Epic Gem Material", new Color(1f, 0.35f, 0.85f));

            InterviewGemType commonType = CreateGemType("Common Gem", 1, 70, commonMaterial, new Color(0.2f, 0.95f, 1f));
            InterviewGemType rareType = CreateGemType("Rare Gem", 2, 25, rareMaterial, new Color(0.45f, 1f, 0.3f));
            InterviewGemType epicType = CreateGemType("Epic Gem", 3, 5, epicMaterial, new Color(1f, 0.35f, 0.85f));

            RuntimeAnimatorController animatorController = CreateAnimatorController();
            InterviewGem gemPrefab = CreateGemPrefab(commonMaterial);
            GameObject playerPrefab = CreatePlayerPrefab(animatorController);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Interview_Test_Landscape";

            BuildLighting();
            GameObject ground = BuildGround();
            BuildClimbableWalls();
            InterviewBoundary boundary = BuildBoundary();

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.name = "Interview Player";
            player.transform.position = new Vector3(0f, 2f, -4f);

            Camera mainCamera = BuildCamera();
            InterviewCameraController cameraController = mainCamera.gameObject.AddComponent<InterviewCameraController>();
            SetObject(cameraController, "target", player.transform);
            SetObject(cameraController, "followOffset", new Vector3(0f, 4.4f, -9f));
            SetObject(cameraController, "rotationSensitivity", 0.18f);
            SetObject(cameraController, "verticalSensitivity", 0.18f);
            SetObject(cameraController, "minPitch", -10f);
            SetObject(cameraController, "maxPitch", 65f);

            GameObject mapCenter = new GameObject("Map Center");
            mapCenter.transform.position = ground.transform.position;

            GameObject introObject = new GameObject("Intro Camera Sequence");
            InterviewIntroCameraSequence intro = introObject.AddComponent<InterviewIntroCameraSequence>();
            SetObject(intro, "followCamera", cameraController);
            SetObject(intro, "mapCenter", mapCenter.transform);
            SetObject(intro, "player", player.transform);
            SetObject(intro, "orbitDuration", 3.2f);
            SetObject(intro, "blendDuration", 1.4f);
            SetObject(intro, "orbitRadius", 28f);
            SetObject(intro, "orbitHeight", 18f);

            BuildGemSystem(gemPrefab, player.transform, commonType, rareType, epicType, out InterviewGemSpawner spawner);
            InterviewUIManager uiManager = BuildUI(player.GetComponent<InterviewPlayerController>(), out InterviewVirtualJoystick joystick, out Transform gemIcon);

            WirePlayer(player, joystick, cameraController, boundary);
            BuildManagers(player.GetComponent<InterviewPlayerController>(), cameraController, intro, spawner, uiManager, player.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log($"Interview test scene built at {ScenePath}");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "InterviewTest");
            EnsureFolder("Assets/InterviewTest", "Data");
            EnsureFolder("Assets/InterviewTest", "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void EnsureLayers()
        {
            SetLayerName(6, "Ground");
            SetLayerName(7, "Climbable");
            SetLayerName(8, "Gem");
        }

        private static void SetLayerName(int layer, string name)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            SerializedProperty layerProperty = layers.GetArrayElementAtIndex(layer);
            if (string.IsNullOrEmpty(layerProperty.stringValue))
            {
                layerProperty.stringValue = name;
                tagManager.ApplyModifiedProperties();
            }
        }

        private static Material CreateGemMaterial(string name, Color color)
        {
            string path = $"{DataFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.8f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static InterviewGemType CreateGemType(string name, int score, int weight, Material material, Color lightColor)
        {
            string path = $"{DataFolder}/{name}.asset";
            InterviewGemType gemType = AssetDatabase.LoadAssetAtPath<InterviewGemType>(path);
            if (gemType == null)
            {
                gemType = ScriptableObject.CreateInstance<InterviewGemType>();
                AssetDatabase.CreateAsset(gemType, path);
            }

            gemType.gemName = name;
            gemType.scoreValue = score;
            gemType.spawnWeight = weight;
            gemType.material = material;
            gemType.lightColor = lightColor;
            EditorUtility.SetDirty(gemType);
            return gemType;
        }

        private static RuntimeAnimatorController CreateAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AnimatorPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            }

            controller.parameters = new AnimatorControllerParameter[0];
            controller.AddParameter("MoveAmount", AnimatorControllerParameterType.Float);
            controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
            controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
            controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("isClimbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            ClearStateMachine(stateMachine);

            AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Character/Models/Idle.anim");
            AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Character/Models/Run.anim");
            AnimationClip climb = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Character/Models/Climb.anim");
            AnimationClip attack = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Character/Models/Attack.anim");
            if (attack == null)
            {
                attack = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Character/Models/Jumping.anim");
            }

            AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(250f, 100f, 0f));
            AnimatorState runState = stateMachine.AddState("Run", new Vector3(500f, 100f, 0f));
            AnimatorState climbState = stateMachine.AddState("Climb", new Vector3(250f, 300f, 0f));
            AnimatorState attackState = stateMachine.AddState("Attack", new Vector3(500f, 300f, 0f));

            idleState.motion = idle;
            runState.motion = run;
            climbState.motion = climb;
            attackState.motion = attack;

            stateMachine.defaultState = idleState;
            AddFloatTransition(idleState, runState, "MoveAmount", 0.1f, true);
            AddFloatTransition(runState, idleState, "MoveAmount", 0.1f, false);
            AddBoolStateTransition(climbState, idleState, "isClimbing", false);
            AddTriggerTransition(stateMachine, attackState, "Attack");
            AnimatorStateTransition attackExit = attackState.AddTransition(idleState);
            attackExit.hasExitTime = true;
            attackExit.exitTime = 0.85f;
            attackExit.duration = 0.08f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void ClearStateMachine(AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                stateMachine.RemoveState(state.state);
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                stateMachine.RemoveStateMachine(child.stateMachine);
            }

            foreach (AnimatorStateTransition transition in stateMachine.anyStateTransitions)
            {
                stateMachine.RemoveAnyStateTransition(transition);
            }
        }

        private static void AddFloatTransition(AnimatorState from, AnimatorState to, string parameter, float threshold, bool greater)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.AddCondition(greater ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less, threshold, parameter);
        }

        private static void AddBoolTransition(AnimatorStateMachine stateMachine, AnimatorState to, string parameter, bool value)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
        }

        private static void AddBoolStateTransition(AnimatorState from, AnimatorState to, string parameter, bool value)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
        }

        private static void AddTriggerTransition(AnimatorStateMachine stateMachine, AnimatorState to, string parameter)
        {
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static InterviewGem CreateGemPrefab(Material material)
        {
            GameObject root = new GameObject("Interview Gem");
            root.layer = LayerMask.NameToLayer("Gem");

            GameObject sourceGem = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Gem.prefab");
            GameObject visual = sourceGem != null ? (GameObject)PrefabUtility.InstantiatePrefab(sourceGem) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * 0.75f;

            Renderer renderer = visual.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.7f;

            Light light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 4f;
            light.intensity = 1.8f;
            light.color = Color.cyan;

            InterviewGem gem = root.AddComponent<InterviewGem>();
            SetObject(gem, "visualRoot", visual.transform);
            SetObject(gem, "gemRenderer", renderer);
            SetObject(gem, "gemLight", light);
            SetObject(gem, "gemCollider", collider);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, GemPrefabPath);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<InterviewGem>();
        }

        private static GameObject CreatePlayerPrefab(RuntimeAnimatorController animatorController)
        {
            GameObject root = new GameObject("Interview Player");
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.height = 1.8f;
            controller.radius = 0.35f;

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Character/Models/Player_LowPoly.fbx");
            if (model != null)
            {
                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
            }

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = FindAvatar("Assets/Character/Models/Player_LowPoly.fbx");
            animator.applyRootMotion = false;

            InterviewPlayerController player = root.AddComponent<InterviewPlayerController>();
            InterviewClimbDetector climb = root.AddComponent<InterviewClimbDetector>();
            SetObject(player, "animator", animator);
            SetObject(player, "climbDetector", climb);
            SetObject(player, "gemLayers", LayerMask.GetMask("Gem"));
            SetObject(player, "attackRadius", 2.1f);
            SetObject(player, "attackOffset", new Vector3(0f, 0.75f, 0f));
            SetObject(climb, "characterController", controller);
            SetObject(climb, "animator", animator);
            SetObject(climb, "climbableLayers", LayerMask.GetMask("Climbable"));
            SetObject(climb, "climbForward", 1.15f);
            SetObject(climb, "climbDuration", 0.95f);
            SetObject(climb, "topSurfacePadding", 0.08f);
            SetObject(climb, "landingClearancePadding", 0.08f);
            SetObject(climb, "blockingLayers", ~LayerMask.GetMask("Gem"));

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Avatar FindAvatar(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Avatar avatar)
                {
                    return avatar;
                }
            }

            return null;
        }

        private static void BuildLighting()
        {
            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 2f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static GameObject BuildGround()
        {
            GameObject sourceGround = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/3D/Map/Location_Ground.fbx");
            GameObject ground = sourceGround != null ? (GameObject)PrefabUtility.InstantiatePrefab(sourceGround) : GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Location_Ground";
            ground.layer = LayerMask.NameToLayer("Ground");
            ground.transform.position = Vector3.zero;
            AddMeshColliders(ground, LayerMask.NameToLayer("Ground"));
            return ground;
        }

        private static void AddMeshColliders(GameObject root, int layer)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                if (meshFilter != null && child.GetComponent<Collider>() == null)
                {
                    MeshCollider meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = meshFilter.sharedMesh;
                }
            }
        }

        private static void BuildClimbableWalls()
        {
            Material wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMaterial.SetColor("_BaseColor", new Color(0.25f, 0.35f, 0.7f));

            CreateWall("Climbable Wall North", new Vector3(0f, 1.5f, 8.5f), new Vector3(6f, 3f, 0.4f), wallMaterial);
            CreateWall("Climbable Wall East", new Vector3(8f, 1.2f, 0f), new Vector3(0.4f, 2.4f, 5f), wallMaterial);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.layer = LayerMask.NameToLayer("Climbable");
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static InterviewBoundary BuildBoundary()
        {
            GameObject boundaryObject = new GameObject("Map Boundary");
            InterviewBoundary boundary = boundaryObject.AddComponent<InterviewBoundary>();
            SetObject(boundary, "xRange", new Vector2(-13.5f, 13.5f));
            SetObject(boundary, "zRange", new Vector2(-13.5f, 13.5f));
            return boundary;
        }

        private static Camera BuildCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void BuildGemSystem(InterviewGem gemPrefab, Transform player, InterviewGemType common, InterviewGemType rare, InterviewGemType epic, out InterviewGemSpawner spawner)
        {
            GameObject system = new GameObject("Gem System");
            InterviewGemFactory factory = system.AddComponent<InterviewGemFactory>();
            InterviewGemPool pool = system.AddComponent<InterviewGemPool>();
            spawner = system.AddComponent<InterviewGemSpawner>();

            GameObject spawnAreaObject = new GameObject("Gem Spawn Area");
            spawnAreaObject.transform.SetParent(system.transform);
            spawnAreaObject.transform.position = new Vector3(0f, 1f, 0f);
            BoxCollider spawnArea = spawnAreaObject.AddComponent<BoxCollider>();
            spawnArea.isTrigger = true;
            spawnArea.size = new Vector3(23f, 2f, 23f);

            SetObject(factory, "gemTypes", new[] { common, rare, epic });
            SetObject(pool, "gemPrefab", gemPrefab);
            SetObject(pool, "initialSize", 18);
            SetObject(spawner, "gemPool", pool);
            SetObject(spawner, "gemFactory", factory);
            SetObject(spawner, "spawnArea", spawnArea);
            SetObject(spawner, "player", player);
            SetObject(spawner, "groundLayers", LayerMask.GetMask("Ground"));
        }

        private static InterviewUIManager BuildUI(InterviewPlayerController player, out InterviewVirtualJoystick joystick, out Transform gemIcon)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif

            GameObject canvasObject = new GameObject("Interview UI Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject hud = CreateUIObject("HUD", canvasObject.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(360f, 82f));
            Text scoreLabel = CreateText("Score Label", hud.transform, "Score", 26, TextAnchor.MiddleLeft);
            Text scoreValueText = CreateText("Score Value", hud.transform, "0/10", 34, TextAnchor.MiddleLeft);

            SetRect(scoreLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(58f, -40f), new Vector2(100f, 34f));
            SetRect(scoreValueText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(178f, -40f), new Vector2(130f, 40f));

            gemIcon = CreatePanel("Gem Icon Target", hud.transform, new Color(0.2f, 0.95f, 1f, 0.9f), new Vector2(56f, 56f)).transform;
            SetRect((RectTransform)gemIcon, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -40f), new Vector2(56f, 56f));

            GameObject controls = CreateUIObject("Controls", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            joystick = BuildJoystick(controls.transform);
            BuildAttackButton(controls.transform, player);

            Button startButton = BuildButton("Start Button", canvasObject.transform, "START", new Vector2(0.5f, 0.5f), new Vector2(260f, 86f), new Vector2(0f, 0f));
            Button resetButton = BuildButton("Reset Button", canvasObject.transform, "RESET", new Vector2(1f, 1f), new Vector2(170f, 58f), new Vector2(-105f, -42f));
            GameObject winPanel = BuildWinPanel(canvasObject.transform);

            InterviewUIManager uiManager = canvasObject.AddComponent<InterviewUIManager>();
            SetObject(uiManager, "scoreValueText", scoreValueText);
            SetObject(uiManager, "gemIconTarget", gemIcon);
            SetObject(uiManager, "canvas", canvas);
            SetObject(uiManager, "controlsRoot", controls);
            SetObject(uiManager, "startButton", startButton);
            SetObject(uiManager, "resetButton", resetButton);
            SetObject(uiManager, "winPanel", winPanel);
            return uiManager;
        }

        private static InterviewVirtualJoystick BuildJoystick(Transform parent)
        {
            GameObject root = CreateUIObject("Virtual Joystick", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(170f, 150f), new Vector2(220f, 220f));
            GameObject background = CreatePanel("Background", root.transform, new Color(0f, 0f, 0f, 0.35f), new Vector2(220f, 220f));
            GameObject handle = CreatePanel("Handle", background.transform, new Color(1f, 1f, 1f, 0.75f), new Vector2(84f, 84f));

            InterviewVirtualJoystick joystick = root.AddComponent<InterviewVirtualJoystick>();
            SetObject(joystick, "background", background.GetComponent<RectTransform>());
            SetObject(joystick, "handle", handle.GetComponent<RectTransform>());
            return joystick;
        }

        private static void BuildAttackButton(Transform parent, InterviewPlayerController player)
        {
            Button button = BuildButton("Attack Button", parent, "ATK", new Vector2(1f, 0f), new Vector2(150f, 150f), new Vector2(-160f, 145f));
            InterviewAttackButton attackButton = button.gameObject.AddComponent<InterviewAttackButton>();
            SetObject(attackButton, "playerController", player);
        }

        private static GameObject BuildWinPanel(Transform parent)
        {
            GameObject panel = CreatePanel("Win Panel", parent, new Color(0.02f, 0.03f, 0.05f, 0.82f), new Vector2(520f, 260f));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            Text text = CreateText("You Win Text", panel.transform, "You Win", 64, TextAnchor.MiddleCenter);
            text.color = new Color(1f, 0.95f, 0.35f);
            return panel;
        }

        private static Button BuildButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject buttonObject = CreatePanel(name, parent, new Color(0.1f, 0.18f, 0.25f, 0.92f), size);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = anchoredPosition;
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.2f, 0.38f, 0.55f, 1f);
            colors.pressedColor = new Color(0.05f, 0.1f, 0.16f, 1f);
            button.colors = colors;
            Text text = CreateText("Label", buttonObject.transform, label, 32, TextAnchor.MiddleCenter);
            text.color = Color.white;
            return button;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = CreateUIObject(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 size)
        {
            GameObject panel = CreateUIObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, anchoredPosition, size);
            return gameObject;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void WirePlayer(GameObject player, InterviewVirtualJoystick joystick, InterviewCameraController cameraController, InterviewBoundary boundary)
        {
            InterviewPlayerController controller = player.GetComponent<InterviewPlayerController>();
            SetObject(controller, "joystick", joystick);
            SetObject(controller, "cameraController", cameraController);
            SetObject(controller, "boundary", boundary);
        }

        private static void BuildManagers(InterviewPlayerController player, InterviewCameraController camera, InterviewIntroCameraSequence intro, InterviewGemSpawner spawner, InterviewUIManager ui, Transform playerTransform)
        {
            GameObject managers = new GameObject("Interview Game Managers");
            managers.AddComponent<InterviewScoreManager>();
            InterviewGameManager gameManager = managers.AddComponent<InterviewGameManager>();
            SetObject(gameManager, "playerController", player);
            SetObject(gameManager, "cameraController", camera);
            SetObject(gameManager, "introCameraSequence", intro);
            SetObject(gameManager, "gemSpawner", spawner);
            SetObject(gameManager, "uiManager", ui);

            GameObject confettiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/ConfettiBlastRainbow.prefab");
            if (confettiPrefab != null)
            {
                GameObject confetti = (GameObject)PrefabUtility.InstantiatePrefab(confettiPrefab);
                confetti.name = "Win Confetti";
                confetti.transform.position = playerTransform.position + Vector3.up * 2.2f;
                confetti.SetActive(false);
                SetObject(gameManager, "winParticle", confetti.GetComponent<ParticleSystem>());
                SetObject(gameManager, "winParticlePrefab", confettiPrefab.GetComponent<ParticleSystem>());
                SetObject(gameManager, "winParticleBurstCount", 7);
                SetObject(gameManager, "winParticleSpacing", 1.8f);
            }
        }

        private static void SetObject(Object target, string propertyName, object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                UnityEngine.Debug.LogWarning($"Missing serialized property {propertyName} on {target.name}");
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    property.intValue = (int)value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float)value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color)value;
                    break;
                default:
                    if (property.isArray && value is Object[] objectArray)
                    {
                        property.arraySize = objectArray.Length;
                        for (int i = 0; i < objectArray.Length; i++)
                        {
                            property.GetArrayElementAtIndex(i).objectReferenceValue = objectArray[i];
                        }
                    }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
