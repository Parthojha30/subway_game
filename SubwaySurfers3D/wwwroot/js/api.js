/* ==========================================================================
   ASP.NET CORE WEB API CLIENT FOR SUBWAY SURFERS 3D
   ========================================================================== */

class ApiService {
    constructor() {
        this.baseUrl = window.location.origin;
    }

    async loginOrRegister(username) {
        try {
            const res = await fetch(`${this.baseUrl}/api/player/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username })
            });
            if (!res.ok) throw new Error('Failed to login');
            return await res.json();
        } catch (err) {
            console.error('API Login Error:', err);
            return null;
        }
    }

    async getProfile(playerId) {
        try {
            const res = await fetch(`${this.baseUrl}/api/player/${playerId}`);
            if (!res.ok) throw new Error('Failed to fetch profile');
            return await res.json();
        } catch (err) {
            console.error('API Profile Error:', err);
            return null;
        }
    }

    async equipItem(playerId, characterId = null, boardId = null) {
        try {
            const res = await fetch(`${this.baseUrl}/api/player/equip`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId, characterId, boardId })
            });
            if (!res.ok) throw new Error('Failed to equip item');
            return await res.json();
        } catch (err) {
            console.error('API Equip Error:', err);
            return null;
        }
    }

    async getShopItems() {
        try {
            const res = await fetch(`${this.baseUrl}/api/shop/items`);
            if (!res.ok) throw new Error('Failed to fetch shop items');
            return await res.json();
        } catch (err) {
            console.error('API Shop Error:', err);
            return [];
        }
    }

    async buyItem(playerId, itemId) {
        try {
            const res = await fetch(`${this.baseUrl}/api/shop/buy`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId, itemId })
            });
            return await res.json();
        } catch (err) {
            console.error('API Buy Error:', err);
            return { success: false, message: 'Server connection failed' };
        }
    }

    async getTopLeaderboard(limit = 10) {
        try {
            const res = await fetch(`${this.baseUrl}/api/leaderboard/top?limit=${limit}`);
            if (!res.ok) throw new Error('Failed to fetch leaderboard');
            return await res.json();
        } catch (err) {
            console.error('API Leaderboard Error:', err);
            return [];
        }
    }

    async getAchievements(playerId) {
        try {
            const res = await fetch(`${this.baseUrl}/api/achievement/${playerId}`);
            if (!res.ok) throw new Error('Failed to fetch achievements');
            return await res.json();
        } catch (err) {
            console.error('API Achievements Error:', err);
            return [];
        }
    }

    async submitGameSession(playerId, score, coinsCollected, distanceRun, maxMultiplier, powerupsUsed) {
        try {
            const res = await fetch(`${this.baseUrl}/api/game/submit`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    playerId,
                    score,
                    coinsCollected,
                    distanceRun,
                    maxMultiplier,
                    powerupsUsed
                })
            });
            if (!res.ok) throw new Error('Failed to submit game session');
            return await res.json();
        } catch (err) {
            console.error('API Session Submit Error:', err);
            return null;
        }
    }
}

window.apiService = new ApiService();
