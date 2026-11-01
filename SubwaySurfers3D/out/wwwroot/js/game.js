/* ==========================================================================
   3D SUBWAY SURFERS GAME ENGINE (EASY DIFFICULTY & ENHANCED ANIMATIONS)
   ========================================================================== */

class SubwaySurfersGame {
    constructor() {
        this.canvas = document.getElementById('game-canvas');
        this.scene = null;
        this.camera = null;
        this.renderer = null;

        // Player State & Account
        this.playerData = {
            id: 0,
            username: 'RunnerPro',
            highScore: 0,
            totalCoins: 250,
            selectedCharacterId: 'jake',
            selectedBoardId: 'starboard',
            ownedItemIds: ['jake', 'starboard']
        };

        // Game World Constants
        this.LANES = [-3.5, 0, 3.5];
        this.currentLaneIndex = 1; // Start in middle lane (0)
        this.targetX = 0;
        this.playerY = 0;
        this.playerZ = 0;

        // Physics & Movement (Forgiving & Easy)
        this.isJumping = false;
        this.jumpVelocity = 0;
        this.gravity = -34;
        this.isSliding = false;
        this.slideTimer = 0;
        this.landingSquash = 0;

        // Powerups Active State (Extended Durations)
        this.powerupState = {
            magnet: 0,       // Duration left (seconds)
            jetpack: 0,      // Duration left
            multiplier: 0,   // Duration left
            hoverboard: false // Active shield
        };

        // Game Loop Variables (Low Difficulty Settings)
        this.isPlaying = false;
        this.isPaused = false;
        this.gameSpeed = 14; // Easy initial speed!
        this.maxSpeed = 26;  // Gentle maximum speed!
        this.distanceRun = 0;
        this.score = 0;
        this.coinsCollectedInRun = 0;
        this.scoreMultiplier = 1;
        this.powerupsUsedInRun = new Set();

        // 3D Objects Arrays
        this.tracks = [];
        this.obstacles = [];
        this.collectibles = [];
        this.particles = [];

        // 3D Player Group
        this.playerGroup = null;
        this.hoverboardMesh = null;
        this.jetpackMesh = null;
        this.animTime = 0;
        this.bankAngle = 0;

        // Assets / Materials cache
        this.shopItems = [];

        this.init();
    }

    async init() {
        this.setupThree();
        this.setupLights();
        this.createEnvironment();
        this.createPlayerMesh();
        this.setupEventListeners();
        
        // Initial API Load
        await this.loadPlayerData();
        await this.loadShopCatalogue();
        this.updateUI();

        // Start render loop
        this.clock = new THREE.Clock();
        requestAnimationFrame((t) => this.animate(t));
    }

    setupThree() {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x0a0c16);
        this.scene.fog = new THREE.FogExp2(0x0a0c16, 0.012);

        this.camera = new THREE.PerspectiveCamera(65, window.innerWidth / window.innerHeight, 0.1, 300);
        this.camera.position.set(0, 4.2, 9);
        this.camera.lookAt(0, 1.5, -10);

        this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas, antialias: true, alpha: false });
        this.renderer.setSize(window.innerWidth, window.innerHeight);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        window.addEventListener('resize', () => {
            this.camera.aspect = window.innerWidth / window.innerHeight;
            this.camera.updateProjectionMatrix();
            this.renderer.setSize(window.innerWidth, window.innerHeight);
        });
    }

    setupLights() {
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.75);
        this.scene.add(ambientLight);

        const dirLight = new THREE.DirectionalLight(0xfffaed, 1.3);
        dirLight.position.set(20, 40, 20);
        dirLight.castShadow = true;
        dirLight.shadow.mapSize.width = 1024;
        dirLight.shadow.mapSize.height = 1024;
        dirLight.shadow.camera.near = 0.5;
        dirLight.shadow.camera.far = 100;
        dirLight.shadow.camera.left = -15;
        dirLight.shadow.camera.right = 15;
        dirLight.shadow.camera.top = 15;
        dirLight.shadow.camera.bottom = -15;
        this.scene.add(dirLight);

        // Neon ambient accent lights
        const pointLight1 = new THREE.PointLight(0xff4757, 1.8, 45);
        pointLight1.position.set(-8, 5, -20);
        this.scene.add(pointLight1);

        const pointLight2 = new THREE.PointLight(0x00d2d3, 1.8, 45);
        pointLight2.position.set(8, 5, -40);
        this.scene.add(pointLight2);
    }

    createEnvironment() {
        for (let i = 0; i < 12; i++) {
            this.spawnTrackSegment(-i * 30);
        }
    }

    spawnTrackSegment(zPos) {
        const segment = new THREE.Group();

        // Main Ground Bed
        const groundGeo = new THREE.PlaneGeometry(16, 30);
        const groundMat = new THREE.MeshStandardMaterial({ color: 0x1e272e, roughness: 0.8 });
        const ground = new THREE.Mesh(groundGeo, groundMat);
        ground.rotation.x = -Math.PI / 2;
        ground.receiveShadow = true;
        segment.add(ground);

        // Steel Rails & Wooden Ties
        const railMat = new THREE.MeshStandardMaterial({ color: 0xdcdde1, metalness: 0.9, roughness: 0.2 });
        const tieMat = new THREE.MeshStandardMaterial({ color: 0x485460, roughness: 0.9 });

        this.LANES.forEach(laneX => {
            [-0.7, 0.7].forEach(offset => {
                const railGeo = new THREE.BoxGeometry(0.12, 0.15, 30);
                const rail = new THREE.Mesh(railGeo, railMat);
                rail.position.set(laneX + offset, 0.08, 0);
                segment.add(rail);
            });

            for (let t = -14; t <= 14; t += 2) {
                const tieGeo = new THREE.BoxGeometry(1.8, 0.08, 0.4);
                const tie = new THREE.Mesh(tieGeo, tieMat);
                tie.position.set(laneX, 0.04, t);
                segment.add(tie);
            }
        });

        // Neon Archways
        if (Math.abs(zPos) % 60 === 0) {
            const archGroup = new THREE.Group();
            const archMat = new THREE.MeshStandardMaterial({
                color: 0x00d2d3,
                emissive: 0x00d2d3,
                emissiveIntensity: 0.7
            });

            const pillarL = new THREE.Mesh(new THREE.CylinderGeometry(0.3, 0.3, 8), archMat);
            pillarL.position.set(-8.5, 4, 0);
            const pillarR = new THREE.Mesh(new THREE.CylinderGeometry(0.3, 0.3, 8), archMat);
            pillarR.position.set(8.5, 4, 0);
            const topBar = new THREE.Mesh(new THREE.CylinderGeometry(0.25, 0.25, 17.5), archMat);
            topBar.rotation.z = Math.PI / 2;
            topBar.position.set(0, 8, 0);

            archGroup.add(pillarL, pillarR, topBar);
            segment.add(archGroup);
        }

        // Side Buildings
        [-11, 11].forEach(sideX => {
            const bldgGeo = new THREE.BoxGeometry(6, 25, 28);
            const bldgMat = new THREE.MeshStandardMaterial({ color: 0x111625, roughness: 0.7 });
            const bldg = new THREE.Mesh(bldgGeo, bldgMat);
            bldg.position.set(sideX, 12.5, 0);
            segment.add(bldg);
        });

        segment.position.z = zPos;
        this.scene.add(segment);
        this.tracks.push(segment);
    }

    createPlayerMesh() {
        this.playerGroup = new THREE.Group();

        const charColor = this.getCharacterColor(this.playerData.selectedCharacterId);

        const skinMat = new THREE.MeshStandardMaterial({ color: 0xffdbac, roughness: 0.5 });
        const clothMat = new THREE.MeshStandardMaterial({ color: charColor, roughness: 0.4 });
        const pantsMat = new THREE.MeshStandardMaterial({ color: 0x2f3542, roughness: 0.6 });

        // Head
        const headGeo = new THREE.BoxGeometry(0.5, 0.5, 0.5);
        this.head = new THREE.Mesh(headGeo, skinMat);
        this.head.position.y = 1.6;
        this.head.castShadow = true;
        this.playerGroup.add(this.head);

        // Cap / Hair
        const capGeo = new THREE.BoxGeometry(0.54, 0.15, 0.6);
        const capMat = new THREE.MeshStandardMaterial({ color: charColor });
        const cap = new THREE.Mesh(capGeo, capMat);
        cap.position.set(0, 1.88, -0.05);
        this.playerGroup.add(cap);

        // Torso
        const torsoGeo = new THREE.BoxGeometry(0.65, 0.7, 0.4);
        this.torso = new THREE.Mesh(torsoGeo, clothMat);
        this.torso.position.y = 1.05;
        this.torso.castShadow = true;
        this.playerGroup.add(this.torso);

        // Articulated Arms & Legs (Upper + Lower Joint Nodes)
        this.limbs = {};

        // Left Arm Group
        this.limbs.leftArmGroup = new THREE.Group();
        this.limbs.leftArmGroup.position.set(-0.42, 1.3, 0);
        const leftArmMesh = new THREE.Mesh(new THREE.BoxGeometry(0.2, 0.55, 0.2), clothMat);
        leftArmMesh.position.y = -0.27;
        this.limbs.leftArmGroup.add(leftArmMesh);
        this.playerGroup.add(this.limbs.leftArmGroup);

        // Right Arm Group
        this.limbs.rightArmGroup = new THREE.Group();
        this.limbs.rightArmGroup.position.set(0.42, 1.3, 0);
        const rightArmMesh = new THREE.Mesh(new THREE.BoxGeometry(0.2, 0.55, 0.2), clothMat);
        rightArmMesh.position.y = -0.27;
        this.limbs.rightArmGroup.add(rightArmMesh);
        this.playerGroup.add(this.limbs.rightArmGroup);

        // Left Leg Group
        this.limbs.leftLegGroup = new THREE.Group();
        this.limbs.leftLegGroup.position.set(-0.2, 0.7, 0);
        const leftLegMesh = new THREE.Mesh(new THREE.BoxGeometry(0.24, 0.65, 0.24), pantsMat);
        leftLegMesh.position.y = -0.32;
        this.limbs.leftLegGroup.add(leftLegMesh);
        this.playerGroup.add(this.limbs.leftLegGroup);

        // Right Leg Group
        this.limbs.rightLegGroup = new THREE.Group();
        this.limbs.rightLegGroup.position.set(0.2, 0.7, 0);
        const rightLegMesh = new THREE.Mesh(new THREE.BoxGeometry(0.24, 0.65, 0.24), pantsMat);
        rightLegMesh.position.y = -0.32;
        this.limbs.rightLegGroup.add(rightLegMesh);
        this.playerGroup.add(this.limbs.rightLegGroup);

        // Hoverboard 3D Mesh
        const boardGeo = new THREE.BoxGeometry(0.8, 0.1, 2.2);
        const boardMat = new THREE.MeshStandardMaterial({
            color: 0x00d2d3,
            emissive: 0x00d2d3,
            emissiveIntensity: 0.8,
            metalness: 0.8
        });
        this.hoverboardMesh = new THREE.Mesh(boardGeo, boardMat);
        this.hoverboardMesh.position.set(0, -0.05, 0);
        this.hoverboardMesh.visible = false;
        this.playerGroup.add(this.hoverboardMesh);

        // Jetpack 3D Mesh
        const jetGeo = new THREE.CylinderGeometry(0.15, 0.15, 0.7);
        const jetMat = new THREE.MeshStandardMaterial({ color: 0xff4757, metalness: 0.8 });
        this.jetpackMesh = new THREE.Group();
        const tube1 = new THREE.Mesh(jetGeo, jetMat);
        tube1.position.x = -0.25;
        const tube2 = new THREE.Mesh(jetGeo, jetMat);
        tube2.position.x = 0.25;
        this.jetpackMesh.add(tube1, tube2);
        this.jetpackMesh.position.set(0, 1.1, 0.3);
        this.jetpackMesh.visible = false;
        this.playerGroup.add(this.jetpackMesh);

        this.playerGroup.position.set(0, 0, 0);
        this.scene.add(this.playerGroup);
    }

    getCharacterColor(charId) {
        switch (charId) {
            case 'tricky': return 0x2ed573;
            case 'fresh': return 0x1e90ff;
            case 'ninja': return 0xa55eea;
            case 'cosmic': return 0xffa502;
            default: return 0xff4757;
        }
    }

    setupEventListeners() {
        window.addEventListener('keydown', (e) => {
            if (!this.isPlaying || this.isPaused) return;

            switch (e.key) {
                case 'ArrowLeft':
                case 'a':
                case 'A':
                    this.moveLane(-1);
                    break;
                case 'ArrowRight':
                case 'd':
                case 'D':
                    this.moveLane(1);
                    break;
                case 'ArrowUp':
                case 'w':
                case 'W':
                    this.jump();
                    break;
                case 'ArrowDown':
                case 's':
                case 'S':
                    this.slide();
                    break;
                case ' ':
                    this.activateHoverboard();
                    break;
            }
        });

        document.getElementById('touch-left').addEventListener('click', () => this.moveLane(-1));
        document.getElementById('touch-right').addEventListener('click', () => this.moveLane(1));
        document.getElementById('touch-up').addEventListener('click', () => this.jump());
        document.getElementById('touch-down').addEventListener('click', () => this.slide());
        document.getElementById('btn-trigger-board').addEventListener('click', () => this.activateHoverboard());

        document.getElementById('btn-start-game').addEventListener('click', () => this.startGame());
        document.getElementById('btn-restart-game').addEventListener('click', () => this.startGame());
        document.getElementById('btn-return-lobby').addEventListener('click', () => this.showLobby());

        document.getElementById('btn-sound-toggle').addEventListener('click', () => {
            const isMuted = window.soundSystem.toggleMute();
            const icon = document.getElementById('sound-icon');
            icon.className = isMuted ? 'fa-solid fa-volume-xmark' : 'fa-solid fa-volume-high';
        });

        document.getElementById('btn-open-shop').addEventListener('click', () => this.openModal('shop-modal'));
        document.getElementById('close-shop').addEventListener('click', () => this.closeModal('shop-modal'));

        document.getElementById('btn-open-chars').addEventListener('click', () => {
            this.openModal('shop-modal');
            this.switchShopTab('character');
        });

        document.getElementById('btn-open-leaderboard').addEventListener('click', async () => {
            await this.loadLeaderboard();
            this.openModal('leaderboard-modal');
        });
        document.getElementById('close-leaderboard').addEventListener('click', () => this.closeModal('leaderboard-modal'));

        document.getElementById('btn-open-achievements').addEventListener('click', async () => {
            await this.loadAchievements();
            this.openModal('achievements-modal');
        });
        document.getElementById('close-achievements').addEventListener('click', () => this.closeModal('achievements-modal'));

        document.getElementById('btn-login-modal').addEventListener('click', () => this.openModal('profile-modal'));
        document.getElementById('close-profile').addEventListener('click', () => this.closeModal('profile-modal'));

        document.getElementById('btn-save-profile').addEventListener('click', async () => {
            const username = document.getElementById('input-username').value.trim();
            if (username) {
                const res = await window.apiService.loginOrRegister(username);
                if (res) {
                    this.playerData = res;
                    this.updateUI();
                    this.closeModal('profile-modal');
                    this.showToast(`Logged in as ${this.playerData.username}`);
                }
            }
        });

        document.querySelectorAll('.shop-tabs .tab-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const cat = e.target.getAttribute('data-tab');
                this.switchShopTab(cat);
            });
        });
    }

    moveLane(direction) {
        const newIndex = this.currentLaneIndex + direction;
        if (newIndex >= 0 && newIndex < this.LANES.length) {
            this.currentLaneIndex = newIndex;
            this.targetX = this.LANES[this.currentLaneIndex];
            
            // Leaning / Bank animation trigger
            this.bankAngle = -direction * 0.28;
            window.soundSystem.playSlide();
        }
    }

    jump() {
        if (!this.isJumping && this.playerY <= 0.05) {
            this.isJumping = true;
            this.jumpVelocity = 14.5;
            window.soundSystem.playJump();
        }
    }

    slide() {
        if (!this.isSliding) {
            this.isSliding = true;
            this.slideTimer = 0.7;
            if (this.isJumping) {
                this.jumpVelocity = -22;
            }
            window.soundSystem.playSlide();
        }
    }

    activateHoverboard() {
        if (this.isPlaying && !this.powerupState.hoverboard) {
            this.powerupState.hoverboard = true;
            this.hoverboardMesh.visible = true;
            this.powerupsUsedInRun.add('Hoverboard');
            window.soundSystem.playHoverboard();
            this.showToast('HOVERBOARD ACTIVE! 1 SHIELD LIFE');
        }
    }

    startGame() {
        this.isPlaying = true;
        this.isPaused = false;
        
        // Low difficulty parameters:
        this.gameSpeed = 14; 
        this.distanceRun = 0;
        this.score = 0;
        this.coinsCollectedInRun = 0;
        this.currentLaneIndex = 1;
        this.targetX = 0;
        this.playerY = 0;
        this.powerupsUsedInRun.clear();

        this.powerupState = { magnet: 0, jetpack: 0, multiplier: 0, hoverboard: false };
        this.hoverboardMesh.visible = false;
        this.jetpackMesh.visible = false;

        this.clearWorldObjects();

        this.scene.remove(this.playerGroup);
        this.createPlayerMesh();

        this.showScreen('game-hud');
        window.soundSystem.startBgMusic();
    }

    clearWorldObjects() {
        this.obstacles.forEach(o => this.scene.remove(o.mesh));
        this.collectibles.forEach(c => this.scene.remove(c.mesh));
        this.particles.forEach(p => this.scene.remove(p.mesh));
        this.obstacles = [];
        this.collectibles = [];
        this.particles = [];
    }

    showScreen(screenId) {
        document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));
        document.getElementById(screenId).classList.add('active');
    }

    openModal(modalId) {
        document.getElementById(modalId).classList.add('active');
    }

    closeModal(modalId) {
        document.getElementById(modalId).classList.remove('active');
    }

    showToast(msg) {
        const toast = document.getElementById('toast');
        document.getElementById('toast-message').innerText = msg;
        toast.classList.add('show');
        setTimeout(() => toast.classList.remove('show'), 2500);
    }

    // ==================== PROCEDURAL GENERATION & PARTICLES ====================

    spawnParticleExplosion(pos, colorHex = 0xffd700, count = 12) {
        for (let i = 0; i < count; i++) {
            const pGeo = new THREE.SphereGeometry(0.12 + Math.random() * 0.08, 8, 8);
            const pMat = new THREE.MeshStandardMaterial({
                color: colorHex,
                emissive: colorHex,
                emissiveIntensity: 0.8
            });
            const pMesh = new THREE.Mesh(pGeo, pMat);
            pMesh.position.copy(pos);

            const vel = new THREE.Vector3(
                (Math.random() - 0.5) * 6,
                Math.random() * 5 + 1,
                (Math.random() - 0.5) * 6
            );

            this.scene.add(pMesh);
            this.particles.push({ mesh: pMesh, vel: vel, life: 0.4 });
        }
    }

    spawnObstacle(zPos) {
        const laneX = this.LANES[Math.floor(Math.random() * this.LANES.length)];
        const types = ['train', 'hurdle_low', 'hurdle_high'];
        const type = types[Math.floor(Math.random() * types.length)];

        let mesh = new THREE.Group();
        let bbox = { width: 2, height: 2, depth: 2 };

        if (type === 'train') {
            bbox = { width: 2.4, height: 3.2, depth: 13 };
            const bodyGeo = new THREE.BoxGeometry(2.4, 3.2, 13);
            const trainMat = new THREE.MeshStandardMaterial({ color: 0xd63031, metalness: 0.7, roughness: 0.3 });
            const body = new THREE.Mesh(bodyGeo, trainMat);
            body.position.y = 1.6;
            body.castShadow = true;
            mesh.add(body);

            // Front Headlights
            const lightGeo = new THREE.CylinderGeometry(0.22, 0.22, 0.1);
            const lightMat = new THREE.MeshStandardMaterial({ color: 0xfffa65, emissive: 0xfffa65, emissiveIntensity: 1 });
            const lightL = new THREE.Mesh(lightGeo, lightMat);
            lightL.rotation.x = Math.PI / 2;
            lightL.position.set(-0.75, 1.1, 6.55);
            const lightR = lightL.clone();
            lightR.position.x = 0.75;
            mesh.add(lightL, lightR);

        } else if (type === 'hurdle_low') {
            bbox = { width: 2.2, height: 1.1, depth: 0.4 };
            const barGeo = new THREE.BoxGeometry(2.2, 0.25, 0.4);
            const barMat = new THREE.MeshStandardMaterial({ color: 0xffa502 });
            const bar = new THREE.Mesh(barGeo, barMat);
            bar.position.y = 0.95;

            const postGeo = new THREE.CylinderGeometry(0.08, 0.08, 1.1);
            const postMat = new THREE.MeshStandardMaterial({ color: 0x2f3542 });
            const post1 = new THREE.Mesh(postGeo, postMat);
            post1.position.set(-0.9, 0.55, 0);
            const post2 = post1.clone();
            post2.position.x = 0.9;

            mesh.add(bar, post1, post2);
        } else {
            bbox = { width: 2.2, height: 2.1, depth: 0.4 };
            const topBarGeo = new THREE.BoxGeometry(2.2, 0.75, 0.4);
            const topBarMat = new THREE.MeshStandardMaterial({ color: 0xff4757 });
            const topBar = new THREE.Mesh(topBarGeo, topBarMat);
            topBar.position.y = 1.95;

            mesh.add(topBar);
        }

        mesh.position.set(laneX, 0, zPos);
        this.scene.add(mesh);

        this.obstacles.push({
            type: type,
            laneX: laneX,
            mesh: mesh,
            bbox: bbox,
            z: zPos
        });
    }

    spawnCollectible(zPos) {
        const laneX = this.LANES[Math.floor(Math.random() * this.LANES.length)];
        const isPowerup = Math.random() < 0.35; // Generous 35% powerup spawn rate!

        if (!isPowerup) {
            for (let c = 0; c < 5; c++) {
                const coinGeo = new THREE.TorusGeometry(0.35, 0.12, 12, 24);
                const coinMat = new THREE.MeshStandardMaterial({
                    color: 0xffd700,
                    emissive: 0xffa502,
                    emissiveIntensity: 0.6,
                    metalness: 0.9,
                    roughness: 0.1
                });
                const coin = new THREE.Mesh(coinGeo, coinMat);
                coin.rotation.y = Math.PI / 2;
                coin.position.set(laneX, 1.2, zPos - (c * 2.2));
                coin.castShadow = true;

                this.scene.add(coin);
                this.collectibles.push({ type: 'coin', mesh: coin });
            }
        } else {
            const pTypes = ['magnet', 'jetpack', 'multiplier'];
            const pType = pTypes[Math.floor(Math.random() * pTypes.length)];

            let geo = new THREE.OctahedronGeometry(0.5);
            let color = 0xff4757;
            if (pType === 'jetpack') color = 0xffa502;
            if (pType === 'multiplier') color = 0x2ed573;

            const mat = new THREE.MeshStandardMaterial({
                color: color,
                emissive: color,
                emissiveIntensity: 0.8
            });
            const pMesh = new THREE.Mesh(geo, mat);
            pMesh.position.set(laneX, 1.4, zPos);

            this.scene.add(pMesh);
            this.collectibles.push({ type: pType, mesh: pMesh });
        }
    }

    // ==================== MAIN GAME LOOP ====================

    animate(timestamp) {
        requestAnimationFrame((t) => this.animate(t));

        const delta = this.clock.getDelta();
        if (delta > 0.1) return;

        if (this.isPlaying && !this.isPaused) {
            this.updatePhysics(delta);
            this.updateWorld(delta);
            this.checkCollisions();
            this.updateHUD();
        }

        this.renderer.render(this.scene, this.camera);
    }

    updatePhysics(delta) {
        this.animTime += delta * 10;

        // Smooth X Lane Lerp & Bank Tilt Animation
        this.playerGroup.position.x += (this.targetX - this.playerGroup.position.x) * 12 * delta;
        this.playerGroup.rotation.z += (this.bankAngle - this.playerGroup.rotation.z) * 10 * delta;
        this.bankAngle *= 0.85; // Decay roll back to 0

        // Y Jump Gravity & Landing Squash
        if (this.isJumping) {
            this.playerY += this.jumpVelocity * delta;
            this.jumpVelocity += this.gravity * delta;

            if (this.playerY <= 0) {
                this.playerY = 0;
                this.isJumping = false;
                this.landingSquash = 0.2; // Trigger touchdown squash effect
                this.spawnParticleExplosion(this.playerGroup.position, 0x888888, 6);
            }
        }

        // Apply Squash & Stretch
        if (this.landingSquash > 0) {
            this.landingSquash -= delta * 2;
            const s = 1 + this.landingSquash;
            this.playerGroup.scale.set(s, 1 / s, s);
        } else if (!this.isSliding) {
            this.playerGroup.scale.set(1, 1, 1);
        }

        // Slide Crouch
        if (this.isSliding) {
            this.slideTimer -= delta;
            this.playerGroup.scale.set(1.1, 0.45, 1.1);
            if (this.slideTimer <= 0) {
                this.isSliding = false;
                this.playerGroup.scale.set(1, 1, 1);
            }
        }

        // Jetpack Mode
        if (this.powerupState.jetpack > 0) {
            this.powerupState.jetpack -= delta;
            this.playerY = 8.5;
            this.jetpackMesh.visible = true;

            // Exhaust particles
            if (Math.random() < 0.5) {
                const exhaustPos = this.playerGroup.position.clone().add(new THREE.Vector3(0, 0.8, 0.5));
                this.spawnParticleExplosion(exhaustPos, 0xffa502, 2);
            }

            if (this.powerupState.jetpack <= 0) {
                this.jetpackMesh.visible = false;
            }
        }

        this.playerGroup.position.y = this.playerY;

        // Smooth Running Joint Animations
        if (!this.isJumping && !this.isSliding && this.powerupState.jetpack <= 0) {
            const swing = Math.sin(this.animTime) * 0.7;
            this.limbs.leftArmGroup.rotation.x = swing;
            this.limbs.rightArmGroup.rotation.x = -swing;
            this.limbs.leftLegGroup.rotation.x = -swing;
            this.limbs.rightLegGroup.rotation.x = swing;

            // Torso subtle rhythmic bobbing
            this.torso.position.y = 1.05 + Math.sin(this.animTime * 2) * 0.04;
            this.head.position.y = 1.6 + Math.sin(this.animTime * 2) * 0.04;
        }

        // Update Hoverboard Bobbing & Glow
        if (this.powerupState.hoverboard) {
            this.hoverboardMesh.position.y = -0.05 + Math.sin(this.animTime * 3) * 0.05;
        }

        // Update Active Powerups
        if (this.powerupState.magnet > 0) this.powerupState.magnet -= delta;
        if (this.powerupState.multiplier > 0) {
            this.powerupState.multiplier -= delta;
            this.scoreMultiplier = 2;
        } else {
            this.scoreMultiplier = 1;
        }

        // Update Particle Systems
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const p = this.particles[i];
            p.life -= delta;
            p.mesh.position.addScaledVector(p.vel, delta);
            p.mesh.scale.multiplyScalar(0.92);

            if (p.life <= 0) {
                this.scene.remove(p.mesh);
                this.particles.splice(i, 1);
            }
        }

        // Camera Follow position with tilt
        const targetCamY = 4.2 + (this.playerY * 0.35);
        this.camera.position.x += (this.playerGroup.position.x * 0.35 - this.camera.position.x) * 6 * delta;
        this.camera.position.y += (targetCamY - this.camera.position.y) * 6 * delta;
        this.camera.rotation.z = -this.playerGroup.rotation.z * 0.3;
    }

    updateWorld(delta) {
        // Slow & Gentle speed acceleration curve (Very Easy Difficulty)
        if (this.gameSpeed < this.maxSpeed) {
            this.gameSpeed += delta * 0.06;
        }

        const moveDist = this.gameSpeed * delta;
        this.distanceRun += moveDist * 0.5;
        this.score += Math.round(moveDist * this.scoreMultiplier);

        // Move Tracks
        this.tracks.forEach(tr => {
            tr.position.z += moveDist;
            if (tr.position.z > 30) {
                tr.position.z -= 360;
            }
        });

        // Obstacles spawned further apart (Easy difficulty!)
        if (Math.random() < 0.016) {
            this.spawnObstacle(-190);
        }
        if (Math.random() < 0.05) {
            this.spawnCollectible(-180);
        }

        // Move Collectibles
        for (let i = this.collectibles.length - 1; i >= 0; i--) {
            const c = this.collectibles[i];
            c.mesh.position.z += moveDist;
            c.mesh.rotation.y += delta * 3.5;

            // Wide Magnet Attraction Radius (22m)
            if (this.powerupState.magnet > 0 && c.type === 'coin') {
                const distToPlayer = c.mesh.position.distanceTo(this.playerGroup.position);
                if (distToPlayer < 22) {
                    c.mesh.position.lerp(this.playerGroup.position, delta * 14);
                }
            }

            if (c.mesh.position.z > 15) {
                this.scene.remove(c.mesh);
                this.collectibles.splice(i, 1);
            }
        }

        // Move Obstacles
        for (let i = this.obstacles.length - 1; i >= 0; i--) {
            const obs = this.obstacles[i];
            obs.mesh.position.z += moveDist;

            if (obs.type === 'train') {
                obs.mesh.position.z += delta * 5; // Slow oncoming train speed
            }

            if (obs.mesh.position.z > 20) {
                this.scene.remove(obs.mesh);
                this.obstacles.splice(i, 1);
            }
        }
    }

    checkCollisions() {
        const playerPos = this.playerGroup.position;

        // Collectibles Pickup Check
        for (let i = this.collectibles.length - 1; i >= 0; i--) {
            const c = this.collectibles[i];
            const dist = c.mesh.position.distanceTo(playerPos);

            if (dist < 1.6) {
                if (c.type === 'coin') {
                    this.coinsCollectedInRun += 1;
                    this.score += 50;
                    window.soundSystem.playCoin();
                    this.spawnParticleExplosion(c.mesh.position, 0xffd700, 8);
                } else if (c.type === 'magnet') {
                    this.powerupState.magnet = 15; // 15 seconds
                    this.powerupsUsedInRun.add('Magnet');
                    this.showToast('MAGNET ACTIVATED (15s)!');
                } else if (c.type === 'jetpack') {
                    this.powerupState.jetpack = 10; // 10 seconds
                    this.powerupsUsedInRun.add('Jetpack');
                    window.soundSystem.playJetpack();
                    this.showToast('JETPACK ROCKET LAUNCH!');
                } else if (c.type === 'multiplier') {
                    this.powerupState.multiplier = 20; // 20 seconds
                    this.powerupsUsedInRun.add('Multiplier 2X');
                    this.showToast('2X MULTIPLIER BOOST!');
                }

                this.scene.remove(c.mesh);
                this.collectibles.splice(i, 1);
            }
        }

        if (this.powerupState.jetpack > 0) return;

        // Forgiving Collision Bounds (Easy difficulty!)
        for (let i = 0; i < this.obstacles.length; i++) {
            const obs = this.obstacles[i];
            const obsPos = obs.mesh.position;

            const zDiff = Math.abs(obsPos.z - playerPos.z);
            const xDiff = Math.abs(obsPos.x - playerPos.x);

            if (xDiff < (obs.bbox.width / 2 + 0.05) && zDiff < (obs.bbox.depth / 2 + 0.05)) {
                
                let hit = false;
                if (obs.type === 'hurdle_low') {
                    if (this.playerY < 0.9) hit = true;
                } else if (obs.type === 'hurdle_high') {
                    if (!this.isSliding) hit = true;
                } else if (obs.type === 'train') {
                    if (this.playerY < 3.0) hit = true;
                }

                if (hit) {
                    if (this.powerupState.hoverboard) {
                        this.powerupState.hoverboard = false;
                        this.hoverboardMesh.visible = false;
                        window.soundSystem.playCrash();
                        this.spawnParticleExplosion(playerPos, 0x00d2d3, 14);
                        this.showToast('HOVERBOARD SAVED YOU FROM CRASHING!');
                        this.scene.remove(obs.mesh);
                        this.obstacles.splice(i, 1);
                    } else {
                        this.onGameOver();
                        break;
                    }
                }
            }
        }
    }

    async onGameOver() {
        this.isPlaying = false;
        window.soundSystem.playCrash();
        window.soundSystem.stopBgMusic();

        const result = await window.apiService.submitGameSession(
            this.playerData.id,
            this.score,
            this.coinsCollectedInRun,
            Math.round(this.distanceRun),
            this.scoreMultiplier,
            Array.from(this.powerupsUsedInRun).join(',')
        );

        document.getElementById('go-score').innerText = this.score.toLocaleString();
        document.getElementById('go-coins').innerText = `+${this.coinsCollectedInRun}`;
        document.getElementById('go-distance').innerText = `${Math.round(this.distanceRun)} m`;

        const tag = document.getElementById('highscore-tag');
        if (result && result.isNewHighScore) {
            tag.style.display = 'inline-block';
            this.playerData.highScore = result.highScore;
        } else {
            tag.style.display = 'none';
        }

        if (result) {
            this.playerData.totalCoins = result.totalCoins;
        }

        const achBanner = document.getElementById('unlocked-achievements-banner');
        if (result && result.newlyUnlockedAchievements && result.newlyUnlockedAchievements.length > 0) {
            achBanner.style.display = 'block';
            achBanner.innerHTML = `🏆 UNLOCKED: <strong>${result.newlyUnlockedAchievements.join(', ')}</strong> (+Bonus Coins!)`;
        } else {
            achBanner.style.display = 'none';
        }

        this.updateUI();
        this.showScreen('gameover-screen');
    }

    updateHUD() {
        document.getElementById('hud-score').innerText = String(this.score).padStart(6, '0');
        document.getElementById('hud-coins').innerText = this.coinsCollectedInRun;
        document.getElementById('hud-multiplier').innerText = `${this.scoreMultiplier}X`;

        this.updatePBar('pbar-magnet', 'pbar-magnet-fill', this.powerupState.magnet, 15);
        this.updatePBar('pbar-jetpack', 'pbar-jetpack-fill', this.powerupState.jetpack, 10);
        this.updatePBar('pbar-multiplier', 'pbar-multiplier-fill', this.powerupState.multiplier, 20);

        const boardBar = document.getElementById('pbar-hoverboard');
        boardBar.style.display = this.powerupState.hoverboard ? 'flex' : 'none';
    }

    updatePBar(containerId, fillId, val, max) {
        const el = document.getElementById(containerId);
        if (val > 0) {
            el.style.display = 'flex';
            document.getElementById(fillId).style.width = `${(val / max) * 100}%`;
        } else {
            el.style.display = 'none';
        }
    }

    updateUI() {
        document.getElementById('player-name').innerText = this.playerData.username;
        document.getElementById('player-coins').innerText = this.playerData.totalCoins.toLocaleString();
        document.getElementById('player-highscore').innerText = this.playerData.highScore.toLocaleString();

        const activeChar = this.shopItems.find(i => i.id === this.playerData.selectedCharacterId);
        if (activeChar) {
            document.getElementById('lobby-char-name').innerText = activeChar.name;
        }

        const activeBoard = this.shopItems.find(i => i.id === this.playerData.selectedBoardId);
        if (activeBoard) {
            document.getElementById('lobby-board-name').innerText = `Board: ${activeBoard.name}`;
        }
    }

    showLobby() {
        this.isPlaying = false;
        this.showScreen('lobby-screen');
        this.updateUI();
    }

    // ==================== BACKEND API INTEGRATIONS ====================

    async loadPlayerData() {
        const player = await window.apiService.loginOrRegister('RunnerPro');
        if (player) {
            this.playerData = player;
        }
    }

    async loadShopCatalogue() {
        this.shopItems = await window.apiService.getShopItems();
        this.renderShop();
    }

    renderShop(filterCategory = 'all') {
        const grid = document.getElementById('shop-grid');
        grid.innerHTML = '';

        const items = filterCategory === 'all' 
            ? this.shopItems 
            : this.shopItems.filter(i => i.category === filterCategory);

        items.forEach(item => {
            const isOwned = this.playerData.ownedItemIds.includes(item.id) || item.unlockedByDefault;
            const isEquipped = (this.playerData.selectedCharacterId === item.id || this.playerData.selectedBoardId === item.id);

            const card = document.createElement('div');
            card.className = 'shop-card';
            card.innerHTML = `
                <div class="shop-card-icon" style="background: ${item.color}22; color: ${item.color};">
                    <i class="fa-solid ${item.iconCss}"></i>
                </div>
                <div class="shop-card-title">${item.name}</div>
                <div class="shop-card-desc">${item.description}</div>
                <div class="shop-card-footer">
                    ${isEquipped 
                        ? `<button class="btn btn-glass btn-full" disabled><i class="fa-solid fa-check"></i> EQUIPPED</button>`
                        : isOwned 
                            ? `<button class="btn btn-primary btn-full equip-btn" data-id="${item.id}" data-cat="${item.category}">EQUIP</button>`
                            : `<button class="btn btn-primary btn-full buy-btn" data-id="${item.id}"><i class="fa-solid fa-coins gold"></i> ${item.price}</button>`
                    }
                </div>
            `;

            grid.appendChild(card);
        });

        grid.querySelectorAll('.buy-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const itemId = e.target.closest('.buy-btn').getAttribute('data-id');
                const res = await window.apiService.buyItem(this.playerData.id, itemId);
                if (res.success) {
                    this.playerData.totalCoins = res.remainingCoins;
                    this.playerData.ownedItemIds = res.ownedItemIds;
                    this.showToast(res.message);
                    this.updateUI();
                    this.renderShop(filterCategory);
                } else {
                    this.showToast(res.message);
                }
            });
        });

        grid.querySelectorAll('.equip-btn').forEach(btn => {
            btn.addEventListener('click', async (e) => {
                const itemId = e.target.closest('.equip-btn').getAttribute('data-id');
                const cat = e.target.closest('.equip-btn').getAttribute('data-cat');
                
                const charId = cat === 'character' ? itemId : null;
                const boardId = cat === 'board' ? itemId : null;

                const res = await window.apiService.equipItem(this.playerData.id, charId, boardId);
                if (res) {
                    this.playerData = res;
                    this.showToast('Item Equipped!');
                    this.updateUI();
                    this.renderShop(filterCategory);
                }
            });
        });
    }

    switchShopTab(category) {
        document.querySelectorAll('.shop-tabs .tab-btn').forEach(b => {
            b.classList.toggle('active', b.getAttribute('data-tab') === category);
        });
        this.renderShop(category);
    }

    async loadLeaderboard() {
        const rowsContainer = document.getElementById('leaderboard-rows');
        rowsContainer.innerHTML = '<tr><td colspan="4" style="text-align:center;">Loading Ranks from .NET API...</td></tr>';

        const topPlayers = await window.apiService.getTopLeaderboard(10);
        rowsContainer.innerHTML = '';

        topPlayers.forEach(p => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><span class="rank-badge">#${p.rank}</span></td>
                <td><i class="fa-solid fa-user-ninja gold"></i> <strong>${p.username}</strong></td>
                <td><strong style="color:var(--accent-cyan);">${p.highScore.toLocaleString()}</strong></td>
                <td><i class="fa-solid fa-coins gold"></i> ${p.totalCoins.toLocaleString()}</td>
            `;
            rowsContainer.appendChild(tr);
        });
    }

    async loadAchievements() {
        const grid = document.getElementById('achievements-grid');
        grid.innerHTML = '<div style="text-align:center;">Loading Achievements from .NET API...</div>';

        const list = await window.apiService.getAchievements(this.playerData.id);
        grid.innerHTML = '';

        list.forEach(a => {
            const div = document.createElement('div');
            div.className = `ach-item ${a.isUnlocked ? 'unlocked' : ''}`;
            div.innerHTML = `
                <div class="ach-icon">
                    <i class="fa-solid fa-${a.icon}"></i>
                </div>
                <div class="ach-details">
                    <div class="ach-title">${a.title} ${a.isUnlocked ? '✓' : ''}</div>
                    <div class="ach-desc">${a.description}</div>
                    <div style="font-size:0.75rem; color:var(--text-muted); margin-top:4px;">
                        Progress: ${Math.min(a.currentProgress, a.targetValue)} / ${a.targetValue}
                    </div>
                </div>
                <div class="ach-reward">
                    <span class="gold"><i class="fa-solid fa-coins"></i> +${a.rewardCoins}</span>
                </div>
            `;
            grid.appendChild(div);
        });
    }
}

window.addEventListener('DOMContentLoaded', () => {
    window.gameInstance = new SubwaySurfersGame();
});
