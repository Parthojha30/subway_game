
/* ==========================================
   LIVE EC2 SERVER IDENTIFICATION
   ========================================== */

document.addEventListener("DOMContentLoaded", () => {

    const serverName = document.getElementById("server-name");
    const instanceId = document.getElementById("server-instance-id");
    const privateIp = document.getElementById("server-private-ip");
    const connectionStatus = document.getElementById("server-connection-status");
    const lastUpdated = document.getElementById("server-last-updated");

    let previousInstanceId = null;

    async function updateServerStatus() {
        try {
            const data = await window.apiService.getServerInfo();

            if (!data) {
                throw new Error("Server information unavailable");
            }

            serverName.textContent = data.serverName || "Unknown Server";
            instanceId.textContent = data.instanceId || "Unavailable";
            privateIp.textContent = data.privateIp || "Unavailable";

            connectionStatus.textContent = "● CONNECTED";
            connectionStatus.style.color = "#2ed573";

            lastUpdated.textContent = new Date().toLocaleTimeString();

            // Detect when ALB sends a request to another EC2
            if (
                previousInstanceId !== null &&
                previousInstanceId !== data.instanceId
            ) {
                console.log(
                    "Server changed:",
                    previousInstanceId,
                    "→",
                    data.instanceId
                );
            }

            previousInstanceId = data.instanceId;

        } catch (error) {

            console.error("Server monitoring error:", error);

            connectionStatus.textContent = "● DISCONNECTED";
            connectionStatus.style.color = "#ff4757";

            serverName.textContent = "Server Unavailable";
            instanceId.textContent = "--";
            privateIp.textContent = "--";
            lastUpdated.textContent = "--";
        }
    }

    // Initial request
    updateServerStatus();

    // Refresh server information every 5 seconds
    setInterval(updateServerStatus, 5000);

});
